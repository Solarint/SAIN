using EFT;
using EFT.Weather;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SAIN.Components.BotController
{
    public class WeatherVisionClass
    {
        public WeatherVisionClass()
        {

        }

        public void Update()
        {
            if (GetNewModifiersTimer < Time.time)
            {
                GetNewModifiersTimer = Time.time + 20f;
                WeatherVisibility = Visibility();
            }
        }

        public float WeatherVisibility { get; private set; }

        private float GetNewModifiersTimer = 0f;

        private static float Visibility()
        {
            if (WeatherController.Instance?.WeatherCurve == null)
            {
                return 1f;
            }

            float fog = WeatherController.Instance.WeatherCurve.Fog;
            float fogmod = FogModifier(fog);

            float rain = WeatherController.Instance.WeatherCurve.Rain;
            float rainmod = RainModifier(rain);

            float clouds = WeatherController.Instance.WeatherCurve.Cloudiness;
            float cloudsmod = CloudsModifier(clouds);

            // Combines ModifiersClass
            float weathermodifier = 1f * fogmod * rainmod * cloudsmod;

            weathermodifier = Mathf.Clamp(weathermodifier, 0.2f, 1f);

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
}
