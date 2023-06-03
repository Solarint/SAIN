using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using EFT;
using SAIN.Components;
using UnityEngine.AI;
using BepInEx.Logging;
using Newtonsoft.Json;

namespace SAIN.Components
{
    public class SquadDecisionComponent : MonoBehaviour
    {
        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }


        private void Update()
        {
            if (!SAIN.BotActive || SAIN.GameIsEnding)
            {
                return;
            }

            if (!SAIN.BotSquad.BotInGroup)
            {
                return;
            }

            if (UpdateTimer < Time.time)
            {
                UpdateTimer = Time.time + 0.25f;

                UpdateMembers();

                if (UpdateVisibleTimer < Time.time)
                {
                    UpdateVisibleTimer = Time.time + 1f;

                    UpdateVisibleMembers();
                }

                UpdateVisibleEnemies();

                GetDecision();
            }
        }

        private float UpdateTimer = 0f;
        private float UpdateVisibleTimer = 0f;

        private SAINSquadDecisions GetDecision()
        {
            SAINSquadDecisions decision = SAINSquadDecisions.None;

            if (CloseMembers < FarMembers)
            {
                decision = SAINSquadDecisions.Regroup;
            }
            else if (InjuredMembers > HealthyMembers)
            {
                decision = SAINSquadDecisions.Retreat;
            }
            else if (Enemies.Count < Members.Length)
            {
                decision = SAINSquadDecisions.Surround;
            }
            else if (TooCloseMembers > 2)
            {
                decision = SAINSquadDecisions.SpreadOut;
            }

            return decision;
        }

        private void UpdateVisibleMembers()
        {
            foreach (var member in Members)
            {
                if (member != null)
                {
                    var direction = member.SAIN.HeadPosition - SAIN.HeadPosition;
                    member.IsVisible = !Physics.Raycast(SAIN.HeadPosition, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask);
                }
            }
        }

        private void UpdateMembers()
        {
            int i = 0;
            int max = BotOwner.BotsGroup.MembersCount - 1;

            var list = new List<SquadMember>();

            while (i < max)
            {
                var member = BotOwner.BotsGroup.Member(i); 

                if (member != null && !member.IsDead)
                {
                    list.Add(new SquadMember(member));
                }
            }

            Members = list.ToArray();

            UpdateMemberArrays();
        }

        private void UpdateMemberArrays()
        {
            var decisions = new List<SAINLogicDecision>();
            var positions = new List<Vector3>();
            var statuses = new List<ETagStatus>();

            int tooclose = 0;
            int closeMembers = 0;
            int farMembers = 0;

            int injuredMembers = 0;
            int healthyMembers = 0;

            foreach (var member in Members)
            {
                if (member != null)
                {
                    decisions.Add(member.CurrentDecision);
                    positions.Add(member.Position);
                    statuses.Add(member.HealthStatus);

                    if (member.HealthStatus == ETagStatus.Healthy)
                    {
                        healthyMembers++;
                    }
                    else
                    {
                        injuredMembers++;
                    }

                    float distance = Vector3.Distance(member.Position, BotOwner.Position);

                    if (distance < 3f)
                    {
                        tooclose++;
                    }

                    if (distance < 20f)
                    {
                        closeMembers++;
                    }
                    else
                    {
                        farMembers++;
                    }
                }
            }

            GroupDecisions = decisions.ToArray();
            SquadPositions = positions.ToArray();
            SquadHealthStatus = statuses.ToArray();

            TooCloseMembers = tooclose;
            CloseMembers = closeMembers;
            FarMembers = farMembers;

            InjuredMembers = injuredMembers;
            HealthyMembers = healthyMembers;
        }

        private void UpdateVisibleEnemies()
        {
            var list = new List<IAIDetails>();
            var position = new List<Vector3>();

            foreach (var member in Members)
            {
                if (member != null && member.SAIN.HasEnemyAndCanShoot)
                {
                    list.Add(member.BotOwner.Memory.GoalEnemy.Person);
                    position.Add(member.EnemyPosition.Value);
                }
            }

            VisibleEnemies = list.ToArray();
            EnemyPositions = position.ToArray();
        }

        public SAINSquadDecisions CurrentDecision { get; private set; }
        public SAINSquadDecisions LastDecision { get; private set; }

        private int TooCloseMembers;
        private int CloseMembers;
        private int FarMembers;
        private int InjuredMembers;
        private int HealthyMembers;

        public SquadMember[] Members { get; private set; }
        public Vector3[] SquadPositions { get; private set; }
        public Vector3[] EnemyPositions { get; private set; }
        public SAINLogicDecision[] GroupDecisions { get; private set; }
        public IAIDetails[] VisibleEnemies { get; private set; }
        public Dictionary<IAIDetails, BotSettingsClass> Enemies => BotOwner.BotsGroup.Enemies;
        public ETagStatus[] SquadHealthStatus { get; private set; }


        private SAINComponent SAIN;
        private BotOwner BotOwner => SAIN.BotOwner;

        private ManualLogSource Logger;
    }

    public class SquadMember : SAINBot
    {
        public SquadMember(BotOwner bot) : base(bot) { }


        public Vector3 Position => BotOwner.Position;
        public ETagStatus HealthStatus => SAIN.BotStatus.HealthStatus;
        public SAINLogicDecision CurrentDecision => SAIN.CurrentDecision;
        public SAINLogicDecision LastDecision => SAIN.Decisions.LastDecision;
        public bool AmmoStatus => BotOwner.WeaponManager.IsReady && BotOwner.WeaponManager.HaveBullets;

        public bool HasEnemy => SAIN.HasGoalEnemy;
        public bool HasGoalTarget => SAIN.HasGoalTarget;
        public Vector3? EnemyPosition => SAIN.GoalEnemyPos;
        public Vector3? GoalTargetPosition => SAIN.GoalTargetPos;

        public bool IsVisible { get; set; }
    }
}
