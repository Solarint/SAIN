using UnityEngine;

namespace SAIN.Classes.Transform;

public struct PlayerVelocityData
{
    public float VelocityMagnitudeNormal;
    public float VelocityMagnitude;
    public Vector3 Velocity;

    public static PlayerVelocityData Update(PlayerVelocityData data, Vector3 velocity)
    {
        const float TRANSFORM_MIN_VELOCITY = 0.25f;
        const float TRANSFORM_MAX_VELOCITY = 5f;

        data.Velocity = velocity;
        float sqrMag = data.Velocity.sqrMagnitude;
        if (sqrMag <= TRANSFORM_MIN_VELOCITY * TRANSFORM_MIN_VELOCITY)
        {
            data.VelocityMagnitude = 0f;
            data.VelocityMagnitudeNormal = 0f;
        }
        else if (sqrMag >= TRANSFORM_MAX_VELOCITY * TRANSFORM_MAX_VELOCITY)
        {
            data.VelocityMagnitude = TRANSFORM_MAX_VELOCITY;
            data.VelocityMagnitudeNormal = 1f;
        }
        else
        {
            float mag = Mathf.Sqrt(sqrMag);
            data.VelocityMagnitude = mag;
            float num = TRANSFORM_MAX_VELOCITY - TRANSFORM_MIN_VELOCITY;
            float num2 = mag - TRANSFORM_MIN_VELOCITY;
            data.VelocityMagnitudeNormal = num2 / num;
        }
        return data;
    }
}