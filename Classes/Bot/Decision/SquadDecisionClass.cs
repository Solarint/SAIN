using EFT;
using SAIN.Components;
using SAIN.Models.Enums;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.Info;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Decision;

public class SquadDecisionClass : BotBase
{
    public SquadDecisionClass(BotComponent sain) : base(sain)
    {
        CanEverTick = false;
    }

    private BotSquadContainer Squad => Bot.Squad;

    public bool GetDecision(out ESquadDecision Decision, Enemy enemy)
    {
        Decision = ESquadDecision.None;
        if (!Squad.BotInGroup || Bot.Squad.SquadInfo?.LeaderComponent == null || Squad.LeaderComponent?.IsDead == true)
        {
            return false;
        }

        if (EnemyDecision(out Decision, enemy))
        {
            return true;
        }

        //if (shallRegroup())
        //{
        //    Decision = SquadDecision.Regroup;
        //    return true;
        //}

        return false;
    }

    float SquaDecision_RadioCom_MaxDistSq = 1200f;
    float SquadDecision_MyEnemySeenRecentTime = 10f;

    private bool EnemyDecision(out ESquadDecision Decision, Enemy enemy)
    {
        Decision = ESquadDecision.None;
        Enemy myEnemy = Bot.GoalEnemy;

        if (shallPushSuppressedEnemy(myEnemy))
        {
            Decision = ESquadDecision.PushSuppressedEnemy;
            return true;
        }
        if (myEnemy != null)
        {
            if (myEnemy.IsVisible || myEnemy.TimeSinceSeen < SquadDecision_MyEnemySeenRecentTime)
            {
                return false;
            }
        }
        if (Bot.Squad.LeaderComponent != null &&
            shallGroupSearch())
        {
            Decision = ESquadDecision.GroupSearch;
            return true;
        }

        foreach (var member in Bot.Squad.Members.Values)
        {
            if (member == null || member.BotOwner == BotOwner || member.BotOwner.IsDead)
            {
                continue;
            }
            if (!HasRadioComms && (Bot.Transform.Position - member.Transform.Position).sqrMagnitude > SquaDecision_RadioCom_MaxDistSq)
            {
                continue;
            }
            if (myEnemy != null
                && member.HasEnemy)
            {
                if (myEnemy.EnemyPlayer == member.GoalEnemy.EnemyPlayer)
                {
                    if (shallSuppressEnemy(member))
                    {
                        Decision = ESquadDecision.Suppress;
                        return true;
                    }
                    if (shallHelp(member))
                    {
                        Decision = ESquadDecision.Help;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static readonly float PushSuppressedEnemyMaxPathDistance = 75f;
    private static readonly float PushSuppressedEnemyMaxPathDistanceSprint = 100f;
    private static readonly float PushSuppressedEnemyLowAmmoRatio = 0.5f;

    private bool shallPushSuppressedEnemy(Enemy enemy)
    {
        if (enemy != null
            && !Bot.Decision.SelfActionDecisions.LowOnAmmo(PushSuppressedEnemyLowAmmoRatio)
            && Bot.Info.PersonalitySettings.Rush.CanRushEnemyReloadHeal)
        {
            bool inRange = false;
            float modifier = enemy.Status.VulnerableAction == EEnemyAction.UsingSurgery ? 1.25f : 1f;
            if (enemy.Path.PathLength < PushSuppressedEnemyMaxPathDistanceSprint * modifier
                && BotOwner?.CanSprintPlayer == true)
            {
                inRange = true;
            }
            else if (enemy.Path.PathLength < PushSuppressedEnemyMaxPathDistance * modifier)
            {
                inRange = true;
            }

            if (inRange
                && (Bot.Memory.Health.HealthStatus == ETagStatus.Healthy || Bot.Memory.Health.HealthStatus == ETagStatus.Injured)
                && Bot.Squad.SquadInfo.SquadIsSuppressEnemy(enemy.EnemyPlayer.ProfileId, out var suppressingMember)
                && suppressingMember != Bot)
            {
                var enemyStatus = enemy.Status;
                if (enemy.Status.VulnerableAction != EEnemyAction.None)
                {
                    return true;
                }
                ETagStatus enemyHealth = enemy.EnemyPlayer.HealthStatus;
                if (enemyHealth == ETagStatus.Dying || enemyHealth == ETagStatus.BadlyInjured)
                {
                    return true;
                }
                else if (enemy.EnemyPlayer.IsInPronePose)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool HasRadioComms => Bot.PlayerComponent.Equipment.GearInfo.HasEarPiece;

    float SquadDecision_SuppressFriendlyDistStart = 30f;
    float SquadDecision_SuppressFriendlyDistEnd = 50f;

    private bool shallSuppressEnemy(BotComponent member)
    {
        if (Bot.GoalEnemy?.SuppressionTarget == null)
        {
            return false;
        }
        if (Bot.GoalEnemy?.IsVisible == true)
        {
            return false;
        }
        if (member.Decision.CurrentCombatDecision != ECombatDecision.Retreat)
        {
            return false;
        }

        float memberDistance = (member.Transform.Position - BotOwner.Position).magnitude;
        float ammo = Bot.Decision.SelfActionDecisions.AmmoRatio;

        if (Bot.Decision.CurrentSquadDecision == ESquadDecision.Suppress)
        {
            return memberDistance <= SquadDecision_SuppressFriendlyDistEnd && ammo >= 0.1f;
        }
        return memberDistance <= SquadDecision_SuppressFriendlyDistStart && ammo >= 0.5f;
    }

    private bool shallGroupSearch(BotComponent member)
    {
        bool squadSearching = member.Decision.CurrentCombatDecision == ECombatDecision.Search || member.Decision.CurrentSquadDecision == ESquadDecision.Search;
        if (squadSearching)
        {
            return true;
        }
        return false;
    }

    private bool shallGroupSearch()
    {
        if (Bot.Info.Profile.IsBoss &&
            Bot.Info.Profile.WildSpawnType != WildSpawnType.bossKnight)
        {
            //return false;
        }

        foreach (var member in Bot.Squad.Members.Values)
        {
            if (member.Decision.CurrentCombatDecision == ECombatDecision.Search && 
                Bot.GoalEnemy != null && 
                doesMemberShareEnemy(member))
            {
                return true;
            }
        }
        return false;
    }

    private bool doesMemberShareEnemy(BotComponent member)
    {
        if (member == null || member.ProfileId == Bot.ProfileId || member.BotOwner?.IsDead == true)
        {
            return false;
        }

        return member.GoalEnemy != null
            && member.GoalEnemy.EnemyPlayer.ProfileId == Bot.GoalEnemy.EnemyPlayer.ProfileId;
    }

    float SquadDecision_StartHelpFriendDist = 30f;
    float SquadDecision_EndHelpFriendDist = 45f;
    float SquadDecision_EndHelp_FriendsEnemySeenRecentTime = 8f;

    private bool shallHelp(BotComponent member)
    {
        float distance = member.GoalEnemy.Path.PathLength;
        bool visible = member.GoalEnemy.IsVisible;

        if (Bot.Decision.CurrentSquadDecision == ESquadDecision.Help
            && member.GoalEnemy.Seen)
        {
            return distance < SquadDecision_EndHelpFriendDist
                && member.GoalEnemy.TimeSinceSeen < SquadDecision_EndHelp_FriendsEnemySeenRecentTime;
        }
        return distance < SquadDecision_StartHelpFriendDist && visible;
    }

    float SquadDecision_Regroup_NoEnemy_StartDist = 125f;
    float SquadDecision_Regroup_NoEnemy_EndDistance = 50f;
    float SquadDecision_Regroup_Enemy_StartDist = 50f;
    float SquadDecision_Regroup_Enemy_EndDistance = 15f;
    float SquadDecision_Regroup_EnemySeenRecentTime = 60f;

    public bool shallRegroup()
    {
        var squad = Bot.Squad;
        if (squad.IAmLeader)
        {
            return false;
        }

        float maxDist = SquadDecision_Regroup_NoEnemy_StartDist;
        float minDist = SquadDecision_Regroup_NoEnemy_EndDistance;

        var enemy = Bot.GoalEnemy;
        if (enemy != null)
        {
            if (enemy.IsVisible || (enemy.Seen && enemy.TimeSinceSeen < SquadDecision_Regroup_EnemySeenRecentTime))
            {
                return false;
            }
            maxDist = SquadDecision_Regroup_Enemy_StartDist;
            minDist = SquadDecision_Regroup_Enemy_EndDistance;
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
            if (Bot.Decision.CurrentSquadDecision == ESquadDecision.Regroup)
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