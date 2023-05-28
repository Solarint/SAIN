using BepInEx.Logging;
using EFT;
using SAIN.Components;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Classes
{
    public class SquadClass : SAINBot
    {
        public SquadClass(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        public void ManualUpdate()
        {
            if (!SAIN.BotActive || SAIN.GameIsEnding)
            {
                return;
            }

            if (BotInGroup)
            {
                if (CheckMembersTimer < Time.time)
                {
                    CheckMembersTimer = Time.time + 5f;

                    if (Leader == null || !Leader.HealthController.IsAlive)
                    {
                        FindSquadLeader();
                    }
                }
            }
        }

        private void FindSquadLeader()
        {
            // Assign current bot as leader to start
            Leader = BotOwner;
            float power = SAIN.Info.PowerLevel;


            // Iterate through each bot component in friendly group to see who has the highest power level
            foreach (var bot in SquadMembers.Values)
            {
                if (bot.Info.PowerLevel > power)
                {
                    power = bot.Info.PowerLevel;
                    Leader = bot.BotOwner;
                }
            }

            // If the current bot is the result, mark the IsSquadLead value as true
            IsSquadLead = Leader == BotOwner;

            Console.WriteLine($"For Bot: [{BotOwner.Profile.Nickname}]: [{Leader.Profile.Nickname}] is Squad lead! Power Level = [{power}] Squad Power = [{SquadPowerLevel}] Members Count = [{SquadMembers.Count}]");
        }

        public float SquadPowerLevel { get; private set; }

        public bool IsSquadLead { get; private set; } = false;

        public BotOwner Leader { get; private set; }

        public bool BotInGroup => BotOwner.BotsGroup.MembersCount > 1;

        public Dictionary<BotOwner, SAINComponent> SquadMembers
        {
            get
            {
                var dictionary = new Dictionary<BotOwner, SAINComponent>();

                if (BotInGroup)
                {
                    var group = BotOwner.BotsGroup;
                    int count = group.MembersCount;

                    int i = 0;

                    while (i < count)
                    {
                        var member = group.Member(i);
                        if (member != null && member != BotOwner && member.HealthController.IsAlive)
                        {
                            dictionary.Add(member, member.GetComponent<SAINComponent>());
                        }
                        i++;
                    }
                }

                SquadPowerLevel = BotOwner.BotsGroup.GroupPower;
                return dictionary;
            }
        }

        public Vector3[] SquadLocations
        {
            get
            {
                List<Vector3> locations = new List<Vector3>();

                if (SquadMembers != null && BotInGroup && SquadMembers.Count > 0)
                {
                    foreach (var member in SquadMembers)
                    {
                        locations.Add(member.Key.Position);
                    }
                }

                return locations.ToArray();
            }
        }

        protected ManualLogSource Logger;

        private float CheckMembersTimer = 0f;
    }
}