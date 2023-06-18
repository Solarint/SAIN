using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AI;
using UnityEngine;
using BepInEx.Logging;
using System.Reflection;
using HarmonyLib;
using SAIN.Classes.Mover;
using SAIN.Helpers;

namespace SAIN.Classes.Mover
{
    public class PoseClass : SAINBot
    {
        public PoseClass(BotOwner owner) : base(owner)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        private void DebugFindOffset()
        {
            float lowWeapRootY = BotOwner.WeaponRoot.position.y - BotOwner.Position.y;
            float playerPose = Player.PoseLevel;
            if (playerPose == 0f)
            {
                Logger.LogInfo($"WeaponRoot at 0 pose: [{lowWeapRootY}]");
            }
            else if (playerPose == 1f)
            {
                Logger.LogInfo($"WeaponRoot at 1 pose: [{lowWeapRootY}]");
            }
            else
            {
                Logger.LogDebug($"WeaponRoot at [{playerPose}] pose: [{lowWeapRootY}]");
            }
        }

        private float DebugPoseTimer = 0f;

        public void Update()
        {
            if (DebugPoseTimer < Time.time)
            {
                DebugPoseTimer = Time.time + 1f;
                //DebugFindOffset();
            }

            FindObjectsInFront();

            BotOwner.Mover.SetPose(TargetPose);
        }

        public bool SetPoseToCover()
        {
            return SetTargetPose(ObjectTargetPoseCover);
        }

        public void SetTargetPose(float num)
        {
            TargetPose = num;
        }

        public bool SetTargetPose(float? num)
        {
            bool set = num != null;
            if (set)
            {
                TargetPose = num.Value;
            }
            return set;
        }

        public void StandUp()
        {
            TargetPose = 1f;
        }

        public void Crouch()
        {
            TargetPose = 0f;
        }

        public bool ObjectInFront => ObjectTargetPoseCover != null;
        public float? ObjectTargetPoseCover { get; private set; }

        private void FindObjectsInFront()
        {
            if (UpdateFindObjectTimer < Time.time)
            {
                UpdateFindObjectTimer = Time.time + 0.5f;

                if (FindCrouchFromCover(out float pose1))
                {
                    ObjectTargetPoseCover = pose1;
                }
                else
                {
                    ObjectTargetPoseCover = null;
                }
            }
            else if (UpdateFindObjectInCoverTimer < Time.time && SAIN.Cover.CoverInUse != null)
            {
                UpdateFindObjectInCoverTimer = Time.time + 0.5f;
                UpdateFindObjectTimer = Time.time + 0.5f;

                if (FindCrouchFromCover(out float pose2))
                {
                    ObjectTargetPoseCover = pose2;
                }
                else
                {
                    ObjectTargetPoseCover = null;
                }
            }
        }

        private float UpdateFindObjectTimer { get; set; }
        private float UpdateFindObjectInCoverTimer {get; set;}

        private bool FindCrouchFromCover(out float targetPose, bool useCollider = false)
        {
            targetPose = 1f;
            if (SAIN.CurrentTargetPosition != null)
            {
                SAINEnemy enemy = SAIN.Enemy;
                if (enemy != null)
                {
                    if (useCollider)
                    {
                        targetPose = FindCrouchHeightColliderSphereCast(enemy.EnemyChestPosition);
                    }
                    else
                    {
                        targetPose = FindCrouchHeightRaycast(enemy.EnemyChestPosition);
                    }
                }
                else
                {
                    if (useCollider)
                    {
                        targetPose = FindCrouchHeightColliderSphereCast(SAIN.CurrentTargetPosition.Value);
                    }
                    else
                    {
                        targetPose = FindCrouchHeightRaycast(SAIN.CurrentTargetPosition.Value);
                    }
                }
            }
            bool foundCover = targetPose < 1f;
            if (foundCover)
            {
                SetTargetPose(targetPose);
            }
            return foundCover;
        }

        private float FindCrouchHeightRaycast(Vector3 target, float rayLength = 3f)
        {
            const float StartHeight = 1.5f;
            const int max = 10;
            const float heightStep = 1f / max;
            LayerMask Mask = LayerMaskClass.HighPolyWithTerrainMask;

            Vector3 offset = Vector3.up * heightStep;
            Vector3 start = BotPosition + Vector3.up * StartHeight;
            Vector3 direction = target - start;
            float targetHeight = StartHeight;
            for (int i = 0; i <= max; i++)
            {
                DebugGizmos.SingleObjects.Ray(start, direction, Color.red, rayLength, 0.05f, true, 0.5f, true);
                if (Physics.Raycast(start, direction, rayLength, Mask))
                {
                    break;
                }
                else
                {
                    start -= offset;
                    direction = target - start;
                    targetHeight -= heightStep;
                }
            }
            return FindCrouchHeight(targetHeight);
        }

        private float FindCrouchHeightColliderSphereCast(Vector3 target, float rayLength = 3f, bool flatDir = true)
        {
            LayerMask Mask = LayerMaskClass.HighPolyWithTerrainMask;
            Vector3 start = BotPosition + Vector3.up * 0.75f;
            Vector3 direction = target - start;
            if (flatDir)
            {
                direction.y = 0f;
            }

            float targetHeight = 1f;
            if (Physics.SphereCast(start, 0.26f, direction, out var hitInfo, rayLength, Mask))
            {
                targetHeight = hitInfo.collider.bounds.size.y;
            }
            return FindCrouchHeight(targetHeight);
        }

        private float FindCrouchHeight(float height)
        {
            const float max = 1.5f;
            const float min = 0.5f;
            const float add = max - min;
            return height - min;
        }

        public float TargetPose { get; private set; }

        private void PoseChangeScatter(float obj)
        {
        }

        private readonly ManualLogSource Logger;
    }
}
