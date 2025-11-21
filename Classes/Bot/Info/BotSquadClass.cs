using EFT;
using SAIN.BotController.Classes;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Info;

public class BotSquadContainer : BotComponentClassBase
{
    public float HUMAN_FRIEND_CLOSE_DISTANCE = 50f;
    public float HUMAN_FRIEND_CLOSE_DISTANCE_INTERVAL = 1f;
    public float CHECK_VISIBLE_MEMBERS_INTERVAL = 1f;
    public float CHECK_VISIBLE_MEMBERS_DISTANCE = 75f;


    public BotSquadContainer(BotComponent bot) : base(bot)
    {
        TickRequirement = ESAINTickState.OnlyNoSleep;
        getSquad();
    }

    public void RemoveFromSquad()
    {
        SquadInfo = null;
        getSquad();
    }

    private void getSquad()
    {
        SquadInfo = BotManagerComponent.Instance.BotSquads.GetSquad(Bot.BotOwner);
    }

    public Squad SquadInfo { get; private set; }

    public float DistanceToSquadLeader { get; private set; }

    public readonly List<BotComponent> VisibleMembers = new();

    private float _updateMemberTime = 0f;

    public bool IAmLeader => SquadInfo.LeaderId == Bot.ProfileId;

    public BotComponent LeaderComponent => SquadInfo?.LeaderComponent;

    public bool BotInGroup => BotOwner.BotsGroup.MembersCount > 1 || HumanFriendClose;

    public Dictionary<string, BotComponent> Members => SquadInfo?.Members;

    public bool HumanFriendClose
    {
        get
        {
            if (_nextCheckhumantime < Time.time)
            {
                _nextCheckhumantime = Time.time + HUMAN_FRIEND_CLOSE_DISTANCE_INTERVAL;
                _humanFriendclose = humanFriendClose(HUMAN_FRIEND_CLOSE_DISTANCE);
            }
            return _humanFriendclose;
        }
    }


    private bool _humanFriendclose;

    private float _nextCheckhumantime;

    private bool humanFriendClose(float distToCheck)
    {
        foreach (var playerComponent in GameWorldComponent.Instance.PlayerTracker.AlivePlayersDictionary.Values)
        {
            if (playerComponent != null &&
                !playerComponent.IsAI &&
                Bot?.EnemyController?.IsPlayerAnEnemy(playerComponent.ProfileId) == false &&
                playerComponent.GetDistanceToPlayer(Bot.ProfileId) < distToCheck)
            {
                return true;
            }
        }
        return false;
    }

    public override void ManualUpdate()
    {
        if (BotOwner.BotsGroup.MembersCount > 1 &&
            SquadInfo != null &&
            _updateMemberTime < Time.time)
        {
            _updateMemberTime = Time.time + CHECK_VISIBLE_MEMBERS_INTERVAL;

            checkVisibleMembers();

            if (!IAmLeader && LeaderComponent != null)
            {
                DistanceToSquadLeader = (Bot.Position - LeaderComponent.Position).magnitude;
            }
        }
        base.ManualUpdate();
    }

    private void checkVisibleMembers()
    {
        VisibleMembers.Clear();
        Vector3 eyePos = Bot.Transform.EyePosition;
        foreach (var member in Members.Values)
        {
            if (member != null && member.GetDistanceToPlayer(Bot.ProfileId) <= CHECK_VISIBLE_MEMBERS_DISTANCE)
            {
                Vector3 direction = member.Transform.BodyPosition - eyePos;
                if (!Physics.Raycast(eyePos, direction.normalized, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                {
                    VisibleMembers.Add(member);
                }
            }
        }
    }
}