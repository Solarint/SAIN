using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Vision.Helpers;
using Vision.UserSettings;
using static SAIN_Helpers.DebugDrawer;
using static UnityEngine.Color;
using DrakiaXYZ;

namespace Vision.Patches
{
    public class VisibleDistancePatch : ModulePatch
    {
        private static float DebugTimer = 0f;
        private static PropertyInfo _LookSensor;
        private static PropertyInfo _clearVisibleDistProperty;
        private static PropertyInfo _visibleDistProperty;
        private static PropertyInfo _HourServerProperty;

        protected override MethodBase GetTargetMethod()
        {
            _LookSensor = AccessTools.Property(typeof(BotOwner), "LookSensor");
            Type lookSensorType = _LookSensor.PropertyType;

            _clearVisibleDistProperty = lookSensorType.GetProperty("ClearVisibleDist");
            _visibleDistProperty = lookSensorType.GetProperty("VisibleDist");
            _HourServerProperty = lookSensorType.GetProperty("HourServer");

            return AccessTools.Method(lookSensorType, "method_2");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref float ___float_3, object __instance)
        {
            if (!VisionConfig.EnableSAINVision.Value)
            {
                return true;
            }

            if (___float_3 < Time.time)
            {
                float timeMod = 1f;

                // Checks to make sure a date and time is present
                if (___botOwner_0.GameDateTime != null)
                {
                    DateTime dateTime = ___botOwner_0.GameDateTime.Calculate();

                    timeMod = Modifiers.Time.Visibilty(dateTime);

                    // Set the value of the "HourServer" property to the hour from the DateTime object
                    _HourServerProperty.SetValue(__instance, (int)((short)dateTime.Hour));
                }

                float weatherMod = Modifiers.Weather.Visibility();

                float visdistcoef = ___botOwner_0.Settings.Current._visibleDistCoef;
                float defaultvision = ___botOwner_0.Settings.Current.CurrentVisibleDistance;

                float MaxVision;
                float currentVisionDistance;
                // User Toggle for if Global Fog is disabled
                if (VisionConfig.NoGlobalFog.Value)
                {
                    // Uses default settings with a max vision cap for safety if a bot is flashed.
                    if (___botOwner_0.FlashGrenade.IsFlashed)
                    {
                        currentVisionDistance = defaultvision;
                        MaxVision = 30f;
                    }
                    // Unlocked Vision distance.
                    else
                    {
                        currentVisionDistance = VisionConfig.AbsoluteMaxVisionDistance.Value * visdistcoef;
                        MaxVision = VisionConfig.AbsoluteMaxVisionDistance.Value;
                    }
                }
                // If global fog toggle is off, use default settings. And Take SQRT of weather mod to reduce its intensity. Clamp at 0.5 as well.
                else
                {
                    currentVisionDistance = defaultvision;
                    MaxVision = defaultvision;
                    weatherMod = Mathf.Sqrt(weatherMod);
                    weatherMod = Mathf.Clamp(weatherMod, 0.5f, 1f);
                }

                // Sets a minimum cap based on weather conditions to avoid bots having too low of a vision distance while at peace in bad weather
                float currentVisionDistanceCapped = Mathf.Clamp(currentVisionDistance * weatherMod, 80f, MaxVision);

                // Applies Time Modifier to the final vision distance results
                float finalVisionDistance = currentVisionDistanceCapped * timeMod;

                _clearVisibleDistProperty.SetValue(__instance, finalVisionDistance);
                _visibleDistProperty.SetValue(__instance, ___botOwner_0.NightVision.UpdateVision(finalVisionDistance));
                _visibleDistProperty.SetValue(__instance, ___botOwner_0.BotLight.UpdateLightEnable(finalVisionDistance));

                // Log Everything!
                if (VisionConfig.DebugWeather.Value && DebugTimer < Time.time)
                {
                    DebugTimer = Time.time + 5f;
                    System.Console.WriteLine($"SAIN Weather: VisibleDist: [{finalVisionDistance}]");
                }

                // Original Logic. Update Frequency - once a minute
                ___float_3 = Time.time + (float)(___botOwner_0.FlashGrenade.IsFlashed ? 3 : 60);
            }
            return false;
        }
    }

    public class GainSightPatch : ModulePatch
    {
        //static float Timer = 0f;
        private static PropertyInfo _GoalEnemy;

        protected override MethodBase GetTargetMethod()
        {
            _GoalEnemy = AccessTools.Property(typeof(BotMemoryClass), "GoalEnemy");
            Type goalEnemyType = _GoalEnemy.PropertyType;
            return AccessTools.Method(goalEnemyType, "method_7");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref float __result)
        {
            if (!VisionConfig.EnableSAINVision.Value)
            {
                return;
            }

            float weatherModifier = Modifiers.Weather.Visibility();

            float inverseMod = Mathf.Sqrt(2f - weatherModifier);

            float finalModifier = __result * inverseMod;

            __result = finalModifier;
        }
    }

    public class VisionOverridesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotGlobalLookData), "Update");
        }

        [PatchPostfix]
        public static void PatchPostfix(BotGlobalLookData __instance)
        {
            if (!VisionConfig.EnableSAINVision.Value)
            {
                return;
            }

            __instance.MAX_DIST_CLAMP_TO_SEEN_SPEED = 1000f;

            __instance.NIGHT_VISION_ON = 75f;
            __instance.NIGHT_VISION_OFF = 125f;
            __instance.NIGHT_VISION_DIST = 125f;
            __instance.VISIBLE_ANG_NIGHTVISION = 90f;

            __instance.LOOK_THROUGH_PERIOD_BY_HIT = 0f;

            __instance.LightOnVisionDistance = 40f;
            __instance.VISIBLE_ANG_LIGHT = 30f;
            __instance.VISIBLE_DISNACE_WITH_LIGHT = 50f;

            __instance.GOAL_TO_FULL_DISSAPEAR = 0.2f;
            __instance.GOAL_TO_FULL_DISSAPEAR_GREEN = 0.35f;
            __instance.GOAL_TO_FULL_DISSAPEAR_SHOOT = 0.0001f;

            __instance.MAX_VISION_GRASS_METERS = 1f;
            __instance.MAX_VISION_GRASS_METERS_OPT = 1f;
            __instance.MAX_VISION_GRASS_METERS_FLARE = 4f;
            __instance.MAX_VISION_GRASS_METERS_FLARE_OPT = 0.25f;

            __instance.NO_GREEN_DIST = 1f;
            __instance.NO_GRASS_DIST = 1f;
        }
    }

    public class IsPartVisiblePatch : ModulePatch
    {
        public static List<string> sightblocker = new List<string> { "filbert", "fibert", "tree", "pine", "plant", "birch", "collider",
        "timber", "spruce", "bush", "metal", "wood"};

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass475), "method_1");
        }

        [PatchPrefix]
        public static bool PatchPrefix(GClass475 __instance, KeyValuePair<BodyPartClass, GClass478> part, float addVisibility, ref bool __result)
        {
            BotOwner Owner = __instance.Owner;
            Vector3 partPosition = part.Key.Position;
            Vector3 headPosition = Owner.LookSensor._headPoint;
            Vector3 directionToEnemyPart = partPosition - headPosition;

            if (Owner.LookSensor.VisibleDist + addVisibility < (partPosition - headPosition).magnitude)
            {
                Ray(headPosition, partPosition - headPosition, Owner.LookSensor.VisibleDist + addVisibility, 0.01f, yellow, 0.25f);
                __result = false;
                return false;
            }

            Ray ray = new Ray(headPosition, directionToEnemyPart);
            if (Physics.Raycast(ray, out RaycastHit rayHit, directionToEnemyPart.magnitude - 5f, LayerMaskClass.HighPolyWithTerrainMaskAI))
            {
                string ObjectName = rayHit.transform.parent.gameObject.name;

                foreach (string foliageObject in sightblocker)
                {
                    if (ObjectName.ToLower().Contains(foliageObject))
                    {
                        Line(headPosition, rayHit.point, 0.01f, green, 0.5f);
                        Sphere(rayHit.point, 0.05f, green, 1f);

                        __result = false;
                        if (Owner.Memory.GoalEnemy != null)
                        {
                            Owner.SuppressShoot.Init(Owner.Memory.GoalEnemy);

                            Vector3 lastSeenPos = Owner.Memory.GoalEnemy.PersonalLastPos;
                            lastSeenPos.y += 1f;
                            Line(Owner.WeaponRoot.position, lastSeenPos, 0.01f, red, 0.5f);
                        }
                        return false;
                    }
                }
            }

            return true;
        }
    }

    public class SetVisiblePatch : ModulePatch
    {
        public static List<string> sightblocker = new List<string> { "filbert", "fibert", "tree", "pine", "plant", "birch", "collider",
        "timber", "spruce", "bush", "metal", "wood"};

        private static PropertyInfo _GoalEnemy;

        protected override MethodBase GetTargetMethod()
        {
            _GoalEnemy = AccessTools.Property(typeof(BotMemoryClass), "GoalEnemy");
            return AccessTools.Method(_GoalEnemy.PropertyType, "method_1");
        }

        [PatchPostfix]
        public static bool PatchPostfix(KeyValuePair<BodyPartClass, GClass478> part, ref BotOwner __owner, ref bool __result)
        {
            BotOwner bot = __owner;
            Vector3 headPosition = bot.LookSensor._headPoint;

            Vector3 partPosition = part.Key.Position;
            Vector3 directionToEnemyPart = partPosition - headPosition;
            Ray ray = new Ray(headPosition, directionToEnemyPart);

            if (Physics.Raycast(ray, out RaycastHit rayHit, directionToEnemyPart.magnitude - 2f, LayerMaskClass.HighPolyWithTerrainMaskAI))
            {
                string ObjectName = rayHit.transform.parent.gameObject.name;

                foreach (string foliageObject in sightblocker)
                {
                    if (!ObjectName.ToLower().Contains(foliageObject))
                    {
                        return true;
                    }
                    if (ObjectName.ToLower().Contains(foliageObject))
                    {
                        bool visible = CheckVisibleMagnitude(headPosition, partPosition, rayHit);

                        if (visible)
                        {
                            return true;
                        }
                        else
                        {
                            __result = false;
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private static bool CheckVisibleMagnitude(Vector3 headPosition, Vector3 partPosition, RaycastHit rayHit)
        {
            Vector3 directionFromPart = headPosition - partPosition;
            float distance = (headPosition - partPosition).magnitude;
            Ray ray2 = new Ray(partPosition, directionFromPart);

            Physics.Raycast(ray2, out RaycastHit rayHit2, distance, LayerMaskClass.HighPolyWithTerrainMaskAI);

            if ((rayHit2.point - rayHit.point).magnitude < 3f)
            {
                Line(rayHit2.point, rayHit.point, 0.025f, green, 5f);
                Sphere(rayHit.point, 0.15f, green, 5f);
                Sphere(rayHit2.point, 0.15f, green, 5f);
                return true;
            }
            return false;
        }
    }
}