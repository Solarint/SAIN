using SAIN.Preset.GlobalSettings;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    // todo: set maxing aiming upgrade by time depending on weapon class and attachments,
    // set bots who use boltys to take longe to aim, but be much more accurate
    // setting each weapon class to have its own "base time to aim" would be good too
    public class EnemyAim : EnemyBase
    {
        private const float CALC_SCATTER_FREQ = 0.025f;
        private const float CALC_SCATTER_FREQ_AI = 0.1f;

        public EnemyAim(Enemy enemy) : base(enemy)
        {
        }

        public float AimAndScatterMultiplier {
            get
            {
                if (_getModTime < Time.time) {
                    _getModTime = Time.time + (Enemy.IsAI ? CALC_SCATTER_FREQ_AI : CALC_SCATTER_FREQ);
                    _modifier = _poseFactor * _visibilityFactor * _opticFactor * _injuryFactor * _velocityFactor;
                }
                return _modifier;
            }
        }

        private float _modifier;
        private float _getModTime;

        private float _injuryFactor => Bot.Info.WeaponInfo.Recoil.ArmInjuryModifier;

        private static AimSettings _aimSettings => SAINPlugin.LoadedPreset.GlobalSettings.Aiming;

        private float _opticFactor {
            get
            {
                var weapon = Enemy.Bot.PlayerComponent.Equipment.CurrentWeapon;
                if (weapon == null) {
                    return 1f;
                }

                float enemyDistance = Enemy.RealDistance;

                if (weapon.HasOptic) {
                    if (enemyDistance >= _aimSettings.OpticFarDistance) {
                        return _aimSettings.OpticFarMulti;
                    }
                    else if (enemyDistance <= _aimSettings.OpticCloseDistance) {
                        return _aimSettings.OpticCloseMulti;
                    }
                }

                if (weapon.HasRedDot) {
                    if (enemyDistance <= _aimSettings.RedDotCloseDistance) {
                        return _aimSettings.RedDotCloseMulti;
                    }
                    else if (enemyDistance >= _aimSettings.RedDotFarDistance) {
                        return _aimSettings.RedDotFarMulti;
                    }
                }

                if (!weapon.HasRedDot &&
                    !weapon.HasOptic) {
                    float min = _aimSettings.IronSightScaleDistanceStart;
                    if (enemyDistance < min) {
                        return 1f;
                    }

                    float multi = _aimSettings.IronSightFarMulti;
                    float max = _aimSettings.IronSightScaleDistanceEnd;
                    if (enemyDistance > max) {
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

        private float _poseFactor {
            get
            {
                if (EnemyPlayer.IsInPronePose) {
                    return _aimSettings.ScatterMulti_Prone;
                }

                float min = _aimSettings.ScatterMulti_PoseLevel;
                float max = 1f;
                float result = Mathf.Lerp(min, max, PoseLevel);

                return result;
            }
        }

        private float _visibilityFactor {
            get
            {
                if (_checkVisTime < Time.time) {
                    _checkVisTime = Time.time + _checkVisFreq;
                    _visFactor = calcVisFactor();
                }
                return _visFactor;
            }
        }

        private float _velocityFactor {
            get
            {
                if (Enemy.Player.IsSprintEnabled) {
                    return _aimSettings.EnemySprintingScatterMulti;
                }
                return Mathf.Lerp(_aimSettings.EnemyVelocityMaxDebuff, _aimSettings.EnemyVelocityMaxBuff, 1f - Enemy.EnemyTransform.VelocityMagnitudeNormal);
            }
        }

        private float calcVisFactor()
        {
            var enemyParts = Enemy.EnemyInfo.AllActiveParts;
            if (enemyParts == null || enemyParts.Count < 1) {
                return 1f;
            }
            int visCount = 0;
            int totalCount = 0;
            foreach (var part in enemyParts) {
                totalCount++;
                if (part.Value.IsVisible) {
                    visCount++;
                }
            }

            totalCount++;
            var bodyPart = Enemy.EnemyInfo.BodyData().Value;
            if (bodyPart.IsVisible) {
                visCount++;
            }

            float ratio = (float)visCount / (float)totalCount;

            float min = _aimSettings.ScatterMulti_PartVis;
            float max = 1f;

            float result = Mathf.Lerp(min, max, ratio);

            return result;
        }

        private float _visFactor;
        private float _checkVisTime;
        private float _checkVisFreq = 0.1f;
    }
}