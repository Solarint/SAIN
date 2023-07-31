using Comfort.Common;
using EFT;
using EFT.Interactive;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
                    if (bot.Value == null || (!bot.Value.Info.IsPMC && !bot.Value.Info.IsScav))
                    {
                        continue;
                    }
                    if (bot.Value.CannotExfil)
                    {
                        if (bot.Value.Squad.LeaderComponent?.ExfilPosition != null)
                        {
                            bot.Value.ExfilPosition = bot.Value.Squad.LeaderComponent?.ExfilPosition;
                        }
                        continue;
                    }
                    if (bot.Value.ExfilPosition == null)
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
                    if (bot.Value.ExfilPosition == null)
                    {
                        bot.Value.CannotExfil = true;
                    }
                }
                return ValidExfils.Count > 0 || ValidScavExfils.Count > 0;
            }
            return false;
        }

        private void FindExfils(SAINComponent bot)
        {
            if (bot == null)
            {
                return;
            }
            if (bot.Info.IsScav && AllScavExfils != null)
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

        public void AssignExfil(SAINComponent bot)
        {
            if (bot?.Info?.IsScav == true)
            {
                if (ValidScavExfils.Count > 0)
                {
                    bot.ExfilPosition = ValidScavExfils.PickRandom().Value;
                }
            }
            if (bot?.Info?.IsPMC == true)
            {
                if (ValidExfils.Count > 0)
                {
                    bot.ExfilPosition = ValidExfils.PickRandom().Value;
                }
            }
        }

        public void AssignSquadExfil(SAINComponent bot)
        {
            var squad = bot.Squad;
            if (squad.IAmLeader)
            {
                if (bot.ExfilPosition == null)
                {
                    AssignExfil(bot);
                }
                if (bot.ExfilPosition != null)
                {
                    if (squad.SquadMembers != null && squad.SquadMembers.Count > 0)
                    {
                        foreach (var member in squad.SquadMembers)
                        {
                            if (member.Value.ExfilPosition == null && member.Value.ProfileId != bot.ProfileId)
                            {
                                Vector3 random = UnityEngine.Random.onUnitSphere * 2f;
                                random.y = 0f;
                                Vector3 point = bot.ExfilPosition.Value + random;
                                if (NavMesh.SamplePosition(point, out var navHit, 1f, -1))
                                {
                                    member.Value.ExfilPosition = navHit.position;
                                }
                                else
                                {
                                    member.Value.ExfilPosition = bot.ExfilPosition;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                bot.ExfilPosition = squad.LeaderComponent?.ExfilPosition;
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
