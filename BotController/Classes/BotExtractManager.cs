using Comfort.Common;
using EFT;
using EFT.Interactive;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using SAIN.SAINComponent;
using SAIN.Helpers;
using System.Collections;

namespace SAIN.Components.BotController
{
    public class BotExtractManager : SAINControl
    {
        public BotExtractManager() { }

        public ExfiltrationControllerClass ExfilController { get; private set; }
        public float TotalRaidTime { get; private set; }

        public void Update()
        {
            if (!GetExfilControl())
            {
                return;
            }

            if (CheckRaidProgressTimer > Time.time)
            {
                return;
            }

            CheckRaidProgressTimer = Time.time + 5f;

            CheckTimeRemaining();

            if (DebugCheckExfilTimer < Time.time)
            {
                DebugCheckExfilTimer = Time.time + 30f;
                Logger.LogInfo(
                    $"Seconds Remaining in Raid: [{TimeRemaining}] Percentage of Raid Remaining: [{PercentageRemaining}]. " +
                    $"Total Raid Seconds: [{TotalRaidTime}] " +
                    $"Found: [{ValidScavExfils.Count}] ScavExfils and " +
                    $"[{ValidExfils.Count}] PMC Exfils to be used."
                    );
                // Logger.LogInfo(
                //     $"Total PMC Exfils on this map: [{AllExfils?.Length}] and " +
                //     $"[{AllScavExfils?.Length}] Total Scav Exfils")
                //     ;
            }
        }

        private bool GetExfilControl()
        {
            if (Singleton<AbstractGame>.Instance?.GameTimer == null)
            {
                return false;
            }

            if (ExfilController == null)
            {
                ExfilController = Singleton<GameWorld>.Instance.ExfiltrationController;
            }
            else
            {
                if (AllScavExfils == null)
                {
                    AllScavExfils = ExfilController.ScavExfiltrationPoints;
                    if (SAINPlugin.DebugMode && AllScavExfils != null)
                    {
                        Logger.LogInfo($"Found {AllScavExfils?.Length} possible Scav Exfil Points in this map.");
                    }
                }
                if (AllExfils == null)
                {
                    AllExfils = ExfilController.ExfiltrationPoints;
                    if (SAINPlugin.DebugMode && AllExfils != null)
                    {
                        Logger.LogInfo($"Found {AllExfils?.Length} possible Exfil Points in this map.");
                    }
                }
            }
            return ExfilController != null;
        }

        private Dictionary<ExfiltrationPoint, float> exfilActivationTimes = new Dictionary<ExfiltrationPoint, float>();

        public bool HasExfilBeenActivated(ExfiltrationPoint exfil)
        {
            // If all bots who paid for the car extract die, it will no longer leave. Therefore, the common extract time needs to be discarded. When this happens,
            // the exfil Status changes from EExfiltrationStatus.Countdown to EExfiltrationStatus.UncompleteRequirements.
            if ((exfil.Settings.ExfiltrationType == EExfiltrationType.SharedTimer) && (exfil.Status == EExfiltrationStatus.UncompleteRequirements))
            {
                if (exfilActivationTimes.ContainsKey(exfil))
                {
                    exfilActivationTimes.Remove(exfil);
                }
            }

            return exfilActivationTimes.ContainsKey(exfil);
        }

        public float GetTimeRemainingForExfil(ExfiltrationPoint exfil)
        {
            if (!HasExfilBeenActivated(exfil))
            {
                return float.MaxValue;
            }

            return Math.Max(0, exfilActivationTimes[exfil] - Time.time);
        }

        public float GetExfilTime(ExfiltrationPoint exfil)
        {
            // If a shared extract has already been activated (namely car extracts), return its departure time
            if (HasExfilBeenActivated(exfil))
            {
                return exfilActivationTimes[exfil];
            }

            float exfilTime = Time.time + exfil.Settings.ExfiltrationTime;

            // Trains are blacklisted right now, so this is just to protect for future changes. Ideally this needs to return the time when the train leaves,
            // but I'm not sure how to get that value. 
            if (exfil.Requirements.Any(x => x.Requirement == ERequirementState.Train))
            {
                return exfilTime;
            }

            // Store the exfil time for car extracts so all bots who pay leave at the same time
            if (exfil.Settings.ExfiltrationType == EExfiltrationType.SharedTimer)
            {
                // This is important! If the bot "extracts", it will be reported as dead in EFT. If it "dies" before the car departs, EFT will see
                // that no players are queued to use it. As a result, the extract timer will stop and the car will never leave. 
                exfilTime += 0.2f;

                exfilActivationTimes.Add(exfil, exfilTime);
            }

            return exfilTime;
        }

        private float DebugCheckExfilTimer = 0f;
        public ScavExfiltrationPoint[] AllScavExfils { get; private set; }
        public Dictionary<ScavExfiltrationPoint, Vector3> ValidScavExfils { get; private set; } = new Dictionary<ScavExfiltrationPoint, Vector3>();

        public ExfiltrationPoint[] AllExfils { get; private set; }
        public Dictionary<ExfiltrationPoint, Vector3> ValidExfils { get; private set; } = new Dictionary<ExfiltrationPoint, Vector3>();

        public bool IsFindingAllValidExfilsForAllBots { get; private set; } = false;

        public IEnumerator EnumerateAllValidExfilsForAllBots()
        {
            if (Bots == null)
            {
                yield break;
            }

            try
            {
                IsFindingAllValidExfilsForAllBots = true;

                foreach (string botKey in Bots.Keys.ToArray())
                {
                    if (!Bots.ContainsKey(botKey))
                    {
                        continue;
                    }

                    yield return TryFindAllValidExfilsForBot(Bots[botKey]);
                }
            }
            finally
            {
                IsFindingAllValidExfilsForAllBots = false;
            }
        }

        private bool TryFindAllValidExfilsForBot(SAINComponentClass bot)
        {
            if (bot.IsDead)
            {
                return false;
            }

            if (bot.Info.Profile.IsScav)
            {
                return CountAllValidExfilsForBot(bot, ValidScavExfils, AllScavExfils) > 0;
            }

            return CountAllValidExfilsForBot(bot, ValidExfils, AllExfils) > 0;
        }

        private int CountAllValidExfilsForBot<T>(SAINComponentClass bot, IDictionary<T, Vector3> validExfils, T[] allExfils) where T : ExfiltrationPoint
        {
            if (bot == null)
            {
                return 0;
            }

            if (allExfils == null)
            {
                return 0;
            }

            foreach (var ex in allExfils)
            {
                if (ex == null)
                {
                    if (SAINPlugin.DebugMode)
                        Logger.LogWarning($"Exfil is null in list!");

                    continue;
                }

                if (validExfils.ContainsKey(ex))
                {
                    continue;
                }

                if (!ex.TryGetComponent<Collider>(out var collider))
                {
                    if (SAINPlugin.DebugMode)
                        Logger.LogWarning($"Could not find collider for {ex.Settings.Name}");

                    continue;
                }

                if (!bot.Mover.CanGoToPoint(collider.transform.position, out Vector3 Destination, true))
                {
                    if (SAINPlugin.DebugMode)
                        Logger.LogWarning($"Could not find valid path to {ex.Settings.Name}");

                    continue;
                }

                validExfils.Add(ex, Destination);
            }

            return validExfils.Count;
        }

        private float exfilSearchRetryDelay = 10;
        private Dictionary<SAINComponentClass, float> botExfilSearchRetryTime = new Dictionary<SAINComponentClass, float>();

        public void ResetExfilSearchTime(SAINComponentClass bot)
        {
            if (botExfilSearchRetryTime.ContainsKey(bot))
            {
                botExfilSearchRetryTime[bot] = Time.time + exfilSearchRetryDelay;
            }
            else
            {
                botExfilSearchRetryTime.Add(bot, Time.time + exfilSearchRetryDelay);
            }
        }

        public bool TryFindExfilForBot(SAINComponentClass bot)
        {
            if (bot == null)
            {
                if (SAINPlugin.DebugMode)
                {
                    Logger.LogInfo("Skipped searching for Exfil for unknown bot because they are null");
                }

                return false;
            }

            // Only allow this to run every so often to prevent it from spamming the game console and to improve performance
            if (botExfilSearchRetryTime.ContainsKey(bot))
            {
                if (Time.time < botExfilSearchRetryTime[bot])
                {
                    return false;
                }
            }

            if (AllExfils.Length == 0)
            {
                ResetExfilSearchTime(bot);

                return false;
            }

            if (!IsBotAllowedToExfil(bot))
            {
                return false;
            }
            
            // If an exfil has already been assigned, don't continue searching
            if ((bot.Memory.ExfilPosition != null) && (bot.Memory.ExfilPoint != null))
            {
                return true;
            }

            if (SAINPlugin.DebugMode)
            {
                Logger.LogInfo($"Looking for Exfil for {bot.name}...");
            }

            if (TryFindAllValidExfilsForBot(bot))
            {
                bool exfilAssigned = bot.Squad.BotInGroup ? TryAssignSquadExfil(bot) : TryAssignExfilForBot(bot);
            }
            else
            {
                if (SAINPlugin.DebugMode)
                {
                    Logger.LogInfo($"Could not select exfil for {bot.name}; no valid ones found");
                }
            }

            if (bot.Memory.ExfilPosition == null)
            {
                if (SAINPlugin.DebugMode)
                {
                    Logger.LogInfo($"{bot.BotOwner.name} Could Not find Exfil. Type: {bot.Info.WildSpawnType}");
                }

                ResetExfilSearchTime(bot);

                return false;
            }

            return true;
        }

        public bool IsBotAllowedToExfil(SAINComponentClass bot)
        {
            if (!bot.Info.Profile.IsPMC && !bot.Info.Profile.IsScav)
            {
                return false;
            }

            return true;
        }

        public bool TryAssignExfilForBot(SAINComponentClass bot)
        {
            if (bot?.Info?.Profile.IsScav == true)
            {
                bot.Memory.ExfilPoint = selectExfilForBot(bot, ValidScavExfils);
            }
            if (bot?.Info?.Profile.IsPMC == true)
            {
                bot.Memory.ExfilPoint = selectExfilForBot(bot, ValidExfils);
            }

            return bot.Memory.ExfilPoint != null;
        }

        public static float MinDistanceToExtract { get; private set; } = 10f;

        private T selectExfilForBot<T>(SAINComponentClass bot, IDictionary<T, Vector3> validExfils) where T: ExfiltrationPoint
        {
            // Check each valid extract to ensure the bot can use it and that it isn't too close. If this method is called when a bot is near an extract, it might be because
            // it got stuck. 
            NavMeshPath Path = new NavMeshPath();
            IDictionary<T, Vector3> possibleExfils = validExfils
                    .Where(x => CanUseExtract(x.Key))
                    .Where(x => Vector3.Distance(bot.Position, x.Value) > MinDistanceToExtract)
                    .Where(x => NavMesh.CalculatePath(bot.Position, x.Value, -1, Path) && Path.status == NavMeshPathStatus.PathComplete)
                    .ToDictionary(x => x.Key, x => x.Value);

            if (!possibleExfils.Any())
            {
                if (SAINPlugin.DebugMode)
                {
                    Logger.LogInfo($"Could not assign bot {bot.name} to any of {validExfils.Count} valid exfils: " + string.Join(", ", validExfils.Select(x => x.Key.Settings.Name)));
                }

                return null;
            }

            KeyValuePair<T, Vector3> selectedExfil = possibleExfils.Random();
            bot.Memory.ExfilPosition = selectedExfil.Value;

            if (SAINPlugin.DebugMode)
            {
                Logger.LogInfo($"bot {bot.name} will extract at {selectedExfil.Key.Settings.Name}");
            }

            return selectedExfil.Key;
        }

        public bool CanUseExtract(ExfiltrationPoint exfil)
        {
            // Only use the extract if it's available in the raid
            // NOTE: Extracts unavailable for you are disabled (exfil.isActiveAndEnabled = false), but we can't use that property because all PMC extracts may be disabled if
            // you're a Scav.
            if (exfil.Status == EExfiltrationStatus.NotPresent)
            {
                return false;
            }

            // Having an unfriendly bot follow another one to a coop exfil would be pretty challenging, so let's just disable them entirely
            if (exfil.Requirements.Any(x => x.Requirement == ERequirementState.ScavCooperation))
            {
                return false;
            }

            // There are no NavMeshObstacles for trains, so bots get stuck on them
            // NOTE: The exfil Status will be EExfiltrationStatus.UncompleteRequirements until the train arrives. After it arrives, the exfil Status is
            // EExfiltrationStatus.AwaitsManualActivation. When it leaves, it changes to EExfiltrationStatus.NotPresent.
            // TODO: Even if NavMeshObstacles are added, how do we get the time when the train will leave?
            if (exfil.Requirements.Any(x => x.Requirement == ERequirementState.Train))
            {
                return false;
            }

            // These extracts typically require a switch to be activated, which can get complicated. An example is the Medical Elevator extract on Labs. You first need
            // to turn on the power, then call the elevator, then press the elevator button. However, the ExfiltrationPoint only monitors the final button press inside
            // the elevator, so there isn't an easy way to check the status of the other switches (without hard-coding the sequence). Therefore, let's just disable these.
            if ((exfil.Status == EExfiltrationStatus.UncompleteRequirements) && (exfil.Requirements.Any(x => x.Requirement == ERequirementState.WorldEvent)))
            {
                return false;
            }

            // If the VEX is just about to leave, don't select it
            if (GetTimeRemainingForExfil(exfil) < 1)
            {
                /*if (SAINPlugin.DebugMode)
                {
                    Logger.LogInfo($"Not enough time remaining for exfil {exfil.Settings.Name}");
                }*/

                return false;
            }

            return true;
        }

        public bool TryAssignSquadExfil(SAINComponentClass bot)
        {
            var squad = bot.Squad;
            if (squad.IAmLeader)
            {
                if (bot.Memory.ExfilPosition == null)
                {
                    if (!TryAssignExfilForBot(bot))
                    {
                        return false;
                    }
                }
                if (bot.Memory.ExfilPosition != null)
                {
                    if (squad.Members != null && squad.Members.Count > 0)
                    {
                        foreach (var member in squad.Members)
                        {
                            if (member.Value.Memory.ExfilPosition == null && member.Value.ProfileId != bot.ProfileId)
                            {
                                Vector3 random = UnityEngine.Random.onUnitSphere * 2f;
                                random.y = 0f;
                                Vector3 point = bot.Memory.ExfilPosition.Value + random;
                                if (NavMesh.SamplePosition(point, out var navHit, 1f, -1))
                                {
                                    member.Value.Memory.ExfilPosition = navHit.position;
                                }
                                else
                                {
                                    member.Value.Memory.ExfilPosition = bot.Memory.ExfilPosition;
                                }

                                member.Value.Memory.ExfilPoint = bot.Memory.ExfilPoint;
                            }
                        }
                    }
                }
            }
            else
            {
                bot.Memory.ExfilPoint = squad.LeaderComponent?.Memory.ExfilPoint;
                bot.Memory.ExfilPosition = squad.LeaderComponent?.Memory.ExfilPosition;
            }

            return (bot.Memory.ExfilPosition != null) && (bot.Memory.ExfilPoint != null);
        }

        private float CheckRaidProgressTimer = 0f;
        public float TimeRemaining { get; private set; } = 999f;
        public float PercentageRemaining { get; private set; } = 100f;

        public void CheckTimeRemaining()
        {
            TotalRaidTime = Aki.SinglePlayer.Utils.InRaid.RaidChangesUtil.OriginalEscapeTimeSeconds;

            //if (Aki.SinglePlayer.Utils.InRaid.RaidTimeUtil.HasRaidStarted())
            if (Singleton<AbstractGame>.Instance.GameTimer.Started())
            {
                TimeRemaining = Aki.SinglePlayer.Utils.InRaid.RaidTimeUtil.GetRemainingRaidSeconds();
                PercentageRemaining = Aki.SinglePlayer.Utils.InRaid.RaidTimeUtil.GetRaidTimeRemainingFraction() * 100;
            }
            else
            {
                TimeRemaining = Aki.SinglePlayer.Utils.InRaid.RaidChangesUtil.NewEscapeTimeSeconds;
                PercentageRemaining = 100f * TimeRemaining / TotalRaidTime;
            }
        }
    }
}
