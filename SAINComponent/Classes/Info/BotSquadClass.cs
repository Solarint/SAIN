using BepInEx.Logging;
using EFT;
using SAIN.BotController.Classes;
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

        public void Init()
        {
            SquadInfo = SAINPlugin.BotController.BotSquads.GetSquad(SAIN);
        }

        public Squad SquadInfo { get; private set; }

        public float DistanceToSquadLeader = 0f;

        public string SquadID => SquadInfo.Id;

        public readonly List<SAINComponentClass> VisibleMembers = new List<SAINComponentClass>();

        private float UpdateMembersTimer = 0f;

        public bool IAmLeader => SquadInfo.LeaderId == SAIN.ProfileId;

        public SAINComponentClass LeaderComponent => SquadInfo?.LeaderComponent;

        public bool BotInGroup => BotOwner.BotsGroup.MembersCount > 1;

        public Dictionary<string, SAINComponentClass> Members => SquadInfo?.Members;

        public List<SoloDecision> SquadSoloDecisions => SquadInfo?.SquadSoloDecisions;

        public List<SquadDecision> SquadDecisions => SquadInfo?.SquadDecisions;

        public List<Vector3> SquadLocations => SquadInfo?.SquadLocations;

        public bool MemberIsFallingBack => SquadInfo?.MemberIsFallingBack == true;

        public void Update()
        {
            if (BotInGroup && SquadInfo != null && UpdateMembersTimer < Time.time)
            {
                UpdateMembersTimer = Time.time + 0.5f;

                UpdateVisibleMembers();

                if (LeaderComponent != null)
                {
                    DistanceToSquadLeader = (SAIN.Position - LeaderComponent.Position).magnitude;
                }
            }
        }

        public void Dispose()
        {
        }

        private void UpdateVisibleMembers()
        {
            VisibleMembers.Clear();
            foreach (var member in Members.Values)
            {
                if (member != null && SAIN.Memory.VisiblePlayers.Contains(member.Player))
                {
                    VisibleMembers.Add(member);
                }
            }
        }
    }
}