using EFT;
using SAIN.Components;
using SAIN.Plugin;
using SAIN.Preset;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.Info;
using System.Collections.Generic;
using UnityEngine;
using static EFT.InventoryLogic.Weapon;
using Random = UnityEngine.Random;

namespace SAIN.SAINComponent.Classes.WeaponFunction;

public class Firerate(BotWeaponInfoClass weaponInfoClass)
{
    private readonly BotWeaponInfoClass WeaponInfo = weaponInfoClass;

    public float CalcFirerateInterval()
    {
        float targetDistance = WeaponInfo.Bot.DistanceToAimTarget;
        float perMeterWait = GetPerMeter(WeaponInfo.EWeaponClass);
        float shootModifier = WeaponInfo.FinalModifier;
        EFireMode firemode = WeaponInfo.CurrentWeapon.FireMode.FireMode;
        float modifier = WeaponInfo.Bot.Info.FileSettings.Shoot.FireratMulti;
        return SemiAutoROF(targetDistance, firemode, perMeterWait, shootModifier, modifier);
    }

    static Firerate()
    {
        PresetHandler.OnPresetUpdated += UpdateSettings;
        UpdateSettings(SAINPlugin.LoadedPreset);
    }

    private static void UpdateSettings(SAINPresetClass preset)
    {
        var settings = preset.GlobalSettings.Shoot;
        PERMETER_SETTINGS = settings.WeaponPerMeter;
        MIN_FIRE_RATE_INTERVAL = settings.MIN_FIRE_RATE_INTERVAL;
        MAX_FIRE_RATE_INTERVAL = settings.MAX_FIRE_RATE_INTERVAL;
        MAX_FIRE_RATE_COEF_FULLAUTO = settings.MAX_FIRE_RATE_COEF_FULLAUTO;
        FIRERATE_RANDOMIZATION_COEF = settings.FIRERATE_RANDOMIZATION_COEF;
    }

    private static float MIN_FIRE_RATE_INTERVAL = 0.1f;
    private static float MAX_FIRE_RATE_INTERVAL = 4f;
    private static float MAX_FIRE_RATE_COEF_FULLAUTO = 0.25f;
    private static float FIRERATE_RANDOMIZATION_COEF = 0.25f;
    private static Dictionary<EWeaponClass, float> PERMETER_SETTINGS;


    public static float SemiAutoROF(float targetDistance, EFireMode firemode, float perMeterWait, float shootModifier, float modifier = 1f)
    {
        float rate = targetDistance / (perMeterWait / shootModifier);
        float final = Mathf.Clamp(rate, MIN_FIRE_RATE_INTERVAL, MAX_FIRE_RATE_INTERVAL);
        // Sets a different time between shots if a weapon is full auto or burst and the enemy isn't close
        if (firemode == EFireMode.fullauto) final *= MAX_FIRE_RATE_COEF_FULLAUTO;
        final /= modifier;
        // Final Result which is randomized +- 15%
        float finalTime = final * Random.Range(1f - FIRERATE_RANDOMIZATION_COEF, 1f + FIRERATE_RANDOMIZATION_COEF);
        return finalTime;
    }

    public static float GetPerMeter(EWeaponClass weaponClass)
    {
        if (PERMETER_SETTINGS.TryGetValue(weaponClass, out float perMeter))
        {
            return perMeter;
        }
        if (PERMETER_SETTINGS.TryGetValue(EWeaponClass.Default, out perMeter))
        {
            return perMeter;
        }
        return 80f;
    }
}