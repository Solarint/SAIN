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

            if (!this.IsSquadLead)
            {
                LeaderComponent = Leader.GetComponent<SAINComponent>();
            }

            Console.WriteLine($"For Bot: [{BotOwner.Profile.Nickname}]: [{Leader.Profile.Nickname}] is Squad lead! Power Level = [{power}] Squad Power = [{SquadPowerLevel}] Members Count = [{SquadMembers.Count}]");
        }

        public float SquadPowerLevel { get; private set; }

        public bool IsSquadLead { get; private set; } = false;

        public BotOwner Leader { get; private set; }
        public SAINComponent LeaderComponent { get; private set; }

        public bool BotInGroup => BotOwner.BotsGroup.MembersCount > 1;

        public Dictionary<BotOwner, SAINComponent> SquadMembers { get; private set; } = new Dictionary<BotOwner, SAINComponent>();

        public SAINLogicDecision[] GroupDecisions { get; private set; }

        private void UpdateMembers()
        {
            var locations = new List<Vector3>();
            var dictionary = new Dictionary<BotOwner, SAINComponent>();
            var decisions = new List<SAINLogicDecision>();

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
                            locations.Add(member.Position);

                            if (component.Decisions.RetreatDecisions.Contains(component.CurrentDecision))
                            {
                                MemberIsFallingBack = true;
                            }
                        }
                    }
                    i++;
                }
            }

            SquadLocations = locations.ToArray();
            GroupDecisions = decisions.ToArray();

            SquadPowerLevel = BotOwner.BotsGroup.GroupPower;
            SquadMembers = dictionary;
        }

        public bool MemberIsFallingBack { get; private set; }

        public Vector3[] SquadLocations { get; private set; }

        protected ManualLogSource Logger;
    }
}