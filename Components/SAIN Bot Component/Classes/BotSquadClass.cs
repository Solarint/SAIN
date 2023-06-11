using BepInEx.Logging;
using EFT;
using SAIN.Components;
using System;
using System.Collections.Generic;
using UnityEngine;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Classes
{
    public class SquadClass : SAINBot
    {
        public SquadClass(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        public void Update()
        {
            if (BotInGroup)
            {
                if (UpdateMembersTimer < Time.time)
                {
                    UpdateMembersTimer = Time.time + 0.25f;

                    UpdateMembers();

                    if (Leader == null || !Leader.HealthController.IsAlive || GroupSize != SquadMembers.Count)
                    {
                        GroupSize = SquadMembers.Count;
                        FindSquadLeader();
                    }
                }
            }
        }

        private int GroupSize = 0;
        private float UpdateMembersTimer = 0f;

        private void FindSquadLeader()
        {
            // If this bot is a boss type, they are the squad leader
            if (SAIN.Info.IsBoss)
            {
                IsSquadLead = true;
                Leader = BotOwner;
                LeaderComponent = SAIN;
                return;
            }
            // Assign current bot as leader to start
            BotOwner newSquadLead = BotOwner;
            float power = SAIN.Info.PowerLevel;

            // Iterate through each bot component in friendly group to see who has the highest power level or if any are bosses
            foreach (var bot in SquadMembers.Values)
            {
                // If this bot is a boss type, they are the squad leader
                if (bot.Info.IsBoss)
                {
                    newSquadLead = bot.BotOwner;
                    break;
                }
                // else If this bot has a higher power level than the last one we checked, they are the squad leader
                if (bot.Info.PowerLevel > power)
                {
                    power = bot.Info.PowerLevel;
                    newSquadLead = bot.BotOwner;
                }
            }

            // If the current bot is the result, mark the IsSquadLead value as true
            IsSquadLead = newSquadLead.ProfileId == BotOwner.ProfileId;
            Leader = newSquadLead;
            LeaderComponent = newSquadLead.GetComponent<SAINComponent>();

            if (DebugBotInfo.Value)
            Logger.LogDebug($"For Bot: [{BotOwner.Profile.Nickname}]: [{newSquadLead.Profile.Nickname}] is Squad lead! Power Level = [{power}] Squad Power = [{SquadPowerLevel}] Members Count = [{SquadMembers.Count}]");
        }

        public float SquadPowerLevel { get; private set; }

        public bool IsSquadLead { get; private set; } = false;

        public BotOwner Leader { get; private set; }
        public SAINComponent LeaderComponent { get; private set; }

        public bool BotInGroup => BotOwner.BotsGroup.MembersCount > 1;

        public Dictionary<BotOwner, SAINComponent> SquadMembers { get; private set; } = new Dictionary<BotOwner, SAINComponent>();

        public SAINSoloDecision[] SquadSoloDecisions { get; private set; }
        public SAINSquadDecision[] SquadDecisions { get; private set; }

        private void UpdateMembers()
        {
            var locations = new List<Vector3>();
            var dictionary = new Dictionary<BotOwner, SAINComponent>();
            var decisions = new List<SAINSoloDecision>();
            var squadDecisions = new List<SAINSquadDecision>();

            MemberIsFallingBack = false;

            if (BotInGroup)
            {
                var group = BotOwner.BotsGroup;
                int count = group.MembersCount;

                int i = 0;

                while (i < count)
                {
                    var member = group.Member(i);
                    if (member != null && member.HealthController.IsAlive)
                    {
                        var component = member.GetComponent<SAINComponent>();

                        if (component != null)
                        {
                            dictionary.Add(member, component);
                            decisions.Add(component.CurrentDecision);
                            squadDecisions.Add(component.Decision.SquadDecision);
                            locations.Add(member.Position);

                            if (component.CurrentDecision == SAINSoloDecision.Retreat)
                            {
                                MemberIsFallingBack = true;
                            }
                        }
                    }
                    i++;
                }
            }

            SquadLocations = locations.ToArray();
            SquadSoloDecisions = decisions.ToArray();
            SquadDecisions = squadDecisions.ToArray();

            SquadPowerLevel = BotOwner.BotsGroup.GroupPower;
            SquadMembers = dictionary;
        }

        public bool MemberIsFallingBack { get; private set; }

        public Vector3[] SquadLocations { get; private set; }

        protected ManualLogSource Logger;
    }
}