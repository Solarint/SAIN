using EFT;
using SAIN.Components;
using SAIN.Helpers;
using SAIN_Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Classes
{
    public class SquadClass : SAINBot
    {
        public SquadClass(BotOwner bot) : base(bot)
        {
        }

        public float SquadPowerLevel { get; private set; }
        public bool IsSquadLead { get; private set; } = false;
        public BotOwner Leader { get; private set; }
        public bool BotInGroup => BotOwner.BotsGroup.MembersCount > 1;

        public void ManualUpdate()
        {
            if (BotOwner.GetPlayer?.HealthController?.IsAlive == false || BotOwner.BotState != EBotState.Active)
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
            float power = Core.Info.PowerLevel;

            // Get each of the sain core components in the bot group
            var allies = GetComponentHelpers.GetBotComponents<SAINCoreComponent>(SquadMembers.ToList());

            // Iterate through each bot component in friendly group to see who has the highest power level
            foreach (var bot in allies)
            {
                if (bot.Info.PowerLevel > power)
                {
                    power = bot.Info.PowerLevel;
                    Leader = bot.BotOwner;
                }
            }

            // If the current bot is the result, mark the IsSquadLead value as true
            IsSquadLead = Leader == BotOwner;

            Console.WriteLine($"For Bot: [{BotOwner.Profile.Nickname}]: [{Leader.Profile.Nickname}] is Squad lead! Power Level = [{power}] Squad Power = [{SquadPowerLevel}] Members Count = [{SquadMembers.Length}]");
        }

        public BotOwner[] SquadMembers
        {
            get
            {
                List<BotOwner> members = new List<BotOwner>();

                if (BotInGroup)
                {
                    var group = BotOwner.BotsGroup;
                    int count = group.MembersCount;

                    int i = 0;

                    while (i < count)
                    {
                        if (group.Member(i)?.HealthController?.IsAlive == true)
                        {
                            members.Add(group.Member(i));
                        }
                        i++;
                    }
                }

                SquadPowerLevel = BotOwner.BotsGroup.GroupPower;

                return members.ToArray();
            }
        }

        public Vector3[] SquadLocations
        {
            get
            {
                List<Vector3> locations = new List<Vector3>();

                if (SquadMembers != null && BotInGroup)
                {
                    foreach (var member in SquadMembers)
                    {
                        locations.Add(member.Transform.position);
                    }
                }

                return locations.ToArray();
            }
        }

        private float CheckMembersTimer = 0f;
    }
}