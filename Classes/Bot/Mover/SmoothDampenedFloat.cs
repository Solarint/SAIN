using UnityEngine;

namespace SAIN.SAINComponent.Classes.Mover;

public class SmoothDampenedFloat(float smoothing, float maxVelocity = 50)
{
    public float LastSmoothedValue { get; private set; }
    public float TargetValue { get; private set; }

    public float Get(float deltaTime)
    {
        if (Mathf.Abs(LastSmoothedValue - TargetValue) < 0.001f)
        {
            LastSmoothedValue = TargetValue;
            _velocity = 0;
            return LastSmoothedValue;
        }
        LastSmoothedValue = Mathf.SmoothDamp(LastSmoothedValue, TargetValue, ref _velocity, _smoothingValue, _maxVelocity, deltaTime);
        return LastSmoothedValue;
    }

    public void Set(float targetValue)
    {
        TargetValue = targetValue;
    }

    private readonly float _smoothingValue = smoothing;
    private readonly float _maxVelocity = maxVelocity;
    private float _velocity = 0;
}