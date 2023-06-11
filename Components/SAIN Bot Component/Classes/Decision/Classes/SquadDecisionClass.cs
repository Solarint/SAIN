using BepInEx.Logging;
using EFT;
using SAIN.Components;
using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Classes
{
    public class SquadDecisionClass : SAINBot
    {
        public SquadDecisionClass(BotOwner bot) : base(bot) { }

        protected ManualLogSource Logger => SAIN.Decision.Logger;

        public bool GetDecision(out SAINSquadDecision Decision)
        {
            Decision = SAINSquadDecision.None;
            if (!SAIN.BotSquad.BotInGroup || SAIN.BotSquad.SquadMembers == null)
            {
                return false;
            }
            if (SAIN.Enemy?.IsVisible == true)
            {
                return false;
            }

            if (!EnemyDecision(out Decision))
            {
                if (StartRegroup())
                {
                    Decision = SAINSquadDecision.Regroup;
                }
            }

            return Decision != SAINSquadDecision.None;
        }

        private bool EnemyDecision(out SAINSquadDecision Decision)
        {
            Decision = SAINSquadDecision.None;
            foreach (var member in SAIN.BotSquad.SquadMembers.Values)
            {
                if (member == null || member.BotOwner == BotOwner || member.BotOwner.IsDead)
                {
                    continue;
                }
                if (SAIN.HasEnemy && member.HasEnemy)
                {
                    if (SAIN.Enemy.Person == member.Enemy.Person)
                    {
                        if (StartHelp(member))
                        {
                            Decision = SAINSquadDecision.Help;
                            return true;
                        }
                        else if (StartSuppression(member))
                        {
                            Decision = SAINSquadDecision.Suppress;
                            return true;
                        }
                        else if (StartGroupSearch(member))
                        {
                            Decision = SAINSquadDecision.Search;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool StartSuppression(SAINComponent member)
        {
            bool memberRetreat = member.CurrentDecision == SAINSoloDecision.Retreat;
            float memberDistance = (member.Position - BotOwner.Position).magnitude;
            float ammo = SAIN.Decision.SelfActionDecisions.AmmoRatio;
            if (memberRetreat && memberDistance < 30f && ammo > 0.5f)
            {
                return true;
            }
            if (SquadDecision == SAINSquadDecision.Suppress && !EndSuppresion(memberDistance, memberRetreat, ammo))
            {
                return true;
            }
            return false;
        }

        private bool EndSuppresion(float memberDistance, bool memberRetreat, float ammoRatio)
        {
            if (!memberRetreat || memberDistance >= 50f || ammoRatio <= 0.1f)
            {
                return true;
            }
            return false;
        }

        private bool StartGroupSearch(SAINComponent member)
        {
            bool squadSearching = member.CurrentDecision == SAINSoloDecision.Search || member.Decision.SquadDecision == SAINSquadDecision.Search;
            if (squadSearching)
            {
                return true;
            }
            return false;
        }

        private bool StartHelp(SAINComponent member)
        {
            float distance = member.Enemy.PathDistance;
            bool visible = member.Enemy.IsVisible;
            if (distance < 15f && visible)
            {
                return true;
            }
            if (SquadDecision == SAINSquadDecision.Help && !EndHelp(member, distance))
            {
                return true;
            }
            return false;
        }

        private bool EndHelp(SAINComponent member, float distance)
        {
            if (distance > 25 || member.Enemy.TimeSinceSeen > 5f)
            {
                return true;
            }
            return false;
        }

        public bool StartRegroup()
        {
            var squad = SAIN.BotSquad;
            if (squad.IsSquadLead)
            {
                return false;
            }

            var enemy = SAIN.Enemy;
            if (enemy != null)
            {
                if (enemy.IsVisible || (enemy.Seen && enemy.TimeSinceSeen < 20f))
                {
                    return false;
                }
            }

            var lead = squad.LeaderComponent;
            if (lead != null)
            {
                Vector3 BotPos = BotOwner.Position;
                Vector3 leadPos = lead.Position;
                Vector3 directionToLead = leadPos - BotPos;
                float leadDistance = directionToLead.magnitude;
                if (enemy != null)
                {
                    Vector3 EnemyPos = enemy.Position;
                    Vector3 directionToEnemy = EnemyPos - BotPos;
                    float EnemyDistance = directionToEnemy.magnitude;
                    if (EnemyDistance < leadDistance)
                    {
                        if (EnemyDistance < 30f && Vector3.Dot(directionToEnemy.normalized, directionToLead.normalized) > 0.25f)
                        {
                            return false;
                        }
                    }
                }
                if (SquadDecision == SAINSquadDecision.Regroup)
                {
                    return leadDistance > 10f;
                }
                else
                {
                    return leadDistance > 30f;
                }
            }
            return false;
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

                GetSquadDecision();
            }
        }

        private float UpdateTimer = 0f;
        private float UpdateVisibleTimer = 0f;

        private SAINSquadDecision GetSquadDecision()
        {
            SAINSquadDecision decision = SAINSquadDecision.None;

            if (CloseMembers < FarMembers)
            {
                decision = SAINSquadDecision.Regroup;
            }
            else if (InjuredMembers > HealthyMembers)
            {
                decision = SAINSquadDecision.Retreat;
            }
            else if (Enemies.Count < Members.Length)
            {
                decision = SAINSquadDecision.Surround;
            }
            else if (TooCloseMembers > 2)
            {
                decision = SAINSquadDecision.SpreadOut;
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
            var decisions = new List<SAINSoloDecision>();
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

        private int TooCloseMembers;
        private int CloseMembers;
        private int FarMembers;
        private int InjuredMembers;
        private int HealthyMembers;

        public SquadMember[] Members { get; private set; }
        public Vector3[] SquadPositions { get; private set; }
        public Vector3[] EnemyPositions { get; private set; }
        public SAINSoloDecision[] GroupDecisions { get; private set; }
        public IAIDetails[] VisibleEnemies { get; private set; }
        public Dictionary<IAIDetails, BotSettingsClass> Enemies => BotOwner.BotsGroup.Enemies;
        public ETagStatus[] SquadHealthStatus { get; private set; }
    }

    public class SquadMember : SAINBot
    {
        public SquadMember(BotOwner bot) : base(bot)
        {
        }

        public Vector3 Position => BotOwner.Position;
        public ETagStatus HealthStatus => SAIN.HealthStatus;
        public bool AmmoStatus => BotOwner.WeaponManager.IsReady && BotOwner.WeaponManager.HaveBullets;

        public bool HasEnemy => SAIN.HasGoalEnemy;
        public bool HasGoalTarget => SAIN.HasGoalTarget;
        public Vector3? EnemyPosition => SAIN.GoalEnemyPos;
        public Vector3? GoalTargetPosition => SAIN.GoalTargetPos;

        public bool IsVisible { get; set; }
    }
}