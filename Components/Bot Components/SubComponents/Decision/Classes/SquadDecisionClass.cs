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
        public SquadDecisionClass(SAINComponent bot) : base(bot) { }

        protected ManualLogSource Logger => SAIN.Decision.Logger;
        private SquadClass Squad => SAIN.Squad;

        public bool GetDecision(out SAINSquadDecision Decision)
        {
            Decision = SAINSquadDecision.None;
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
                Decision = SAINSquadDecision.Regroup;
                return true;
            }

            return false;
        }

        private bool EnemyDecision(out SAINSquadDecision Decision)
        {
            Decision = SAINSquadDecision.None;
            foreach (var member in SAIN.Squad.SquadMembers.Values)
            {
                if (member == null || member.BotOwner == BotOwner || member.BotOwner.IsDead)
                {
                    continue;
                }
                if (!HasRadioComms && (SAIN.Position - member.Position).sqrMagnitude > 1200f)
                {
                    continue;
                }
                var myEnemy = SAIN.Enemy;
                if (myEnemy != null && member.HasEnemy)
                {
                    if (myEnemy.Person == member.Enemy.Person)
                    {
                        if (StartSuppression(member))
                        {
                            Decision = SAINSquadDecision.Suppress;
                            return true;
                        }
                        if (myEnemy.IsVisible || myEnemy.TimeSinceSeen < 5f)
                        {
                            return false;
                        }
                        if (StartGroupSearch(member))
                        {
                            Decision = SAINSquadDecision.Search;
                            return true;
                        }
                        if (StartHelp(member))
                        {
                            Decision = SAINSquadDecision.Help;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool HasRadioComms => SAIN.Equipment.HasEarPiece;

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
            bool squadSearching = member.CurrentDecision == SAINSoloDecision.Search || member.Decision.CurrentSquadDecision == SAINSquadDecision.Search;
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
                Vector3 leadPos = lead.Position;
                Vector3 directionToLead = leadPos - BotPos;
                float leadDistance = directionToLead.magnitude;
                if (enemy != null)
                {
                    Vector3 EnemyPos = enemy.CurrPosition;
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