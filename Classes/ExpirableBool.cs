using UnityEngine;
using Random = UnityEngine.Random;

namespace SAIN.SAINComponent.Classes;

public class ExpirableBool
{
    public ExpirableBool(float expireTime, float randomMin, float randomMax)
    {
        _expireTime = expireTime;
        _randomMin = randomMin;
        _randomMax = randomMax;
    }

    public bool Value
    {
        get
        {
            if (_value && _resetTime < Time.time)
            {
                _value = false;
            }
            return _value;
        }
        set
        {
            if (value)
            {
                TimeSet = Time.time;
                _resetTime = TimeSet + _expireTime * Random.Range(_randomMin, _randomMax);
            }
            _value = value;
        }
    }

    public float TimeSet;
    private bool _value;
    private float _resetTime;
    private readonly float _expireTime;
    private readonly float _randomMin;
    private readonly float _randomMax;
}