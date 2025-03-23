using EFT;
using HarmonyLib;
using SAIN.Components;
using SAIN.SAINComponent.Classes.Sense;
using SPT.Reflection.Patching;
using System.Reflection;
using System;
using UnityEngine;
using static EFT.SpeedTree.TreeWind;
using SAIN.Helpers;

namespace SAIN.SAINComponent.Classes
{
    public class SAINVisionClass : BotBase, IBotClass
    {
        public float VISIONDISTANCE_UPDATE_FREQ = 5f;
        public float VISIONDISTANCE_UPDATE_FREQ_FLASHED = 0.5f;
        public float TimeLastCheckedLOS { get; set; }
        public float TimeSinceCheckedLOS => Time.time - TimeLastCheckedLOS;
        public FlashLightDazzleClass FlashLightDazzle { get; private set; }
        public SAINBotLookClass BotLook { get; private set; }

        static SAINVisionClass()
        {
            _clearVisibleDistProperty = typeof(LookSensor).GetProperty("ClearVisibleDist");
            _visibleDistProperty = typeof(LookSensor).GetProperty("VisibleDist");
            _HourServerProperty = typeof(LookSensor).GetProperty("HourServer");
        }

        public SAINVisionClass(BotComponent component) : base(component)
        {
            FlashLightDazzle = new FlashLightDazzleClass(component);
            BotLook = new SAINBotLookClass(component);
        }

        public void Init()
        {
            BotLook.Init();
        }

        public void Update()
        {
            updateVisionDistance();
            FlashLightDazzle.CheckIfDazzleApplied(Bot.Enemy);
        }

        public void Dispose()
        {
            BotLook.Dispose();
        }

        private static PropertyInfo _clearVisibleDistProperty;
        private static PropertyInfo _visibleDistProperty;
        private static PropertyInfo _HourServerProperty;

        private void updateVisionDistance()
        {
            BotOwner botOwner = BotOwner;
            if (_nextUpdateVisibleDist < Time.time) {
                _nextUpdateVisibleDist = Time.time + (botOwner.FlashGrenade.IsFlashed ? VISIONDISTANCE_UPDATE_FREQ_FLASHED : VISIONDISTANCE_UPDATE_FREQ);
                var timeSettings = GlobalSettings.Look.Time;
                var lookSensor = botOwner.LookSensor;

                float timeMod = 1f;
                float weatherMod = 1f;
                var botController = SAINBotController.Instance;
                if (botController != null) {
                    timeMod = botController.TimeVision.TimeVisionDistanceModifier;
                    weatherMod = Mathf.Clamp(botController.WeatherVision.VisionDistanceModifier, timeSettings.VISION_WEATHER_MIN_COEF, 1f);
                    DateTime? dateTime = botController.TimeVision.DateTime;
                    if (dateTime != null) {
                        _HourServerProperty.SetValue(lookSensor, (int)((short)dateTime.Value.Hour));
                    }
                }

                //var curve = botOwner.Settings.Curv.StandartVisionSettings;
                //if (curve != null) {
                //    if (!JsonUtility.Load.LoadObject(out AnimationCurve importedCurve, "StandardVisionCurve")) {
                //        JsonUtility.SaveObjectToJson(curve, "StandardVisionCurve");
                //    }
                //}

                float currentVisionDistance = botOwner.Settings.Current.CurrentVisibleDistance;
                // Sets a minimum cap based on weather conditions to avoid bots having too low of a vision Distance while at peace in bad weather
                float currentVisionDistanceCapped = Mathf.Clamp(currentVisionDistance * weatherMod, timeSettings.VISION_WEATHER_MIN_DIST_METERS, currentVisionDistance);

                // Applies SeenTime Modifier to the final vision Distance results
                float finalVisionDistance = currentVisionDistanceCapped * timeMod;

                _clearVisibleDistProperty.SetValue(lookSensor, finalVisionDistance);

                finalVisionDistance = botOwner.NightVision.UpdateVision(finalVisionDistance);
                finalVisionDistance = botOwner.BotLight.UpdateLightEnable(finalVisionDistance);
                _visibleDistProperty.SetValue(lookSensor, finalVisionDistance);
            }
            // Not sure what this does, but its new, so adding it here since this patch replaces the old.
            botOwner.BotLight?.UpdateStrope();
        }

        private float _nextUpdateVisibleDist;
    }
}