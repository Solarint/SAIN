using EFT;
using SAIN.Components;
using SAIN.SAINComponent.Classes.Sense;
using System;
using UnityEngine;

namespace SAIN.SAINComponent.Classes;

public class SAINVisionClass : BotComponentClassBase
{
    public float VISIONDISTANCE_UPDATE_FREQ = 5f;
    public float VISIONDISTANCE_UPDATE_FREQ_FLASHED = 0.5f;
    public FlashLightDazzleClass FlashLightDazzle { get; private set; }
    public SAINBotLookClass BotLook { get; private set; }

    public SAINVisionClass(BotComponent component) : base(component)
    {
        TickRequirement = ESAINTickState.OnlyNoSleep;
        FlashLightDazzle = new FlashLightDazzleClass(component);
        BotLook = new SAINBotLookClass(component);
    }

    public override void Init()
    {
        BotLook.Init();
        base.Init();
    }

    public override void ManualUpdate()
    {
        UpdateVisionDistance();
        FlashLightDazzle.CheckIfDazzleApplied(Bot.GoalEnemy);
        base.ManualUpdate();
    }

    public override void Dispose()
    {
        BotLook.Dispose();
        base.Dispose();
    }

    private void UpdateVisionDistance()
    {
        if (_nextUpdateVisibleDist < Time.time)
        {
            _nextUpdateVisibleDist = Time.time + (BotOwner.FlashGrenade.IsFlashed ? VISIONDISTANCE_UPDATE_FREQ_FLASHED : VISIONDISTANCE_UPDATE_FREQ);
            var timeSettings = GlobalSettings.Look.Time;
            var lookSensor = BotOwner.LookSensor;

            float timeMod = 1f;
            float weatherMod = 1f;
            var botController = BotManagerComponent.Instance;
            if (botController != null)
            {
                timeMod = botController.TimeVision.TimeVisionDistanceModifier;
                weatherMod = Mathf.Clamp(botController.WeatherVision.VisionDistanceModifier, timeSettings.VISION_WEATHER_MIN_COEF, 1f);
                DateTime? dateTime = botController.TimeVision.DateTime;
                if (dateTime != null)
                {
                    lookSensor.HourServer = dateTime.Value.Hour;
                }
            }

            float currentVisionDistance = BotOwner.Settings.Current.CurrentVisibleDistance;
            // Sets a minimum cap based on weather conditions to avoid bots having too low of a vision Distance while at peace in bad weather
            float currentVisionDistanceCapped = Mathf.Clamp(currentVisionDistance * weatherMod, timeSettings.VISION_WEATHER_MIN_DIST_METERS, currentVisionDistance);

            // Applies SeenTime Modifier to the final vision Distance results
            float finalVisionDistance = currentVisionDistanceCapped * timeMod;

            lookSensor.ClearVisibleDist = finalVisionDistance;

            finalVisionDistance = BotOwner.NightVision.UpdateVision(finalVisionDistance);
            finalVisionDistance = BotOwner.BotLight.UpdateLightEnable(finalVisionDistance);
            lookSensor.VisibleDist = finalVisionDistance;
        }

        // Not sure what this does, but its new, so adding it here since this patch replaces the old.
        BotOwner.BotLight?.UpdateStrope();
    }

    private float _nextUpdateVisibleDist;
}