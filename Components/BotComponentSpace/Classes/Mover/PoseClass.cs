using EFT;
using UnityEngine;
using SAIN.Helpers;
using System;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.Preset.GlobalSettings;

namespace SAIN.SAINComponent.Classes.Mover
{
    public class PoseClass : BotBase, IBotClass
    {
        public PoseClass(BotComponent sain) : base(sain)
        {
        }

        public void Init()
        {
            base.SubscribeToPreset(null);
        }

        public void Update()
        {
            if (Player.IsSprintEnabled) {
                _stopSprintPoseTime = Time.time + 1f;
                BotOwner.SetPose(1f);
            }
        }

        public void Dispose()
        {
        }

        public bool SetPoseToCover()
        {
            if (!Bot.Info.FileSettings.Move.AUTOCROUCH_TOGGLE || !GlobalSettingsClass.Instance.Move.AUTOCROUCH_TOGGLE) {
                return false;
            }
            FindObjectsInFront();
            return SetTargetPose(ObjectTargetPoseCover);
        }

        public bool SetTargetPose(float num)
        {
            if (canChangePose()) {
                BotOwner.Mover?.SetPose(num);
                return true;
            }
            return false;
        }

        private bool canChangePose()
        {
            return _stopSprintPoseTime < Time.time && !Bot.Mover.Crawling;
        }

        private float _stopSprintPoseTime;

        public bool SetTargetPose(float? num)
        {
            return num != null && SetTargetPose(num.Value);
        }

        public bool ObjectInFront => ObjectTargetPoseCover != null;
        public float? ObjectTargetPoseCover { get; private set; }

        private void FindObjectsInFront()
        {
            if (UpdateFindObjectTimer < Time.time) {
                UpdateFindObjectTimer = Time.time + 0.5f;

                if (FindCrouchFromCover(out float pose1)) {
                    ObjectTargetPoseCover = pose1;
                }
                else {
                    ObjectTargetPoseCover = null;
                }
            }
        }

        private float UpdateFindObjectTimer { get; set; }
        private float UpdateFindObjectInCoverTimer { get; set; }

        private bool FindCrouchFromCover(out float targetPose, bool useCollider = false)
        {
            targetPose = 1f;
            if ((Bot.AILimit.CurrentAILimit == AILimitSetting.None || Bot.Enemy?.IsAI == false)) {
                Enemy enemy = Bot.Enemy;
                if (enemy?.LastKnownPosition != null) {
                    Vector3 position = enemy.LastKnownPosition.Value + Vector3.up;
                    if (useCollider) {
                        targetPose = FindCrouchHeightColliderSphereCast(position);
                    }
                    else {
                        targetPose = FindCrouchHeightRaycast(position);
                    }
                }
            }
            return targetPose < 1f;
        }

        private float FindCrouchHeightRaycast(Vector3 target, float rayLength = 4f)
        {
            const float StartHeight = 1.6f;
            const int max = 6;
            const float heightStep = 1f / max;
            LayerMask Mask = LayerMaskClass.HighPolyWithTerrainMask;

            Vector3 offset = Vector3.up * heightStep;
            Vector3 start = Bot.Transform.Position + Vector3.up * StartHeight;
            Vector3 direction = target - start;
            float targetHeight = StartHeight;
            for (int i = 0; i <= max; i++) {
                DebugGizmos.Ray(start, direction, Color.red, rayLength, 0.05f, true, 0.5f, true);
                if (Physics.Raycast(start, direction, rayLength, Mask)) {
                    return FindCrouchHeight(targetHeight);
                }
                else {
                    start -= offset;
                    direction = target - start;
                    targetHeight -= heightStep;
                }
            }
            return 1f;
        }

        private float FindCrouchHeightColliderSphereCast(Vector3 target, float rayLength = 3f, bool flatDir = true)
        {
            LayerMask Mask = LayerMaskClass.HighPolyWithTerrainMask;
            Vector3 start = Bot.Transform.Position + Vector3.up * 0.75f;
            Vector3 direction = target - start;
            if (flatDir) {
                direction.y = 0f;
            }

            float targetHeight = 1f;
            if (Physics.SphereCast(start, 0.26f, direction, out var hitInfo, rayLength, Mask)) {
                targetHeight = hitInfo.collider.bounds.size.y;
                return FindCrouchHeight(targetHeight);
            }
            return 1f;
        }

        private float FindCrouchHeight(float height)
        {
            const float min = 0.5f;
            return height - min;
        }
    }
}