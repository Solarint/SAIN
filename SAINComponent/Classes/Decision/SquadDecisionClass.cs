using BepInEx.Logging;
using EFT;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.Info;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Decision
{
    public class SquadDecisionClass : SAINBase, ISAINClass
    {
        public SquadDecisionClass(SAINComponentClass sain) : base(sain)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        private SAINSquadClass Squad => SAIN.Squad;

        public bool GetDecision(out SquadDecision Decision)
        {
            Decision = SquadDecision.None;
            if (!Squad.BotInGroup || Squad.Leader?.IsDead == true)
            {
                return false;
            }
            if (SAIN.Enemy?.IsVisible == true || SAIN.Enemy?.TimeSinceSeen < 3f)
            {
                return false;
            }

            if (EnemyDecision(out Decision))
            {
                return true;
            }
            if (StartRegroup())
            {
                Decision = SquadDecision.Regroup;
                return true;
            }

            return false;
        }

        private bool EnemyDecision(out SquadDecision Decision)
        {
            Decision = SquadDecision.None;
            foreach (var member in SAIN.Squad.SquadMembers.Values)
            {
                if (member == null || member.BotOwner == BotOwner || member.BotOwner.IsDead)
                {
                    continue;
                }
                if (!HasRadioComms && (SAIN.Transform.Position - member.Transform.Position).sqrMagnitude > 1200f)
                {
                    continue;
                }
                var myEnemy = SAIN.Enemy;
                if (myEnemy != null && member.HasEnemy)
                {
                    if (myEnemy.EnemyIPlayer == member.Enemy.EnemyIPlayer)
                    {
                        if (StartSuppression(member))
                        {
                            Decision = SquadDecision.Suppress;
                            return true;
                        }
                        if (myEnemy.IsVisible || myEnemy.TimeSinceSeen < 5f)
                        {
                            return false;
                        }
                        if (StartGroupSearch(member))
                        {
                            Decision = SquadDecision.Search;
                            return true;
                        }
                        if (StartHelp(member))
                        {
                            Decision = SquadDecision.Help;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool HasRadioComms => SAIN.Equipment.HasEarPiece;

        private bool StartSuppression(SAINComponentClass member)
        {
            bool memberRetreat = member.Memory.Decisions.Main.Current == SoloDecision.Retreat;
            float memberDistance = (member.Transform.Position - BotOwner.Position).magnitude;
            float ammo = SAIN.Decision.SelfActionDecisions.AmmoRatio;
            if (memberRetreat && memberDistance < 30f && ammo > 0.5f)
            {
                return true;
            }
            if (SAIN.Memory.Decisions.Squad.Current == SquadDecision.Suppress && !EndSuppresion(memberDistance, memberRetreat, ammo))
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

        private bool StartGroupSearch(SAINComponentClass member)
        {
            bool squadSearching = member.Memory.Decisions.Main.Current == SoloDecision.Search || member.Decision.CurrentSquadDecision == SquadDecision.Search;
            if (squadSearching)
            {
                return true;
            }
            return false;
        }

        private bool StartHelp(SAINComponentClass member)
        {
            float distance = member.Enemy.PathDistance;
            bool visible = member.Enemy.IsVisible;
            if (distance < 15f && visible)
            {
                return true;
            }
            if (SAIN.Memory.Decisions.Squad.Current == SquadDecision.Help && !EndHelp(member, distance))
            {
                return true;
            }
            return false;
        }

        private bool EndHelp(SAINComponentClass member, float distance)
        {
            if (distance > 25 || member.Enemy.TimeSinceSeen > 5f)
            {
                return true;
            }
            return false;
        }

        public bool StartRegroup()
        {
            var squad = SAIN.Squad;
            if (squad.IAmLeader)
            {
                return false;
            }

            float maxDist = 125f;
            float minDist = 50f;

            var enemy = SAIN.Enemy;
            if (enemy != null)
            {
                if (enemy.IsVisible || (enemy.Seen && enemy.TimeSinceSeen < 60f))
                {
                    return false;
                }
                maxDist = 50f;
                minDist = 15f;
            }

            var lead = squad.LeaderComponent;
            if (lead != null)
            {
                Vector3 BotPos = BotOwner.Position;
                Vector3 leadPos = lead.Transform.Position;
                Vector3 directionToLead = leadPos - BotPos;
                float leadDistance = directionToLead.magnitude;
                if (enemy != null)
                {
                    Vector3 EnemyPos = enemy.EnemyPosition;
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
                if (SAIN.Memory.Decisions.Squad.Current == SquadDecision.Regroup)
                {
                    return leadDistance > minDist;
                }
                else
                {
                    return leadDistance > maxDist;
                }
            }
            return false;
        }
    }
}