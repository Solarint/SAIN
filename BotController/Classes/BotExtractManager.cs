using Comfort.Common;
using EFT;
using EFT.Interactive;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using SAIN.SAINComponent;
using SAIN.Helpers;
using JetBrains.Annotations;
using System.Linq;
using SAIN.Preset.GlobalSettings.Categories;

namespace SAIN.Components.BotController
{
    public class BotExtractManager : SAINControl
    {
        public BotExtractManager() { }

        public static float MinDistanceToExtract { get; private set; } = 10f;

        public ExfiltrationControllerClass ExfilController { get; private set; }
        public float TotalRaidTime { get; private set; }

        private Dictionary<ExfiltrationPoint, float> exfilActivationTimes = new Dictionary<ExfiltrationPoint, float>();

        public void Update()
        {
            if (!GetExfilControl())
            {
                return;
            }

            if (CheckExtractTimer < Time.time)
            {
                CheckExtractTimer = Time.time + 5f;
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

                if (Bots == null)
                {
                    return;
                }

                foreach (string botKey in Bots.Keys)
                {
                    TryFindExfils(Bots[botKey]);
                }
            }
        }

        public bool HasExfilBeenActivated(ExfiltrationPoint exfil)
        {
            if (exfil.Status == EExfiltrationStatus.UncompleteRequirements)
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
            if (HasExfilBeenActivated(exfil))
            {
                return exfilActivationTimes[exfil];
            }

            float exfilTime = Time.time + exfil.Settings.ExfiltrationTime + 0.5f;

            if (exfil.Settings.ExfiltrationType == EExfiltrationType.SharedTimer)
            {
                exfilActivationTimes.Add(exfil, exfilTime);
            }

            return exfilTime;
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

        private float DebugCheckExfilTimer = 0f;
        public ScavExfiltrationPoint[] AllScavExfils { get; private set; }
        public Dictionary<ScavExfiltrationPoint, Vector3> ValidScavExfils { get; private set; } = new Dictionary<ScavExfiltrationPoint, Vector3>();

        public ExfiltrationPoint[] AllExfils { get; private set; }
        public Dictionary<ExfiltrationPoint, Vector3> ValidExfils { get; private set; } = new Dictionary<ExfiltrationPoint, Vector3>();

        private float exfilSearchRetryDelay = 10;
        private Dictionary<SAINComponentClass, float> botExfilSearchRetryTime = new Dictionary<SAINComponentClass, float>();

        public bool TryFindExfilForBot(SAINComponentClass bot)
        {
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

            if (bot != null && (bot.Info.Profile.IsPMC || bot.Info.Profile.IsScav))
            {
                if (bot.Memory.CannotExfil)
                {
                    if (bot.Squad.LeaderComponent?.Memory.ExfilPosition != null)
                    {
                        if (SAINPlugin.DebugMode)
                        {
                            Logger.LogInfo($"Setting {bot.name} Exfil to Squad Leaders Exfil");
                        }
                        bot.Memory.ExfilPoint = bot.Squad.LeaderComponent?.Memory.ExfilPoint;
                        bot.Memory.ExfilPosition = bot.Squad.LeaderComponent?.Memory.ExfilPosition;
                        bot.Memory.CannotExfil = false;
                    }
                    return false;
                }

                if (bot.Memory.ExfilPosition != null)
                {
                    return false;
                }

                if (SAINPlugin.DebugMode)
                {
                    Logger.LogInfo($"Looking for Exfil for {bot.name}");
                }

                if (TryFindExfils(bot))
                {
                    if (bot.Squad.BotInGroup)
                    {
                        TryAssignSquadExfil(bot);
                    }
                    else
                    {
                        TryAssignExfil(bot);
                    }
                }
                else
                {
                    Logger.LogInfo($"Could not select exfil for {bot.name}; no valid ones found");
                }
                
                if (bot.Memory.ExfilPosition == null)
                {
                    //bot.Memory.CannotExfil = true;

                    if (SAINPlugin.DebugMode)
                    {
                        Logger.LogInfo($"{bot.BotOwner.name} Could Not find Exfil. Type: {bot.Info.WildSpawnType}");
                    }

                    ResetExfilSearchTime(bot);

                    return false;
                }

                return true;
            }
            else if (bot != null)
            {
                if (SAINPlugin.DebugMode)
                {
                    // Logger.LogInfo($"Skipped searching for Exfil for {bot.Value.name}. WildSpawnType: {bot.Value.Info.WildSpawnType}");
                }
            }
            else
            {
                if (SAINPlugin.DebugMode)
                {
                    Logger.LogInfo("Skipped searching for Exfil for unknown bot because they are null");
                }
            }

            return false;
        }

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

        private bool TryFindExfils(SAINComponentClass bot)
        {
            if (bot == null)
            {
                return false;
            }
            if (bot.Info.Profile.IsScav && AllScavExfils != null)
            {
                foreach (var ex in AllScavExfils)
                {
                    if (ex != null && ex.isActiveAndEnabled && !ValidScavExfils.ContainsKey(ex))
                    {
                        if (ex.TryGetComponent<Collider>(out var collider))
                        {
                            if (bot.Mover.CanGoToPoint(collider.transform.position, out Vector3 Destination, true, 3f))
                            {
                                ValidScavExfils.Add(ex, Destination);
                            }
                        }
                    }
                }

                return ValidScavExfils.Count > 0;
            }
            else
            {
                if (AllExfils != null)
                {
                    foreach (var ex in AllExfils)
                    {
                        // ex.isActiveAndEnabled && 
                        if (ex != null && !ValidExfils.ContainsKey(ex))
                        {
                            if (ex.TryGetComponent<Collider>(out var collider))
                            {
                                if (bot.Mover.CanGoToPoint(collider.transform.position, out Vector3 Destination, true))
                                {
                                    ValidExfils.Add(ex, Destination);
                                }
                                else
                                {
                                    if (SAINPlugin.DebugMode)
                                        Logger.LogWarning($"Could not find valid path to {ex.Settings.Name}");
                                }
                            }
                            else
                            {
                                if (SAINPlugin.DebugMode)
                                    Logger.LogWarning($"Could not find collider for {ex.Settings.Name}");
                            }
                        }
                        else if (ex == null)
                        {
                            if (SAINPlugin.DebugMode)
                                Logger.LogWarning($"Exfil is null in list!");
                        }
                    }

                    return ValidExfils.Count > 0;
                }
            }

            return false;
        }

        public bool TryAssignExfil(SAINComponentClass bot)
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

        private T selectExfilForBot<T>(SAINComponentClass bot, IDictionary<T, Vector3> validExfils) where T: ExfiltrationPoint
        {
            IDictionary<T, Vector3> possibleExfils = validExfils
                    .Where(x => CanUseExtract(x.Key))
                    .Where(x => Vector3.Distance(bot.Position, x.Value) > MinDistanceToExtract)
                    .ToDictionary(x => x.Key, x => x.Value);

            if (!possibleExfils.Any())
            {
                //if (SAINPlugin.DebugMode)
                {
                    Logger.LogInfo($"Could not assign bot {bot.name} to any of {validExfils.Count} valid exfils: " + string.Join(", ", validExfils.Select(x => x.Key.Settings.Name)));
                }

                return null;
            }

            KeyValuePair<T, Vector3> selectedExfil = possibleExfils.Random();
            bot.Memory.ExfilPosition = selectedExfil.Value;

            Logger.LogInfo($"bot {bot.name} will extract at {selectedExfil.Key.Settings.Name}");

            return selectedExfil.Key;
        }

        public bool CanUseExtract(ExfiltrationPoint exfil)
        {
            if (exfil.Status == EExfiltrationStatus.NotPresent)
            {
                return false;
            }

            if (exfil.Status == EExfiltrationStatus.AwaitsManualActivation)
            {
                //return false;
            }

            if ((exfil.Status == EExfiltrationStatus.UncompleteRequirements) && (exfil.Requirements.Any(x => x.Requirement == ERequirementState.WorldEvent)))
            {
                return false;
            }

            if (exfil.Requirements.Any(x => x.Requirement == ERequirementState.ScavCooperation))
            {
                return false;
            }

            if (exfil.Requirements.Any(x => x.Requirement == ERequirementState.Train))
            {
                return false;
            }

            if (GetTimeRemainingForExfil(exfil) < 1)
            {
                Logger.LogInfo($"Not enough time remaining for exfil {exfil.Settings.Name}");
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
                    if (!TryAssignExfil(bot))
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

            return bot.Memory.ExfilPoint != null;
        }

        private float CheckExtractTimer = 0f;
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
