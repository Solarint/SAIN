using EFT;
using SAIN.Components;
using SAIN.Models.Enums;
using UnityEngine;

namespace SAIN.Helpers;

public class Shoot
{
    public static float FullAutoBurstLength(BotComponent bot, float distance)
    {
        if (bot.IsCheater)
        {
            return 1f;
        }

        if (bot.Suppression.SuppressingTarget && bot.Info.WeaponInfo.EWeaponClass == EWeaponClass.machinegun)
        {
            return 0.75f * Random.Range(0.66f, 1.33f);
        }

        if (bot.BotOwner.WeaponManager?.Stationary?.Taken == true)
        {
            return 1.5f * Random.Range(0.66f, 1.33f);
        }

        float scaledBurstLength = 1f - (Mathf.Clamp(distance, 0f, 50f) / 50f);
        scaledBurstLength /= bot.Info.WeaponInfo.FinalModifier;
        scaledBurstLength *= bot.Info.FileSettings.Shoot.BurstMulti;
        scaledBurstLength = Mathf.Clamp(scaledBurstLength, 0.001f, 2f);

        if (distance > 50f)
        {
            scaledBurstLength = 0.001f;
        }
        else if (distance < 10f)
        {
            scaledBurstLength = 2f;
        }

        return scaledBurstLength * Random.Range(0.66f, 1.33f);
    }

    public static float InverseScaleWithLogisticFunction(float originalValue, float k, float x0 = 20f)
    {
        float scaledValue = 1f - 1f / (1f + Mathf.Exp(k * (originalValue - x0)));
        return (float)System.Math.Round(scaledValue, 3);
    }
}