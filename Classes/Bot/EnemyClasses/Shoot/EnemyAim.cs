using SAIN.Preset.GlobalSettings;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

// todo: set maxing aiming upgrade by time depending on weapon class and attachments,
// set bots who use boltys to take longe to aim, but be much more accurate
// setting each weapon class to have its own "base time to aim" would be good too
public class EnemyAim : EnemyBase
{
    private const float CALC_SCATTER_FREQ = 0.025f;
    private const float CALC_SCATTER_FREQ_AI = 0.1f;

    public EnemyAim(EnemyData enemyData) : base(enemyData, enemyData.Enemy.Bot)
    {
    }

    public float AimAndScatterMultiplier
    {
        get
        {
            if (_getModTime < Time.time)
            {
                _getModTime = Time.time + (Enemy.IsAI ? CALC_SCATTER_FREQ_AI : CALC_SCATTER_FREQ);
                _modifier = PoseFactor * VisibilityFactor * OpticFactor * InjuryFactor * VelocityFactor;
            }
            return _modifier;
        }
    }

    private float _modifier = 1;
    private float _getModTime;

    private float InjuryFactor => Bot.Info.WeaponInfo.Recoil.ArmInjuryModifier;

    private static AimSettings AimSettings => SAINPlugin.LoadedPreset.GlobalSettings.Aiming;

    private float OpticFactor
    {
        get
        {
            var weapon = Enemy.Bot.PlayerComponent.Equipment.CurrentWeaponInfo;
            if (weapon == null)
            {
                return 1f;
            }

            float enemyDistance = Enemy.RealDistance;

            if (weapon.HasOptic)
            {
                if (enemyDistance >= AimSettings.OpticFarDistance)
                {
                    return AimSettings.OpticFarMulti;
                }
                else if (enemyDistance <= AimSettings.OpticCloseDistance)
                {
                    return AimSettings.OpticCloseMulti;
                }
            }

            if (weapon.HasRedDot)
            {
                if (enemyDistance <= AimSettings.RedDotCloseDistance)
                {
                    return AimSettings.RedDotCloseMulti;
                }
                else if (enemyDistance >= AimSettings.RedDotFarDistance)
                {
                    return AimSettings.RedDotFarMulti;
                }
            }

            if (!weapon.HasRedDot &&
                !weapon.HasOptic)
            {
                float min = AimSettings.IronSightScaleDistanceStart;
                if (enemyDistance < min)
                {
                    return 1f;
                }

                float multi = AimSettings.IronSightFarMulti;
                float max = AimSettings.IronSightScaleDistanceEnd;
                if (enemyDistance > max)
                {
                    return multi;
                }
                float num = max - min;
                float num2 = enemyDistance - min;
                float scaled = 1f - num2 / num;
                float result = Mathf.Lerp(multi, 1f, scaled);
                //Logger.LogInfo($"{result} : Dist: {enemyDistance}");
                return result;
            }
            return 1f;
        }
    }

    private float PoseLevel => EnemyPlayer.PoseLevel;

    private float PoseFactor
    {
        get
        {
            if (EnemyPlayer.IsInPronePose)
            {
                return AimSettings.ScatterMulti_Prone;
            }

            float min = AimSettings.ScatterMulti_PoseLevel;
            float max = 1f;
            float result = Mathf.Lerp(min, max, PoseLevel);

            return result;
        }
    }

    private float VisibilityFactor
    {
        get
        {
            if (_checkVisTime < Time.time)
            {
                _checkVisTime = Time.time + _checkVisFreq;
                _visFactor = CalcVisFactor();
            }
            return _visFactor;
        }
    }

    private float VelocityFactor
    {
        get
        {
            if (Enemy.Player.IsSprintEnabled)
            {
                return AimSettings.EnemySprintingScatterMulti;
            }
            return Mathf.Lerp(AimSettings.EnemyVelocityMaxDebuff, AimSettings.EnemyVelocityMaxBuff, 1f - Enemy.EnemyTransform.VelocityData.VelocityMagnitudeNormal);
        }
    }

    private float CalcVisFactor()
    {
        var enemyParts = Enemy.EnemyInfo.AllActiveParts;
        if (enemyParts == null || enemyParts.Count < 1)
        {
            return 1f;
        }
        int visCount = 0;
        int totalCount = 0;
        foreach (var part in enemyParts)
        {
            totalCount++;

            /*
            if (part.Value.IsVisible)
            {
                visCount++;
            }
            */
        }

        totalCount++;

        /*
        var bodyPart = Enemy.EnemyInfo.BodyData().Value;
        if (bodyPart.IsVisible)
        {
            visCount++;
        }
        */

        float ratio = (float)visCount / (float)totalCount;

        float min = AimSettings.ScatterMulti_PartVis;
        float max = 1f;

        float result = Mathf.Lerp(min, max, ratio);

        return result;
    }

    private float _visFactor;
    private float _checkVisTime;
    private float _checkVisFreq = 0.1f;
}