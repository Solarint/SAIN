using EFT;
using SAIN.Classes.Transform;
using SAIN.Components;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes.Mover;

public class SAINMoverClass : BotComponentClassBase, IBotPathFinder
{
    public IBotPathData ActivePath {
        get
        {
            return _activePath;
        }
    }

    public bool Moving => _activePath != null && _activePath.Moving;
    public bool Running => _activePath != null && _activePath.Running;
    public bool Crawling => _activePath != null && _activePath.Crawling;

    public event Action<OperationResult, IBotPathData> OnPathComplete;

    public event Action<BotPathCorner, int, int> OnPathCornerSet;

    public event Action<BotPathCorner, int, int> OnPathCornerComplete;

    public SAINMoverClass(BotComponent bot) : base(bot)
    {
        TickRequirement = ESAINTickState.OnlyNoSleep;
        BlindFire = new BlindFireController(bot);
        Lean = new LeanClass(bot);
        Prone = new ProneClass(bot);
        Pose = new PoseClass(bot);
        DogFight = new DogFight(bot);
        _preparedPath1 = new BotPathDataManual(bot, this);
        _preparedPath2 = new BotPathDataManual(bot, this);
    }

    public override void ManualUpdate()
    {
        Pose.ManualUpdate();
        Lean.ManualUpdate();
        BlindFire.ManualUpdate();

        if (Bot.SAINLayersActive)
        {
#if DEBUG
            if (Bot.CurrentAction == null)
            {
                Logger.LogWarning($"[{Bot.name}] No current action set, cannot update movement and steering!.");
            }
#endif
            Bot.CurrentAction?.UpdateMovement();
            if (!CheckTickPath())
            {
                Bot.CurrentAction?.OnSteeringTicked();
                Bot.Steering.TickPlayerSteering();
            }
        }
        UpdateStance(Time.time);
        base.ManualUpdate();
    }

    private bool CheckTickPath()
    {
        if (_activePath == null)
        {
            return false;
        }
        if (_activePath.PathRecalcRequested && !_activePath.CancelRequested)
        {
            _activePath.PathRecalcRequested = false;
            if (!TriggerRecalcPath())
            {
#if DEBUG
                Logger.LogWarning("Failed to recalculate path, cancelling active path.");
#endif
                _activePath.Cancel();
                return false;
            }
        }
        _activePath.TickPath(Time.fixedDeltaTime, Time.time);
        return true;
    }

    public DogFight DogFight { get; private set; }

    public void PathComplete(OperationResult result, IBotPathData pathData)
    {
        // We are swapping between two preallocated paths, so we need to check which one is active and check if the other one is prepared.
        if (pathData == _preparedPath1)
        {
            //Logger.LogDebug($"[{Bot.name}] Path 1 Completed");
            if (!_disposing && _preparedPath2.Status == EBotMoveStatus.ReadyToMove)
            {
                //Logger.LogDebug($"[{Bot.name}] Path 2 Started");
                _activePath = _preparedPath2;
                _activePath.Start();
            }
            else
            {
                _activePath = null;
            }
        }
        else if (pathData == _preparedPath2)
        {
            //Logger.LogDebug($"[{Bot.name}] Path 2 Completed");
            if (!_disposing && _preparedPath1.Status == EBotMoveStatus.ReadyToMove)
            {
                //Logger.LogDebug($"[{Bot.name}] Path 1 Started");
                _activePath = _preparedPath1;
                _activePath.Start();
            }
            else
            {
                _activePath = null;
            }
        }
        else
        {
            Logger.LogError($"[{Bot.name}] what"); // This should never happen, but if it does...
        }
    }

    private BotPathDataManual _activePath;
    private readonly BotPathDataManual _preparedPath1;
    private readonly BotPathDataManual _preparedPath2;

    public bool RunToPoint(Vector3 point, bool mustHaveCompletePath = true, float reachDist = -1, ESprintUrgency urgency = ESprintUrgency.Low, bool checkSameWay = true)
    {
        if (reachDist <= 0) reachDist = BASE_DESTINATION_REACH_DIST;
        if ((point - Bot.Transform.NavData.Position).sqrMagnitude <= reachDist * reachDist) return true;
        if ((point - Bot.Position).sqrMagnitude <= reachDist * reachDist) return true;

        if (checkSameWay && TryUpdatePath(point))
        {
            if (!_activePath.WantToSprint)
            {
                _activePath.RequestStartSprint(urgency, "path updated");
            }
            _activePath.SetDestinationReachDistance(reachDist);
            return true;
        }

        if (Bot.Mover.CanGoToPoint(point, out NavMeshPath path, mustHaveCompletePath))
        {
            TriggerNewMove(path.corners, point, true, urgency, path);
            _activePath.SetDestinationReachDistance(reachDist);
            _activePath.PathStatus = path.status;
            return true;
        }
        return false;
    }

    public bool RunToPointByWay(NavMeshPath path, bool mustHaveCompletePath = true, float reachDist = -1, ESprintUrgency urgency = ESprintUrgency.Low, bool checkSameWay = true)
    {
        if (path == null) return false;
        if (path.status == NavMeshPathStatus.PathInvalid) return false;
        if (mustHaveCompletePath && path.status != NavMeshPathStatus.PathComplete) return false;

        Vector3[] pathCorners = path.corners;
        if (pathCorners.Length <= 1) return false;
        Vector3 lastCorner = pathCorners[pathCorners.Length - 1];
        if (reachDist <= 0) reachDist = BASE_DESTINATION_REACH_DIST;
        if ((lastCorner - Bot.Transform.NavData.Position).sqrMagnitude <= reachDist * reachDist) return true;
        if ((lastCorner - Bot.Position).sqrMagnitude <= reachDist * reachDist) return true;

        if (checkSameWay && TryUpdatePath(lastCorner))
        {
            if (!_activePath.WantToSprint) _activePath.RequestStartSprint(urgency, "path updated");
            _activePath.SetDestinationReachDistance(reachDist);
            return true;
        }
        TriggerNewMove(path.corners, lastCorner, true, urgency, path);
        _activePath.SetDestinationReachDistance(reachDist);
        return true;
    }

    private const float BASE_DESTINATION_REACH_DIST = 0.5f;

    public bool WalkToPoint(Vector3 point, bool mustHaveCompletePath = true, float reachDist = -1, bool checkSameWay = true)
    {
        if (reachDist <= 0) reachDist = BASE_DESTINATION_REACH_DIST;
        if ((point - Bot.Transform.NavData.Position).sqrMagnitude <= reachDist * reachDist) return true;
        if ((point - Bot.Position).sqrMagnitude <= reachDist * reachDist) return true;

        if (checkSameWay && TryUpdatePath(point))
        {
            if (_activePath.WantToSprint) _activePath.RequestEndSprint(ESprintUrgency.None, "path updated");
            return true;
        }
        if (Bot.Mover.CanGoToPoint(point, out NavMeshPath path, mustHaveCompletePath))
        {
            TriggerNewMove(path.corners, point, false, ESprintUrgency.None, path);
            _activePath.SetDestinationReachDistance(reachDist);
            _activePath.PathStatus = path.status;
            return true;
        }
        return false;
    }

    public bool WalkToPointByWay(NavMeshPath path, bool mustHaveCompletePath = true, float reachDist = -1, bool checkSameWay = true)
    {
        if (path == null) return false;
        if (path.status == NavMeshPathStatus.PathInvalid) return false;
        if (mustHaveCompletePath && path.status == NavMeshPathStatus.PathPartial) return false;

        Vector3[] pathCorners = path.corners;
        if (pathCorners.Length <= 1) return false;
        Vector3 lastCorner = pathCorners[pathCorners.Length - 1];
        if (reachDist <= 0) reachDist = BASE_DESTINATION_REACH_DIST;
        if ((lastCorner - Bot.Transform.NavData.Position).sqrMagnitude <= reachDist * reachDist) return true;
        if ((lastCorner - Bot.Position).sqrMagnitude <= reachDist * reachDist) return true;

        if (checkSameWay && TryUpdatePath(lastCorner))
        {
            if (_activePath.WantToSprint)
            {
                _activePath.RequestEndSprint(ESprintUrgency.None, "path updated");
            }
            return true;
        }

        TriggerNewMove(pathCorners, lastCorner, false, ESprintUrgency.None, path);
        _activePath.SetDestinationReachDistance(reachDist);
        return true;
    }

    private bool TryUpdatePath(Vector3 point)
    {
        return _preparedPath1.TryUpdatePath(point) || _preparedPath2.TryUpdatePath(point);
    }

    private void TriggerNewMove(Vector3[] pathCorners, Vector3 point, bool shallSprint, ESprintUrgency urgency, NavMeshPath path)
    {
        if (_activePath != null)
        {
            _activePath.Cancel();
            if (_activePath == _preparedPath1)
            {
                _preparedPath2.Initialize(Bot.NavMeshPosition, point, shallSprint, urgency, pathCorners, path);
            }
            else
            {
                _preparedPath1.Initialize(Bot.NavMeshPosition, point, shallSprint, urgency, pathCorners, path);
            }
            return;
        }
        _activePath = _preparedPath1;
        _activePath.Initialize(Bot.NavMeshPosition, point, shallSprint, urgency, pathCorners, path);
        _activePath.Start();
    }

    private bool TriggerRecalcPath()
    {
        if (_activePath == null) return false;
        if (_activePath.WantToSprint)
            return RunToPoint(_activePath.Destination, true, -1, _activePath.SprintUrgency, true);
        return WalkToPoint(_activePath.Destination, true, -1, false);
    }

    private bool CanSetPatrol()
    {
        if (BotOwner.WeaponManager?.Reload?.Reloading == true) return false;
        if (BotOwner.Medecine?.Using == true) return false;

        // Credit to Fontaine, these checks are taken from realism mod's code.
        if (Player.HandsController is Player.FirearmController firearmController &&
            !firearmController.IsAiming &&
            !firearmController.IsInReloadOperation() &&
            !firearmController.IsInventoryOpen() &&
            !firearmController.IsInInteractionStrictCheck() &&
            !firearmController.IsInSpawnOperation() &&
            !firearmController.IsHandsProcessing())
        {
            return true;
        }
        return false;
    }

    public override void Dispose()
    {
        _disposing = true;
        _preparedPath1?.Dispose();
        _preparedPath2?.Dispose();
        base.Dispose();
    }

    private bool _disposing;

    public BlindFireController BlindFire { get; private set; }
    public LeanClass Lean { get; private set; }
    public PoseClass Pose { get; private set; }
    public ProneClass Prone { get; private set; }

    public bool CrawlToPoint(Vector3 point, bool mustHaveCompletePath = true)
    {
        throw new NotImplementedException();
    }

    public bool CanGoToPoint(Vector3 point, out NavMeshPath path, bool mustHaveCompletePath = true, float navSampleRange = 0.5f)
    {
        var navData = Bot.Transform.NavData;
        if (!navData.IsOnNavMesh)
        {
            path = null;
            return false;
        }
        if (NavMesh.SamplePosition(point, out NavMeshHit targetHit, navSampleRange, -1))
        {
            path = new NavMeshPath();
            if (NavMesh.CalculatePath(navData.Position, targetHit.position, -1, path) && path.corners.Length > 1)
            {
                if (mustHaveCompletePath
                    && path.status != NavMeshPathStatus.PathComplete)
                {
                    return false;
                }
                return true;
            }
        }
        path = null;
        return false;
    }

    public bool GoToCoverPoint(CoverPoint point, bool sprint, ESprintUrgency urgency = ESprintUrgency.Low)
    {
        if (CanGoToCoverPoint(point, Bot.Transform.NavData))
        {
            if (sprint)
                return RunToPointByWay(point.PathData.Path, true, -1, urgency);
            return WalkToPointByWay(point.PathData.Path);
        }
        else
        {
            //Logger.LogDebug("cant go to point");
        }
        return false;
    }

    private static bool CanGoToCoverPoint(CoverPoint point, PlayerNavData navData)
    {
        //if (point.GetDistance(navData.Position) < BASE_DESTINATION_REACH_DIST * BASE_DESTINATION_REACH_DIST) return false;
        if (navData.IsOnNavMesh)
        {
            PathData coverPathData = point.PathData;
            if (coverPathData.TimeSinceLastUpdated > 0.5f)
            {
                coverPathData.Path.ClearCorners();
                NavMesh.CalculatePath(navData.Position, point.Position, -1, coverPathData.Path);
            }
            if (coverPathData.Path.status == NavMeshPathStatus.PathComplete)
            {
                return true;
            }
        }
        return false;
    }

    private static bool TryRecalcCoverPointPath(CoverPoint point, PlayerNavData navData, PathData coverPathData)
    {
        coverPathData.Path.ClearCorners();
        if (NavMesh.CalculatePath(navData.Position, point.Position, -1, coverPathData.Path) && coverPathData.Path.status == NavMeshPathStatus.PathComplete)
        {
            return true;
        }
#if DEBUG
#endif
        Logger.LogDebug($"Failed to recalculate path to cover point, path status: {coverPathData.Path.status}");
        return false;
    }

    public bool SetTargetPose(float pose)
    {
        return Pose.SetTargetPose(pose);
    }

    public void SetTargetMoveSpeed(float speed)
    {
        PlayerComponent.CharacterController.SetTargetMoveSpeed(speed);
    }

    public void Stop()
    {
        BotOwner?.Mover?.Stop();
        _activePath?.Cancel();
    }

    public void PauseMovement(float forDuration)
    {
        _activePath?.Pause(forDuration);
    }

    public void RecalcPath()
    {
        if (_activePath != null) _activePath.PathRecalcRequested = true;
    }

    public bool TryJump()
    {
        if (_nextJumpTime < Time.time &&
            Player.MovementContext?.CanJump == true)
        {
            _nextJumpTime = Time.time + 0.5f;
            Player.MovementContext?.TryJump();
            TimeLastJumped = Time.time;
            return true;
        }
        return false;
    }

    public bool TryVault()
    {
        bool vaulted = Player?.MovementContext?.TryVaulting() == true;
        if (vaulted)
        {
            TimeLastVaulted = Time.time;
        }
        return vaulted;
    }

    public float TimeLastJumped { get; private set; }
    public float TimeLastVaulted { get; private set; }

    private void UpdateStance(float time)
    {
        if (_nextChangeStanceTime < time)
        {
            _nextChangeStanceTime = time + CHANGE_STANCE_INTERVAL;

            MovementContext movementContext = Player.MovementContext;
            if (movementContext != null)
            {
                LeftStanceController leftStanceController = movementContext.LeftStanceController;

                bool wantToPatrolStance = !movementContext.IsSprintEnabled && Bot.GoalEnemy == null;
                if (wantToPatrolStance != movementContext.IsInPatrol)
                {
                    // If we are in left stance and want to patrol, reset back to normal  before setting patrol next update.
                    //if (wantToPatrolStance && leftStance?.LeftStance == true)
                    //{
                    //    leftStance.ToggleLeftStance();
                    //    return;
                    //}
                    movementContext.SetPatrol(wantToPatrolStance && CanSetPatrol());
                    return;
                }

                if (leftStanceController != null &&
                    leftStanceController.LeftStance != _wantLeftStance)
                {
                    leftStanceController.ToggleLeftStance();
                }
            }
        }
    }

    private void CheckSetBotToNavMesh()
    {
        if (Player.UpdateQueue != EUpdateQueue.Update)
        {
            return;
        }
        // Is the bot currently Moving somewhere?
        if (Moving || BotOwner.Mover.HasPathAndNoComplete)
        {
            _movingTime = Time.time + 1f;
            return;
        }
        if (_movingTime > Time.time)
        {
            return;
        }
        // Did the bot jump recently?
        if (Time.time - TimeLastJumped < _timeAfterJumpVaultReset)
        {
            return;
        }
        // Did the bot vault recently?
        if (Time.time - TimeLastVaulted < _timeAfterJumpVaultReset)
        {
            return;
        }
        // Is the bot currently falling?
        if (!Player.MovementContext.IsGrounded)
        {
            _ungroundedTime = Time.time + 1f;
            return;
        }
        if (_ungroundedTime < Time.time)
        {
        }
    }

    public void PathCornerSet(BotPathCorner corner, int index, int totalCorners)
    {
        OnPathCornerSet?.Invoke(corner, index, totalCorners);
    }

    public void PathCornerComplete(BotPathCorner corner, int index, int totalCorners)
    {
        OnPathCornerComplete?.Invoke(corner, index, totalCorners);
    }

    public void PathSteeringTicked(BotPathCorner corner, int index, int totalCorners)
    {
        Bot.CurrentAction?.OnSteeringTicked();
        Bot.Steering.TickPlayerSteering();
    }

    private float _ungroundedTime;
    private float _movingTime;

    private readonly float _timeAfterJumpVaultReset = 1.25f;
    private bool _wantLeftStance => Lean.LeanAngleValue.TargetValue < 0;

    private float _nextJumpTime = 0f;

    private const float CHANGE_STANCE_INTERVAL = 0.25f;
    private float _nextChangeStanceTime;
}