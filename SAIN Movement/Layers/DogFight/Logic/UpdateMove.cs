using BepInEx.Logging;
using EFT;
using Movement.Helpers;
using UnityEngine;
using UnityEngine.AI;
using static HairRenderer;

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

                if (CanSeeEnemy)
                {
                    EnemyLastSeenTime = Time.time;
                    LastEnemyPosition = BotOwner.Memory.GoalEnemy.CurrPosition;
                }

                if (CheckFallBackConditions(debug, debugDrawAll))
                {
                    SetSprint(true);
                    return;
                }
                else SetSprint(false);

                UpdateDoorOpener();

                if (CanShootEnemyAndVisible)
                {
                    if (DodgeTimer < Time.time)
                    {
                        FullSpeed();
                        DodgeTimer = Time.time + 1f;
                        Dodge.Execute();
                    }
                    return;
                }
                else
                {
                    if (ShouldISneak) Sneak();
                    else if (!IsEnemyClose) SlowWalk();
                    else if (IsEnemyVeryClose) FullSpeed();
                    else NormalSpeed();

                    if (!BotIsAtLastEnemyPosition) BotOwner.MoveToEnemyData.TryMoveToEnemy(LastEnemyPosition);
                    else BotOwner.MoveToEnemyData.TryMoveToEnemy(BotOwner.Memory.GoalEnemy.CurrPosition);
                }
            }
        }

        public bool FallingBack { get; private set; }
        public Vector3 LastEnemyPosition { get; private set; }
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
        public bool BotIsAtLastEnemyPosition => Vector3.Distance(LastEnemyPosition, BotOwner.Transform.position) < 2f;
        public bool Reloading => BotOwner.WeaponManager.Reload.Reloading;
        public bool NoTargetPosition => targetPos == null;
        public bool CanShootEnemyAndVisible => CanShootEnemy && CanSeeEnemy;
        public bool CanShootEnemy => BotOwner.Memory.GoalEnemy.IsVisible;
        public bool CanSeeEnemy => BotOwner.Memory.GoalEnemy.CanShoot;
        public bool ShouldISneak => !CanShootEnemyAndVisible && EnemyLastSeenTime + 10f < Time.time;

        private bool CheckFallBackConditions(bool debugMode = false, bool debugDrawAll = false)
        {
            // If we have a target position, and we're already there, clear it
            if (BotIsAtTargetPosition || !Reloading)
            {
                ResetTarget();
                return false;
            }

            if (NoTargetPosition)
            {
                if (CoverFinder.FindFallbackPosition(out Vector3? coverPosition, debugMode, debugDrawAll))
                {
                    targetPos = coverPosition;
                    FallingBack = true;

                    if (debugMode)
                        Logger.LogDebug($"{BotOwner.name} is falling back while reloading");

                    BotOwner.GoToPoint(targetPos.Value, true, -1f, false, true, true);

                    UpdateDoorOpener();

                    return true;
                }
            }

            if (!NoTargetPosition) return true;

            return false;
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

        private readonly BotOwner BotOwner;
        private readonly ManualLogSource Logger;
        private readonly BotDodge Dodge;
        private readonly CoverFinder CoverFinder;
        private float DodgeTimer = 0f;
        private Vector3? targetPos = null;
        private NavMeshPath Path = new NavMeshPath();
        private float LastDistanceCheck = 0f;
        private float PathLength = 0f;
        private float ReactionTimer = 0f;
    }
}