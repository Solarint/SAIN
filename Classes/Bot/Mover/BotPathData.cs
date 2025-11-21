using EFT;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes.Mover;

public interface IBotPathFinder
{
    public IBotPathData ActivePath { get; }
    public bool Moving { get; }
    public bool Running { get; }
    public bool Crawling { get; }

    public event Action<OperationResult, IBotPathData> OnPathComplete;

    public void PathComplete(OperationResult result, IBotPathData pathData);

    /// <summary>
    /// The Corner we are moving to, the index we are current at, and the total indexes in the path.
    /// </summary>
    public event Action<BotPathCorner, int, int> OnPathCornerSet;

    public void PathCornerSet(BotPathCorner corner, int index, int totalCorners);

    /// <summary>
    /// The Corner we are moving to, the index we are current at, and the total indexes in the path.
    /// </summary>
    public event Action<BotPathCorner, int, int> OnPathCornerComplete;

    public void PathCornerComplete(BotPathCorner corner, int index, int totalCorners);

    public void PathSteeringTicked(BotPathCorner corner, int index, int totalCorners);
}

public class BotPathDataManual(BotComponent bot, IBotPathFinder pathFinder) : IBotPathData
{
    public IBotPathFinder PathFinder { get; } = pathFinder;

    public void TickPath(float deltaTime, float CurrentTime)
    {
        Vector3 botPosition = BotPosition();
        BotPathCorner currentCorner = GetCurrentCorner();

        if (!CanProceedWithPath(botPosition))
        {
            PathFinder.PathSteeringTicked(currentCorner, CurrentIndex, PathCorners.Count);
            return;
        }

        CurrentCornerMoveData = CalculateMoveData(botPosition, currentCorner);

        if (InteractWithDoor())
        {
            //Logger.LogDebug($"[{Bot.name}] door interaction");
            Status = EBotMoveStatus.DoorInteraction;
            PathFinder.PathSteeringTicked(currentCorner, CurrentIndex, PathCorners.Count);
            return;
        }

        if (CheckPaused())
        {
            //Logger.LogDebug($"[{Bot.name}] paused");
            Status = EBotMoveStatus.Paused;
            PathFinder.PathSteeringTicked(currentCorner, CurrentIndex, PathCorners.Count);
            return;
        }

        Status = EBotMoveStatus.Moving;

        Util.StopVanillaMover(Bot.BotOwner.Mover);
        Bot.Mover.Prone.SetProne(Crawling);
        
#if DEBUG
        Util.DrawMoverDebug(botPosition, currentCorner.Position);
#endif

        CurrentSprintStatus = GetSprintStatus(botPosition);
        SetSprint(CurrentSprintStatus == EBotSprintStatus.Running, $"{CurrentSprintStatus}");

        bool slowAtCorners = Bot.GoalEnemy != null && Bot.GoalEnemy.Events.OnSearch.Value && Bot.Info.PersonalitySettings.Search.SlowAtCorners;
        Vector3 startSlowDestination = slowAtCorners ? currentCorner.Position : Destination;
        float startSlowDist = slowAtCorners ? 2f : 0.85f;
        float stopSprintDist = slowAtCorners ? 2f : 1.1f;
        Bot.PlayerComponent.CharacterController.SetTargetMoveDirection(currentCorner.Position - Bot.Position, startSlowDestination, Bot.PlayerComponent, startSlowDist, stopSprintDist);

        if (!CheckSprintSteering(currentCorner))
        {
            PathFinder.PathSteeringTicked(currentCorner, CurrentIndex, PathCorners.Count);
        }
        else
        {
            Bot.BotOwner.ShootData.EndShoot();
            //Bot.BotOwner.ShootData.BlockFor(0.05f);
            Bot.Aim.LoseAimTarget();
            Bot.ManualShoot.Reset();
            Bot.Suppression.ResetSuppressing();
        }

        if (CheckCornerComplete())
        {
            CompleteCorner(CurrentIndex);
            CurrentIndex++;
            if (CurrentIndex == PathCorners.Count)
            {
                Stop(true, "complete");
                return;
            }
            StartCorner(CurrentIndex);
        }
        else if (CheckStuck(true, currentCorner))
        {
            return;
        }
    }

    private bool CheckCornerComplete()
    {
        float currentDist = CurrentCornerMoveData.Magnitude;
        if (currentDist <= CornerReachDist())
        {
            return true;
        }
        if (currentDist < 1f && CurrentIndex + 1 < PathCorners.Count)
        {
            BotPathCorner nextCorner = PathCorners[CurrentIndex + 1];
            if (!NavMesh.Raycast(Bot.NavMeshPosition, nextCorner.Position, out _, NavMesh.AllAreas))
            {
                return true;
            }
        }
        return false;
    }

    private bool CanProceedWithPath(Vector3 botPosition)
    {
        if (CancelRequested && _cancelTime < Time.time)
        {
            Stop(false, "canceled");
            return false;
        }
        if ((Destination - botPosition).sqrMagnitude < DestinationReachDistance * DestinationReachDistance)
        {
            Stop(true, "arrived");
            return false;
        }
        return true;
    }

    public void UpdateSprint(ESprintUrgency urgency)
    {
        SprintUrgency = urgency;
    }

    public void RequestStartSprint(ESprintUrgency urgency, string reason)
    {
        WantToSprint = true;
        SprintUrgency = urgency;
        SprintReason = reason;
    }

    public void RequestEndSprint(ESprintUrgency urgency, string reason)
    {
        WantToSprint = false;
        SprintReason = reason;
    }

    public string SprintReason { get; private set; }

    public bool Crawling { get; private set; }

    public void RequestStartCrawl()
    {
        throw new NotImplementedException();
    }

    public void RequestEndCrawl()
    {
        throw new NotImplementedException();
    }

    public bool Moving {
        get
        {
            return Status switch {
                EBotMoveStatus.Moving or EBotMoveStatus.DoorInteraction or EBotMoveStatus.Paused => true,
                _ => false,
            };
        }
    }

    public bool Running => Moving && WantToSprint;
    public int CurrentIndex { get; private set; }
    public CornerMoveData CurrentCornerMoveData { get; private set; }

    public EBotMoveStatus Status { get; private set; }
    public float DestinationReachDistance { get; private set; }
    public Vector3 Destination { get; private set; }
    public NavMeshPath NavMeshPath { get; private set; }

    public List<BotPathCorner> PathCorners { get; } = [];
    public List<Vector3> PathPoints { get; } = [];

    public EBotSprintStatus CurrentSprintStatus { get; private set; }
    public ESprintUrgency SprintUrgency { get; private set; }
    public bool WantToSprint { get; private set; }

    public bool PathRecalcRequested { get; set; }

    public float TimeStarted { get; private set; }

    public float PathLength { get; private set; }

    public Vector3 StartPosition { get; private set; }

    public bool PausedRequested { get; private set; }
    public bool CancelRequested { get; private set; }

    public bool OnLastCorner => CurrentIndex == PathCorners.Count - 1;
    public float CurrentCornerDistanceSqr => CurrentCornerMoveData.SqrMagnitude;
    public float CurrentCornerDistance => CurrentCornerMoveData.Magnitude;

    public NavMeshPathStatus PathStatus { get; set; }

    public int Id { get; private set; } = _moveID++;

    /// <summary>
    /// Prepare a path, but dont start it yet. Must call Start() to start the path.
    /// </summary>
    public void Initialize(Vector3 botNavPosition, Vector3 destination, bool shallSprint, ESprintUrgency urgency, Vector3[] corners, NavMeshPath path)
    {
        ClearCachedData();
        NavMeshPath = path;
        StartPosition = botNavPosition;
        WantToSprint = shallSprint;
        SprintUrgency = urgency;
        CurrentCornerMoveData = new() {
            Dot = 1,
            SqrMagnitude = float.MaxValue,
        };
        CreatePath(corners);
        Destination = destination;
        PathLength = PathCorners.CalcPathLength();
        Status = EBotMoveStatus.ReadyToMove;
    }

    private float _lastCheckStuckTime;
    private float _timeNotMoving;
    private CornerMoveData _lastCheckedMoveData;
    private float _unpauseTime;
    private float _pauseStartTime;
    private float _cancelTime;
    private static int _moveID = 0;

    protected readonly BotComponent Bot = bot;

    private void CreatePath(Vector3[] corners)
    {
        // skip first corner
        PathCorners.Clear();
        for (int i = 1; i < corners.Length; i++)
        {
            Vector3 start = corners[i - 1];
            EBotCornerType type = Util.FindCornerType(corners.Length, i);
            PathCorners.Add(new(start, corners[i], type, i));
        }
    }

    /// <summary>
    /// Checks if the destination is the same as where we already going, or if we can update our path to a modified destination
    /// </summary>
    /// <param name="possibleDestination"></param>
    /// <param name="shallSprint"></param>
    /// <returns></returns>
    public bool TryUpdatePath(Vector3 possibleDestination)
    {
        if (Moving && !CancelRequested)
        {
            const float MIN_DIST_CHANGE_DESTINATION = 0.5f;
            // If the place being requested to move to is very close to where we are already moving to, we dont need to change anything.
            if ((Destination - possibleDestination).sqrMagnitude < MIN_DIST_CHANGE_DESTINATION)
            {
                return true;
            }
        }
        return false;
    }

    public void RecalcPath()
    {
        throw new NotImplementedException();
    }

    public BotPathCorner GetCurrentCorner()
    {
        return PathCorners[CurrentIndex];
    }

    public BotPathCorner GetLastCorner()
    {
        return PathCorners[PathCorners.Count - 1];
    }

    public void Pause(float duration)
    {
        if (CancelRequested) return;
        if (duration > 0f)
        {
            duration = Mathf.Max(duration, 0.25f);
            PausedRequested = true;
            _pauseStartTime = Time.time + 0.25f;
            _unpauseTime = Time.time + duration;
        }
    }

    public void UnPause()
    {
        if (PausedRequested)
        {
            _unpauseTime = Time.time;
            PausedRequested = false;
        }
    }

    /// <summary>
    /// Cancel the move after a minimum delay
    /// </summary>
    /// <param name="delay"></param>
    public void Cancel(float delay = 0.25f)
    {
        if (!CancelRequested)
        {
            CancelRequested = true;
            delay = Mathf.Max(delay, 0.33f);
            _cancelTime = Time.time + delay;
        }
    }

    public void Start()
    {
        Util.PrepareBot(Bot, WantToSprint);
        Status = EBotMoveStatus.Moving;
        TimeStarted = Time.time;
    }

    /// <summary>
    /// Cancel the move immedietly.
    /// </summary>
    private void Stop(bool success, string reason)
    {
        Status = EBotMoveStatus.Complete;
        PathFinder.PathComplete(new OperationResult(success, reason), this);
        //Logger.LogDebug($"[{Bot.name}]:[{Id}]: Complete Move: Duration [{Time.time - TimeStarted}] Reason: [{reason}]");
        ClearCachedData();
    }

    private void ClearCachedData()
    {
        PathStatus = NavMeshPathStatus.PathInvalid;
        CurrentIndex = 0;
        NavMeshPath = null;
        StartPosition = Vector3.zero;
        TimeStarted = -1;
        WantToSprint = false;
        SprintUrgency = ESprintUrgency.None;
        CurrentCornerMoveData = new();
        Destination = Vector3.zero;
        PathLength = 0;
        PathCorners.Clear();
        PathPoints.Clear();
        PausedRequested = false;
        CancelRequested = false;
        PathRecalcRequested = false;
        DestinationReachDistance = -1;

        _lastCheckStuckTime = -1f;
        _timeNotMoving = -1f;
        _lastCheckedMoveData = new();
        _unpauseTime = -1f;
        _pauseStartTime = -1f;
        _cancelTime = -1f;
    }

    public void Dispose()
    {
        Stop(false, "Dispose");
    }

    private bool InteractWithDoor()
    {
        if (Bot.DoorOpener.SelectDoor(out EInteractionType type, out DoorDataStruct data, this))
        {
            Bot.Mover.Prone.SetProne(false);
            SetSprint(false, "door");
            Bot.Mover.SetTargetPose(1f);
            Bot.Mover.SetTargetMoveSpeed(1f);
            Bot.AimDownSightsController.SetADS(false);
            if (Bot.DoorOpener.Interacting || Bot.DoorOpener.TryInteractWithDoor(type, Time.time, data))
            {
                //DoorDataStruct doorData = Bot.DoorOpener.GetActiveDoor();
                //Logger.LogDebug($"[{Bot.name}]:[{Id}]: Reverse Path");
                Vector3 position = CurrentIndex == 0 ? StartPosition : PathCorners[CurrentIndex - 1].Position;
                Vector3 dirNormal = (position - Bot.Position).normalized;
                Bot.PlayerComponent.CharacterController.SetTargetMoveDirection(position, position, Bot.PlayerComponent, 0.33f, 1.33f);
                return true;
            }
        }
        //if (!_activeDoorInteraction.lookedAtHandle) _activeDoorInteraction.lookedAtHandle = Bot.Steering.IsLookingAtPoint(_activeDoorInteraction.doorHandleLook, out _);
        //if (_activeDoorInteraction.lookedAtHandle)
        //{
        //DebugGizmos.DrawLine(Bot.Transform.WeaponData.WeaponRoot, _activeDoorInteraction.doorHandleLook, Color.green, 0.05f, 1f / 60f, true);
        //Logger.LogDebug($"[{Bot.name}]:[{Id}]:[{doorData.Link.Id}] looking at handle. DoorState: [{doorData.Door.DoorState}] Desired State: [{desiredDoorState}]");

        //Bot.Steering.Unlock();
        //if (_activeDoorInteraction.pullOpenDoor) BackUpFromDoor(doorData);
        //return true;
        //}
        //Logger.LogDebug($"[{Bot.name}]:[{Id}]:[{doorData.Link.Id}] Look To Handle. DoorState: [{doorData.Door.DoorState}] Desired State: [{_activeDoorInteraction.desiredDoorState}]");
        //MoveToInteract(_activeDoorInteraction.doorHandleLook, _activeDoorInteraction.movePosition);
        //return true;

        //Bot.Steering.Unlock();
        return false;
    }

    private bool CheckPaused()
    {
        if (PausedRequested)
        {
            if (_unpauseTime < Time.time)
            {
                //Logger.LogDebug($"[{Bot.name}]:[{Id}]: unpaused");
                UnPause();
            }
            else if (_pauseStartTime < Time.time)
            {
                //Logger.LogDebug($"[{Bot.name}]:[{Id}]: paused");
                SetSprint(false, "paused");
                return true;
            }
        }
        return false;
    }

    private bool CheckStuck(bool canTryVault, BotPathCorner activeCorner)
    {
        if (Time.time - _lastCheckStuckTime < 0.5f)
        {
            return false;
        }
        _lastCheckStuckTime = Time.time;
        if (Time.time - activeCorner.TimeStarted > 1f)
        {
            var currentMoveData = CurrentCornerMoveData;
            if (currentMoveData.Dot < 0.75f)
            {
                //Logger.LogDebug($"[{Bot.name}]:[{Id}]: recalc from Dot MoveData: " +
                //    $"{currentMoveData.CornerDirectionFromBot}:" +
                //    $"{currentMoveData.CornerDirectionFromBotNormal}:" +
                //    $"{currentMoveData.Dot}:" +
                //    $"{currentMoveData.SqrMagnitude}");
                PathRecalcRequested = true;
                return true;
            }

            if (_lastCheckedMoveData.SqrMagnitude - currentMoveData.SqrMagnitude > 0.01f)
            {
                _timeNotMoving = -1f;
            }
            else if (_timeNotMoving < 0)
            {
                _timeNotMoving = Time.time;
            }
            _lastCheckedMoveData = currentMoveData;
            if (_timeNotMoving > 0f)
            {
                if (CheckObjectInWay(BotPosition(), CurrentCornerMoveData.CornerDirectionFromBot, 1f, 0.2f, 1f))
                {
                    //Logger.LogDebug($"[{Bot.name}]:[{Id}]: recalc from object in way: " +
                    //    $"{currentMoveData.CornerDirectionFromBot}:" +
                    //    $"{currentMoveData.CornerDirectionFromBotNormal}:" +
                    //    $"{currentMoveData.Dot}:" +
                    //    $"{currentMoveData.SqrMagnitude}");
                    PathRecalcRequested = true;
                    return true;
                }
                float timeSinceNoMove = Time.time - _timeNotMoving;
                if (timeSinceNoMove > 2)
                //if (timeSinceNoMove > GlobalSettingsClass.Instance.Move.BOT_NOMOVE_RECALC_TIME)
                {
                    //Logger.LogDebug($"[{Bot.name}]:[{Id}]: recalc from no move: " +
                    //    $"{currentMoveData.CornerDirectionFromBot}:" +
                    //    $"{currentMoveData.CornerDirectionFromBotNormal}:" +
                    //    $"{currentMoveData.Dot}:" +
                    //    $"{currentMoveData.SqrMagnitude}");
                    PathRecalcRequested = true;
                    return true;
                }
                else if (timeSinceNoMove > GlobalSettingsClass.Instance.Move.BOT_NOMOVE_TRYJUMP_TIME)
                {
                    Bot.Mover.TryJump();
                }
                else if (canTryVault && timeSinceNoMove > GlobalSettingsClass.Instance.Move.BOT_NOMOVE_TRYVAULT_TIME)
                {
                    Bot.Mover.TryVault();
                }
            }
        }
        return false;
    }

    private static bool CheckObjectInWay(Vector3 botPosition, Vector3 direction, float height = 1f, float sphereCastRadius = 0.2f, float maxDist = 1f)
    {
        if (Physics.SphereCast(botPosition + (Vector3.up * height), sphereCastRadius, direction, out RaycastHit hit, maxDist, LayerMaskClass.PlayerStaticCollisionsMask))
        {
#if DEBUG
            DebugGizmos.DrawLine(botPosition + (Vector3.up * height), hit.point, Color.green, 0.1f, 3);
#endif
            return true;
        }
        return false;
    }

    private Vector3 BotPosition()
    {
        var navData = Bot.Transform.NavData;
        Vector3 botPosition = navData.IsOnNavMesh ? navData.Position : Bot.Transform.Position;
        return botPosition;
    }

    private float CornerReachDist()
    {
        return WantToSprint ? SAINPlugin.LoadedPreset.GlobalSettings.Move.BotSprintCornerReachDist : SAINPlugin.LoadedPreset.GlobalSettings.Move.BotWalkCornerReachDist;
    }

    private void StartCorner(int i)
    {
        CurrentIndex = i;
        BotPathCorner startedCorner = PathCorners[i];
        startedCorner.TimeStarted = Time.time;
        startedCorner.Status = EBotCornerStatus.Active;
        PathFinder.PathCornerSet(startedCorner, i, PathCorners.Count);
        PathCorners[i] = startedCorner;
    }

    private void CompleteCorner(int i)
    {
        BotPathCorner completeCorner = PathCorners[i];
        completeCorner.TimeComplete = Time.time;
        completeCorner.Status = EBotCornerStatus.Used;
        PathFinder.PathCornerComplete(completeCorner, i, PathCorners.Count);
        PathCorners[i] = completeCorner;
    }

    private void SetSprint(bool value, string reason)
    {
        Bot.PlayerComponent.CharacterController.SetWantToSprint(value);
    }

    private CornerMoveData CalculateMoveData(Vector3 botPosition, BotPathCorner activeCorner)
    {
        Vector3 dir = (activeCorner.Position - botPosition);
        Vector3 normal = dir.normalized;
        float sqrMag = dir.sqrMagnitude;
        float dot = Vector3.Dot(normal, activeCorner.DirectionFromPrevious.DirectionNormalized);
        return new() {
            CornerDirectionFromBot = dir,
            CornerDirectionFromBotNormal = normal,
            Dot = dot,
            SqrMagnitude = sqrMag,
            Magnitude = Mathf.Sqrt(sqrMag),
            PercentageComplete = activeCorner.GetPercentageOfCornerComplete(sqrMag),
        };
    }

    /// <summary>
    /// If a bot is not sprinting right now, and the current corner is less than this distance away (meters), the bot will not start sprinting yet.
    /// </summary>
    private const float START_SPRINT_CORNER_MIN_DIST = 0.25f;

    /// <summary>
    /// Angle from the direction a bot is looking to the direction of the corner, if the angle is greater than this, the bot is "turning" and stops sprinting.
    /// </summary>
    private const float SPRINT_CORNER_DIR_ANGLE_MAX = 45f;

    private EBotSprintStatus GetSprintStatus(Vector3 botPosition)
    {
        if (WantToSprint)
        {
            if (CancelRequested)
            {
                return EBotSprintStatus.Canceling;
            }
            if (PausedRequested)
            {
                return EBotSprintStatus.Pausing;
            }
            // I cant sprint :(
            if (!Bot.Player.MovementContext.CanSprint)
            {
                return EBotSprintStatus.CantSprint;
            }

            bool sprintingNow = Bot.Player.MovementContext.IsSprintEnabled;
            float stamina = Bot.Player.Physical.Stamina.NormalValue;
            // We are out of stamina, stop sprinting.
            if (sprintingNow && Util.ShallPauseSprintStamina(stamina, SprintUrgency))
            {
                return EBotSprintStatus.NoStamina;
            }
            // If we are not looking in the direction of the corner we are moving toward, dont sprint.
            Vector3 lookDir = Bot.PlayerComponent.CharacterController.CurrentControlLookDirection;
            lookDir.y = 0;
            Vector3 cornerDir = CurrentCornerMoveData.CornerDirectionFromBotNormal;
            cornerDir.y = 0;

            if (Vector3.Angle(lookDir.normalized, cornerDir.normalized) > SPRINT_CORNER_DIR_ANGLE_MAX)
            {
                return EBotSprintStatus.Turning;
            }
            if (sprintingNow)
            {
                return EBotSprintStatus.Running;
            }
            //if (CurrentCornerDistance >= START_SPRINT_CORNER_MIN_DIST && Util.ShallStartSprintStamina(stamina, SprintUrgency))
            if (Util.ShallStartSprintStamina(stamina, SprintUrgency))
            {
                //Logger.LogDebug($"start sprint {staminaNormal}");
                return EBotSprintStatus.Running;
            }
        }
        return EBotSprintStatus.None;
    }

    private bool CheckSprintSteering(BotPathCorner corner)
    {
        if (WantToSprint)
        {
            switch (CurrentSprintStatus)
            {
                case EBotSprintStatus.Running:
                case EBotSprintStatus.Turning:
                    Bot.Steering.LookToFloorPoint(corner.Position);
                    Bot.Steering.TickPlayerSteering();
                    return true;

                case EBotSprintStatus.Canceling:
                    if (Bot.Player.IsSprintEnabled)
                    {
                        Bot.Steering.LookToFloorPoint(corner.Position);
                        Bot.Steering.TickPlayerSteering();
                        return true;
                    }
                    break;

                default:
                    break;
            }
        }
        return false;
    }

    internal void SetDestinationReachDistance(float reachDist)
    {
        DestinationReachDistance = reachDist;
    }

    private static class Util
    {
        public static void PrepareBot(BotComponent bot, bool sprinting)
        {
            if (bot == null || bot.BotOwner == null) return;
            StopVanillaMover(bot.BotOwner.Mover);
            bot.BotOwner.WeaponManager?.Stationary?.StartMove();
            if (sprinting)
            {
                bot.Aim.LoseAimTarget();
                bot.AimDownSightsController.SetADS(false);
                bot.Mover.Prone.SetProne(false);
            }
        }

        public static EBotCornerType FindCornerType(int count, int i)
        {
            if (i == count - 1) // LastCorner?
            {
                return EBotCornerType.PathEnd;
            }
            else if (i == count - 2) // Second to last corner?
            {
                return EBotCornerType.PathEndApproach;
            }
            else
            {
                return EBotCornerType.PathPoint;
            }
        }

        public static void StopVanillaMover(BotMover botOwnerMover)
        {
            if (botOwnerMover == null)
            {
                return;
            }
            // Make sure the vanilla path finder is NOT active
            if (botOwnerMover.IsMoving || botOwnerMover.HasPathAndNoComplete)
            {
                botOwnerMover.Stop(); // Backwards sprint / moonwalking fix
#if DEBUG
                Logger.LogDebug($"vanilla mover stopped");
#endif
            }
        }

        public static void DrawMoverDebug(Vector3 BotPos, Vector3 destination)
        {
            Vector3 debugOffset = Vector3.up * 0.25f;
            DebugGizmos.DrawSphere(destination, 0.2f, Color.white, 0.02f);
            DebugGizmos.DrawLine(destination, destination + debugOffset, Color.white, 0.075f, 0.02f);
            DebugGizmos.DrawLine(destination + debugOffset, BotPos + debugOffset, Color.white, 0.075f, 0.02f);
        }

        private static float FindStartSprintStamina(ESprintUrgency urgency)
        {
            return urgency switch {
                ESprintUrgency.None or ESprintUrgency.Low => 0.9f,
                ESprintUrgency.Middle => 0.6f,
                ESprintUrgency.High => 0.4f,
                _ => 0.5f,
            };
        }

        private static float FindEndSprintStamina(ESprintUrgency urgency)
        {
            return urgency switch {
                ESprintUrgency.None or ESprintUrgency.Low => 0.25f,
                ESprintUrgency.Middle => 0.15f,
                ESprintUrgency.High => 0.01f,
                _ => 0.25f,
            };
        }

        public static bool ShallPauseSprintStamina(float stamina, ESprintUrgency urgency) => stamina <= FindEndSprintStamina(urgency);

        public static bool ShallStartSprintStamina(float stamina, ESprintUrgency urgency) => stamina >= FindStartSprintStamina(urgency);
    }
}