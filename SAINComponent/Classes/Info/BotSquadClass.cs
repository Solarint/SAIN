using BepInEx.Logging;
using EFT;
using SAIN.Components;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Info
{
    public class SAINSquadClass : SAINBase, ISAINClass
    {
        public SAINSquadClass(SAINComponentClass sain) : base(sain)
        {
        }

        public Action<IPlayer, DamageInfo, float> LeaderKilled { get; set; }
        public Action<SAINComponentClass, float> NewLeaderFound { get; set; }

        public void Init()
        {
        }

        private void LeaderWasKilled(Player player, IPlayer lastAggressor, DamageInfo lastDamageInfo, EBodyPart lastBodyPart)
        {
            if (SAINPlugin.DebugMode)
            {
                Logger.LogInfo(
                    $"Leader Name [{LeaderComponent?.name}] " +
                    $"was killed for Squad: [{SquadID}] " +
                    $"by [{lastAggressor?.Profile.Nickname}] " +
                    $"at Time: [{Time.time}] " +
                    $"by damage type: [{lastDamageInfo.DamageType}] " +
                    $"to Body part: [{lastBodyPart}]"
                    );
            }

            if (lastAggressor != null)
            {
                LeaderKilled?.Invoke(lastAggressor, lastDamageInfo, Time.time);
            }

            if (LeaderComponent?.BotOwner?.GetPlayer != null)
            {
                LeaderComponent.BotOwner.GetPlayer.OnPlayerDead -= LeaderWasKilled;
            }

            Leader = null;
            LeaderComponent = null;
        }

        public bool BotWasInSquad { get; private set; } = false;

        public void Update()
        {
            if (BotInGroup && UpdateMembersTimer < Time.time)
            {
                UpdateMembersTimer = Time.time + 0.25f;

                UpdateMembers();
                UpdateVisibleMembers();

                if (!SAIN.HasEnemy && (LeaderComponent == null || LastSquadCount != SquadMembers.Count))
                {
                    FindSquadLeader();
                    LastSquadCount = SquadMembers.Count;
                }

                if (LeaderComponent?.BotIsAlive == true)
                {
                    DistanceToSquadLeader = (SAIN.Position - LeaderComponent.Position).magnitude;
                }

                if (SquadID == "None" && LeaderComponent != null && !IAmLeader)
                {
                    SquadID = LeaderComponent.Squad.SquadID;
                }

                if (!BotWasInSquad && SquadMembers != null && SquadMembers.Count > 0)
                {
                    BotWasInSquad = true;
                }
            }
            else if (BotWasInSquad && !BotInGroup)
            {
                ClearSquadCache();
            }
        }

        public float DistanceToSquadLeader = 0f;

        public void Dispose()
        {
        }

        public string SquadID { get; private set; } = "None";

        private void UpdateVisibleMembers()
        {
            VisibleMembers.Clear();
            foreach (var member in SAIN.Squad.SquadMembers.Values)
            {
                if (member != null && SAIN.Memory.VisiblePlayers.Contains(member.Player))
                {
                    VisibleMembers.Add(member);
                }
            }
        }

        public readonly List<SAINComponentClass> VisibleMembers = new List<SAINComponentClass>();

        public float LeaderDieTime { get; private set; } = 0f;
        private int LastSquadCount = 0;
        private float UpdateMembersTimer = 0f;

        private void FindSquadLeader()
        {
            // Assign current bot as leader to start
            float power = SAIN.Info.Profile.PowerLevel;
            var leadComponent = SAIN;

            if (SAIN.Info.Profile.IsBoss)
            {
                AddLeader(leadComponent);
                return;
            }

            // Iterate through each bot component in friendly group to see who has the highest power level or if any are bosses
            foreach (var bot in SquadMembers.Values)
            {
                if (bot == null || bot.IsDead) continue;

                // If this bot is a boss type, they are the squad leader
                if (bot.Info.Profile.IsBoss)
                {
                    leadComponent = bot;
                    break;
                }
                // else If this bot has a higher power level than the last one we checked, they are the squad leader
                if (bot.Info.Profile.PowerLevel > power)
                {
                    power = bot.Info.Profile.PowerLevel;
                    leadComponent = bot;
                }
            }

            AddLeader(leadComponent);
        }

        private void AddLeader(SAINComponentClass leaderComponent)
        {
            IAmLeader = leaderComponent.ProfileId == BotOwner.ProfileId;
            LeaderComponent = leaderComponent;
            Leader = leaderComponent.BotOwner;

            if (IAmLeader && SquadID == "None")
            {
                SquadID = Guid.NewGuid().ToString("N");
            }

            NewLeaderFound?.Invoke(leaderComponent, Time.time);
            leaderComponent.BotOwner.GetPlayer.OnPlayerDead += LeaderWasKilled;

            if (SAINPlugin.DebugMode)
            {
                Logger.LogInfo(
                    $" Found New Leader. Name [{LeaderComponent?.name}]" +
                    $" for Squad: [{SquadID}]" +
                    $" at Time: [{Time.time}]" +
                    $" Group Size: [{LastSquadCount}]"
                    );
            }
        }

        public float SquadPowerLevel { get; private set; }

        public bool IAmLeader { get; private set; } = false;

        public BotOwner Leader { get; private set; }

        public SAINComponentClass LeaderComponent { get; private set; }

        public bool BotInGroup => BotOwner.BotsGroup.MembersCount > 1;

        public readonly Dictionary<BotOwner, SAINComponentClass> SquadMembers = new Dictionary<BotOwner, SAINComponentClass>();
        public readonly List<SoloDecision> SquadSoloDecisions = new List<SoloDecision>();
        public readonly List<SquadDecision> SquadDecisions = new List<SquadDecision>();

        private void ClearSquadCache()
        {
            SquadMembers.Clear();
            SquadLocations.Clear();
            SquadSoloDecisions.Clear();
            SquadDecisions.Clear();
        }

        private void UpdateMembers()
        {
            ClearSquadCache();

            if (BotInGroup)
            {
                var group = BotOwner.BotsGroup;
                int count = group.MembersCount;

                for (int i = 0; i < count; i++)
                {
                    var member = group.Member(i);
                    if (member?.HealthController?.IsAlive == true 
                        && SAINPlugin.BotController.GetBot(member.ProfileId, out var component))
                    {
                        SquadMembers.Add(member, component);
                        SquadSoloDecisions.Add(component.Memory.Decisions.Main.Current);
                        SquadDecisions.Add(component.Decision.CurrentSquadDecision);
                        SquadLocations.Add(member.Position);
                    }
                }
            }

            MemberIsFallingBack = SquadSoloDecisions.Contains(SoloDecision.Retreat) || SquadSoloDecisions.Contains(SoloDecision.RunToCover) || SquadSoloDecisions.Contains(SoloDecision.RunAway);

            SquadPowerLevel = BotOwner.BotsGroup.GroupPower;
        }

        public bool MemberIsFallingBack { get; private set; }

        public readonly List<Vector3> SquadLocations = new List<Vector3>();
    }
}