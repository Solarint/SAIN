using System;
using UnityEngine;

namespace SAIN.Components.BotController
{
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

        private float VisibilityTimer = 0f;

        private float Visibilty()
        {
            if (BotController.Bots.Count > 0)
            {
                var nightSettings = SAINPlugin.LoadedPreset.GlobalSettings.Vision;
                GameDateTime = BotController.Bots.PickRandom().Value.BotOwner.GameDateTime.Calculate();
                float minutes = GameDateTime.Minute / 59f;
                float timeofday = GameDateTime.Hour + minutes;

                float timemodifier = 1f;
                // SeenTime Check
                if (timeofday >= nightSettings.HourDuskStart || timeofday <= nightSettings.HourDawnEnd)
                {
                    // Night
                    if (timeofday > nightSettings.HourDuskEnd || timeofday < nightSettings.HourDawnStart)
                    {
                        timemodifier = nightSettings.NightTimeVisionModifier;
                    }
                    else
                    {
                        float scalingA = 1f - nightSettings.NightTimeVisionModifier;
                        float scalingB = nightSettings.NightTimeVisionModifier;

                        // Dawn
                        if (timeofday <= nightSettings.HourDawnEnd)
                        {
                            float dawnDiff = nightSettings.HourDawnEnd - nightSettings.HourDawnStart;
                            float dawnHours = (timeofday - nightSettings.HourDawnStart) / dawnDiff;
                            float scaledDawnHours = dawnHours * scalingA + scalingB;

                            // assigns modifier to our output
                            timemodifier = scaledDawnHours;
                        }
                        // Dusk
                        else if (timeofday >= nightSettings.HourDuskStart)
                        {
                            float duskDiff = nightSettings.HourDuskEnd - nightSettings.HourDuskStart;
                            float duskHours = (timeofday - nightSettings.HourDuskStart) / duskDiff;
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
                    timemodifier = 1f;
                }
                if (SAINPlugin.DebugModeEnabled)
                {
                    Logger.LogInfo($"Time Vision Modifier: [{timemodifier}] at [{timeofday}] with Config Settings VisionModifier: [{nightSettings.NightTimeVisionModifier}]");
                }
                return timemodifier;
            }
            return 1f;
        }
    }
}