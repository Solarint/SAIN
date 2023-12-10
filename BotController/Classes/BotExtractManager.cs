using Comfort.Common;
using EFT;
using EFT.Interactive;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using SAIN.SAINComponent;
using SAIN.Helpers;

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
                if (CheckExfilTimer < Time.time)
                {
                    CheckExfilTimer = Time.time + 3f;
                    try
                    {
                        FindExfilsForBots();
                    }
                    catch ( Exception e )
                    {
                        Logger.LogError(e);
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
        private float CheckExfilTimer = 0f;
        public ScavExfiltrationPoint[] AllScavExfils { get; private set; }
        public Dictionary<ScavExfiltrationPoint, Vector3> ValidScavExfils { get; private set; } = new Dictionary<ScavExfiltrationPoint, Vector3>();

        public ExfiltrationPoint[] AllExfils { get; private set; }
        public Dictionary<ExfiltrationPoint, Vector3> ValidExfils { get; private set; } = new Dictionary<ExfiltrationPoint, Vector3>();

        private bool FindExfilsForBots()
        {
            if (Bots?.Count > 0)
            {
                foreach (var bot in Bots)
                {
                    if (bot.Value != null && (bot.Value.Info.Profile.IsPMC || bot.Value.Info.Profile.IsScav))
                    {
                        if (bot.Value.Memory.CannotExfil)
                        {
                            if (bot.Value.Squad.LeaderComponent?.Memory.ExfilPosition != null)
                            {
                                if (SAINPlugin.DebugMode)
                                {
                                    Logger.LogInfo($"Setting {bot.Value.name} Exfil to Squad Leaders Exfil");
                                }
                                bot.Value.Memory.ExfilPosition = bot.Value.Squad.LeaderComponent?.Memory.ExfilPosition;
                                bot.Value.Memory.CannotExfil = false;
                            }
                            continue;
                        }

                        if (bot.Value.Memory.ExfilPosition != null)
                        {
                            continue;
                        }

                        if (SAINPlugin.DebugMode)
                        {
                            Logger.LogInfo($"Looking for Exfil for {bot.Value.name}");
                        }

                        FindExfils(bot.Value);
                        if (bot.Value.Squad.BotInGroup)
                        {
                            AssignSquadExfil(bot.Value);
                        }
                        else
                        {
                            AssignExfil(bot.Value);
                        }

                        if (bot.Value.Memory.ExfilPosition == null)
                        {
                            bot.Value.Memory.CannotExfil = true;

                            if (SAINPlugin.DebugMode)
                            {
                                Logger.LogInfo($"{bot.Value.BotOwner.name} Could Not find Exfil. Type: {bot.Value.Info.WildSpawnType}");
                            }
                        }
                    }
                    else if (bot.Value != null)
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
                }
                return ValidExfils.Count > 0 || ValidScavExfils.Count > 0;
            }
            return false;
        }

        private void FindExfils(SAINComponentClass bot)
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
                            if (ex.TryGetComponent<Collider>(out var collider))
                            {
                                if (bot.Mover.CanGoToPoint(collider.transform.position, out Vector3 Destination, true))
                                {
                                    ValidExfils.Add(ex, Destination);
                                }
                                else
                                {
                                    if (SAINPlugin.DebugMode)
                                        Logger.LogWarning($"Could not find valid path to {ex.name}");
                                }
                            }
                            else
                            {
                                if (SAINPlugin.DebugMode)
                                    Logger.LogWarning($"Could not find collider for {ex.name}");
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

        public void AssignExfil(SAINComponentClass bot)
        {
            if (bot?.Info?.Profile.IsScav == true)
            {
                if (ValidScavExfils.Count > 0)
                {
                    bot.Memory.ExfilPosition = ValidScavExfils.PickRandom().Value;
                }
                else
                {
                    if (SAINPlugin.DebugMode)
                    {
                        Logger.LogInfo("Valid Scav Exfils count is 0!");
                    }
                }
            }
            if (bot?.Info?.Profile.IsPMC == true)
            {
                if (ValidExfils.Count > 0)
                {
                    bot.Memory.ExfilPosition = ValidExfils.PickRandom().Value;
                }
                else
                {
                    if (SAINPlugin.DebugMode)
                    {
                        Logger.LogInfo("Valid PMC Exfils count is 0!");
                    }
                }
            }
        }

        public void AssignSquadExfil(SAINComponentClass bot)
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
