using BepInEx.Logging;
using EFT;
using Movement.Components;
using Movement.Helpers;
using SAIN_Helpers;
using UnityEngine;
using UnityEngine.AI;
using static Movement.UserSettings.Debug;
using static SAIN_Helpers.SAIN_Math;

namespace SAIN.Movement.Layers.DogFight
{
    internal class UpdateMove
    {
        public UpdateMove(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = bot;
            Dodge = new BotDodge(bot);
            CoverFinder = new CoverFinder(bot);
            DynamicLean = bot.gameObject.GetComponent<DynamicLean>();
        }

        private const float UpdateFrequency = 0.2f;

        public void Update()
        {
            if (!GoalEnemyNull && CanShootEnemy)
            {
                EnemyLastSeenTime = Time.time;
            }

            if (ReactionTimer < Time.time)
            {
                UpdateDoorOpener();

                ReactionTimer = Time.time + UpdateFrequency;

                if (!GoalEnemyNull)
                {
                    TargetPosition = BotOwner.Memory.GoalEnemy.CurrPosition;
                    GoalEnemyLogic();
                }
                else if (!GoalTargetNull)
                {
                    TargetPosition = BotOwner.Memory.GoalTarget.Position;
                    GoalTargetLogic();
                }
            }
        }

        private void GoalTargetLogic()
        {
            DecideMovementSpeed();

            if (!BotIsAtTargetPosition && TargetPosition.HasValue)
            {
                if (BotOwner.GoToPoint(BotOwner.Memory.GoalTarget.Position.Value) == NavMeshPathStatus.PathComplete)
                {
                    return;
                }
            }

            SearchForTarget();
        }

        private void GoalEnemyLogic()
        {
            if (CheckFallBack())
            {
                MovingToLastEnemyPos = false;
                return;
            }

            DecideMovementSpeed();

            if (TakeCover())
            {
                MovingToLastEnemyPos = false;
                return;
            }

            if (!MovingToLastEnemyPos && BotOwner.MoveToEnemyData.TryMoveToEnemy(BotOwner.Memory.GoalEnemy.EnemyLastPosition))
            {
                if (DebugMode)
                {
                    Logger.LogDebug($"{BotOwner.name} is Moving to last known enemy position.");
                }

                MovingToLastEnemyPos = true;
            }
            else if (MovingToLastEnemyPos && BotOwner.Mover.IsComeTo(2f, false))
            {
                if (DebugMode)
                {
                    Logger.LogDebug($"{BotOwner.name} is moving to current enemy position.");
                }

                BotOwner.MoveToEnemyData.TryMoveToEnemy(BotOwner.Memory.GoalEnemy.CurrPosition);
            }
            else
            {
                if (DebugMode)
                {
                    Logger.LogDebug($"{BotOwner.name} has no place to go!");
                }

                BotOwner.MoveToEnemyData.TryMoveToEnemy(BotOwner.Memory.GoalEnemy.CurrPosition);
            }
        }

        private void SearchForTarget()
        {
            if (DebugMode)
            {
                Logger.LogDebug($"{BotOwner.name} is searching for enemy");
            }

            try
            {
                BotOwner.SearchData.UpdateByNode();
            }
            catch
            {
                Logger.LogError($"{BotOwner.name} Search Error");
            }
        }

        private void DecideMovementSpeed()
        {
            if (GoalEnemyNull && ShouldBotSneak)
            {
                if (DebugMode)
                {
                    Logger.LogDebug($"Sneak");
                }

                Sneak();
                return;
            }

            if (IsTargetVeryClose)
            {
                if (DebugMode)
                {
                    Logger.LogDebug($"FullSpeed");
                }

                FullSpeed();
            }
            else if (IsTargetClose)
            {
                if (DebugMode)
                {
                    Logger.LogDebug($"NormalSpeed");
                }

                NormalSpeed();
            }
            else if (ShouldBotSneak)
            {
                if (DebugMode)
                {
                    Logger.LogDebug($"Sneak");
                }

                Sneak();
            }
            else
            {
                if (DebugMode)
                {
                    Logger.LogDebug($"SlowWalk");
                }

                SlowWalk();
            }
        }

        private void RecheckCoverPosition()
        {
            if (FallBackCoverPoint == null)
            {
                return;
            }

            if (RecheckTime < Time.time)
            {
                RecheckTime = Time.time + 0.5f;

                if (CoverFinder.Analyzer.AnalyseCoverPosition(BotOwner.Memory.GoalEnemy.CurrPosition, FallBackCoverPoint.CoverPosition, out CustomCoverPoint cover, 1.0f))
                {
                    if (DebugMode)
                    {
                        Logger.LogDebug($"Old FallBack Position is still good for {BotOwner.name}");
                    }

                    FallBackCoverPoint = cover;
                }
                else
                {
                    if (DebugMode)
                    {
                        Logger.LogDebug($"Old FallBack Position is BAD for {BotOwner.name}");
                    }

                    FallBackCoverPoint = null;
                }
            }
        }

        private void StartFallback(Vector3 coverPosition)
        {
            if (DebugMode)
            {
                Logger.LogDebug($"{BotOwner.name} Is falling back");
            }

            FallingBack = true;

            if (HasStamina)
            {
                BotOwner.Steering.LookToMovingDirection();
                SetSprint(true);
            }

            BotOwner.GoToPoint(coverPosition, true, -1, false, true, true);
            UpdateDoorOpener();
        }

        private void DebugDrawFallback()
        {
            if (DebugMode)
            {
                DebugDrawer.Line(FallBackCoverPoint.CoverPosition, BotOwner.MyHead.position, 0.1f, Color.green, 3f);
                DebugDrawer.Line(BotOwner.Memory.GoalEnemy.Owner.MyHead.position, BotOwner.MyHead.position, 0.1f, Color.green, 3f);
                DebugDrawer.Line(BotOwner.Memory.GoalEnemy.Owner.MyHead.position, FallBackCoverPoint.CoverPosition, 0.1f, Color.green, 3f);
            }
        }

        private bool TakeCover()
        {
            if (CanShootEnemy && CanSeeEnemy)
            {
                if (DebugMode)
                {
                    Logger.LogDebug($"{BotOwner.name} Can See Enemy, and Shoot Enemy. Engaging.");
                }

                MovingToLastEnemyPos = false;
                DynamicLean.HoldLean = true;
                CheckSelfForCover();
                DecidePoseFromCoverLevel(SelfCover);
            }
            else if (!CanShootEnemy && CanSeeEnemy)
            {
                if (DebugMode)
                {
                    Logger.LogDebug($"{BotOwner.name} Can See Enemy, but can't shoot!. Moving Position.");
                }

                MovingToLastEnemyPos = false;
                DynamicLean.HoldLean = false;
                BotNeedsToBackup();
            }
            else
            {
                return false;
            }

            return true;
        }

        private void DecidePoseFromCoverLevel(CustomCoverPoint cover)
        {
            if (cover != null)
            {
                float coverLevel = cover.CoverLevel;
                float poseLevel;

                if (coverLevel > 0.75)
                {
                    poseLevel = 0.5f;
                }
                else if (coverLevel > 0.5f)
                {
                    poseLevel = 0.25f;
                }
                else
                {
                    poseLevel = 0.0f;
                }

                if (DebugMode)
                {
                    Logger.LogDebug($"{BotOwner.name} Is taking Cover from enemy in front. Pose Level = [{poseLevel}] because Cover Level = [{coverLevel}]");
                }

                BotOwner.SetPose(poseLevel);
            }
            else
            {
                if (DebugMode)
                {
                    Logger.LogDebug($"{BotOwner.name} Checked Self for Cover but has no cover. Proceeding to backup behavior.");
                }

                BotOwner.SetPose(1f);
                BotNeedsToBackup();
            }
        }

        private void CheckSelfForCover()
        {
            if (SelfCoverCheckTime < Time.time)
            {
                SelfCoverCheckTime = Time.time + 0.5f;

                if (CoverFinder.Analyzer.AnalyseCoverPosition(BotOwner.Memory.GoalEnemy.CurrPosition, BotOwner.Transform.position, out CustomCoverPoint cover, 0.2f))
                {
                    if (DebugMode)
                    {
                        Logger.LogDebug($"{BotOwner.name} Checked Self for Cover and found cover.");
                    }

                    SelfCover = cover;
                }
                else
                {
                    SelfCover = null;
                }
            }
        }

        private void BotNeedsToBackup()
        {
            const float angleStep = 15f;
            const float rangeStep = 3f;
            const int maxIterations = 5;
            int iterations = 0;
            while (iterations < maxIterations)
            {
                float angleAdd = angleStep * iterations;
                float currentAngle = UnityEngine.Random.Range(-15f - angleAdd, 15f + angleAdd);
                float currentRange = rangeStep * iterations + 1f;

                Vector3 DodgeFallBack = CoverFinder.FindArcPoint(BotOwner.Transform.position, TargetPosition.Value, currentRange, currentAngle);
                if (NavMesh.SamplePosition(DodgeFallBack, out NavMeshHit hit, 2f, -1))
                {
                    if (BotOwner.GoToPoint(hit.position, false, -1, false, false, true) == NavMeshPathStatus.PathComplete)
                    {
                        if (DebugMode)
                        {
                            Logger.LogInfo($"Found Backup Position after [{iterations}] iterations. Angle = [{currentAngle}] Range = [{currentRange}]");
                        }

                        UpdateDoorOpener();
                        return;
                    }
                }

                if (DebugMode)
                {
                    Logger.LogDebug($"Moving to next iteration. Found No Backup Position after [{iterations}] iterations. Angle = [{currentAngle}] Range = [{currentRange}]");
                }

                iterations++;
            }

            if (DebugMode)
            {
                Logger.LogWarning($"Found No Backup Position after [{iterations}] iterations.");
            }
        }

        /// <summary>
        /// Old
        /// </summary>
        public bool CheckEnemyDistance(out Vector3 trgPos)
        {
            Vector3 a = -NormalizeFastSelf(BotOwner.Memory.GoalEnemy.Direction);

            trgPos = Vector3.zero;

            float num = 0f;
            if (NavMesh.SamplePosition(BotOwner.Position + a * 2f / 2f, out NavMeshHit navMeshHit, 1f, -1))
            {
                trgPos = navMeshHit.position;

                Vector3 a2 = trgPos - BotOwner.Position;

                float magnitude = a2.magnitude;

                if (magnitude != 0f)
                {
                    Vector3 a3 = a2 / magnitude;

                    num = magnitude;

                    if (NavMesh.SamplePosition(BotOwner.Position + a3 * 2f, out navMeshHit, 1f, -1))
                    {
                        trgPos = navMeshHit.position;

                        num = (trgPos - BotOwner.Position).magnitude;
                    }
                }
            }
            if (num != 0f && num > BotOwner.Settings.FileSettings.Move.REACH_DIST)
            {
                BotBackupPath.ClearCorners();
                if (NavMesh.CalculatePath(BotOwner.Position, trgPos, -1, BotBackupPath) && BotBackupPath.status == NavMeshPathStatus.PathComplete)
                {
                    trgPos = BotBackupPath.corners[BotBackupPath.corners.Length - 1];

                    return CheckStraightDistance(BotBackupPath, num);
                }
            }
            return false;
        }

        /// <summary>
        /// Old
        /// </summary>
        private bool CheckStraightDistance(NavMeshPath path, float straighDist)
        {
            return path.CalculatePathLength() < straighDist * 1.2f;
        }

        private void UpdateDoorOpener()
        {
            BotOwner.DoorOpener.Update();
        }

        /// <summary>
        /// Sets the sprint of the bot to true or false.
        /// </summary>
        /// <param name="value">The value to set the sprint to.</param>
        private void SetSprint(bool value)
        {
            if (value)
            {
                BotOwner.GetPlayer.EnableSprint(true);
                BotOwner.Sprint(true);
                BotOwner.SetPose(1f);
                BotOwner.SetTargetMoveSpeed(1f);
                return;
            }

            BotOwner.GetPlayer.EnableSprint(false);
            BotOwner.Sprint(false);
        }

        private void FullSpeed()
        {
            BotOwner.SetTargetMoveSpeed(1f);
        }

        private void Sneak()
        {
            BotOwner.SetPose(0f);
            BotOwner.SetTargetMoveSpeed(0f);
        }

        private void SlowWalk()
        {
            BotOwner.SetTargetMoveSpeed(0.33f);
        }

        private void NormalSpeed()
        {
            BotOwner.SetTargetMoveSpeed(0.75f);
        }

        private void CheckPathLength()
        {
            if (LastDistanceCheck < Time.time)
            {
                Vector3? placeForCheck;
                if (BotOwner.Memory.GoalEnemy == null)
                {
                    placeForCheck = BotOwner.Memory.GoalTarget.Position;
                }
                else
                {
                    placeForCheck = BotOwner.Memory.GoalEnemy.CurrPosition;
                }

                if (placeForCheck != null)
                {
                    LastDistanceCheck = Time.time + 0.5f;
                    NavMesh.CalculatePath(BotOwner.Transform.position, placeForCheck.Value, -1, BotPath);
                    PathLength = BotPath.CalculatePathLength();
                }
            }
        }

        private bool CheckFallBack()
        {
            if (Reloading || BotOwner.Medecine.FirstAid.Using)
            {
                FallingBack = true;
            }
            else
            {
                SetSprint(false);
                FallingBack = false;
                return false;
            }

            RecheckCoverPosition();

            if (FallBackCoverPoint != null && FallingBack && !BotOwner.Mover.IsMoving)
            {
                if (DebugMode)
                {
                    Logger.LogDebug($"Bot is not moving but falling back. Taking Cover! {BotOwner.name}");
                }

                DecidePoseFromCoverLevel(FallBackCoverPoint);
                return true;
            }

            if (FallBackCoverPoint == null && CoverFinder.FindCover(out CustomCoverPoint coverPoint, TargetPosition.Value, 1f))
            {
                if (DebugMode)
                {
                    Logger.LogDebug($"Found New FallBack Position for {BotOwner.name}");
                }

                FallBackCoverPoint = coverPoint;

                DebugDrawFallback();

                StartFallback(coverPoint.CoverPosition);
            }
            else
            {
                if (DebugMode)
                {
                    Logger.LogWarning($"Couldn't Find Position for {BotOwner.name}");
                }
            }

            return true;
        }

        private bool ShouldBotSneak
        {
            get
            {
                if (GoalEnemyNull)
                {
                    return true;
                }
                else
                {
                    return !CanShootEnemyAndVisible && EnemyLastSeenTime + 10f < Time.time;
                }
            }
        }
        private bool IsTargetClose
        {
            get
            {
                CheckPathLength();
                return PathLength < 10f;
            }
        }
        private bool IsTargetVeryClose
        {
            get
            {
                CheckPathLength();
                return PathLength < 3f;
            }
        }
        private bool BotIsAtTargetPosition => TargetPosition != null && Vector3.Distance(TargetPosition.Value, BotOwner.Transform.position) < 2f;
        private bool Reloading => BotOwner.WeaponManager != null && BotOwner.WeaponManager.Reload.Reloading;
        private bool CanShootEnemyAndVisible => !GoalEnemyNull && CanShootEnemy && CanSeeEnemy;
        private bool CanShootEnemy => BotOwner.Memory.GoalEnemy != null && BotOwner.Memory.GoalEnemy.IsVisible;
        private bool CanSeeEnemy => BotOwner.Memory.GoalEnemy != null && BotOwner.Memory.GoalEnemy.CanShoot;
        private bool GoalEnemyNull => BotOwner.Memory.GoalEnemy == null;
        private bool GoalTargetNull => BotOwner.Memory.GoalTarget == null;
        private bool HasStamina => BotOwner.GetPlayer.Physical.Stamina.NormalValue > 0f;
        public bool IsSprintingFallback => FallingBack && BotOwner.Mover.IsMoving;
        private bool DebugMode => DebugUpdateMove.Value;
        public DynamicLean DynamicLean { get; private set; }

        private bool FallingBack;
        private float ReactionTimer = 0f;
        private float EnemyLastSeenTime;
        private float LastDistanceCheck = 0f;
        private float PathLength = 0f;
        private float RecheckTime = 0f;
        private float SelfCoverCheckTime = 0f;
        private Vector3? TargetPosition;
        private CustomCoverPoint SelfCover;
        public CustomCoverPoint FallBackCoverPoint;
        private NavMeshPath BotBackupPath = new NavMeshPath();
        private NavMeshPath BotPath = new NavMeshPath();
        private readonly BotOwner BotOwner;
        private readonly ManualLogSource Logger;
        private readonly BotDodge Dodge;
        private readonly CoverFinder CoverFinder;
        private bool MovingToLastEnemyPos;
    }
}