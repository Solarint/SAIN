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
using HarmonyLib;

namespace SAIN.Components.BotController
{
    public class BotExtractManager : SAINControl
    {
        public BotExtractManager() { }

        public float TotalRaidTime { get; private set; }
        
        public void Update()
        {
            if (Singleton<AbstractGame>.Instance?.GameTimer == null)
            {
                return;
            }

            if (CheckRaidProgressTimer > Time.time)
            {
                return;
            }

            CheckTimeRemaining();
            CheckRaidProgressTimer = Time.time + 5f;
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

            int validExfils = GameWorldHandler.SAINGameWorld.ExtractFinder.CountValidExfilsForBot(bot);
            if (validExfils == 0)
            {
                if (SAINPlugin.DebugMode)
                {
                    Logger.LogInfo($"Could not select exfil for {bot.name}; no valid ones found");
                }

                ResetExfilSearchTime(bot);
                return false;
            }

            bool exfilAssigned = bot.Squad.BotInGroup ? TryAssignSquadExfil(bot) : TryAssignExfilForBot(bot);
            if (!exfilAssigned)
            {
                if (SAINPlugin.DebugMode)
                {
                    Logger.LogInfo($"{bot.name} could not find exfil. Bot spawn type: {bot.Info.WildSpawnType}");
                }

                ResetExfilSearchTime(bot);
                return false;
            }

            Logger.LogInfo($"{bot.name} has selected {bot.Memory.ExfilPoint.Settings.Name} for extraction");

            return true;
        }

        public static bool IsBotAllowedToExfil(SAINComponentClass bot)
        {
            if (!bot.Info.Profile.IsPMC && !bot.Info.Profile.IsScav)
            {
                return false;
            }

            return true;
        }

        private bool TryAssignExfilForBot(SAINComponentClass bot)
        {
            IDictionary<ExfiltrationPoint, Vector3> validExfils = GameWorldHandler.SAINGameWorld.ExtractFinder.GetValidExfilsForBot(bot);
            bot.Memory.ExfilPoint = selectExfilForBot(bot, validExfils);

            return bot.Memory.ExfilPoint != null;
        }

        public static float MinDistanceToExtract { get; private set; } = 10f;

        private ExfiltrationPoint selectExfilForBot(SAINComponentClass bot, IDictionary<ExfiltrationPoint, Vector3> validExfils)
        {
            // Check each valid extract to ensure the bot can use it and that it isn't too close. If this method is called when a bot is near an extract, it might be because
            // it got stuck. 
            NavMeshPath path = new NavMeshPath();
            IDictionary<ExfiltrationPoint, Vector3> possibleExfils = validExfils
                    .Where(x => CanBotsUseExtract(x.Key))
                    .Where(x => Vector3.Distance(bot.Position, x.Value) > MinDistanceToExtract)
                    .Where(x => NavMesh.CalculatePath(bot.Position, x.Value, -1, path) && (path.status == NavMeshPathStatus.PathComplete))
                    .ToDictionary(x => x.Key, x => x.Value);

            if (!possibleExfils.Any())
            {
                if (SAINPlugin.DebugMode)
                {
                    Logger.LogInfo($"Could not assign bot {bot.name} to any of {validExfils.Count} valid exfils: " + string.Join(", ", validExfils.Select(x => x.Key.Settings.Name)));
                }

                return null;
            }

            KeyValuePair<ExfiltrationPoint, Vector3> selectedExfil = possibleExfils.Random();
            bot.Memory.ExfilPosition = selectedExfil.Value;

            if (SAINPlugin.DebugMode)
            {
                Logger.LogInfo($"bot {bot.name} will extract at {selectedExfil.Key.Settings.Name}");
            }

            return selectedExfil.Key;
        }

        public bool CanBotsUseExtract(ExfiltrationPoint exfil)
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

        private bool TryAssignSquadExfil(SAINComponentClass bot)
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

        private void CheckTimeRemaining()
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
