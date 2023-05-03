using BepInEx.Logging;
using EFT;
using Movement.Helpers;
using UnityEngine;
using UnityEngine.AI;
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
        }

        public void Update(bool debug = false, bool debugDrawAll = false)
        {
            if (ReactionTimer < Time.time)
            {
                ReactionTimer = Time.time + 0.25f;
                UpdateDoorOpener();

                if (CanSeeEnemy)
                {
                    EnemyLastSeenTime = Time.time;
                    LastEnemyPosition = BotOwner.Memory.GoalEnemy.CurrPosition;
                }
                else if (BotIsAtLastEnemyPosition)
                {
                    LastEnemyPosition = null;
                }

                if (CheckFallBackConditions(debug, debugDrawAll))
                {
                    SetSprint(HasStamina);
                    return;
                }
                else
                {
                    SetSprint(false);
                }

                if (CanShootEnemyAndVisible)
                {
                    FullSpeed();
                    if (DodgeTimer < Time.time)
                    {
                        DodgeTimer = Time.time + 1f;
                        Dodge.Execute();
                    }
                    else if (CheckEnemyDistance(out Vector3 trgPos))
                    {
                        Vector3 DodgeFallBack = Dodge.FindArcPoint(BotOwner.Transform.position, BotOwner.Memory.GoalEnemy.CurrPosition, 3f, 30f, 1f, 3f);
                        DodgeFallBack -= BotOwner.Transform.position;
                        trgPos += DodgeFallBack;

                        BotOwner.GoToPoint(trgPos, false, 1f);
                        UpdateDoorOpener();
                    }
                    return;
                }
                else
                {
                    DecideMovementSpeed();

                    if (!BotIsAtLastEnemyPosition && LastEnemyPosition != null)
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
            if (ShouldISneak)
            {
                Sneak();
            }
            else if (!IsEnemyClose)
            {
                SlowWalk();
            }
            else if (IsEnemyVeryClose)
            {
                FullSpeed();
            }
            else
            {
                NormalSpeed();
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
        public bool BotIsAtTargetPosition => !NoTargetPosition && Vector3.Distance(targetPos.Value, BotOwner.Transform.position) < 2f;
        public bool BotIsAtLastEnemyPosition => Vector3.Distance(LastEnemyPosition.Value, BotOwner.Transform.position) < 2f;
        public bool Reloading => BotOwner.WeaponManager.Reload.Reloading;
        public bool NoTargetPosition => targetPos == null;
        public bool CanShootEnemyAndVisible => CanShootEnemy && CanSeeEnemy;
        public bool CanShootEnemy => BotOwner.Memory.GoalEnemy.IsVisible;
        public bool CanSeeEnemy => BotOwner.Memory.GoalEnemy.CanShoot;
        public bool ShouldISneak => !CanShootEnemyAndVisible && EnemyLastSeenTime + 10f < Time.time;
        private bool HasStamina => BotOwner.GetPlayer.Physical.Stamina.NormalValue > 0f;

        private bool CheckFallBackConditions(bool debugMode = false, bool debugDrawAll = false)
        {
            if (!Reloading) return false;

            // If we have a target position, and we're already there, clear it
            if (BotIsAtTargetPosition) ResetTarget();

            if (NoTargetPosition)
            {
                if (CoverFinder.FindFallbackPosition(out Vector3? coverPosition, debugMode, debugDrawAll))
                {
                    targetPos = coverPosition;
                    FallingBack = true;

                    if (debugMode) Logger.LogDebug($"{BotOwner.name} is falling back while reloading");

                    BotOwner.GoToPoint(targetPos.Value, false, 1f);
                    UpdateDoorOpener();

                    return true;
                }
                else
                {
                    ResetTarget();
                    return false;
                }
            }

            return true;
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

        private void ResetTarget()
        {
            FallingBack = false;
            targetPos = null;
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
            BotOwner.Sprint(false);
            BotOwner.SetPose(1f);
            BotOwner.SetTargetMoveSpeed(1f);
        }

        private void CrouchWalk()
        {
            BotOwner.Sprint(false);
            BotOwner.SetPose(0f);
            BotOwner.SetTargetMoveSpeed(1f);
        }

        private void Sneak()
        {
            BotOwner.Sprint(false);
            BotOwner.SetPose(0f);
            BotOwner.SetTargetMoveSpeed(0f);
        }

        private void SlowWalk()
        {
            BotOwner.Sprint(false);
            BotOwner.SetPose(0.75f);
            BotOwner.SetTargetMoveSpeed(0.33f);
        }

        private void NormalSpeed()
        {
            BotOwner.Sprint(false);
            BotOwner.SetPose(1f);
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
        private Vector3? targetPos = null;
        private float ReactionTimer = 0f;
    }
}