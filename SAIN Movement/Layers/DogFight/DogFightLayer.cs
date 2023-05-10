using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using UnityEngine;
using UnityEngine.AI;
using static SAIN_Helpers.DebugDrawer;
using static SAIN_Helpers.SAIN_Math;
using static Movement.UserSettings.DebugConfig;
using SAIN_Helpers;

namespace SAIN.Movement.Layers
{
    namespace DogFight
    {
        internal class DogFightLayer : CustomLayer
        {
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
                if (BotOwner.BewareGrenade?.GrenadeDangerPoint?.Grenade != null)
                {
                    isActive = false;
                    return false;
                }

                CheckForDanger();

                if (TargetPosition != null && TargetPosition.HasValue && !TargetFar)
                {
                    CheckPathLength();

                    if (PathLength < DogFightIn)
                    {
                        isActive = true;
                    }
                    else if (PathLength > DogFightOut)
                    {
                        isActive = false;
                    }

                    DebugDrawPath();

                    return isActive;
                }

                return false;
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

            private void CheckForDanger()
            {
                if (BotOwner.Memory.GoalEnemy != null)
                {
                    TargetPosition = BotOwner.Memory.GoalEnemy.CurrPosition;
                }
                else if (BotOwner.Memory.GoalTarget != null)
                {
                    if (BotOwner.Memory.GoalTarget.IsDanger)
                    {
                        TargetPosition = BotOwner.Memory.GoalTarget.Position;
                    }
                    else
                    {
                        TargetPosition = null;
                    }
                }
                else
                {
                    TargetPosition = null;
                }
            }

            private void DebugDrawPath()
            {
                if (isActive && DebugMode && DebugTimer < Time.time)
                {
                    DebugTimer = Time.time + 1f;
                    for (int i = 0; i < Path.corners.Length - 1; i++)
                    {
                        Vector3 corner1 = Path.corners[i];
                        Vector3 corner2 = Path.corners[i + 1];
                        Line(corner1, corner2, 0.05f, Color.red, 1f);
                    }
                }
            }

            private void CheckPathLength()
            {
                if (TargetPosition.HasValue && LastDistanceCheck < Time.time)
                {
                    LastDistanceCheck = Time.time + 0.5f;
                    NavMesh.CalculatePath(BotOwner.Transform.position, TargetPosition.Value, -1, Path);
                    PathLength = Path.CalculatePathLength();
                }
            }

            protected bool isActive = false;
            private float DebugTimer = 0f;
            private float LastDistanceCheck = 0f;
            private float PathLength = 0f;
            private readonly float DogFightIn = 25f;
            private readonly float DogFightOut = 30f;
            protected ManualLogSource Logger;
            private Vector3? TargetPosition;
            private NavMeshPath Path = new NavMeshPath();

            private bool DebugMode => DebugDogFightLayer.Value;
            private bool TargetFar => TargetDistance > 50f;
            private float TargetDistance => Vector3.Distance(BotOwner.Transform.position, TargetPosition.Value);
        }
    }
}