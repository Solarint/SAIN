using SAIN.Helpers;
using System;
using System.Windows.Forms;
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
                GameDateTime = BotController.Bots.PickRandom().Value.BotOwner.GameDateTime.Calculate();
                float minutes = GameDateTime.Minute / 59f;
                float timeofday = GameDateTime.Hour + minutes;

                float timemodifier = 1f;
                // SeenTime Check
                if (timeofday >= 20f || timeofday <= 8f)
                {
                    // Night
                    if (timeofday > 22f || timeofday < 6f)
                    {
                        timemodifier = 0.15f;
                    }
                    else
                    {
                        // Dawn
                        if (timeofday <= 8f)
                        {
                            // Turns hour 6 to 8 into 0 to 2 and then scales it to 0 to 1
                            float dawnHours = (timeofday - 6f) / 2f;

                            // scales to 0.1 to 1
                            float scaledDawnHours = dawnHours * 0.75f + 0.25f;

                            // assigns modifier to our output
                            timemodifier = scaledDawnHours;
                        }
                        // Dusk
                        else if (timeofday >= 20f)
                        {
                            // Turns hour 20 to 22 into 0 to 2 and then scales it to 0 to 1
                            float duskHours = (timeofday - 20f) / 2f;

                            // scales to 0.1 to 1
                            float scaledDuskHours = duskHours * 0.75f + 0.25f;

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
                return timemodifier;
            }
            return 1f;
        }
    }
}