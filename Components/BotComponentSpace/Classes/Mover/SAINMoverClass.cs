using EFT;
using HarmonyLib;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes.Mover
{
    public class SAINMoverClass : BotBase, IBotClass
    {
        static SAINMoverClass()
        {
            _pathControllerField = AccessTools.Field(typeof(BotMover), "_pathController");
            _patrolField = AccessTools.Field(typeof(MovementContext), "_isInPatrol");
        }

        private static FieldInfo _pathControllerField;
        private static FieldInfo _patrolField;

        public SAINMoverClass(BotComponent sain) : base(sain)
        {
            _pathController = _pathControllerField.GetValue(sain.BotOwner.Mover) as PathControllerClass;
            BlindFire = new BlindFireController(sain);
            SideStep = new SideStepClass(sain);
            Lean = new LeanClass(sain);
            Prone = new ProneClass(sain);
            Pose = new PoseClass(sain);
            SprintController = new SAINSprint(sain);
            DogFight = new DogFight(sain);
        }

        private PathControllerClass _pathController { get; }

        public event Action<Vector3, Vector3> OnNewGoToPoint;

        public DogFight DogFight { get; private set; }
        public SAINSprint SprintController { get; private set; }

        public void Init()
        {
            base.SubscribeToPreset(null);
            UpdateBodyNavObstacle(false);
        }

        public void UpdateBodyNavObstacle(bool value)
        {
            if (BotBodyObstacle == null) {
                //BotBodyObstacle = SAIN.GetOrAddComponent<NavMeshObstacle>();
                if (BotBodyObstacle == null) {
                    //Logger.LogError($"Bot Body Navmesh obstacle is null for [{SAIN.BotOwner.name}]");
                    return;
                }
                //BotBodyObstacle.radius = 0.25f;
                //BotBodyObstacle.shape = NavMeshObstacleShape.Capsule;
                //BotBodyObstacle.carveOnlyStationary = false;
            }
            //BotBodyObstacle.enabled = false;
            //BotBodyObstacle.carving = value;
        }

        public void Update()
        {
            if (SprintController.Running || Player.IsSprintEnabled) {
                float time = Time.time + 0.5f;
                _changSpeedTime = time;
            }
            if (Crawling && !BotOwner.Mover.IsMoving) {
                Crawling = false;
            }

            handlePatrolStance();
            //updateStamina();
            Pose.Update();
            Lean.Update();
            Prone.Update();
            BlindFire.Update();
            SprintController.Update();
            checkSetBotToNavMesh();
        }

        private void handlePatrolStance()
        {
            bool wantToPatrolStance = BotOwner.Memory.IsPeace && !Player.IsSprintEnabled;
            bool inPatrol = _isInPatrol;
            if (wantToPatrolStance == inPatrol) {
                return;
            }
            Player.MovementContext.SetPatrol(wantToPatrolStance && canSetPatrol());
        }

        private bool canSetPatrol()
        {
            // Credit to Fontaine, these checks are taken from realism mod's code.
            var firearmController = Bot.Transform.WeaponData.FirearmController;
            if (firearmController != null &&
                !firearmController.IsAiming &&
                !firearmController.IsInReloadOperation() &&
                !firearmController.IsInventoryOpen() &&
                !firearmController.IsInInteractionStrictCheck() &&
                !firearmController.IsInSpawnOperation() &&
                !firearmController.IsHandsProcessing()) {
                return true;
            }
            return false;
        }

        private bool _isInPatrol => (bool)_patrolField.GetValue(Player.MovementContext);

        private void checkSetBotToNavMesh()
        {
            if (Player.UpdateQueue != EUpdateQueue.Update) {
                return;
            }
            // Is the bot currently Moving somewhere?
            if (SprintController.Running ||
                BotOwner.Mover?.HasPathAndNoComplete == true) {
                return;
            }
            // Did the bot jump recently?
            if (Time.time - TimeLastJumped < _timeAfterJumpVaultReset) {
                return;
            }
            // Did the bot vault recently?
            if (Time.time - TimeLastVaulted < _timeAfterJumpVaultReset) {
                return;
            }
            // Is the bot currently falling?
            if (!Player.MovementContext.IsGrounded) {
                _ungroundedTime = Time.time;
                return;
            }
            if (_ungroundedTime + 1f < Time.time) {
                ResetToNavMesh();
            }
        }

        private float _ungroundedTime;

        public void ResetToNavMesh()
        {
            if (BotOwner.Mover == null) {
                Logger.LogWarning("Bot Mover Null");
                return;
            }
            Vector3 position = Bot.Position;
            if ((_prevLinkPos - position).sqrMagnitude > 0f) {
                Vector3 castPoint = position + Vector3.up * 0.3f;
                // TAHVOHCK: Seems they're implying the current position from the cast point. dnSpy didn't unroll
                // the function correctly.
                BotOwner.Mover.SetPlayerToNavMesh(castPoint);
                _prevLinkPos = position;
            }
        }

        private Vector3 _prevLinkPos;

        private readonly float _timeAfterJumpVaultReset = 1.25f;

        public void Dispose()
        {
            SprintController?.Dispose();
        }

        public BlindFireController BlindFire { get; private set; }
        public SideStepClass SideStep { get; private set; }
        public LeanClass Lean { get; private set; }
        public PoseClass Pose { get; private set; }
        public ProneClass Prone { get; private set; }

        public NavMeshObstacle BotBodyObstacle { get; private set; }

        public bool Crawling { get; private set; }

        public Vector3 CurrentMoveDestination { get; private set; }

        public bool Moving => BotOwner.Mover?.IsMoving == true || BotOwner.Mover?.HasPathAndNoComplete == true;

        public bool GoToPoint(Vector3 point, out bool calculating, float reachDist = -1f, bool crawl = false, bool slowAtEnd = true, bool mustHaveCompletePath = true)
        {
            calculating = false;
            //if (SprintController.Running)
            //{
            //    SprintController.CancelRun(0.25f);
            //    if (SprintController.Canceling)
            //    {
            //        return false;
            //    }
            //}
            if (reachDist < 0f) {
                reachDist = SAINPlugin.LoadedPreset.GlobalSettings.General.BaseReachDistance;
            }

            bool wasMoving = Moving;
            CurrentPathStatus = BotOwner.Mover.GoToPoint(point, slowAtEnd, reachDist, false, false, true);
            if (CurrentPathStatus != NavMeshPathStatus.PathInvalid) {
                SprintController.CancelRun();
                Crawling = crawl && Bot.Info.FileSettings.Move.PRONE_TOGGLE && GlobalSettingsClass.Instance.Move.PRONE_TOGGLE;
                Prone.SetProne(crawl);
                checkNewMove(point, wasMoving);
                return true;
            }
            Crawling = false;
            return false;
        }

        public bool GoToEnemy(Enemy enemy, float reachDist = -1f, bool crawl = false, bool mustHaveCompletePath = true)
        {
            if (enemy == null) {
                return false;
            }

            var status = enemy.Path.PathToEnemyStatus;
            switch (status) {
                case NavMeshPathStatus.PathInvalid:
                    return false;

                case NavMeshPathStatus.PathPartial:
                    if (mustHaveCompletePath) {
                        return false;
                    }
                    break;

                default:
                    break;
            }

            CurrentPathStatus = status;
            return GoToPointByWay(enemy.Path.PathToEnemy.corners, reachDist, crawl);
        }

        public bool GoToPointByWay(Vector3[] way, float reachDist = -1f, bool crawl = false)
        {
            int length = way.Length;
            if (way == null || length < 2) {
                return false;
            }

            if (reachDist < 0f)
                reachDist = SAINPlugin.LoadedPreset.GlobalSettings.General.BaseReachDistance;

            if (crawl && Bot.Info.FileSettings.Move.PRONE_TOGGLE && GlobalSettingsClass.Instance.Move.PRONE_TOGGLE)
                Prone.SetProne(true);

            bool wasMoving = Moving;
            BotOwner.Mover.GoToByWay(way, reachDist);
            SprintController.CancelRun();
            checkNewMove(way[length - 1], wasMoving);
            return true;
        }

        private void checkNewMove(Vector3 destination, bool wasMoving)
        {
            if (!wasMoving || (CurrentMoveDestination - destination).sqrMagnitude > 0.1f) {
                Vector3 currentCorner = _pathController.CurrentCorner();
                OnNewGoToPoint?.Invoke(currentCorner, destination);
            }
            CurrentMoveDestination = destination;
        }

        public NavMeshPathStatus CurrentPathStatus { get; private set; } = NavMeshPathStatus.PathInvalid;

        public bool CanGoToPoint(Vector3 point, out NavMeshPath path, bool mustHaveCompletePath = true, float navSampleRange = 1f)
        {
            if (NavMesh.SamplePosition(point, out NavMeshHit targetHit, navSampleRange, -1)
                && NavMesh.SamplePosition(Bot.Transform.Position, out NavMeshHit botHit, navSampleRange, -1)) {
                path = new NavMeshPath();
                if (NavMesh.CalculatePath(botHit.position, targetHit.position, -1, path) && path.corners.Length > 1) {
                    if (mustHaveCompletePath
                        && path.status != NavMeshPathStatus.PathComplete) {
                        return false;
                    }
                    return true;
                }
            }
            path = null;
            return false;
        }

        public SAINMovementPlan MovementPlan { get; private set; }

        public NavMeshPath CurrentPath { get; private set; }

        private void updateStamina()
        {
            if (Bot.SAINLayersActive &&
                Bot.ActiveLayer != ESAINLayer.Extract &&
                !SprintController.Running &&
                !Player.IsSprintEnabled) {
                float staminaDivisor;
                float minStamina;
                if (ModDetection.RealismLoaded && BotOwner.Mover.TargetPose < 1f) {
                    staminaDivisor = 1.5f;
                    minStamina = 0.5f;
                }
                else {
                    staminaDivisor = 2f;
                    minStamina = 0.01f;
                }
                if (CurrentStamina < minStamina) {
                    Player.Physical.Stamina.UpdateStamina(Player.Physical.Stamina.TotalCapacity / staminaDivisor);
                }
            }
        }

        public float CurrentStamina => Player.Physical.Stamina.NormalValue;

        public bool SetTargetPose(float pose)
        {
            return Pose.SetTargetPose(pose);
        }

        public void SetTargetMoveSpeed(float speed)
        {
            if (canSetSpeed()) {
                BotOwner.Mover?.SetTargetMoveSpeed(speed);
            }
        }

        private bool canSetSpeed()
        {
            if (SprintController.Running || Player.IsSprintEnabled) {
                _changSpeedTime = Time.time + 0.5f;
                BotOwner.Mover?.SetTargetMoveSpeed(1f);
            }
            return _changSpeedTime < Time.time;
        }

        private bool canSetPose()
        {
            if (SprintController.Running || Player.IsSprintEnabled) {
                _changePoseTime = Time.time + 0.5f;
                //BotOwner.Mover?.SetTargetMoveSpeed(1f);
            }
            return _changePoseTime < Time.time;
        }

        private float _changePoseTime;
        private float _changSpeedTime;

        public void StopMove(float delay = 0.1f, float forDuration = 0f)
        {
            if (Player?.IsSprintEnabled == true) {
                Sprint(false);
            }
            if (delay <= 0f) {
                stop(forDuration);
                return;
            }
            if (!_stopping &&
                (BotOwner?.Mover?.IsMoving == true || Bot.Mover.SprintController.Running)) {
                _stopping = true;
                Bot.StartCoroutine(StopAfterDelay(delay, forDuration));
            }
        }

        private IEnumerator StopAfterDelay(float delay, float forDuration)
        {
            yield return new WaitForSeconds(delay);
            stop(forDuration);
            _stopping = false;
        }

        private void stop(float forDuration)
        {
            if (BotOwner?.Mover?.IsMoving == true) {
                BotOwner.Mover.Stop();
            }
            Bot?.Mover.SprintController.CancelRun();
            PauseMovement(forDuration);
        }

        public void PauseMovement(float forDuration)
        {
            if (forDuration > 0) {
                BotOwner?.Mover?.MovementPause(forDuration);
            }
        }

        public void ResetPath(float delay)
        {
            Bot.StartCoroutine(resetPath(0.2f));
        }

        private IEnumerator resetPath(float delay)
        {
            yield return StopAfterDelay(delay, 0f);
            BotOwner?.Mover?.RecalcWay();
        }

        private bool _stopping;

        public void Sprint(bool value)
        {
            if (BotOwner == null || BotOwner.DoorOpener == null) {
                return;
            }
            if (BotOwner.DoorOpener.Interacting) {
                value = false;
            }
            if (value) {
                //SAINBot.Steering.LookToMovingDirection();
                FastLean(0f);
            }
            BotOwner.Mover.Sprint(value);
        }

        public void EnableSprintPlayer(bool value)
        {
            if (value) {
                FastLean(0f);
            }
            Player.EnableSprint(value);
        }

        public bool TryJump()
        {
            if (_nextJumpTime < Time.time &&
                CanJump) {
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
            if (vaulted) {
                TimeLastVaulted = Time.time;
            }
            return vaulted;
        }

        public float TimeLastJumped { get; private set; }
        public float TimeLastVaulted { get; private set; }

        public void FastLean(LeanSetting value)
        {
            float num;
            switch (value) {
                case LeanSetting.Left:
                    num = -5f; break;
                case LeanSetting.Right:
                    num = 5f; break;
                default:
                    num = 0f; break;
            }
            FastLean(num);
        }

        public void FastLean(float value)
        {
            setTilt(value);
            handleShoulderSwap(value);
        }

        private void setTilt(float value)
        {
            if (Player.MovementContext.Tilt != value) {
                Player.MovementContext.SetTilt(value);
            }
        }

        private void handleShoulderSwap(float leanValue)
        {
            bool shoulderSwapped = isShoulderSwapped;
            if ((leanValue < 0 && !shoulderSwapped)
                || (leanValue >= 0 && shoulderSwapped)) {
                Player.MovementContext.LeftStanceController.ToggleLeftStance();
            }
        }

        private bool isShoulderSwapped => Player.MovementContext?.LeftStanceController?.LeftStance == true;

        public bool CanJump => Player.MovementContext?.CanJump == true;

        private float _nextJumpTime = 0f;
    }
}