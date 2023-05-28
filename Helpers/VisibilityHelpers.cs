using EFT.Weather;
using System;
using UnityEngine;
using SAIN.UserSettings;

namespace SAIN.Helpers
{
    public class Modifiers
    {
        public class Weather
        {
            private static float DebugTimer = 0f;
            public static float Visibility()
            {
                if (WeatherController.Instance?.WeatherCurve == null)
                {
                    if (VisionConfig.DebugWeather.Value && DebugTimer < UnityEngine.Time.time)
                    {
                        DebugTimer = UnityEngine.Time.time + 5f;
                        System.Console.WriteLine($"SAIN Weather: No Weather Found!");
                    }
                    return 1f;
                }

                float fog = WeatherController.Instance.WeatherCurve.Fog;
                float fogmod = FogModifier(fog);

                float rain = WeatherController.Instance.WeatherCurve.Rain;
                float rainmod = RainModifier(rain);

                float clouds = WeatherController.Instance.WeatherCurve.Cloudiness;
                float cloudsmod = CloudsModifier(clouds);

                // Combines Modifiers
                float weathermodifier = 1f * fogmod * rainmod * cloudsmod;

                weathermodifier = Mathf.Clamp(weathermodifier, 0.2f, 1f);

                // Log Everything!
                if (VisionConfig.DebugWeather.Value && DebugTimer < UnityEngine.Time.time)
                {
                    DebugTimer = UnityEngine.Time.time + 5f;
                    System.Console.WriteLine($"SAIN Weather: Final Weather Modifier: [{weathermodifier}], Fog: [{fogmod}], Rain: [{rainmod}], Clouds: [{cloudsmod}]");
                }

                return weathermodifier;
            }
            private static float FogModifier(float Fog)
            {
                // Points where fog values actually matter. Anything over 0.018 has little to no effect
                float fogMax = 0.018f;
                float fogValue = Mathf.Clamp(Fog, 0f, fogMax);

                // scales from 0 to 1 instead of 0 to 0.018
                fogValue /= fogMax;

                // Fog Tiers
                float fogScaleMin;
                // Very Light Fog
                /*
                if (fogValue <= 0.1f)
                {
                    fogScaleMin = 0.75f;
                }
                else
                {
                    // Light Fog
                    if (fogValue < 0.35f)
                    {
                        fogScaleMin = 0.5f;
                    }
                    else
                    {
                        // Normal Fog
                        if (fogValue < 0.5f)
                        {
                            fogScaleMin = 0.4f;
                        }
                        else
                        {
                            // Heavy Fog
                            if (fogValue < 0.75f)
                            {
                                fogScaleMin = 0.35f;
                            }
                            // I can't see shit
                            else
                            {
                                fogScaleMin = 0.25f;
                            }
                        }
                    }
                }
                */

                fogScaleMin = 0.2f;

                float fogModifier = InverseScaling(fogValue, fogScaleMin, 1f);

                return fogModifier;
            }
            private static float RainModifier(float Rain)
            {
                // Rain Tiers
                float rainScaleMin;
                // Sprinkling
                if (Rain <= 0.1f)
                {
                    rainScaleMin = 0.95f;
                }
                else
                {
                    // Light Rain
                    if (Rain < 0.35f)
                    {
                        rainScaleMin = 0.65f;
                    }
                    else
                    {
                        // Normal Rain
                        if (Rain < 0.5f)
                        {
                            rainScaleMin = 0.5f;
                        }
                        else
                        {
                            // Heavy Rain
                            if (Rain < 0.75f)
                            {
                                rainScaleMin = 0.45f;
                            }
                            // Downpour
                            else
                            {
                                rainScaleMin = 0.4f;
                            }
                        }
                    }
                }

                // Scales rain modifier depending on strength of rain found above
                float rainModifier = InverseScaling(Rain, rainScaleMin, 1f);

                return rainModifier;
            }
            private static float CloudsModifier(float Clouds)
            {
                // Clouds value usually scales between -1 and 1, this sets it to scale between 0 and 1
                float cloudsScaled = (Clouds + 1f) / 2f;

                // Cloudiness Tiers
                float minScaledClouds;
                // Scattered Clouds
                if (cloudsScaled <= 0.33f)
                {
                    minScaledClouds = 1f;
                }
                else
                {
                    // Cloudy
                    if (cloudsScaled <= 0.66)
                    {
                        minScaledClouds = 0.75f;
                    }
                    // Heavy Overcast
                    else
                    {
                        minScaledClouds = 0.5f;
                    }
                }

                float cloudsModifier = InverseScaling(Clouds, minScaledClouds, 1f);

                return cloudsModifier;
            }
            private static float InverseScaling(float value, float min, float max)
            {
                // Inverse
                float InverseValue = 1f - value;

                // Scaling
                float ScaledValue = (InverseValue * (max - min)) + min;

                value = ScaledValue;

                return value;
            }
        }
        public class Time
        {
            public static float Visibilty(DateTime time)
            {
                float minutes = time.Minute / 59f;
                float timeofday = time.Hour + minutes;

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
                            float scaledDawnHours = dawnHours * 0.85f + 0.15f;

                            // assigns modifier to our output
                            timemodifier = scaledDawnHours;
                        }
                        // Dusk
                        else if (timeofday >= 20f)
                        {
                            // Turns hour 20 to 22 into 0 to 2 and then scales it to 0 to 1
                            float duskHours = (timeofday - 20f) / 2f;

                            // scales to 0.1 to 1
                            float scaledDuskHours = duskHours * 0.85f + 0.15f;

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
                /*
                if (DebugWeather.Value)
                {
                    System.Console.WriteLine($"SAIN Weather: TimeModifier: [{timemodifier}], TimeofDay: [{timeofday}]");
                }
                */
                return timemodifier;
            }
        }
    }
}
