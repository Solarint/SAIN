using BepInEx.Logging;
using EFT;
using Movement.Components;
using Movement.Helpers;
using SAIN_Helpers;
using UnityEngine;
using UnityEngine.AI;
using static Movement.UserSettings.Debug;

namespace SAIN.Movement.Layers.DogFight
{
    internal class UpdateMove
    {
        public UpdateMove(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = bot;
            Cover = new CoverSystem(bot);
            Dodge = new BotDodge(bot);
        }

        private readonly BotDodge Dodge;
        private readonly CoverSystem Cover;
        private const float UpdateFrequency = 0.1f;

        public void Update(bool canSee, bool canShoot)
        {
            CanSeeEnemy = canSee;
            CanShootEnemy = canShoot;

            Cover.Update(canShoot, canSee);

            if (ReactionTimer < Time.time)
            {
                if (CanShootEnemy && CanSeeEnemy)
                {
                    EnemyLastSeenTime = Time.time;
                }

                UpdateDoorOpener();

                ReactionTimer = Time.time + UpdateFrequency;

                if (!GoalEnemyNull)
                {
                    if (CheckRunAway)
                    {
                        return;
                    }

                    DecideMovementSpeed();

                    if (Cover.TakeCover())
                    {
                        return;
                    }

                    BotOwner.MoveToEnemyData.TryMoveToEnemy(BotOwner.Memory.GoalEnemy.CurrPosition);
                }
            }
        }

        private bool CheckRunAway
        {
            get
            {
                if (Reloading || BotOwner.Medecine.FirstAid.Using || !BotOwner.WeaponManager.HaveBullets)
                {
                    FallingBack = true;

                    var coverPoint = Cover.RunAway();

                    if (coverPoint != null)
                    {
                        SetSprint(true);
                        FallBack(coverPoint.CoverPosition);
                    }
                    else
                    {
                        Dodge.Execute();
                    }
                    return true;
                }
                return false;
            }
        }

        private void DecideMovementSpeed()
        {
            if (GoalEnemyNull)
            {
                SlowWalk();
                return;
            }

            if (IsTargetVeryClose)
            {
                FullSpeed();
            }
            else if (IsTargetClose)
            {
                NormalSpeed();
            }
            else if (ShouldBotSneak)
            {
                Sneak();
            }
            else
            {
                SlowWalk();
            }
        }

        private void FallBack(Vector3 coverPosition)
        {
            if (HasStamina)
            {
                BotOwner.Steering.LookToMovingDirection();
                SetSprint(true);
            }

            BotOwner.GoToPoint(coverPosition, false, -1, false, true, true);
            UpdateDoorOpener();
        }

        /// <summary>
        /// Sets the sprint of the Player to true or false.
        /// </summary>
        /// <param name="value">The value to set the sprint to.</param>
        private void SetSprint(bool value)
        {
            BotOwner.SetPose(1f);
            BotOwner.SetTargetMoveSpeed(1f);
            BotOwner.GetPlayer.EnableSprint(value);
            BotOwner.Sprint(value);
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

        private void UpdateDoorOpener()
        {
            BotOwner.DoorOpener.Update();
        }

        private bool CanShootEnemy;
        private bool CanSeeEnemy;

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
                    return !CanShootEnemyAndVisible && EnemyLastSeenTime < Time.time - 10f;
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

        private bool Reloading => BotOwner.WeaponManager != null && BotOwner.WeaponManager.Reload.Reloading;
        private bool CanShootEnemyAndVisible => !GoalEnemyNull && CanShootEnemy && CanSeeEnemy;
        private bool GoalEnemyNull => BotOwner.Memory.GoalEnemy == null;
        private bool HasStamina => BotOwner.GetPlayer.Physical.Stamina.NormalValue > 0f;
        public bool IsSprintingFallback => FallingBack && BotOwner.Mover.IsMoving;
        private bool DebugMode => DebugUpdateMove.Value;
        public LeanComponent DynamicLean { get; private set; }

        private bool FallingBack;
        private float ReactionTimer = 0f;
        private float EnemyLastSeenTime;
        private float LastDistanceCheck = 0f;
        private float PathLength = 0f;
        public CustomCoverPoint FallBackCoverPoint;
        private NavMeshPath BotBackupPath = new NavMeshPath();
        private NavMeshPath BotPath = new NavMeshPath();
        private readonly BotOwner BotOwner;
        private readonly ManualLogSource Logger;
    }

    public class CoverSystem
    {
        public CoverSystem(BotOwner botOwner)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = botOwner;
            Dodge = new BotDodge(botOwner);
            CoverFinder = new CoverFinder(botOwner);
            DynamicLean = botOwner.gameObject.GetComponent<LeanComponent>();
            CoverFinderNew = botOwner.gameObject.GetComponent<CoverFinderComponent>();
        }

        public void Update(bool CanShoot, bool CanSee)
        {
            CanShootEnemy = CanShoot;
            CanSeeEnemy = CanSee;
        }

        public bool CheckSelfForCover()
        {
            if (SelfCover != null && SelfCoverCheckTime < Time.time)
            {
                SelfCoverCheckTime = Time.time + 0.5f;

                if (CoverFinder.Analyzer.CheckPosition(BotOwner.Memory.GoalEnemy.CurrPosition, BotOwner.Transform.position, out SelfCover, 0.33f))
                {
                    return true;
                }
            }
            return false;
        }

        public void DebugDrawFallback()
        {
            if (DebugMode)
            {
                DebugDrawer.Line(FallBackCoverPoint.CoverPosition, BotOwner.MyHead.position, 0.1f, Color.green, 3f);
                DebugDrawer.Line(BotOwner.Memory.GoalEnemy.Owner.MyHead.position, BotOwner.MyHead.position, 0.1f, Color.green, 3f);
                DebugDrawer.Line(BotOwner.Memory.GoalEnemy.Owner.MyHead.position, FallBackCoverPoint.CoverPosition, 0.1f, Color.green, 3f);
            }
        }

        public bool TakeCover()
        {
            if (CanSeeEnemy)
            {
                if (CheckSelfForCover() && CanShootEnemy)
                {
                    DecidePoseFromCoverLevel(SelfCover);
                }
                else if (!CanBotBackUp() && CanShootEnemy)
                {
                    Dodge.Execute();
                }
                else
                {
                    var point = CoverFinderNew.CloseCoverPoints.PickRandom();
                    BotOwner.GoToPoint(point.Position);
                }
                return true;
            }
            return false;
        }

        public void DecidePoseFromCoverLevel(CustomCoverPoint cover)
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

                BotOwner.SetPose(poseLevel);
            }
            else
            {
                BotOwner.SetPose(1f);
                CanBotBackUp();
            }
        }

        public CustomCoverPoint RunAway(float minCover = 0.75f)
        {
            if (FallBackCoverPoint != null && RecheckCoverTime < Time.time)
            {
                RecheckCoverTime = Time.time + 0.25f;
                if (!RecheckCoverPosition)
                {
                    FallBackCoverPoint = null;
                }
            }
            if (FallBackCoverPoint == null && CoverFinder.FindCover(out CustomCoverPoint coverPoint, BotOwner.Memory.GoalEnemy.CurrPosition, minCover))
            {
                FallBackCoverPoint = coverPoint;
                DebugDrawFallback();
            }

            return FallBackCoverPoint;
        }

        private bool RecheckCoverPosition => CoverFinder.Analyzer.CheckPosition(BotOwner.Memory.GoalEnemy.CurrPosition, FallBackCoverPoint.CoverPosition, out CustomCoverPoint cover, 0.66f);

        public bool CanBotBackUp()
        {
            if (FightCover != null)
            {
                if (CoverFinder.Analyzer.CheckPosition(BotOwner.Memory.GoalEnemy.CurrPosition, FightCover.CoverPosition, out FightCover, 0.25f))
                {
                    BotOwner.GoToPoint(FightCover.CoverPosition, false, -1, false, true, true);
                    UpdateDoorOpener();
                    return true;
                }
            }

            const float angleStep = 15f;
            const float rangeStep = 2f;
            const int max = 10;
            int i = 0;
            while (i < max)
            {
                float angleAdd = angleStep * i;
                float currentAngle = UnityEngine.Random.Range(-5f - angleAdd, 5f + angleAdd);
                float currentRange = rangeStep * i + 2f;

                Vector3 DodgeFallBack = CoverFinder.FindArcPoint(BotOwner.Transform.position, BotOwner.Memory.GoalEnemy.CurrPosition, currentRange, currentAngle);
                if (NavMesh.SamplePosition(DodgeFallBack, out NavMeshHit hit, 2f, -1))
                {
                    if (CoverFinder.Analyzer.CheckPosition(BotOwner.Memory.GoalEnemy.CurrPosition, hit.position, out FightCover, 0.25f))
                    {
                        if (BotOwner.GoToPoint(hit.position, false, -1, false, false, true) == NavMeshPathStatus.PathComplete)
                        {
                            UpdateDoorOpener();
                            return true;
                        }
                    }
                }

                i++;
            }
            return false;
        }

        private CustomCoverPoint FightCover;

        private void UpdateDoorOpener()
        {
            BotOwner.DoorOpener.Update();
        }

        public LeanComponent DynamicLean { get; private set; }
        private bool CanShootEnemy;
        private bool CanSeeEnemy;
        private bool DebugMode => DebugUpdateMove.Value;
        private readonly CoverFinder CoverFinder;
        private float RecheckCoverTime = 0f;
        private float SelfCoverCheckTime = 0f;
        private readonly ManualLogSource Logger;
        private readonly BotOwner BotOwner;
        private CustomCoverPoint SelfCover;
        public CustomCoverPoint FallBackCoverPoint;
        private readonly BotDodge Dodge;
        private CoverFinderComponent CoverFinderNew;
    }
}