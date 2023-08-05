using Comfort.Common;
using EFT;
using EFT.Interactive;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using SAIN.SAINComponent;

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
                    CheckExtractTimer = Time.time + 10f;
                    CheckTimeRemaining();
                    if (DebugCheckExfilTimer < Time.time)
                    {
                        DebugCheckExfilTimer = Time.time + 30f;
                        Console.WriteLine($"Seconds Remaining in Raid: [{TimeRemaining}] Percentage of Raid Remaining: [{PercentageRemaining}]. Total Raid Seconds: [{TotalRaidTime}] Found: [{ValidScavExfils.Count}] ScavExfils and [{ValidExfils.Count}] PMC Exfils to be used.");
                    }
                }
                if (CheckExfilTimer < Time.time)
                {
                    CheckExfilTimer = Time.time + 3f;
                    FindExfilsForBots();
                }
            }
        }

        private bool GetExfilControl()
        {
            if (ExfilController == null)
            {
                ExfilController = Singleton<GameWorld>.Instance.ExfiltrationController;
            }
            else
            {
                if (AllScavExfils == null)
                {
                    AllScavExfils = ExfilController.ScavExfiltrationPoints;
                }
                if (AllExfils == null)
                {
                    AllExfils = ExfilController.ExfiltrationPoints;
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
                    if (bot.Value == null || (!bot.Value.Info.Profile.IsPMC && !bot.Value.Info.Profile.IsScav))
                    {
                        continue;
                    }
                    if (bot.Value.Memory.CannotExfil)
                    {
                        if (bot.Value.Squad.LeaderComponent?.Memory.ExfilPosition != null)
                        {
                            bot.Value.Memory.ExfilPosition = bot.Value.Squad.LeaderComponent?.Memory.ExfilPosition;
                        }
                        continue;
                    }
                    if (bot.Value.Memory.ExfilPosition == null)
                    {
                        FindExfils(bot.Value);
                        if (bot.Value.Squad.BotInGroup)
                        {
                            AssignSquadExfil(bot.Value);
                        }
                        else
                        {
                            AssignExfil(bot.Value);
                        }
                    }
                    if (bot.Value.Memory.ExfilPosition == null)
                    {
                        bot.Value.Memory.CannotExfil = true;
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
                        if (ex != null && ex.isActiveAndEnabled && !ValidExfils.ContainsKey(ex))
                        {
                            if (ex.TryGetComponent<Collider>(out var collider))
                            {
                                if (bot.Mover.CanGoToPoint(collider.transform.position, out Vector3 Destination, true))
                                {
                                    ValidExfils.Add(ex, Destination);
                                }
                            }
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
            }
            if (bot?.Info?.Profile.IsPMC == true)
            {
                if (ValidExfils.Count > 0)
                {
                    bot.Memory.ExfilPosition = ValidExfils.PickRandom().Value;
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
                    if (squad.SquadMembers != null && squad.SquadMembers.Count > 0)
                    {
                        foreach (var member in squad.SquadMembers)
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
            var GameTime = Singleton<AbstractGame>.Instance?.GameTimer;
            if (GameTime?.StartDateTime != null && GameTime?.EscapeDateTime != null)
            {
                var StartTime = GameTime.StartDateTime.Value;
                var EscapeTime = GameTime.EscapeDateTime.Value;
                var Span = EscapeTime - StartTime;
                TotalRaidTime = (float)Span.TotalSeconds;
                TimeRemaining = EscapeTimeSeconds(GameTime);
                float ratio = TimeRemaining / TotalRaidTime;
                PercentageRemaining = Mathf.Round(ratio * 100f);
            }
        }

        public float EscapeTimeSeconds(GameTimerClass timer)
        {
            DateTime? escapeDateTime = timer.EscapeDateTime;
            return (float)((escapeDateTime != null) ? (escapeDateTime.Value - GClass1292.UtcNow) : TimeSpan.MaxValue).TotalSeconds;
        }
    }
}
