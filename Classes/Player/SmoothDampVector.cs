using UnityEngine;

namespace SAIN.Components.PlayerComponentSpace;

public class SmoothDampVector(float capDistance = -1)
{
    public void Calculate(float deltaTime, float smoothing, float maxSpeed, float xCoef = 1, float yCoef = 1, float zCoef = 1)
    {
        Vector3 targetDir = Target;
        if (CapLengthDistance > 0 && Target.sqrMagnitude > 1)
        {
            targetDir.Normalize();
        }

        Current = new(
            Mathf.SmoothDamp(Current.x, targetDir.x, ref Velocity.x, smoothing * xCoef, maxSpeed, deltaTime),
            Mathf.SmoothDamp(Current.y, targetDir.y, ref Velocity.y, smoothing * yCoef, maxSpeed, deltaTime),
            Mathf.SmoothDamp(Current.z, targetDir.z, ref Velocity.z, smoothing * zCoef, maxSpeed, deltaTime)
            );
    }

    public Vector3 Current = Vector3.forward;
    public Vector3 Target = Vector3.forward;
    public Vector3 Velocity = Vector3.zero;
    public float CapLengthDistance = capDistance;
}

public class SmoothDampVectorDirectionNormal
{
    public void Calculate(float deltaTime, float smoothing, float maxSpeed, float pitchClamp)
    {
        Vector3 dir = Target.normalized;

        float targetYaw = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        float targetPitch = Mathf.Clamp(Mathf.Asin(dir.y) * Mathf.Rad2Deg, -pitchClamp, pitchClamp);

        currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, smoothing, maxSpeed, deltaTime);
        currentPitch = Mathf.SmoothDampAngle(currentPitch, targetPitch, ref pitchVelocity, smoothing, maxSpeed, deltaTime);

        float yawRad = currentYaw * Mathf.Deg2Rad;
        float pitchRad = currentPitch * Mathf.Deg2Rad;
        float cosPitch = Mathf.Cos(pitchRad);

        Current = new Vector3(
            Mathf.Cos(yawRad) * cosPitch,
            Mathf.Sin(pitchRad),
            Mathf.Sin(yawRad) * cosPitch
        );
    }

    public Vector3 Current = Vector3.forward;
    public Vector3 Target = Vector3.forward;

    private float currentYaw;
    private float currentPitch;
    private float yawVelocity;
    private float pitchVelocity;
}