using EFT;
using SAIN.Components;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Components.RotationController;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.Types.TurnSmoothing;
using System;
using UnityEngine;

namespace SAIN.Classes;

public class PlayerMovementController
{
    public Vector3 CurrentControlLookDirection => TurnData.CurrentLookDirection;

    public BotTurnData TurnData { get; set; } = new(Vector3.forward);

    public void UpdateTurnSettings(float deltaTime, BotOwner botOwner, BotComponent botComponent, bool randomSwayEnabled)
    {
        var settings = GetTurnSettings(botOwner, botComponent);
        BotTurnData turnData = TurnData;
        turnData.Config.SmoothingFactor = settings.SmoothingValue;
        turnData.Config.MaxAngularVelocity = settings.MaxTurnSpeed;
        TurnData = turnData;
        UpdateRandomSway(deltaTime, botOwner.GetPlayer, botOwner, botComponent, randomSwayEnabled);
    }

    public void UpdateBotTurnData(float deltaTime)
    {
        TurnData = PredictiveLookSmoothing.UpdateSmoothedDirection(TurnData, deltaTime);
    }

    public void RotatePlayer(PlayerComponent playerComp)
    {
        Vector3 dir = TurnData.CurrentLookDirection + RandomSwayOffset;
        if (playerComp.BotComponent != null) dir = playerComp.BotComponent.Info.WeaponInfo.Recoil.ApplyRecoil(dir);

        SetXAngle(playerComp.BotOwner, dir);
        SetYAngle(CalcYByDir(dir), playerComp.Player, playerComp.BotOwner);
    }

    private void UpdateRandomSway(float deltaTime, Player player, BotOwner botOwner, BotComponent botComponent, bool randomSwayEnabled)
    {
        if (player.HandsController is Player.FirearmController firearmController && firearmController.IsAiming)
        {
            RandomSwayOffset = Vector3.zero; // Disable random sway while aiming
            return;
        }
        if (randomSwayEnabled) RandomSwayOffset = CalcRandomSway(deltaTime) * CalcRandomSwayModifier(player, botOwner, botComponent);
        else RandomSwayOffset = Vector3.zero;
    }

    private void SetPlayerSpeed(Player player, float magnitude, float playerSpeed, float START_SLOW_DIST, bool shallSprint)
    {
        const float SLOW_COEF = 10f;
        if (shallSprint || player.IsSprintEnabled)
        {
            ChangeSpeed(player, 1f - playerSpeed);
        }
        else
        {
            float targetSpeed = _targetMoveSpeed;
            if (magnitude <= START_SLOW_DIST)
            {
                targetSpeed *= (magnitude / SLOW_COEF);
            }
            float speedDelta = targetSpeed - playerSpeed;
            ChangeSpeed(player, speedDelta);
        }
    }

    private static void ChangeSpeed(Player player, float delta)
    {
        if (Math.Abs(delta) >= 1E-45f) player.ChangeSpeed(delta);
    }

    private static bool CheckCanSprintByDistanceRemaining(float magnitude, float STOP_SPRINT_DIST)
    {
        bool canSprint = false;
        if (magnitude < STOP_SPRINT_DIST)
        {
            canSprint = false;
        }
        else if (magnitude >= STOP_SPRINT_DIST * 1.25f)
        {
            canSprint = true;
        }
        else
        {
            canSprint = false;
        }
        return canSprint;
    }

    public void SetWantToSprint(bool value)
    {
        _wantToSprint = value;
    }

    private bool _wantToSprint;

    /// <summary>
    /// Value between 0 and 1
    /// </summary>
    public void SetTargetMoveSpeed(float value)
    {
        _targetMoveSpeed = Mathf.Clamp01(value);
    }

    private float _targetMoveSpeed;

    private Vector3 RandomSwayOffset;

    private Vector3 CalcRandomSway(float deltaTime)
    {
        var settings = GlobalSettingsClass.Instance.Steering;
        loopTime += deltaTime;
        float t = (loopTime % settings.RANDOMSWAY_LOOP_DURATION) / settings.RANDOMSWAY_LOOP_DURATION; // 0 to 1 looping

        // Base circular loop
        float angle = t * Mathf.PI * 2f;
        float x = Mathf.Cos(angle);
        float z = Mathf.Sin(angle);

        // Add smooth looping wobble using extra sine waves
        float y = Mathf.Sin(angle * 2f) * 0.5f + Mathf.Sin(angle * 3.1f) * 0.3f;

        Vector3 basePos = new Vector3(x, y, z).normalized * settings.RANDOMSWAY_CIRCLE_RADIUS;

        // Add small chaotic offset for more natural motion
        float wobble = Mathf.Sin(angle * 4f + Mathf.PI * 0.5f) * settings.RANDOMSWAY_CIRCLE_SCALE;
        return basePos + new Vector3(wobble, -wobble, wobble * 0.5f);
    }

    private float loopTime;

    public void SetTargetLookDirection(Vector3 targetDirection, BotOwner botOwner, BotComponent bot)
    {
        BotTurnData turnData = TurnData;
        turnData.NewTargetLookDirection = targetDirection;
        TurnData = turnData;
    }

    public virtual void SetXAngle(BotOwner botOwner, Vector3 lookDirection)
    {
        float target;
        if (botOwner.LookedTransform != null)
        {
            Vector3 normalized = (botOwner.LookedTransform.position - botOwner.WeaponRoot.position).normalized;
            target = 57.29578f * Mathf.Atan2(normalized.x, normalized.z);
        }
        else
        {
            target = 57.29578f * Mathf.Atan2(lookDirection.x, lookDirection.z);
        }
        float num = Mathf.DeltaAngle(botOwner.GetPlayer.Rotation.x, target);
        //if (botOwner.BotLay.IsLay && num > botOwner.Settings.FileSettings.Look.ANGLE_FOR_GETUP)
        //{
        //    botOwner.BotLay.GetUp(true);
        //}
        botOwner.AimingManager.CurrentAiming.RotateX(num);
        botOwner.GetPlayer.Rotate(new Vector2(num, 0f), true);
    }

    public void SetYAngle(float angle, Player player, BotOwner botOwner)
    {
        float clampedAngle = Mathf.Max(angle, -65f);
        float num = Mathf.DeltaAngle(player.Rotation.y, clampedAngle);
        botOwner.AimingManager.CurrentAiming.RotateY(num);
        player.Rotate(new Vector2(0f, num), true);
    }

    private static float CalcYByDir(Vector3 dir)
    {
        float magnitude = dir.magnitude;
        float num = -dir.y / magnitude;
        num = Mathf.Clamp(num, -1f, 1f);
        num = 57.29578f * Mathf.Asin(num);
        return -Mathf.Abs(num) * Mathf.Sign(dir.y);
    }

    public void SetTargetMoveDirection(Vector3 direction, Vector3 finalMoveDestination, PlayerComponent playerComp, float START_SLOW_DIST = 0.75f, float STOP_SPRINT_DIST = 1f)
    {
        if (direction.sqrMagnitude < Mathf.Epsilon) return;

        Player player = playerComp.Player;
        float playerSpeed = player.Speed;

        float destinationDistance = (finalMoveDestination - playerComp.Position).magnitude;
        bool canSprint = player.MovementContext.CanSprint && CheckCanSprintByDistanceRemaining(destinationDistance, STOP_SPRINT_DIST);
        bool shallSprint = canSprint && _wantToSprint;
        SetPlayerSpeed(player, destinationDistance, playerSpeed, START_SLOW_DIST, shallSprint);
        player.EnableSprint(shallSprint);

        direction.Normalize();
        //if (destinationDistance > 0.1f)
        player.CharacterController.SetSteerDirection(direction);
        Vector2 moveDir = FindMoveDirection(direction, player.Rotation);
        player.Move(moveDir);
        playerComp.BotOwner?.AimingManager?.CurrentAiming?.Move(player.Speed);
    }

    private static float CalcRandomSwayModifier(Player player, BotOwner botOwner, BotComponent botComponent)
    {
        float result = 1f;
        if (botComponent?.Mover?.Running == true)
        {
            return 0.001f;
        }
        bool sprinting = player.IsSprintEnabled;
        if (sprinting)
        {
            return 0.001f;
        }

        bool moving = botComponent?.Mover?.Moving == true || botOwner.Mover?.IsMoving == true;
        bool aiming = botOwner.AimingManager.CurrentAiming is BotAimingClass aimClass && aimClass.AimStatus_0 != AimStatus.NoTarget;
        bool aimingDownSights = player.HandsController is Player.FirearmController firearmController && firearmController.IsAiming;

        if (aimingDownSights && aiming)
        {
            if (!moving) return 0.0001f;
            return 0.01f;
        }

        MovementContext movementContext = player.MovementContext;
        bool armsDamaged = movementContext.PhysicalConditionIs(EPhysicalCondition.LeftArmDamaged) || movementContext.PhysicalConditionIs(EPhysicalCondition.RightArmDamaged);
        bool noStamina = player.Physical.Stamina.NormalValue <= 0.1f;

        if (aimingDownSights) result *= 0.2f;
        if (aiming) result *= 0.5f;
        if (moving) result *= 1.25f;
        if (armsDamaged) result *= 1.5f;
        if (noStamina) result *= 1.5f;
        return Mathf.Clamp(result, 0.001f, 2.5f);
    }

    private static Vector2 FindMoveDirection(Vector3 direction, Vector2 playerRotation)
    {
        Vector3 vector = Quaternion.Euler(0f, 0f, playerRotation.x) * new Vector2(direction.x, direction.z);
        return new Vector2(vector.x, vector.y);
    }

    private static TurnSettings AIM_COMPLETE_SETTINGS = new(1f, 360f);

    private static TurnSettings GetTurnSettings(BotOwner botOwner, BotComponent botComponent)
    {
        var settings = GlobalSettingsClass.Instance?.Steering;
        if (settings?.SMOOTHING_BY_STATE != null && botOwner != null)
        {
            var currentAim = botOwner.AimingManager?.CurrentAiming;
            if (currentAim != null)
            {
                if (currentAim.IsReady)
                {
                    return AIM_COMPLETE_SETTINGS;
                }
                if (currentAim is BotAimingClass aimclass && aimclass.AimStatus_0 != AimStatus.NoTarget)
                {
                    return settings.SMOOTHING_BY_STATE[EBotLookMode.Aiming];
                }
            }
            if (botComponent != null)
            {
                if (botComponent.Mover?.Running == true)
                {
                    return settings.SMOOTHING_BY_STATE[EBotLookMode.CombatSprint];
                }
                if (botComponent.Steering?.CurrentSteerPriority == Models.Enums.ESteerPriority.RandomLook)
                {
                    return settings.SMOOTHING_BY_STATE[EBotLookMode.RandomLook];
                }
                Enemy enemy = botComponent.GoalEnemy;
                if (enemy != null)
                {
                    if (enemy.IsVisible)
                    {
                        return settings.SMOOTHING_BY_STATE[EBotLookMode.CombatVisibleEnemy];
                    }
                    return settings.SMOOTHING_BY_STATE[EBotLookMode.Combat];
                }
            }
            else if (botOwner.Memory?.GoalEnemy != null)
            {
                return settings.SMOOTHING_BY_STATE[EBotLookMode.Combat];
            }
            else if (botOwner.Mover?.Sprinting == true)
            {
                return settings.SMOOTHING_BY_STATE[EBotLookMode.CombatSprint];
            }
        }
        return settings.SMOOTHING_BY_STATE[EBotLookMode.Peace];
    }

    public static class Util
    {
        public static Vector3 CalculateBallisticOffset(Vector3 weaponRoot, Vector3 targetPosition, Vector3 playerVelocity, float muzzleVelocity, float gravity = 9.81f)
        {
            // Basic vector from shooter to target
            Vector3 displacement = targetPosition - weaponRoot;
            float horizontalDistance = new Vector3(displacement.x, 0, displacement.z).magnitude;

            // Target is directly above/below, no ballistic correction needed
            Vector3 bulletDropOffset = horizontalDistance < 0.01f ? Vector3.zero : CalculateSimpleBallisticOffset(displacement, muzzleVelocity, gravity);
            Vector3 leadOffset = playerVelocity * (displacement.magnitude / muzzleVelocity);
            //DebugGizmos.DrawLine(targetPosition, targetPosition + playerVelocity, Color.blue, 0.015f, 5f);
            //DebugGizmos.DrawLine(targetPosition, targetPosition + leadOffset, Color.green, 0.015f, 5f);
            DebugGizmos.DrawLine(targetPosition, targetPosition + leadOffset + bulletDropOffset, Color.green, 0.015f, 5f);
            return bulletDropOffset + leadOffset;
        }

        private static Vector3 CalculateSimpleBallisticOffset(Vector3 displacement, float muzzleVelocity, float gravity)
        {
            var horizontalDistance = new Vector3(displacement.x, 0, displacement.z).magnitude;
            var heightDifference = displacement.y;

            // Calculate time of flight using quadratic formula
            var timeOfFlight = CalculateTimeOfFlight(horizontalDistance, heightDifference, muzzleVelocity, gravity);

            if (timeOfFlight <= 0)
            {
                // No valid solution (target too far or velocity too low)
                return Vector3.zero;
            }

            // Calculate vertical drop
            var verticalDrop = 0.5f * gravity * timeOfFlight * timeOfFlight;

            // Return the upward offset needed to compensate for drop
            return new Vector3(0, verticalDrop, 0);
        }

        private static float CalculateTimeOfFlight(
            float horizontalDistance,
            float heightDifference,
            float muzzleVelocity,
            float gravity)
        {
            // For a given horizontal distance and height difference,
            // solve for the time using the kinematic equation

            // We assume optimal launch angle for maximum range
            var discriminant = muzzleVelocity * muzzleVelocity * muzzleVelocity * muzzleVelocity -
                               gravity * (gravity * horizontalDistance * horizontalDistance + 2 * heightDifference * muzzleVelocity * muzzleVelocity);

            if (discriminant < 0)
            {
                // No real solution - target is unreachable
                return -1f;
            }

            // Calculate the optimal launch angle
            var tanTheta = (muzzleVelocity * muzzleVelocity - Mathf.Sqrt(discriminant)) / (gravity * horizontalDistance);
            var launchAngle = Mathf.Atan(tanTheta);

            // Calculate horizontal velocity component
            var horizontalVelocity = muzzleVelocity * Mathf.Cos(launchAngle);

            // Time of flight
            return horizontalDistance / horizontalVelocity;
        }
    }
}