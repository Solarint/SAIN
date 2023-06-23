using BepInEx.Logging;
using EFT;
using SAIN.Components;
using System;
using System.Collections.Generic;
using UnityEngine;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Classes
{
    public class SquadClass : MonoBehaviour
    {
        private SAINComponent SAIN;
        private BotOwner BotOwner => SAIN.BotOwner;

        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
        }

        public string SquadID { get; private set; } = "None";

        private void Update()
        {
            if (BotOwner == null || SAIN == null) return;
            if (BotInGroup)
            {
                if (UpdateMembersTimer < Time.time)
                {
                    UpdateMembersTimer = Time.time + 0.25f;

                    UpdateMembers();
                    UpdateVisibleMembers();

                    if (Leader != null || Leader?.IsDead == true)
                    {
                        if (LeaderDieTime == 0f)
                        {
                            LeaderDieTime = Time.time;
                        }
                        if (TimeSinceLeaderDied > 30f)
                        {
                            FindSquadLeader();
                        }
                    }
                }

                if (SquadID == "None" && LeaderComponent != null && !IAmLeader)
                {
                    SquadID = LeaderComponent.Squad.SquadID;
                }
            }
        }

        private void UpdateVisibleMembers()
        {
            VisibleMembers.Clear();
            foreach (var member in SAIN.Squad.SquadMembers.Values)
            {
                if (member != null && SAIN.VisiblePlayers.Contains(member.Player))
                {
                    VisibleMembers.Add(member);
                }
            }
        }

        public List<SAINComponent> VisibleMembers { get; private set; } = new List<SAINComponent>();

        public float TimeSinceLeaderDied => LeaderDieTime == 0f ? 0f : Time.time - LeaderDieTime;
        public float LeaderDieTime { get; private set; } = 0f;
        private int GroupSize = 0;
        private float UpdateMembersTimer = 0f;

        private void FindSquadLeader()
        {
            // Assign current bot as leader to start
            float power = SAIN.Info.PowerLevel;
            var leadComponent = SAIN;

            if (SAIN.Info.IAmBoss)
            {
                AddLeader(leadComponent);
                return;
            }

            // Iterate through each bot component in friendly group to see who has the highest power level or if any are bosses
            foreach (var bot in SquadMembers.Values)
            {
                if (bot == null || bot.IsDead) continue;

                // If this bot is a boss type, they are the squad leader
                if (bot.Info.IAmBoss)
                {
                    leadComponent = bot;
                    break;
                }
                // else If this bot has a higher power level than the last one we checked, they are the squad leader
                if (bot.Info.PowerLevel > power)
                {
                    power = bot.Info.PowerLevel;
                    leadComponent = bot;
                }
            }

            AddLeader(leadComponent);
        }

        private void AddLeader(SAINComponent sain)
        {
            LeaderDieTime = 0f;
            IAmLeader = sain.ProfileId == BotOwner.ProfileId;
            LeaderComponent = sain;
            Leader = sain.BotOwner;
            if (IAmLeader && SquadID == "None")
            {
                SquadID = Guid.NewGuid().ToString("N");
            }
            if (DebugBotInfo.Value)
            {
                Logger.LogDebug($"For Bot: [{SAIN.BotOwner.name}]: [{sain.BotOwner.name}] is Squad lead! " +
                    $"Lead is Boss? {sain.Info.IAmBoss} Lead Power: [{sain.Info.PowerLevel}] My Power: [{SAIN.Info.PowerLevel}] " +
                    $"Squad Power = [{SquadPowerLevel}] Members Count = [{SquadMembers.Count}]");
            }
        }

        public float SquadPowerLevel { get; private set; }

        public bool IAmLeader { get; private set; } = false;

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

            if (BotInGroup)
            {
                var group = BotOwner.BotsGroup;
                int count = group.MembersCount;

                for (int i = 0; i < count; i++)
                {
                    var member = group.Member(i);
                    if (member != null && member.HealthController.IsAlive)
                    {
                        if (SAINPlugin.BotController.GetBot(member.ProfileId, out var component))
                        {
                            dictionary.Add(member, component);
                            decisions.Add(component.CurrentDecision);
                            squadDecisions.Add(component.Decision.CurrentSquadDecision);
                            locations.Add(member.Position);
                        }
                    }
                }
            }

            MemberIsFallingBack = decisions.Contains(SAINSoloDecision.Retreat) || decisions.Contains(SAINSoloDecision.RunToCover) || decisions.Contains(SAINSoloDecision.RunAway);

            SquadLocations = locations.ToArray();
            SquadSoloDecisions = decisions.ToArray();
            SquadDecisions = squadDecisions.ToArray();

            SquadPowerLevel = BotOwner.BotsGroup.GroupPower;
            SquadMembers = dictionary;

            if (Leader != null || Leader?.IsDead == true)
            {
                if (LeaderDieTime == 0f)
                {
                    LeaderDieTime = Time.time;
                }
                if (TimeSinceLeaderDied > 30f)
                {
                    FindSquadLeader();
                }
                return;
            }
            if ((Leader == null || GroupSize != SquadMembers.Count) && TimeSinceLeaderDied == 0f)
            {
                GroupSize = SquadMembers.Count;
                FindSquadLeader();
            }
        }

        public bool MemberIsFallingBack { get; private set; }

        public Vector3[] SquadLocations { get; private set; }

        protected ManualLogSource Logger;
    }
}