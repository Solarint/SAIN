using System;
using UnityEngine;

namespace SAIN.Components.BotController
{
    public enum TimeOfDayEnum
    {
        Night,
        Day,
        Dusk,
        Dawn
    }
    public class TimeClass : SAINControl
    {
        public void Update()
        {
            if (VisibilityTimer < Time.time)
            {
                VisibilityTimer = Time.time + 5f;
                TimeOfDayVisibility = Visibilty();
            }
        }

        public DateTime GameDateTime { get; private set; }
        public float TimeOfDayVisibility { get; private set; }
        public TimeOfDayEnum TimeOfDay { get; private set; }

        private float VisibilityTimer = 0f;

        private float Visibilty()
        {
            if (BotController.Bots.Count > 0)
            {
                var nightSettings = SAINPlugin.LoadedPreset.GlobalSettings.Look;
                GameDateTime = BotController.Bots.PickRandom().Value.BotOwner.GameDateTime.Calculate();
                float minutes = GameDateTime.Minute / 59f;
                float time = GameDateTime.Hour + minutes;

                float timemodifier = 1f;
                // SeenTime Check
                if (time >= nightSettings.HourDuskStart || time <= nightSettings.HourDawnEnd)
                {
                    // Night
                    if (time > nightSettings.HourDuskEnd || time < nightSettings.HourDawnStart)
                    {
                        TimeOfDay = TimeOfDayEnum.Night;
                        timemodifier = nightSettings.NightTimeVisionModifier;
                    }
                    else
                    {
                        float scalingA = 1f - nightSettings.NightTimeVisionModifier;
                        float scalingB = nightSettings.NightTimeVisionModifier;

                        // Dawn
                        if (time <= nightSettings.HourDawnEnd)
                        {
                            TimeOfDay = TimeOfDayEnum.Dawn;
                            float dawnDiff = nightSettings.HourDawnEnd - nightSettings.HourDawnStart;
                            float dawnHours = (time - nightSettings.HourDawnStart) / dawnDiff;
                            float scaledDawnHours = dawnHours * scalingA + scalingB;

                            // assigns modifier to our output
                            timemodifier = scaledDawnHours;
                        }
                        // Dusk
                        else if (time >= nightSettings.HourDuskStart)
                        {
                            TimeOfDay = TimeOfDayEnum.Dusk;
                            float duskDiff = nightSettings.HourDuskEnd - nightSettings.HourDuskStart;
                            float duskHours = (time - nightSettings.HourDuskStart) / duskDiff;
                            float scaledDuskHours = duskHours * scalingA + scalingB;

                            // Inverse Scale to reduce modifier as night falls
                            float inverseScaledDuskHours = 1f - scaledDuskHours;

                            // assigns modifier to our output
                            timemodifier = inverseScaledDuskHours;
                        }
                    }
                }
                // Day
                else
                {
                    TimeOfDay = TimeOfDayEnum.Day;
                    timemodifier = 1f;
                }
                if (SAINPlugin.DebugModeEnabled)
                {
                    Logger.LogInfo($"Time Vision Modifier: [{timemodifier}] at [{time}] with Config Settings VisionModifier: [{nightSettings.NightTimeVisionModifier}]");
                }
                return timemodifier;
            }
            return 1f;
        }
    }
}