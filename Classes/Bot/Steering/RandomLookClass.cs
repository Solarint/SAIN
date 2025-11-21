using SAIN.Helpers;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SAIN.SAINComponent.Classes.Mover;

public class RandomLookClass : BotSubClass<SAINSteeringClass>
{
    public RandomLookClass(SAINSteeringClass steeringClass) : base(steeringClass)
    {
    }

    public Vector3? UpdateRandomLook()
    {
        if (_randomLookTime < Time.time)
        {
            _lookRandomToggle = !_lookRandomToggle;
            _randomLookPoint = FindRandomLookPos(out bool isRandom);
            if (_randomLookPoint == null)
            {
                _randomLookTime = Time.time + 0.1f;
            }
            else
            {
                float baseTime = isRandom ? 2f : 4f;
                _randomLookTime = Time.time + baseTime * Random.Range(0.66f, 1.33f);
            }
        }
        return _randomLookPoint;
    }

    private Vector3? FindRandomLookPos(out bool isRandomLook, int percentChancetoRandomLook = 40)
    {
        _lookRandomToggle = EFTMath.RandomBool(percentChancetoRandomLook);
        if (_lookRandomToggle && GenerateRandomLookPos(out Vector3 randomLookPosition))
        {
            isRandomLook = true;
            return randomLookPosition;
        }
        isRandomLook = false;

        if (EFTMath.RandomBool() && BaseClass.FindLastKnownTarget(Bot.GoalEnemy, out Vector3 EnemyPosition))
        {
            return EnemyPosition;
        }
        EnemyList KnownEnemies = Bot.EnemyController.KnownEnemies;
        int enemyCount = KnownEnemies.Count;
        if (enemyCount > 0)
        {
            if (enemyCount == 1)
            {
                if (BaseClass.FindLastKnownTarget(KnownEnemies[0], out EnemyPosition))
                {
                    return EnemyPosition;
                }
                return null;
            }
            for (int i = 0; i < Mathf.Min(enemyCount, 4); i++)
            {
                if (BaseClass.FindLastKnownTarget(KnownEnemies[Random.Range(0, enemyCount - 1)], out EnemyPosition))
                {
                    return EnemyPosition;
                }
            }
        }
        return null;
    }

    private bool GenerateRandomLookPos(out Vector3 result)
    {
        const int MaxIterations = 6;
        const float RaycastDistance = 12f;

        LayerMask Mask = LayerMaskClass.HighPolyWithTerrainMaskAI;
        var headPos = Bot.Transform.EyePosition;

        bool randomDirFound = false;
        float pointDistance = 0f;
        result = Vector3.zero;
        for (int i = 0; i < MaxIterations; i++)
        {
            Vector3 random = UnityEngine.Random.onUnitSphere;
            random.y = 0;
            random.Normalize();
            if (!Physics.Raycast(headPos, random, out var hit, RaycastDistance, Mask))
            {
                result = random + headPos;
                return true;
            }
            else if (hit.distance > pointDistance)
            {
                pointDistance = hit.distance;
                result = hit.point;
                randomDirFound = true;
            }
        }
        return randomDirFound;
    }

    private Vector3? _randomLookPoint;
    private float _randomLookTime = 0f;
    private bool _lookRandomToggle;
}