using EFT;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings;
using System;
using UnityEngine;

namespace SAIN.Components.BotController
{
    public class TimeClass : SAINControllerBase
    {
        public float TIME_CALC_FREQ = 5f;

        public event Action<TimeClass> OnTimeUpdated;

        public float VisibilityRatio { get; private set; }
        public DateTime? DateTime { get; private set; }
        public float TimeVisionDistanceModifier { get; private set; } = 1f;
        public float TimeGainSightModifier { get; private set; } = 1f;
        public ETimeOfDay TimeOfDay { get; private set; }

        public TimeClass(SAINBotController botController) : base(botController)
        {
        }

        public void Update()
        {
            if (_visUpdateTime < Time.time) {
                var gameWorld = GameWorld;
                if (gameWorld == null) {
                    return;
                }
                var gameDateTime = gameWorld.GameDateTime;
                if (gameDateTime == null) {
                    return;
                }
                _visUpdateTime = Time.time + TIME_CALC_FREQ;

                DateTime = gameDateTime.Calculate();
                float time = calcTime(DateTime.Value);
                TimeOfDay = getTimeEnum(time);
                TimeVisionDistanceModifier = getModifier(time, TimeOfDay, out float visibilityRatio);
                VisibilityRatio = visibilityRatio;
                TimeGainSightModifier = Mathf.Lerp(1f, GlobalSettingsClass.Instance.Look.Time.TIME_GAIN_SIGHT_SCALE_MAX, 1f - visibilityRatio);
                OnTimeUpdated?.Invoke(this);

                //if (_nextTestTime < Time.time)
                //{
                //    StringBuilder builder = new StringBuilder();
                //    _nextTestTime = Time.time + 10f;
                //    for (int i = 0; i < 24;  i++)
                //    {
                //        var timeOFDay = getTimeEnum(i + 1);
                //        float test = getModifier(i + 1, timeOFDay);
                //        builder.AppendLine($"{i + 1} {test} {timeOFDay}");
                //    }
                //    Logger.LogInfo(builder.ToString());
                //}
            }
        }

        private static float calcTime(DateTime dateTime)
        {
            return (dateTime.Hour + (dateTime.Minute / 59f)).Round100();
        }

        private static float getModifier(float time, ETimeOfDay timeOfDay, out float visibilityRatio)
        {
            var nightSettings = SAINPlugin.LoadedPreset.GlobalSettings.Look.Time;
            float max = 1f;
            float min = GameWorldComponent.Instance.Location.WinterActive ? nightSettings.NightTimeVisionModifierSnow : nightSettings.NightTimeVisionModifier;
            float difference;
            float current;
            switch (timeOfDay) {
                default:
                    visibilityRatio = 1f;
                    return max;

                case ETimeOfDay.Night:
                    visibilityRatio = 0f;
                    return min;

                case ETimeOfDay.Dawn:
                    difference = nightSettings.HourDawnEnd - nightSettings.HourDawnStart;
                    current = time - nightSettings.HourDawnStart;
                    visibilityRatio = current / difference;
                    break;

                case ETimeOfDay.Dusk:
                    difference = nightSettings.HourDuskEnd - nightSettings.HourDuskStart;
                    current = time - nightSettings.HourDuskStart;
                    visibilityRatio = 1f - current / difference;
                    break;
            }
            return Mathf.Lerp(min, max, visibilityRatio);
        }

        private static ETimeOfDay getTimeEnum(float time)
        {
            var nightSettings = SAINPlugin.LoadedPreset.GlobalSettings.Look.Time;
            if (time <= nightSettings.HourDuskStart &&
                time >= nightSettings.HourDawnEnd) {
                return ETimeOfDay.Day;
            }
            if (time >= nightSettings.HourDuskEnd ||
                time <= nightSettings.HourDawnStart) {
                return ETimeOfDay.Night;
            }
            if (time < nightSettings.HourDawnEnd) {
                return ETimeOfDay.Dawn;
            }
            return ETimeOfDay.Dusk;
        }

        private float _visUpdateTime = 0f;
    }
}