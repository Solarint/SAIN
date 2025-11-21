using SAIN.Classes.Transform;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes;

public class HearingDispersionClass(SAINHearingSensorClass hearing) : BotSubClass<SAINHearingSensorClass>(hearing), IBotClass
{
    private const float MIN_DISTANCE_LAST_KNOWN_NO_RANDOMIZATION = 3f;
    private const float MAX_DISTANCE_LASTKNOWN_REDUCE_RANDOM = 50;
    private const float MIN_COEF_LASTKNOWN_REDUCE_RANDOM = 0.05f;

    public Vector3 CalcRandomizedPosition(AISoundData Sound, float addDispersion)
    {
        EnemyPlace enemyLastKnown = Sound.Enemy.KnownPlaces.LastKnownPlace;
        float distanceFromLastKnown = enemyLastKnown != null ? enemyLastKnown.DistanceToEnemyRealPosition : float.MaxValue;

        if (distanceFromLastKnown <= MIN_DISTANCE_LAST_KNOWN_NO_RANDOMIZATION)
        {
            return enemyLastKnown.Position;
        }
        float lastKnownDistCoef = 1f;
        if (distanceFromLastKnown < MAX_DISTANCE_LASTKNOWN_REDUCE_RANDOM)
        {
            float ratio = distanceFromLastKnown - MIN_DISTANCE_LAST_KNOWN_NO_RANDOMIZATION / MAX_DISTANCE_LASTKNOWN_REDUCE_RANDOM - MIN_DISTANCE_LAST_KNOWN_NO_RANDOMIZATION;
            lastKnownDistCoef = Mathf.Lerp(MIN_COEF_LASTKNOWN_REDUCE_RANDOM, 1f, ratio);
        }
        float baseDispersion = getBaseDispersion(Sound.PlayerDistance, Sound.SoundType);
        float dispersionMod = getDispersionModifier(Sound.Enemy) * addDispersion;
        float finalDispersion = baseDispersion * dispersionMod * lastKnownDistCoef;

        HearingSettings hearingSettings = GlobalSettingsClass.Instance.Hearing;
        finalDispersion = Mathf.Clamp(finalDispersion, 0f, hearingSettings.HEAR_DISPERSION_MAX_DISPERSION);
        //Vector3 randomBox = getRandomizedDirection(finalDispersion, 1f, 0.1f, 1f);
        if (GetRandomReachablePointAroundPlayer(Sound.HeardPlayerComponent, finalDispersion, 0f, out Vector3 result, 4f, 20))
        {
#if DEBUG
            if (Sound.HeardPlayer.IsYourPlayer)
            {
                //DebugGizmos.DrawBox(Sound.HeardPlayer.Position, randomBox * 0.5f, Color.magenta, 10f);
                var line = DebugGizmos.DrawLine(Vector3.zero, Vector3.forward, Color.magenta, 0.1f, 10f, false);
                DebugGizmos.SetLinePositions(line, _randomPath.corners);
            }
#endif
            return result;
        }
        if (enemyLastKnown != null)
        {
#if DEBUG
            if (SAINPlugin.DebugMode) Logger.LogWarning($"[{Bot.name}] Failed to find reachable point from perception event for Enemy: [{Sound.Enemy.Player.name}]! but we had their old last known to fall back on.");
#endif
            return enemyLastKnown.Position;
        }
        
#if DEBUG
        Logger.LogWarning($"[{Bot.name}] Failed to find reachable point from perception event for Enemy: [{Sound.Enemy.Player.name}]! had to use enemy's real location as fallback!");
#endif
        return Sound.Enemy.EnemyPosition;
    }

    public static bool GetRandomReachablePointInBoxAroundPlayer(PlayerComponent playerComp, Vector3 size, out Vector3 result, out NavMeshPath path, float navSampleRange = 2f, int maxTries = 10)
    {
        //for (int i = _preAlocPathList.Count - 1; i < maxTries; i++)
        //{
        //    _preAlocPathList.Add(new());
        //}
        PlayerNavData navData = playerComp.Transform.NavData;
        Vector3 minSize = size * 0.5f;
        Vector3 origin = navData.Status == EPlayerNavMeshDistance.OffNavMesh ? playerComp.Position : playerComp.Transform.NavData.Position;
        for (int i = 0; i < maxTries; i++)
        {
            Vector3 randomPoint = RandomPointInBox(origin, size);
            if (!NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, navSampleRange, -1))
            {
#if DEBUG
                Logger.LogDebug($"Failed nav sample [randomDir magnitude: {(randomPoint - origin).magnitude}] [Box Size magnitude {size.magnitude}]");
#endif
                continue; // No valid point found
            }
            path = new NavMeshPath();
            if (!NavMesh.CalculatePath(origin, hit.position, -1, path))
            {
#if DEBUG
                Logger.LogDebug($"Failed nav path");
#endif
                continue;
            }
            Vector3[] corners = path.corners;
            int length = corners.Length;
            if (length > 1)
            {
                result = corners[length - 1];
                return true;
            }
        }

        result = Vector3.zero;
        path = null;
        return false; // No valid point found
    }

    public static bool GetRandomReachablePointAroundPlayer(PlayerComponent playerComp, float radius, float height, out Vector3 result, float navSampleRange = 2f, int maxTries = 10)
    {
        //for (int i = _preAlocPathList.Count - 1; i < maxTries; i++)
        //{
        //    _preAlocPathList.Add(new());
        //}
        PlayerNavData navData = playerComp.Transform.NavData;
        Vector3 origin = navData.Status == EPlayerNavMeshDistance.OffNavMesh ? playerComp.Position : playerComp.Transform.NavData.Position;
        for (int i = 0; i < maxTries; i++)
        {
            Vector3 randomDir = new(
                Random.Range(-radius, radius),
                Random.Range(-height, height),
                Random.Range(-radius, radius)
            );
            Vector3 randomPoint = origin + randomDir;
            if (!NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, navSampleRange, -1))
            {
#if DEBUG
                Logger.LogDebug($"Failed nav sample [randomDir magnitude: {randomDir.magnitude}] [Radius: {radius}]");
#endif
                continue; // No valid point found
            }
            _randomPath ??= new NavMeshPath();
            _randomPath.ClearCorners();
            if (!NavMesh.CalculatePath(origin, hit.position, -1, _randomPath))
            {
#if DEBUG
                Logger.LogDebug($"Failed nav path");
#endif
                continue;
            }
            Vector3[] corners = _randomPath.corners;
            int length = corners.Length;
            if (length > 1)
            {
                result = corners[length - 1];
                return true;
            }
        }

        result = Vector3.zero;
        return false; // No valid point found
    }

    private static NavMeshPath _randomPath;

    public static Vector3 RandomPointInBox(Vector3 center, Vector3 size)
    {
        Vector3 halfSize = size * 0.5f;

        return new Vector3(
            Random.Range(-halfSize.x, halfSize.x),
            Random.Range(-halfSize.y, halfSize.y),
            Random.Range(-halfSize.z, halfSize.z)
        ) + center;
    }

    private static readonly List<NavMeshPath> _preAlocPathList = [];

    private float getBaseDispersion(float enemyDistance, SAINSoundType soundType)
    {
        HearingSettings hearingSettings = GlobalSettingsClass.Instance.Hearing;
        if (hearingSettings.HEAR_DISPERSION_VALUES.TryGetValue(soundType, out float dispersionValue) == false)
        {
            dispersionValue = 12.5f;
            //Logger.LogWarning($"Could not find [{soundType}] in Hearing Dispersion Dictionary!");
        }
        return enemyDistance / dispersionValue;
    }

    private float getDispersionModifier(Enemy Enemy)
    {
        float dotProduct = Vector3.Dot(Bot.LookDirection.normalized, Enemy.EnemyDirectionNormal);
        float scaled = (dotProduct + 1) / 2;

        HearingSettings hearingSettings = GlobalSettingsClass.Instance.Hearing;
        float dispersionModifier = Mathf.Lerp(hearingSettings.HEAR_DISPERSION_ANGLE_MULTI_MAX, hearingSettings.HEAR_DISPERSION_ANGLE_MULTI_MIN, scaled);

        //Logger.LogInfo($"Dispersion Modifier for Sound [{dispersionModifier}] Dot Product [{dotProduct}]");
        return dispersionModifier;
    }

    private static Vector3 getRandomizedDirection(float dispersion, float xCoef = 1f, float yCoef = 0.25f, float zCoef = 1f)
    {
        float randomX = Random.Range(-dispersion * xCoef, dispersion * xCoef);
        float randomy = Random.Range(-dispersion * yCoef, dispersion * yCoef);
        float randomZ = Random.Range(-dispersion * zCoef, dispersion * zCoef);
        return new(randomX, randomy, randomZ);
    }
}