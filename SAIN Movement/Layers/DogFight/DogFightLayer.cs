using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using UnityEngine;
using UnityEngine.AI;
using static SAIN_Helpers.DebugDrawer;
using static SAIN_Helpers.SAIN_Math;
using static Movement.UserSettings.Debug;
using SAIN_Helpers;

namespace SAIN.Movement.Layers
{
    namespace DogFight
    {
        internal class DogFightLayer : CustomLayer
        {
            private readonly float DogFightIn = 25f;

            private readonly float DogFightOut = 30f;

            protected ManualLogSource Logger;

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
                if (BotOwner.Memory.GoalEnemy == null || BotOwner.BewareGrenade?.GrenadeDangerPoint?.Grenade != null || EnemyFar)
                {
                    isActive = false;
                    return false;
                }

                CheckPathLength();

                if (PathLength < DogFightIn)
                {
                    isActive = true;
                }
                else if (PathLength > DogFightOut)
                {
                    isActive = false;
                }

                if (isActive)
                {
                    DebugDrawPath();
                }

                return isActive;
            }

            public override Action GetNextAction()
            {
                Logger.LogInfo($"Called DogFight GetAction for {BotOwner.name}");
                return new Action(typeof(DogFightLogic), "DogFight");
            }

            public override bool IsCurrentActionEnding()
            {
                return false;
            }

            private void DebugDrawPath()
            {
                if (DebugDogFightLayerDraw.Value && DebugTimer < Time.time)
                {
                    DebugTimer = Time.time + 1f;
                    for (int i = 0; i < Path.corners.Length - 1; i++)
                    {
                        Vector3 corner1 = Path.corners[i];
                        Vector3 corner2 = Path.corners[i + 1];
                        Line(corner1, corner2, 0.025f, Color.white, 1f);
                    }
                }
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

            private float DebugTimer = 0f;
            private NavMeshPath Path = new NavMeshPath();
            private float LastDistanceCheck = 0f;
            private float PathLength = 0f;

            private bool EnemyFar => EnemyDistance > 50f;
            private float EnemyDistance => Vector3.Distance(BotOwner.Transform.position, BotOwner.Memory.GoalEnemy.CurrPosition);
        }
    }
}