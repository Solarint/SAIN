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

        public ExfiltrationControllerClass ExfilController { get; private set; }
        public float TotalRaidTime { get; private set; }

        public void Update()
        {
            if (GetExfilControl())
            {
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
                }
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

        private float DebugCheckExfilTimer = 0f;
        public static ScavExfiltrationPoint[] AllScavExfils { get; private set; }
        public static Dictionary<ScavExfiltrationPoint, Vector3> ValidScavExfils { get; private set; } = new Dictionary<ScavExfiltrationPoint, Vector3>();

        public static ExfiltrationPoint[] AllExfils { get; private set; }
        public static Dictionary<ExfiltrationPoint, Vector3> ValidExfils { get; private set; } = new Dictionary<ExfiltrationPoint, Vector3>();

        public static bool TryFindExfilForBot(SAINComponentClass bot)
        {
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

                FindExfils(bot);
                if (bot.Squad.BotInGroup)
                {
                    AssignSquadExfil(bot);
                }
                else
                {
                    AssignExfil(bot);
                }

                if (bot.Memory.ExfilPosition == null)
                {
                    bot.Memory.CannotExfil = true;

                    if (SAINPlugin.DebugMode)
                    {
                        Logger.LogInfo($"{bot.BotOwner.name} Could Not find Exfil. Type: {bot.Info.WildSpawnType}");
                    }

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

        private static void FindExfils(SAINComponentClass bot)
        {
            if (bot == null)
            {
                return;
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
                            testExfil(ex, bot);
                            
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
                }
            }
        }

        private static void testExfil(ExfiltrationPoint exfil, SAINComponentClass bot)
        {
            if (!exfil.Requirements.Any(x => x.Requirement == ERequirementState.TransferItem))
            {
                return;
            }

            exfil.OnItemTransferred(bot.Player);

            if (exfil.Status == EExfiltrationStatus.UncompleteRequirements)
            {
                switch (exfil.Settings.ExfiltrationType)
                {
                    case EExfiltrationType.Individual:
                        exfil.SetStatusLogged(EExfiltrationStatus.RegularMode, "Proceed-3");
                        break;
                    case EExfiltrationType.SharedTimer:
                        exfil.SetStatusLogged(EExfiltrationStatus.Countdown, "Proceed-1");
                        Logger.LogInfo($"bot {bot.name} has started the VEX exfil");
                        break;
                    case EExfiltrationType.Manual:
                        exfil.SetStatusLogged(EExfiltrationStatus.AwaitsManualActivation, "Proceed-2");
                        break;
                }
            }
        }

        public static void AssignExfil(SAINComponentClass bot)
        {
            if (bot?.Info?.Profile.IsScav == true)
            {
                bot.Memory.ExfilPoint = selectExfilForBot(bot, ValidScavExfils);
            }
            if (bot?.Info?.Profile.IsPMC == true)
            {
                bot.Memory.ExfilPoint = selectExfilForBot(bot, ValidExfils);
            }
        }

        private static T selectExfilForBot<T>(SAINComponentClass bot, IDictionary<T, Vector3> validExfils) where T: ExfiltrationPoint
        {
            if (validExfils.Count > 0)
            {
                KeyValuePair<T, Vector3> selectedExfil = validExfils
                    .Where(x => CanUseExtract(x.Key))
                    .PickRandom();
                
                bot.Memory.ExfilPosition = selectedExfil.Value;

                Logger.LogInfo($"bot {bot.name} will extract at {selectedExfil.Key.Settings.Name}");

                return selectedExfil.Key;
            }
            else
            {
                if (SAINPlugin.DebugMode)
                {
                    Logger.LogInfo("Valid PMC Exfils count is 0!");
                }
            }

            return null;
        }

        public static bool CanUseExtract(ExfiltrationPoint exfil)
        {
            if (exfil.Status == EExfiltrationStatus.NotPresent)
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

            return true;
        }

        public static void AssignSquadExfil(SAINComponentClass bot)
        {
            var squad = bot.Squad;
            if (squad.IAmLeader)
            {
                if (bot.Memory.ExfilPosition == null)
                {
                    AssignExfil(bot);
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
