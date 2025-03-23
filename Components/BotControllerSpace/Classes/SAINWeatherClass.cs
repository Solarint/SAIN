using EFT.Weather;
using SAIN.Preset.GlobalSettings;
using UnityEngine;

namespace SAIN.Components.BotController
{
    public class SAINWeatherClass : SAINControllerBase
    {
        public readonly float WEATHER_VISION_UPDATE_FREQ = 5f;
        public readonly float WEATHER_RAINSOUND_UPDATE_FREQ = 1f;

        public float VisionDistanceModifier { get; private set; } = 1f;

        public float GainSightModifier { get; private set; } = 1f;

        public float RainSoundModifierOutdoor { get; private set; } = 1f;
        public float RainSoundModifierIndoor { get; private set; } = 1f;

        public static SAINWeatherClass Instance { get; private set; }

        public SAINWeatherClass(SAINBotController botController) : base(botController)
        {
            Instance = this;
        }

        public void Update()
        {
            if (_weatherCheckTime < Time.time) {
                _weatherCheckTime = Time.time + WEATHER_VISION_UPDATE_FREQ;
                VisionDistanceModifier = calcWeatherVisibility();
                GainSightModifier = 2f - VisionDistanceModifier;
            }
            if (_rainCheckTime < Time.time) {
                _rainCheckTime = Time.time + WEATHER_RAINSOUND_UPDATE_FREQ;
                var curve = WeatherController.Instance?.WeatherCurve;
                if (curve == null) {
                    RainSoundModifierOutdoor = 1f;
                    RainSoundModifierIndoor = 1f;
                    return;
                }
                var hearingSettings = GlobalSettingsClass.Instance.Hearing;
                RainSoundModifierOutdoor = Mathf.Lerp(1f, hearingSettings.RAIN_SOUND_COEF_OUTSIDE, curve.Rain);
                RainSoundModifierIndoor = Mathf.Lerp(1f, hearingSettings.RAIN_SOUND_COEF_INSIDE, curve.Rain);
            }
        }

        private float calcWeatherVisibility()
        {
            IWeatherCurve weatherCurve = WeatherController.Instance?.WeatherCurve;
            if (weatherCurve == null) {
                return 1f;
            }

            float weathermodifier = 1f * (fogModifier(weatherCurve.Fog) * rainModifier(weatherCurve.Rain) * cloudsModifier(weatherCurve.Cloudiness));
            weathermodifier = Mathf.Clamp(weathermodifier, 0.01f, 1f);

            //if (GameWorldComponent.Instance.Location.WinterActive) {
            //    weathermodifier = Mathf.Clamp(weathermodifier, 0.01f, 1f);
            //    //Logger.LogWarning("Snow Active");
            //}

            return weathermodifier;
        }

        private float fogModifier(float Fog)
        {
            // Points where fog values actually matter. Anything over 0.018 has little to no effect
            float fogMax = 0.018f;
            float fogValue = Mathf.Clamp(Fog, 0f, fogMax) / fogMax;
            return Mathf.Lerp(1f, _timeSettings.VISION_WEATHER_FOG_MAXCOEF, fogValue);
        }

        private float rainModifier(float rainValue0to1)
        {
            // Rain Tiers
            float rainScaleMin;
            // Sprinkling
            if (rainValue0to1 <= _timeSettings.VISION_WEATHER_RAIN_SRINKLE_THRESH) {
                rainScaleMin = _timeSettings.VISION_WEATHER_RAIN_SRINKLE_COEF;
            }
            else if (rainValue0to1 < _timeSettings.VISION_WEATHER_RAIN_LIGHT_THRESH) {
                rainScaleMin = _timeSettings.VISION_WEATHER_RAIN_LIGHT_COEF;
            }
            else if (rainValue0to1 < _timeSettings.VISION_WEATHER_RAIN_NORMAL_THRESH) {
                rainScaleMin = _timeSettings.VISION_WEATHER_RAIN_NORMAL_COEF;
            }
            else if (rainValue0to1 < _timeSettings.VISION_WEATHER_RAIN_HEAVY_THRESH) {
                rainScaleMin = _timeSettings.VISION_WEATHER_RAIN_HEAVY_COEF;
            }
            else {
                rainScaleMin = _timeSettings.VISION_WEATHER_RAIN_DOWNPOUR_COEF;
            }
            return Mathf.Lerp(1f, rainScaleMin, rainValue0to1);
        }

        private float cloudsModifier(float Clouds)
        {
            // Clouds Rounding usually scales between -1 and 1, this sets it to scale between 0 and 1
            float cloudsScaled = (Clouds + 1f) / 2f;
            // Scattered Clouds
            if (cloudsScaled <= _timeSettings.VISION_WEATHER_NOCLOUDS_THRESH) {
                return 1f;
            }
            // Cloudiness Tiers
            float minScale;
            if (cloudsScaled <= _timeSettings.VISION_WEATHER_CLOUDY_THRESH) {
                minScale = _timeSettings.VISION_WEATHER_CLOUDY_COEF;
            }
            else {
                minScale = _timeSettings.VISION_WEATHER_OVERCAST_COEF;
            }
            return Mathf.Lerp(1f, minScale, cloudsScaled);
        }

        private TimeSettings _timeSettings => GlobalSettingsClass.Instance.Look.Time;
        private float _rainCheckTime;
        private float _weatherCheckTime;
    }
}