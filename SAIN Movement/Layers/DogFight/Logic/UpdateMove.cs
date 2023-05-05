using BepInEx.Logging;
using EFT;
using Movement.Helpers;
using SAIN_Helpers;
using UnityEngine;
using UnityEngine.AI;
using static SAIN_Helpers.SAIN_Math;

namespace SAIN.Movement.Layers.DogFight
{
    internal class UpdateMove
    {
        private const float UpdateFrequency = 0.1f;

        public UpdateMove(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = bot;
            Dodge = new BotDodge(bot);
            CoverFinder = new CoverFinder(bot);
        }

        public void Update(bool debug = false, bool debugDrawAll = false)
        {
            if (ReactionTimer < Time.time)
            {
                ReactionTimer = Time.time + UpdateFrequency;
                UpdateDoorOpener();

                if (CanShootEnemy)
                {
                    EnemyLastSeenTime = Time.time;
                    LastEnemyPosition = BotOwner.Memory.GoalEnemy.CurrPosition;
                }
                else if (BotIsAtLastEnemyPosition)
                {
                    LastEnemyPosition = null;
                }

                if (CheckFallBackConditions())
                {
                    return;
                }

                if (CanShootEnemy)
                {
                    DecidePoseFromCoverLevel();
                }
                else
                {
                    DecideMovementSpeed();

                    if (LastEnemyPosition != null && !BotIsAtLastEnemyPosition)
                    {
                        BotOwner.MoveToEnemyData.TryMoveToEnemy(LastEnemyPosition.Value);
                    }
                    else
                    {
                        BotOwner.MoveToEnemyData.TryMoveToEnemy(BotOwner.Memory.GoalEnemy.CurrPosition);
                    }
                }
            }
        }

        private void DecideMovementSpeed()
        {
            if (IsEnemyVeryClose)
            {
                FullSpeed();
            }
            else if (IsEnemyClose)
            {
                NormalSpeed();
            }
            else if (ShouldISneak)
            {
                Sneak();
            }
            else
            {
                SlowWalk();
            }
        }

        public bool FallingBack { get; private set; }
        public Vector3? LastEnemyPosition { get; private set; }
        public float EnemyLastSeenTime { get; private set; }

        public bool IsEnemyClose
        {
            get
            {
                CheckPathLength();
                return PathLength < 10f;
            }
        }

        public bool IsEnemyVeryClose
        {
            get
            {
                CheckPathLength();
                return PathLength < 3f;
            }
        }

        public bool BotIsAtLastEnemyPosition => LastEnemyPosition != null && Vector3.Distance(LastEnemyPosition.Value, BotOwner.Transform.position) < 2f;
        public bool Reloading => BotOwner.WeaponManager.Reload.Reloading;
        public bool CanShootEnemyAndVisible => CanShootEnemy && CanSeeEnemy;
        public bool CanShootEnemy => BotOwner.Memory.GoalEnemy.IsVisible;
        public bool CanSeeEnemy => BotOwner.Memory.GoalEnemy.CanShoot;
        public bool ShouldISneak => !CanShootEnemyAndVisible && EnemyLastSeenTime + 10f < Time.time;
        private bool HasStamina => BotOwner.GetPlayer.Physical.Stamina.NormalValue > 0f;
        public bool IsSprintingFallback => FallingBack && BotOwner.Mover.IsMoving;

        private bool CheckFallBackConditions(bool debugMode = false)
        {
            if (!Reloading && !BotOwner.Medecine.FirstAid.Using || BotOwner.BotLay.IsLay)
            {
                SetSprint(false);
                FallingBack = false;
                return false;
            }

            if (FallBackCoverPoint != null)
            {
                CustomCoverPoint cover = CoverFinder.Analyzer.AnalyseCoverPosition(FallBackCoverPoint.CoverPosition);
                if (cover != null)
                {
                    DebugDrawFallback(debugMode);
                    StartFallback(cover.CoverPosition);

                    Logger.LogDebug($"Old CoverPoint is still good for {BotOwner.name}");
                    return true;
                }
                else
                {
                    FallBackCoverPoint = null;
                }
            }

            if (FallBackCoverPoint == null && CoverFinder.FindCover(out CustomCoverPoint coverPoint))
            {
                FallBackCoverPoint = coverPoint;
                DebugDrawFallback(debugMode);
                StartFallback(coverPoint.CoverPosition);

                Logger.LogDebug($"Found New CoverPoint for {BotOwner.name}");
                return true;
            }
            else
            {
                return false;
            }
        }

        public CustomCoverPoint FallBackCoverPoint { get; private set; }

        private void StartFallback(Vector3 coverPosition)
        {
            FallingBack = true;

            if (HasStamina)
            {
                BotOwner.Steering.LookToMovingDirection();
                SetSprint(HasStamina);
            }

            BotOwner.GoToPoint(coverPosition, true, -1, false, true, true);
            UpdateDoorOpener();
        }

        private void DebugDrawFallback(bool debugMode)
        {
            if (debugMode)
            {
                DebugDrawer.Line(FallBackCoverPoint.CoverPosition, BotOwner.MyHead.position, 0.1f, Color.green, 3f);
                DebugDrawer.Line(BotOwner.Memory.GoalEnemy.Owner.MyHead.position, BotOwner.MyHead.position, 0.1f, Color.green, 3f);
                DebugDrawer.Line(BotOwner.Memory.GoalEnemy.Owner.MyHead.position, FallBackCoverPoint.CoverPosition, 0.1f, Color.green, 3f);
                Logger.LogDebug($"{BotOwner.name} is falling back while reloading");
            }
        }

        private void DecidePoseFromCoverLevel()
        {
            SelfCover = CoverFinder.Analyzer.AnalyseCoverPosition(BotOwner.Transform.position);

            if (SelfCover != null)
            {
                if (SelfCover.CoverLevel > 0.9)
                {
                    BotOwner.SetPose(1f);
                }
                else if (SelfCover.CoverLevel > 0.75)
                {
                    BotOwner.SetPose(0.66f);
                }
                else if (SelfCover.CoverLevel > 0.5)
                {
                    BotOwner.SetPose(0.25f);
                }
                else if (SelfCover.CoverLevel > 0.25)
                {
                    BotOwner.SetPose(0f);
                }
                else
                {
                    BotOwner.SetPose(0.9f);
                    BotNeedsToBackup();
                }
            }
            else
            {
                BotOwner.SetPose(0.9f);
                BotNeedsToBackup();
            }
        }

        private CustomCoverPoint SelfCover;

        private bool BotNeedsToBackup()
        {
            if (CheckEnemyDistance(out Vector3 trgPos))
            {
                Vector3 DodgeFallBack = Dodge.FindArcPoint(trgPos, BotOwner.Memory.GoalEnemy.CurrPosition, 2f, 10f, 1f, 2f);
                if (NavMesh.SamplePosition(DodgeFallBack, out NavMeshHit hit, 10f, -1))
                {
                    trgPos = hit.position;
                }

                BotOwner.GoToPoint(trgPos, false, -1, false, false, true);
                UpdateDoorOpener();

                return true;
            }
            return false;
        }

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
                navMeshPath_0.ClearCorners();
                if (NavMesh.CalculatePath(BotOwner.Position, trgPos, -1, navMeshPath_0) && navMeshPath_0.status == NavMeshPathStatus.PathComplete)
                {
                    trgPos = navMeshPath_0.corners[navMeshPath_0.corners.Length - 1];

                    return CheckStraightDistance(navMeshPath_0, num);
                }
            }
            return false;
        }

        private bool CheckStraightDistance(NavMeshPath path, float straighDist)
        {
            return path.CalculatePathLength() < straighDist * 1.2f;
        }

        private void UpdateDoorOpener()
        {
            BotOwner.DoorOpener.Update();
        }

        private void SetSprint(bool value)
        {
            if (value)
            {
                BotOwner.GetPlayer.EnableSprint(true);
                BotOwner.Sprint(true);
                BotOwner.SetPose(1f);
                BotOwner.SetTargetMoveSpeed(1f);
            }
            else
            {
                BotOwner.GetPlayer.EnableSprint(false);
                BotOwner.Sprint(false);
            }
        }

        private void FullSpeed()
        {
            BotOwner.SetTargetMoveSpeed(1f);
        }

        private void CrouchWalk()
        {
            BotOwner.SetPose(0f);
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
                LastDistanceCheck = Time.time + 0.5f;
                NavMesh.CalculatePath(BotOwner.Transform.position, BotOwner.Memory.GoalEnemy.CurrPosition, -1, Path);
                PathLength = Path.CalculatePathLength();
            }
        }

        private NavMeshPath Path = new NavMeshPath();
        private float LastDistanceCheck = 0f;
        private float PathLength = 0f;

        private NavMeshPath navMeshPath_0 = new NavMeshPath();
        private readonly BotOwner BotOwner;
        private readonly ManualLogSource Logger;
        private readonly BotDodge Dodge;
        private readonly CoverFinder CoverFinder;
        private float DodgeTimer = 0f;
        private float ReactionTimer = 0f;
    }
}