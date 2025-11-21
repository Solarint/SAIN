using SAIN.Preset.GlobalSettings;
using UnityEngine;

namespace SAIN.Types.TurnSmoothing;

public struct BotTurnData(Vector3 lookDir)
{
    public Vector3 CurrentLookDirection = lookDir;
    public Vector3 NewTargetLookDirection = lookDir;
    public Vector3 LastTargetLookDirection = lookDir;
    public Vector3 TargetVelocity = Vector3.zero;
    public float TargetVelocityMagnitude = 0f;

    public Vector3 RandomSwayOffset = Vector3.zero;

    public SmoothTurnConfig Config = new(
        0.06f,
        300f,
        SAINPlugin.LoadedPreset.GlobalSettings.Steering.ConvergenceBoost,
        SAINPlugin.LoadedPreset.GlobalSettings.Steering.MIN_STEERING_PITCH
    );
}

public struct SmoothTurnConfig(float smoothingFactor, float turnSpeedCap, float convergenceBoost, float minPitch)
{
    public float SmoothingFactor = smoothingFactor;
    public float MaxAngularVelocity = turnSpeedCap;
    public float ConvergenceBoost = convergenceBoost;
    public float MinPitch = minPitch;
}

public static class PredictiveLookSmoothing
{
    /// <summary>
    /// Updates the smoothed look direction towards the target with predictive compensation.
    /// </summary>
    public static BotTurnData UpdateSmoothedDirection(BotTurnData turnData, float deltaTime)
    {
        if (deltaTime <= 0f) return turnData;

        // Calculate target angular velocity
        turnData = UpdateTargetVelocity(turnData, deltaTime);
        // Apply adaptive smoothing with convergence boost
        float adaptiveSmoothingFactor = CalculateAdaptiveSmoothingFactor(turnData.CurrentLookDirection, turnData.LastTargetLookDirection, deltaTime, turnData.Config);
        // Perform the smoothed rotation
        turnData.CurrentLookDirection = SmoothTowardsTarget(turnData.CurrentLookDirection, turnData.LastTargetLookDirection, adaptiveSmoothingFactor, turnData.Config);

        return turnData;
    }

    private static BotTurnData UpdateTargetVelocity(BotTurnData turnData, float deltaTime)
    {
        if (turnData.NewTargetLookDirection == Vector3.zero)
            turnData.NewTargetLookDirection = Vector3.forward; // Default to forward if no direction is set

        turnData.NewTargetLookDirection.Normalize();

        // Calculate angular velocity using cross product and dot product
        var cross = Vector3.Cross(turnData.LastTargetLookDirection, turnData.NewTargetLookDirection);
        var dot = Vector3.Dot(turnData.LastTargetLookDirection, turnData.NewTargetLookDirection);

        // Calculate angular velocity in degrees per second
        var angularChange = Mathf.Atan2(cross.magnitude, dot) * Mathf.Rad2Deg;
        var angularVelocityVector = cross.magnitude > 0.001f ? cross.normalized * (angularChange / deltaTime) : Vector3.zero;

        // Smooth the velocity estimate to reduce noise
        var velocitySmoothingFactor = Mathf.Clamp01(deltaTime * 10f);
        turnData.TargetVelocity = Vector3.Lerp(turnData.TargetVelocity, angularVelocityVector, velocitySmoothingFactor);
        turnData.TargetVelocityMagnitude = turnData.TargetVelocity.magnitude;

        // Clamp to maximum angular velocity
        if (turnData.TargetVelocityMagnitude > turnData.Config.MaxAngularVelocity)
        {
            turnData.TargetVelocity = turnData.TargetVelocity.normalized * turnData.Config.MaxAngularVelocity;
        }

        turnData.LastTargetLookDirection = turnData.NewTargetLookDirection;
        return turnData;
    }

    private static float CalculateAdaptiveSmoothingFactor(Vector3 currentDirection, Vector3 targetDirection, float deltaTime, SmoothTurnConfig config)
    {
        // Calculate angular difference
        var angularDifference = Vector3.Angle(currentDirection, targetDirection);

        // Apply convergence boost when far from target
        var convergenceMultiplier = 1f;
        const float CONVERGENCE_THRESHOLD = 45f; // Threshold for convergence boost
        if (angularDifference > CONVERGENCE_THRESHOLD) // If more than 30 degrees off
        {
            // The convergence boost is applied over a 180 degree range (inner rangnowe of 20 + 160)
            convergenceMultiplier = Mathf.Lerp(1f, config.ConvergenceBoost, (angularDifference - CONVERGENCE_THRESHOLD) / (180 - CONVERGENCE_THRESHOLD));
        }

        // Scale smoothing factor by delta time and convergence boost
        var adaptiveFactor = config.SmoothingFactor * convergenceMultiplier;

        return adaptiveFactor;
        // Ensure we don't overshoot with large delta times
        //return Mathf.Clamp01(adaptiveFactor * (deltaTime * 60f)); // Normalize for 60 FPS baseline
    }

    private static Vector3 SmoothTowardsTarget(Vector3 current, Vector3 target, float smoothingFactor, SmoothTurnConfig config)
    {
        // Use Quaternion.Slerp for smooth angular interpolation
        var currentRotation = Quaternion.LookRotation(current, Vector3.up);
        var targetRotation = Quaternion.LookRotation(target, Vector3.up);

        var smoothedRotation = Quaternion.Slerp(currentRotation, targetRotation, smoothingFactor);
        //return smoothedRotation * Vector3.forward; // Already normalized due to quaternion math

        // Limit pitch angle
        var eulerAngles = smoothedRotation.eulerAngles;

        // Convert to -180 to 180 range for easier clamping
        var pitch = eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;

        // Clamp pitch within limits
        pitch = Mathf.Max(pitch, config.MinPitch);

        // Apply clamped rotation
        smoothedRotation = Quaternion.Euler(pitch, eulerAngles.y, eulerAngles.z);

        return smoothedRotation * Vector3.forward; // Already normalized due to quaternion math
    }
}