using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using UnityEngine;
using UnityEngine.AI;
using static SAIN_Helpers.SAIN_Math;
using static SAIN_Helpers.DebugDrawer;

namespace SAIN.Movement.Layers
{
    internal class DogFightLayer : CustomLayer
    {
        protected ManualLogSource Logger;
        protected float nextRoamCheckTime = 0f;
        protected bool isActive = false;

        public DogFightLayer(BotOwner botOwner, int priority) : base(botOwner, priority)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            Logger.LogInfo($"Added SAIN DogFight to {botOwner.name}");
        }

        public override string GetName()
        {
            return "SAIN DogFight";
        }

        public override bool IsActive()
        {
            if (BotOwner.Memory.IsPeace || BotOwner.Memory.GoalEnemy == null)
            {
                isActive = false;
                return false;
            }

            if (BotOwner.Memory.GoalEnemy.Distance > DogFightOut)
            {
                isActive = false;
                return false;
            }

            if (BotOwner.BewareGrenade?.GrenadeDangerPoint?.Grenade != null)
            {
                isActive = false;
                return false;
            }

            // If we're active already, then stay active
            if (isActive)
            {
                return true;
            }

            if (BotOwner.Memory.GoalEnemy.Distance < DogFightIn)
            {
                if (CheckEnemyDistance(out Vector3 vector))
                {
                    Logger.LogDebug("  DOGFIGHT");
                    isActive = true;
                    return true;
                }
                else
                {
                    Logger.LogDebug("  No Dogfight");
                }
            }

            return false;
        }

        public override Action GetNextAction()
        {
            Logger.LogInfo($"Called DogFight GetAction for {BotOwner.name}");
            return new Action(typeof(DogFightLogic), "DogFight Mode Engaged");
        }

        public override bool IsCurrentActionEnding()
        {
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

                        Sphere(trgPos, 0.15f, Color.yellow, 1f);

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

                    Sphere(trgPos, 0.25f, Color.white, 1f);
                    Line(trgPos, BotOwner.Transform.position, 0.05f, Color.white, 1f);

                    return CheckStraightDistance(navMeshPath_0, num);
                }
            }
            return false;
        }

        private bool CheckStraightDistance(NavMeshPath path, float straighDist)
        {
            return path.CalculatePathLength() < straighDist * 1.2f;
        }

        private readonly NavMeshPath navMeshPath_0 = new NavMeshPath();
        private static readonly float DogFightIn = 25f;
        private static readonly float DogFightOut = 30f;
    }
}