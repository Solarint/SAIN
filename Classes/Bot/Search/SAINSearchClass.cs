using EFT;
using SAIN.Components;
using SAIN.Models.Enums;
using SAIN.Models.Structs;
using SAIN.Preset.Personalities;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace SAIN.SAINComponent.Classes.Search;

public class SAINSearchClass : BotComponentClassBase
{
    public bool SearchActive { get; private set; }
    public Enemy SearchTarget { get; private set; }

    public ESearchMove NextState { get; private set; }
    public ESearchMove CurrentState { get; private set; }
    public ESearchMove LastState { get; private set; }

    public BotPeekPlan? PeekPoints => PathFinder.PeekPoints;

    public SearchDeciderClass SearchDecider { get; private set; }
    public SearchPathFinder PathFinder { get; private set; }

    public SAINSearchClass(BotComponent sain) : base(sain)
    {
        CanEverTick = false;
        SearchDecider = new SearchDeciderClass(this);
        PathFinder = new SearchPathFinder(this);
    }

    public void ToggleSearch(bool value, Enemy target)
    {
        float currentTime = Time.time;
        if (value)
        {
            if (SearchTarget != null)
            {
                if (SearchTarget == target)
                {
                    SearchActive = true;
                    return;
                }
                SearchTarget.Events.OnSearch.CheckToggle(false, currentTime);
                SearchTarget = null;
            }
            if (target != null)
            {
                target.Events.OnSearch.CheckToggle(true, currentTime);
                SearchTarget = target;
            }
            SearchActive = true;
            return;
        }
        if (SearchTarget != null)
        {
            SearchTarget.Events.OnSearch.CheckToggle(false, currentTime);
            SearchTarget = null;
        }
        SearchActive = false;
        Reset();
    }

    public void Search(bool shallSprint, Enemy enemy)
    {
        PathFinder.UpdateSearchDestination(enemy);
        SwitchSearchModes(shallSprint, enemy);
    }

    private bool WaitAtPoint()
    {
        if (_waitAtPointTimer < 0)
        {
            float baseTime = 3;
            baseTime *= Bot.Info.PersonalitySettings.Search.SearchWaitMultiplier;
            float waitTime = baseTime * Random.Range(0.25f, 1.25f);
            _waitAtPointTimer = Time.time + waitTime;
            Bot.Mover.ActivePath?.Pause(waitTime);
        }
        if (_waitAtPointTimer < Time.time)
        {
            Bot.Mover.ActivePath?.UnPause();
            _waitAtPointTimer = -1;
            return false;
        }
        return true;
    }

    private bool MoveToEnemy(Enemy enemy, bool shallSprint)
    {
        if (Time.time - _timeLastMoved < 1f) return true;

        if (shallSprint &&
            Bot.Mover.RunToPointByWay(enemy.Path.PathToEnemy, false, 1f, Mover.ESprintUrgency.Middle, true))
        {
            _timeLastMoved = Time.time;
            return true;
        }
        if (Bot.Mover.WalkToPointByWay(enemy.Path.PathToEnemy, false, 1f))
        {
            _timeLastMoved = Time.time;
            return true;
        }
        return false;
    }

    private float _timeLastMoved;

    private void HandleLight(bool stealthy)
    {
        if (_Running)
        {
            return;
        }
        if (stealthy || _searchSettings.Sneaky)
        {
            Bot.BotLight.ToggleLight(false);
            return;
        }
        if (Bot.Mover.Moving)
        {
            Bot.BotLight.HandleLightForSearch(Bot.Mover.ActivePath.CurrentCornerDistanceSqr);
        }
    }

    private void SwitchSearchModes(bool shallSprint, Enemy enemy)
    {
        EnemyPlace targetPlace = PathFinder.TargetPlace;
        if (targetPlace == null)
        {
            //Logger.LogWarning($"{BotOwner.name}'s targetPlace is null, cannot search!");
            return;
        }

        if (CheckEndPeek())
        {
            LastState = CurrentState;
            CurrentState = ESearchMove.None;
        }

        bool shallBeStealthy = SearchDecider.ShallBeStealthyDuringSearch(enemy);
        GetSpeedandPose(out float speed, out float pose, shallSprint, shallBeStealthy);
        HandleLight(shallBeStealthy);

        if (CheckShallWaitandReload())
        {
            shallSprint = false;
        }
        if (shallSprint && 
            Bot.Mover.RunToPointByWay(enemy.Path.PathToEnemy, true, -1, Mover.ESprintUrgency.Middle, true))
        {
            LastState = CurrentState;
            CurrentState = ESearchMove.DirectMove;
            SetSpeedPose(1f, 1f);
            return;
        }

        // if a bot is looking towards something they heard or got shot, pause their search movement temporarily
        //switch (Bot.Steering.CurrentSteerPriority)
        //{
        //    case ESteerPriority.LastHit:
        //    case ESteerPriority.HeardThreat:
        //        Bot.Mover.PathFollower.Pause(0.33f);
        //        break;
        //
        //    default:
        //        if (CurrentState != ESearchMove.Wait)
        //            Bot.Mover.PathFollower.Unpause();
        //        break;
        //}

        ESearchMove previousState = CurrentState;
        switch (CurrentState)
        {
            case ESearchMove.None:
                if (MoveToEnemy(enemy, shallSprint))
                {
                    CurrentState = ESearchMove.DirectMove;
                    break;
                }
#if DEBUG
                Logger.LogWarning($"{BotOwner.name}'s cannot peek and cannot direct move!");
#endif
                break;

            case ESearchMove.DirectMove:

                SetSpeedPose(speed, pose);
                MoveToEnemy(enemy, shallSprint);
                break;

            case ESearchMove.Advance:

                if (_advanceTime < 0)
                {
                    _advanceTime = Time.time + 5f;
                }
                if (_advanceTime < Time.time)
                {
                    _advanceTime = -1f;
                    CurrentState = ESearchMove.None;
                    PathFinder.FinishedPeeking = true;
                    break;
                }

                SetSpeedPose(speed, pose);
                MoveToEnemy(enemy, shallSprint);
                break;

            case ESearchMove.Wait:
                if (WaitAtPoint())
                {
                    //Bot.Mover.SetTargetMoveSpeed(0f);
                    Bot.Mover.SetTargetPose(0.75f);
                    break;
                }
                Bot.Mover.SetTargetMoveSpeed(speed);
                Bot.Mover.SetTargetPose(pose);
                CurrentState = NextState;
                break;
        }

        if (previousState != CurrentState)
        {
            LastState = previousState;
        }
    }

    private bool CheckEndPeek()
    {
        switch (CurrentState)
        {
            case ESearchMove.None:
            case ESearchMove.DirectMove:
            case ESearchMove.Wait:
            case ESearchMove.Advance:
                return false;

            default:
                if (PeekPoints == null)
                {
                    return true;
                }
                if (PathFinder.FinishedPeeking)
                {
                    return true;
                }
                return false;
        }
    }

    private void GetSpeedandPose(out float speed, out float pose, bool sprinting, bool stealthy)
    {
        speed = 1f;
        pose = 1f;
        // are we sprinting?
        if (sprinting || Player.IsSprintEnabled || _Running || Bot.Mover.Running)
        {
            return;
        }
        // are we indoors?
        if (GetIndoorsSpeedPose(stealthy, out speed, out pose))
        {
            return;
        }
        // we are outside...
        if (_searchSettings.Sneaky &&
            Bot.Cover.CoverPoints.Count > 2 &&
            Time.time - BotOwner.Memory.UnderFireTime > 30f)
        {
            speed = 0.25f;
            pose = 0.6f;
            return;
        }
        if (stealthy)
        {
            speed = 0.5f;
            pose = 0.7f;
            return;
        }
    }

    private bool GetIndoorsSpeedPose(bool stealthy, out float speed, out float pose)
    {
        speed = 1f;
        pose = 1f;
        if (!Bot.Memory.Location.IsIndoors)
        {
            return false;
        }
        var searchSettings = _searchSettings;
        if (searchSettings.Sneaky)
        {
            speed = searchSettings.SneakySpeed;
            pose = searchSettings.SneakyPose;
        }
        else if (stealthy)
        {
            speed = 0.33f;
            pose = 1f;
        }
        return true;
    }

    private bool CheckShallWaitandReload()
    {
        if (BotOwner.WeaponManager?.Reload?.Reloading == true &&
            CurrentState != ESearchMove.Wait)
        {
            NextState = CurrentState;
            CurrentState = ESearchMove.Wait;
            return true;
        }
        return false;
    }

    private bool ShallStartPeek(bool shallSprint)
    {
        return false;
    }

    private void SetSpeedPose(float speed, float pose)
    {
        Bot.Mover.SetTargetMoveSpeed(speed);
        Bot.Mover.SetTargetPose(pose);
    }

    public void Reset()
    {
        ResetStates();
        PathFinder.Reset();
    }

    public void ResetStates()
    {
        CurrentState = ESearchMove.None;
        LastState = ESearchMove.None;
        NextState = ESearchMove.None;
    }

    public bool BotIsAtPoint(Vector3 point, float reachDist = 0.5f)
    {
        return DistanceToDestination(point) < reachDist;
    }

    public float DistanceToDestination(Vector3 point)
    {
        return (point - Bot.Position).magnitude;
    }

    private bool _Running => Bot.Mover.Running;
    private float _waitAtPointTimer = -1;
    private float _advanceTime;
    private PersonalitySearchSettings _searchSettings => Bot.Info.PersonalitySettings.Search;
}