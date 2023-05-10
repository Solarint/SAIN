using BepInEx.Logging;
using EFT;
using Movement.Classes;
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
            MovementSpeed = new MovementSpeed(bot);
        }

        private readonly MovementSpeed MovementSpeed; 
        private readonly BotDodge Dodge;
        private readonly CoverSystem Cover;
        private const float UpdateFrequency = 0.1f;

        public void ManualUpdate(bool canSee, bool canShoot)
        {
            CanSeeEnemy = canSee;
            CanShootEnemy = canShoot;

            if (canSee)
            {
                EnemyLastSeenTime = Time.time;
            }

            MovementSpeed.ManualUpdate(canShoot, canSee, EnemyLastSeenTime);

            Cover.ManualUpdate(canShoot, canSee);

            if (ReactionTimer < Time.time)
            {
                UpdateDoorOpener();

                ReactionTimer = Time.time + UpdateFrequency;

                if (!GoalEnemyNull)
                {
                    if (CheckRunAway)
                    {
                        return;
                    }

                    MovementSpeed.DecideMovementSpeed();

                    if (Cover.TakeCover())
                    {
                        return;
                    }

                    MovementSpeed.SetSprint(false);

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
                    var coverPoint = Cover.CoverFinderNew.FallBackPoint;

                    if (coverPoint != null)
                    {
                        FallBack(coverPoint.CoverPosition);
                    }
                    else if (DodgeTimer < Time.time)
                    {
                        DodgeTimer = Time.time + Random.Range(0.5f, 1f);
                        Dodge.Execute();
                    }
                    return true;
                }
                return false;
            }
        }

        private void FallBack(Vector3 position)
        {
            MovementSpeed.SetSprint(true);
            BotOwner.Steering.LookToMovingDirection();
            BotOwner.GoToPoint(position, false);
            UpdateDoorOpener();
        }

        private void UpdateDoorOpener()
        {
            BotOwner.DoorOpener.Update();
        }

        private bool CanShootEnemy;
        private bool CanSeeEnemy;

        private bool Reloading => BotOwner.WeaponManager != null && BotOwner.WeaponManager.Reload.Reloading;
        private bool CanShootEnemyAndVisible => !GoalEnemyNull && CanShootEnemy && CanSeeEnemy;
        private bool GoalEnemyNull => BotOwner.Memory.GoalEnemy == null;
        private bool HasStamina => BotOwner.GetPlayer.Physical.Stamina.NormalValue > 0f;
        private bool DebugMode => DebugUpdateMove.Value;
        public LeanComponent DynamicLean { get; private set; }

        private float DodgeTimer = 0f;
        private float ReactionTimer = 0f;
        private float EnemyLastSeenTime;
        private float LastDistanceCheck = 0f;
        private float PathLength = 0f;
        public CustomCoverPoint FallBackCoverPoint;
        private NavMeshPath BotPath = new NavMeshPath();
        private readonly BotOwner BotOwner;
        private readonly ManualLogSource Logger;
    }

    public class MovementSpeed
    {
        public MovementSpeed(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = bot;
        }

        public void ManualUpdate(bool canShoot, bool canSee, float lastSeen)
        {
            CanShootEnemy = canShoot;
            CanSeeEnemy = canSee;
            EnemyLastSeenTime = lastSeen;
        }

        private readonly BotOwner BotOwner;
        private readonly ManualLogSource Logger;

        public void DecideMovementSpeed()
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

        public void SetSprint(bool value)
        {
            if (value)
            {
                BotOwner.SetPose(1f);
                BotOwner.SetTargetMoveSpeed(1f);
            }
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
            BotOwner.SetTargetMoveSpeed(0.45f);
        }

        private void NormalSpeed()
        {
            BotOwner.SetTargetMoveSpeed(0.85f);
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
        private bool CanShootEnemyAndVisible => !GoalEnemyNull && CanShootEnemy && CanSeeEnemy;
        private bool GoalEnemyNull => BotOwner.Memory.GoalEnemy == null;
        private bool HasStamina => BotOwner.GetPlayer.Physical.Stamina.NormalValue > 0f;
        public bool IsMoving => BotOwner.Mover.IsMoving;

        private float LastDistanceCheck = 0f;
        private float PathLength = 0f;
        private NavMeshPath BotPath = new NavMeshPath();
        private bool CanShootEnemy;
        private bool CanSeeEnemy;
        private float EnemyLastSeenTime;
    }

    public class CoverSystem
    {
        public CoverSystem(BotOwner botOwner)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = botOwner;
            Dodge = new BotDodge(botOwner);
            DynamicLean = botOwner.gameObject.GetComponent<LeanComponent>();
            CoverFinderNew = botOwner.gameObject.GetComponent<CoverFinderComponent>();
        }

        public void ManualUpdate(bool CanShoot, bool CanSee)
        {
            CanShootEnemy = CanShoot;
            CanSeeEnemy = CanSee;
        }

        public bool CheckSelfForCover()
        {
            if (SelfCover != null && SelfCoverCheckTime < Time.time)
            {
                SelfCoverCheckTime = Time.time + 0.5f;

                if (CoverFinderNew.Analyzer.CheckPosition(BotOwner.Memory.GoalEnemy.CurrPosition, BotOwner.Transform.position, out SelfCover, 0.33f))
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
                    DynamicLean.HoldLean = true;
                    DecidePoseFromCoverLevel(SelfCover);
                }
                else if (CoverFinderNew.SafeCoverPoints.Count > 0)
                {
                    BotOwner.GoToPoint(CoverFinderNew.SafeCoverPoints[0].Position, false);
                }
                else if (!CanBotBackUp() && CanShootEnemy)
                {
                    Dodge.Execute();
                }
                else
                {
                    BotOwner.SetPose(0f);
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
        }

        public bool CanBotBackUp()
        {
            if (FightCover != null)
            {
                if (CoverFinderNew.Analyzer.CheckPosition(BotOwner.Memory.GoalEnemy.CurrPosition, FightCover.CoverPosition, out FightCover, 0.25f))
                {
                    BotOwner.GoToPoint(FightCover.CoverPosition, false);
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

                Vector3 DodgeFallBack = HelperClasses.FindArcPoint(BotOwner.Transform.position, BotOwner.Memory.GoalEnemy.CurrPosition, currentRange, currentAngle);
                if (NavMesh.SamplePosition(DodgeFallBack, out NavMeshHit hit, 5f, -1))
                {
                    if (CoverFinderNew.Analyzer.CheckPosition(BotOwner.Memory.GoalEnemy.CurrPosition, hit.position, out FightCover, 0.25f))
                    {
                        if (BotOwner.GoToPoint(hit.position, false) == NavMeshPathStatus.PathComplete)
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
        private float SelfCoverCheckTime = 0f;
        private readonly ManualLogSource Logger;
        private readonly BotOwner BotOwner;
        private CustomCoverPoint SelfCover;
        public CustomCoverPoint FallBackCoverPoint;
        private readonly BotDodge Dodge;
        public CoverFinderComponent CoverFinderNew { get; private set; }
    }
}