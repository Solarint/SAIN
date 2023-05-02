using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using Movement.Helpers;
using SAIN_Helpers;
using UnityEngine;
using UnityEngine.AI;
using static SAIN_Helpers.DebugDrawer;
using static SAIN_Helpers.SAIN_Math;

namespace SAIN.Movement.Layers.DogFight
{
    internal class UpdateMove
    {
        private readonly BotOwner BotOwner;

        protected ManualLogSource Logger;

        private readonly BotDodge Dodge;

        private readonly CoverChecker Cover;

        private float DodgeTimer = 0f;

        private bool MovingToEnemy = false;

        public UpdateMove(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = bot;
            Dodge = new BotDodge(bot);
            Cover = new CoverChecker(bot);
        }

        public void Update()
        {
            if (!MovingToEnemy)
            {
                FullSpeed();
            }

            if (BotOwner.Memory.GoalEnemy.CanShoot)
            {
                MovingToEnemy = false;
                return;
            }

            // If we have a target position, and we're already there, clear it
            if (targetPos != null && (targetPos.Value - BotOwner.Position).sqrMagnitude < 4f)
            {
                targetPos = null;
            }

            if (targetPos == null && BotOwner.WeaponManager.Reload.Reloading)
            {
                targetPos = FindFallbackPosition();

                if (targetPos == null)
                {
                    Logger.LogWarning($"Unable to find a location for {BotOwner.name}");
                }
                else
                {
                    Cover.AnalyseCoverPosition(targetPos.Value);

                    Logger.LogDebug("FLEE YOU FOOL!");

                    BotOwner.Steering.LookToMovingDirection();

                    BotOwner.GoToPoint(targetPos.Value, true, -1f, false, true, true);

                    SetToSprint();

                    UpdateDoorOpener();

                    return;
                }
            }

            if (BotOwner.Memory.GoalEnemy.CanShoot && DodgeTimer < Time.time)
            {
                DodgeTimer = Time.time + 0.5f;

                FullSpeed();
                Dodge.Execute();
                return;
            }

            MovingToEnemy = true;

            BotOwner.MoveToEnemyData.TryMoveToEnemy(BotOwner.Memory.GoalEnemy.CurrPosition);
        }

        private Vector3? targetPos = null;

        private Vector3? FindFallbackPosition()
        {
            // If we don't have a target position yet, pick one
            int i = 0;
            while (targetPos == null && i < 100)
            {
                Vector3 randomPos = UnityEngine.Random.insideUnitSphere * 20f;
                randomPos += BotOwner.Position;
                if (NavMesh.SamplePosition(randomPos, out var navHit, 100f, NavMesh.AllAreas))
                {
                    // Debug
                    Ray(navHit.position, Vector3.up, 3f, 0.1f, Color.white, 3f);

                    targetPos = navHit.position;

                    Vector3 RunPosition = navHit.position;
                    RunPosition.y += BotOwner.MyHead.position.y;

                    Vector3 direction = BotOwner.Memory.GoalEnemy.CurrPosition - RunPosition;
                    float distance = Vector3.Distance(RunPosition, BotOwner.Memory.GoalEnemy.CurrPosition);

                    Ray visionCheck = new Ray(RunPosition, direction);

                    if (Physics.Raycast(visionCheck, out RaycastHit hit, distance, LayerMaskClass.HighPolyWithTerrainMask) && hit.transform != BotOwner.Memory.GoalEnemy.Person.GetPlayer.gameObject.transform)
                    {
                        return navHit.position;
                    }
                    else
                    {
                        return null;
                    }
                }

                i++;
            }

            return null;
        }

        protected bool UpdateDoorOpener()
        {
            return BotOwner.DoorOpener.Update();
        }

        protected void SetToSprint()
        {
            BotOwner.Sprint(true);
            BotOwner.SetPose(1f);
            BotOwner.SetTargetMoveSpeed(1f);
        }

        protected void FullSpeed()
        {
            BotOwner.Sprint(false);
            BotOwner.SetPose(1f);
            BotOwner.SetTargetMoveSpeed(1f);
        }

        protected void CrouchWalk()
        {
            BotOwner.Sprint(false);
            BotOwner.SetPose(0f);
            BotOwner.SetTargetMoveSpeed(1f);
        }
    }
}