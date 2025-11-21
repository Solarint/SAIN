using EFT;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Mover;

public class PoseClass : BotBase
{
    public PoseClass(BotComponent sain) : base(sain)
    {
    }

    public override void ManualUpdate()
    {
        if (Bot.Mover.Running)
        {
            _stopSprintAndPoseChangeTime = Time.time + 0.5f;
        }
        if (Bot.SAINLayersActive || Bot.HasEnemy)
        {
            float deltaTime = Time.fixedDeltaTime;
            if (_stopSprintAndPoseChangeTime > Time.time)
            {
                PoseValue.Set(1);
                PoseValue.Get(deltaTime);
                SetPlayerPoseLevel(1);
                return;
            }
            SetPlayerPoseLevel(PoseValue.Get(deltaTime));
        }
    }

    private void SetPlayerPoseLevel(float value)
    {
        if (Player.IsInPronePose)
        {
            return;
        }
        BotOwner?.SetPose(value);
        const float poseChangeSpeedCoef = 1f;
        float difference = value - this.Player.PoseLevel;
        if (Math.Abs(difference) >= 1E-45f)
        {
            this.Player.ChangePose(difference * poseChangeSpeedCoef);
        }
    }

    public void SetTargetSpeed(float value)
    {
        SpeedValue.Set(value);
    }

    private void SetPlayerSpeed(float value)
    {
        const float SPEED_CHANGE_SPEED_COEF = 1f;
        float difference = value - this.Player.Speed;
        BotOwner?.SetTargetMoveSpeed(value);
        if (Math.Abs(difference) >= 1E-45f)
        {
            this.Player.ChangeSpeed(difference * SPEED_CHANGE_SPEED_COEF);
        }
    }

    public bool SetPoseToCover(Enemy enemy)
    {
        if (enemy == null)
        {
            return false;
        }
        if (!Bot.Info.FileSettings.Move.AUTOCROUCH_TOGGLE || !GlobalSettingsClass.Instance.Move.AUTOCROUCH_TOGGLE)
        {
            return false;
        }
        FindObjectsInFront(enemy);
        return SetTargetPose(ObjectTargetPoseCover);
    }

    public bool SetTargetPose(float num)
    {
        PoseValue.Set(num);
        return canChangePose();
    }

    private bool canChangePose()
    {
        return _stopSprintAndPoseChangeTime < Time.time && !Player.IsInPronePose;// && !Bot.Mover.Crawling;
    }

    private float _stopSprintAndPoseChangeTime;

    public bool SetTargetPose(float? num)
    {
        return num != null && SetTargetPose(num.Value);
    }

    public bool ObjectInFront => ObjectTargetPoseCover != null;
    public float? ObjectTargetPoseCover { get; private set; }

    private void FindObjectsInFront(Enemy enemy)
    {
        if (UpdateFindObjectTimer < Time.time)
        {
            UpdateFindObjectTimer = Time.time + 0.33f;

            if (FindCrouchFromCover(enemy, out float pose1))
            {
                ObjectTargetPoseCover = pose1;
            }
            else
            {
                ObjectTargetPoseCover = null;
            }
        }
    }

    private float UpdateFindObjectTimer { get; set; }
    private float UpdateFindObjectInCoverTimer { get; set; }

    private bool FindCrouchFromCover(Enemy enemy, out float targetPose, bool useCollider = false)
    {
        targetPose = 1f;
        //if ((Bot.AILimit.CurrentAILimit == AILimitSetting.None || Bot.GoalEnemy?.IsAI == false))
            if (enemy.FindLookPoint(out Vector3 position, out _))
            {
                if (useCollider)
                {
                    targetPose = FindCrouchHeightColliderSphereCast(position);
                }
                else
                {
                    targetPose = FindCrouchHeightRaycast(position);
                }
            }
        //}
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
        for (int i = 0; i <= max; i++)
        {
            DebugGizmos.Ray(start, direction, Color.red, rayLength, 0.05f, 0.5f, true);
            if (Physics.Raycast(start, direction, rayLength, Mask))
            {
                return FindCrouchHeight(targetHeight);
            }
            else
            {
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
        if (flatDir)
        {
            direction.y = 0f;
        }

        float targetHeight = 1f;
        if (Physics.SphereCast(start, 0.26f, direction, out var hitInfo, rayLength, Mask))
        {
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

    public SmoothDampenedFloat PoseValue { get; } = new(0.3f);
    public SmoothDampenedFloat SpeedValue { get; } = new(0.3f);
}