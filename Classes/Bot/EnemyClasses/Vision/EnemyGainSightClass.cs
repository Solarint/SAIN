using EFT;
using SAIN.Components;
using SAIN.Preset.GlobalSettings;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    public static class EnemyGainSightClass
    {
        public static float GetGainSightModifier(Enemy enemy)
        {
            return CalcModifier(enemy) * CalcRepeatSeenCoef(enemy.KnownPlaces);
        }

        private const float UNDER_FIRE_FROM_ME_COEF = 0.5f;

        private const float DIST_SEEN_MIN_COEF = 0.01f;
        private const float DIST_SEEN_MIN_DIST = 1f;
        private const float DIST_SEEN_MAX_DIST = 25f;

        private const float DIST_HEARD_MIN_COEF = 0.2f;
        private const float DIST_HEARD_MIN_DIST = 1f;
        private const float DIST_HEARD_MAX_DIST = 20f;

        private const float TIME_MAX_DIST_CLAMP = 200f;
        private const float TIME_MAX_DIST_CLAMP_NVGS = 250f;
        private const float TIME_MIN_DIST_CLAMP = 10f;
        private const float TIME_MIN_DIST_CLAMP_NVGS = 65f;

        private const float ENEMYLIGHT_WHITELIGHT_MOD = 0.75f;
        private const float ENEMYLIGHT_LASER_MOD = 0.95f;
        private const float ENEMYLIGHT_NVGS_IR_LASER_MOD = 0.7f;
        private const float ENEMYLIGHT_NVGS_IR_LIGHT_MOD = 0.85f;

        private const float PARTS_VISIBLE_MIN_DIST = 12.5f;
        private const float PARTS_VISIBLE_MAX_PARTS = 6;
        private const float PARTS_VISIBLE_MIN_PARTS = 2;

        private static float PARTS_VISIBLE_MAX_COEF => Settings.PartsVisibility.PARTS_VISIBLE_MAX_COEF;
        private static float PARTS_VISIBLE_MIN_COEF => Settings.PartsVisibility.PARTS_VISIBLE_MIN_COEF;

        private const float ELEVATION_LASTKNOWN_MAX_DIST = 1.5f;
        private const float ELEVATION_MIN_ANGLE = 5f;

        private static float THIRDPARTY_VISION_START_ANGLE => Settings.ThirdParty.THIRDPARTY_VISION_START_ANGLE;
        private static float THIRDPARTY_VISION_MAX_COEF => Settings.ThirdParty.THIRDPARTY_VISION_MAX_COEF;

        private const float THIRDPARTY_VISION_MAX_DIST_LASTKNOWN = 50f;

        private static float PERIPHERAL_VISION_START_ANGLE => Settings.Peripheral.PERIPHERAL_VISION_START_ANGLE;
        private static float PERIPHERAL_VISION_MAX_REDUCTION_COEF => Settings.Peripheral.PERIPHERAL_VISION_MAX_REDUCTION_COEF;

        private const float PERIPHERAL_VISION_SPEED_DIRECT_FRONT_ANGLE = 3f;
        private const float PERIPHERAL_VISION_SPEED_DIRECT_FRONT_MOD = 0.66f;
        private const float PERIPHERAL_VISION_SPEED_CLOSE_FRONT_ANGLE = 6f;
        private const float PERIPHERAL_VISION_SPEED_CLOSE_FRONT_MOD = 0.8f;
        private const float PERIPHERAL_VISION_SPEED_ENEMY_CLOSE_DIST = 10;
        private const float PERIPHERAL_VISION_SPEED_ENEMY_CLOSE_MOD = 0.9f;
        private const float PERIPHERAL_VISION_SPEED_ENEMY_VERYCLOSE_DIST = 5;
        private const float PERIPHERAL_VISION_SPEED_ENEMY_VERYCLOSE_MOD = 0.8f;

        private static float PRONE_VISION_SPEED_COEF => Settings.Pose.PRONE_VISION_SPEED_COEF;
        private static float DUCK_VISION_SPEED_COEF => Settings.Pose.DUCK_VISION_SPEED_COEF;

        private const float UNKNOWN_ENEMY_HAS_ENEMY_COEF = 1.5f;

        private static float CalcUnknownMod(Enemy Enemy)
        {
            if (Enemy.EnemyKnown)
            {
                return 1f;
            }
            if (Enemy.Bot.HasEnemy)
            {
                return UNKNOWN_ENEMY_HAS_ENEMY_COEF;
            }
            return 1f;
        }

        private static float CalcModifier(Enemy enemy)
        {
            float partMod = CalcPartsMod(enemy);
            float gearMod = enemy.EnemyPlayerComponent.AIData.AIGearModifier.StealthModifier(enemy.RealDistance);

            bool flareEnabled = 
                enemy.EnemyPlayer.AIData?.GetFlare == true &&
                enemy.EnemyPlayerComponent.Equipment.CurrentWeaponInfo?.HasSuppressor != true;

            bool underFire = enemy.BotOwner.Memory.IsUnderFire && enemy.Bot.Memory.LastUnderFireEnemy == enemy;

            float underFireMod = underFire ? UNDER_FIRE_FROM_ME_COEF : 1f;
            float weatherMod = CalcWeatherMod(flareEnabled, enemy);
            float timeMod = CalcTimeModifier(flareEnabled, enemy);
            float moveMod = CalcMoveModifier(enemy);
            float elevMod = CalcElevationModifier(enemy);
            float thirdPartyMod = CalcThirdPartyMod(enemy);
            float angleMod = CalcAngleMod(enemy);
            float poseMod = PoseModifier(enemy);
            float unknownMod = CalcUnknownMod(enemy);

            float notLookMod = 1f;
            if (!enemy.IsAI)
                notLookMod = SAINNotLooking.GetVisionSpeedDecrease(enemy.EnemyInfo);

            float result =
                1f *
                underFireMod *
                partMod *
                gearMod *
                weatherMod *
                timeMod *
                moveMod *
                elevMod *
                thirdPartyMod *
                angleMod *
                notLookMod *
                unknownMod *
                poseMod;

            //if (enemy.EnemyPlayer.IsYourPlayer)
            //{
            //    Logger.LogWarning($"GainSight Time Result: [{result}] :" +
            //        $": underFireMod {underFireMod} " +
            //        $": partMod {partMod} " +
            //        $": gearMod {gearMod} " +
            //        $": weatherMod {weatherMod} " +
            //        $": timeMod {timeMod} " +
            //        $": moveMod {moveMod} " +
            //        $": elevMod {elevMod} " +
            //        $": thirdPartyMod {thirdPartyMod} " +
            //        $": angleMod {angleMod} " +
            //        $": notLookMod {notLookMod} " +
            //        $": unknownMod {unknownMod} " +
            //        $": poseMod {poseMod} " +
            //        "");
            //}

            return result;
        }

        private static float PoseModifier(Enemy enemy)
        {
            if (!Settings.Pose.Enabled)
            {
                return 1f;
            }
            float result = 1f;
            if (enemy.EnemyPlayer.IsInPronePose)
            {
                result *= PRONE_VISION_SPEED_COEF;
            }
            else if (enemy.EnemyPlayer.Pose == EPlayerPose.Duck)
            {
                result *= DUCK_VISION_SPEED_COEF;
            }
            return result;
        }

        private static float CalcRepeatSeenCoef(EnemyKnownPlaces places)
        {
            EnemyPlace lastSeen = places.LastSeenPlace;
            float result = 1f;
            if (lastSeen != null)
            {
                result *= CalcVisionSpeedPositional(
                    lastSeen.DistanceToEnemyRealPosition,
                    DIST_SEEN_MIN_COEF,
                    DIST_SEEN_MIN_DIST,
                    DIST_SEEN_MAX_DIST,
                    SeenSpeedCheck.Vision);
            }
            EnemyPlace lastHeard = places.LastHeardPlace;
            if (lastHeard != null)
            {
                result *= CalcVisionSpeedPositional(
                    lastHeard.DistanceToEnemyRealPosition,
                    DIST_HEARD_MIN_COEF,
                    DIST_HEARD_MIN_DIST,
                    DIST_HEARD_MAX_DIST,
                    SeenSpeedCheck.Audio);
            }
            return result;
        }

        private enum SeenSpeedCheck
        {
            None = 0,
            Vision = 1,
            Audio = 2,
        }

        private static float CalcVisionSpeedPositional(float distance, float minSpeedCoef, float minDist, float maxDist, SeenSpeedCheck check)
        {
            if (distance <= minDist)
            {
                return minSpeedCoef;
            }
            if (distance >= maxDist)
            {
                return 1f;
            }

            float num = maxDist - minDist;
            float num2 = distance - minDist;
            float ratio = num2 / num;
            float result = Mathf.Lerp(minSpeedCoef, 1f, ratio);
            //Logger.LogInfo($"{check} Distance from Position: {distance} Result: {result}");
            return result;
        }

        private static float CalcTimeModifier(bool flareEnabled, Enemy Enemy)
        {
            float baseModifier = BaseTimeModifier(flareEnabled);

            if (baseModifier <= 1f)
            {
                return 1f;
            }

            if (EnemyUsingLight(out float lightModifier, Enemy))
            {
                return lightModifier;
            }

            bool usingNVGS = Enemy.BotOwner.NightVision.UsingNow;
            float enemyDist = Enemy.RealDistance;

            if (EnemyInRangeOfLight(enemyDist, usingNVGS, Enemy))
                return 1f;

            float max = 1f + baseModifier;
            float min = 1f;
            float maxDist = usingNVGS ? TIME_MAX_DIST_CLAMP_NVGS : TIME_MAX_DIST_CLAMP;
            float minDist = usingNVGS ? TIME_MIN_DIST_CLAMP_NVGS : TIME_MIN_DIST_CLAMP;

            if (enemyDist >= maxDist)
                return max;
            if (enemyDist < minDist)
                return min;

            float enemyVelocity = Enemy.Vision.EnemyVelocity;
            bool moving = enemyVelocity > 0.1f;
            if (!moving)
            {
                max += 1f;
            }

            float num = maxDist - minDist;
            float num2 = enemyDist - minDist;
            float ratio = num2 / num;
            float result = Mathf.Lerp(min, max, ratio);
            return result;
        }

        private static bool EnemyUsingLight(out float modifier, Enemy Enemy)
        {
            var flashlight = Enemy.EnemyPlayerComponent.Flashlight;
            if (flashlight.WhiteLight)
            {
                modifier = ENEMYLIGHT_WHITELIGHT_MOD;
                return true;
            }
            if (flashlight.Laser)
            {
                modifier = ENEMYLIGHT_LASER_MOD;
                return true;
            }
            bool usingNVGS = Enemy.BotOwner.NightVision.UsingNow;
            if (usingNVGS)
            {
                if (flashlight.IRLaser)
                {
                    modifier = ENEMYLIGHT_NVGS_IR_LASER_MOD;
                    return true;
                }
                if (flashlight.IRLight)
                {
                    modifier = ENEMYLIGHT_NVGS_IR_LIGHT_MOD;
                    return true;
                }
            }
            modifier = 1f;
            return false;
        }

        private static bool EnemyInRangeOfLight(float enemyDist, bool usingNVGS, Enemy Enemy)
        {
            var Bot = Enemy.Bot;
            var settings = Bot.Info.FileSettings.Look;
            if (Bot.PlayerComponent.Flashlight.WhiteLight &&
                enemyDist <= settings.VISIBLE_DISNACE_WITH_LIGHT)
            {
                return true;
            }
            if (usingNVGS && Bot.PlayerComponent.Flashlight.IRLight && enemyDist <= settings.VISIBLE_DISNACE_WITH_IR_LIGHT)
            {
                return true;
            }
            return false;
        }

        private static float CalcWeatherMod(bool flareEnabled, Enemy Enemy)
        {
            float baseModifier = BaseWeatherMod(flareEnabled, Enemy);

            if (baseModifier <= 1f)
                return 1f;

            float max = 1f + baseModifier;
            float min = 1f;
            float maxDist = 200f;
            float minDist = 30f;
            float enemyDist = Enemy.RealDistance;

            if (enemyDist >= maxDist)
                return max;
            if (enemyDist < minDist)
                return min;
            if (EnemyUsingLight(out _, Enemy))
                return min;

            //bool moving = Enemy.Vision.EnemyVelocity > 0.1f;
            //if (!moving)
            //    max += 1f;

            float num = maxDist - minDist;
            float num2 = enemyDist - minDist;
            float ratio = num2 / num;
            float result = Mathf.Lerp(min, max, ratio);
            return result;
        }

        private static float BaseWeatherMod(bool flareEnabled, Enemy Enemy)
        {
            if (flareEnabled && Enemy.RealDistance < 100f)
            {
                return 1f;
            }
            return BotManagerComponent.Instance.WeatherVision.GainSightModifier;
        }

        private static float BaseTimeModifier(bool flareEnabled)
        {
            if (flareEnabled)
            {
                return 1f;
            }
            return BotManagerComponent.Instance.TimeVision.TimeGainSightModifier;
        }

        // private static float _nextLogTime;

        private static VisionSpeedSettings Settings => GlobalSettingsClass.Instance.Look.VisionSpeed;

        private static float CalcPartsMod(Enemy Enemy)
        {
            if (Settings.PartsVisibility.Enabled == false)
            {
                return 1f;
            }
            if (Enemy.IsAI)
            {
                return 1f;
            }
            if (Enemy.RealDistance < PARTS_VISIBLE_MIN_DIST)
            {
                return 1f;
            }
            float max = PARTS_VISIBLE_MAX_COEF;
            float min = PARTS_VISIBLE_MIN_COEF;

            float partRatio = GetRatioPartsVisible(out int visibleCount, Enemy);
            if (visibleCount <= PARTS_VISIBLE_MIN_PARTS)
            {
                return max;
            }
            if (visibleCount >= PARTS_VISIBLE_MAX_PARTS)
            {
                return min;
            }

            if (partRatio >= 1f)
            {
                return min;
            }
            float result = Mathf.Lerp(max, min, partRatio);
            return result;
        }

        private static float GetRatioPartsVisible(out int visibleCount, Enemy Enemy)
        {
            int partCount = 0;
            visibleCount = 0;
            var parts = Enemy.Vision.EnemyParts.Parts.Values;
            foreach (EnemyPartDataClass part in parts)
            {
                partCount++;
                if (part.CanBeSeen)
                    visibleCount++;
            }
            return (float)visibleCount / (float)partCount;
        }

        private static float CalcMoveModifier(Enemy Enemy)
        {
            if (Settings.Movement.Enabled == false)
            {
                return 1f;
            }
            return Mathf.Lerp(1, Settings.Movement.MOVEMENT_VISION_MULTIPLIER, Enemy.Vision.EnemyVelocity);
        }

        private static bool IsLastKnownAtSameElev(Enemy Enemy)
        {
            var lastKnown = Enemy.LastKnownPosition;
            if (lastKnown != null)
            {
                Vector3 enemyPosition = Enemy.EnemyPosition;
                if (Mathf.Abs(enemyPosition.y - lastKnown.Value.y) < ELEVATION_LASTKNOWN_MAX_DIST)
                {
                    return true;
                }
            }
            return false;
        }

        private static float CalcElevationModifier(Enemy Enemy)
        {
            if (Settings.Elevation.Enabled == false)
            {
                return 1f;
            }
            if (IsLastKnownAtSameElev(Enemy))
                return 1f;

            var settings = SAINPlugin.LoadedPreset.GlobalSettings.Look.VisionSpeed.Elevation;
            var angles = Enemy.Vision.Angles;
            float min = ELEVATION_MIN_ANGLE;

            float elevationAngle = angles.AngleToEnemyVertical;
            if (elevationAngle < min)
                return 1f;

            bool enemyAbove = angles.AngleToEnemyVerticalSigned > 0;
            float max = enemyAbove ? settings.HighElevationMaxAngle : settings.LowElevationMaxAngle;
            float targetCoef = enemyAbove ? settings.HighElevationVisionModifier : settings.LowElevationVisionModifier;

            if (elevationAngle > max)
                return targetCoef;

            float num = max - min;
            float diff = elevationAngle - min;
            float ratio = diff / num;
            float result = Mathf.Lerp(1f, targetCoef, ratio);
            return result;
        }

        private static float CalcThirdPartyMod(Enemy Enemy)
        {
            if (Settings.ThirdParty.Enabled == false)
            {
                return 1f;
            }
            if (Enemy.IsCurrentEnemy)
            {
                return 1f;
            }
            if (Enemy.EnemyKnown &&
                Enemy.KnownPlaces.EnemyDistanceFromLastKnown > THIRDPARTY_VISION_MAX_DIST_LASTKNOWN)
            {
                return 1f;
            }
            Enemy activeEnemy = Enemy.Bot.GoalEnemy;
            if (activeEnemy == null)
            {
                return 1f;
            }
            Vector3? activeEnemyLastKnown = activeEnemy.LastKnownPosition;
            if (activeEnemyLastKnown == null)
            {
                return 1f;
            }

            Vector3 currentEnemyDir = (activeEnemyLastKnown.Value - Enemy.Bot.Position).normalized;
            currentEnemyDir.y = 0;
            Vector3 myDir = Enemy.EnemyDirectionNormal;
            myDir.y = 0;
            float angle = Vector3.Angle(currentEnemyDir, myDir);

            float minAngle = THIRDPARTY_VISION_START_ANGLE;
            float maxRatio = THIRDPARTY_VISION_MAX_COEF;
            if (angle <= minAngle)
            {
                return 1f;
            }
            float maxAngle = Enemy.Vision.Angles.MaxVisionAngle;
            if (angle >= maxAngle)
            {
                return maxRatio;
            }

            float num = maxAngle - minAngle;
            float num2 = angle - minAngle;
            float ratio = num2 / num;
            float reductionMod = Mathf.Lerp(1f, maxRatio, ratio);

            return reductionMod;
        }

        private static float CalcAngleMod(Enemy Enemy)
        {
            if (Settings.Peripheral.Enabled == false)
            {
                return 1f;
            }
            float angle = Enemy.Vision.Angles.AngleToEnemyHorizontal;
            if (angle < PERIPHERAL_VISION_SPEED_DIRECT_FRONT_ANGLE)
            {
                return PERIPHERAL_VISION_SPEED_DIRECT_FRONT_MOD;
            }
            if (angle < PERIPHERAL_VISION_SPEED_CLOSE_FRONT_ANGLE)
            {
                return PERIPHERAL_VISION_SPEED_CLOSE_FRONT_MOD;
            }
            if (Enemy.RealDistance < PERIPHERAL_VISION_SPEED_ENEMY_VERYCLOSE_DIST)
            {
                return PERIPHERAL_VISION_SPEED_ENEMY_VERYCLOSE_MOD;
            }
            if (Enemy.RealDistance < PERIPHERAL_VISION_SPEED_ENEMY_CLOSE_DIST)
            {
                return PERIPHERAL_VISION_SPEED_ENEMY_CLOSE_MOD;
            }
            float minAngle = PERIPHERAL_VISION_START_ANGLE;
            if (angle < minAngle)
            {
                return 1f;
            }
            float maxAngle = Enemy.Vision.Angles.MaxVisionAngle;
            float maxRatio = PERIPHERAL_VISION_MAX_REDUCTION_COEF;
            if (angle > maxAngle)
            {
                return maxRatio;
            }
            float angleDiff = maxAngle - minAngle;
            float enemyAngleDiff = angle - minAngle;
            float ratio = enemyAngleDiff / angleDiff;
            return Mathf.Lerp(1f, maxRatio, ratio);
        }
    }
}