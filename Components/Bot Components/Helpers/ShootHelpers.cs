using EFT;
using SAIN.Components;
using UnityEngine;
using static SAIN.UserSettings.EditorSettings;

namespace SAIN.Helpers
{
    public class Shoot
    {
        public static float FullAutoBurstLength(BotOwner BotOwner, float distance)
        {
            var component = BotOwner.GetComponent<SAINComponent>();

            if (component == null)
            {
                return 0.001f;
            }

            float modifier = component.Info.WeaponInfo.FinalModifier;

            float k = 0.08f * modifier; // How fast for the burst length to falloff with Distance
            float scaledDistance = InverseScaleWithLogisticFunction(distance, k, 20f);

            scaledDistance = Mathf.Clamp(scaledDistance, 0.001f, 1f);

            if (distance > 80f)
            {
                scaledDistance = 0.001f;
            }
            else if (distance < 8f)
            {
                scaledDistance = 1f;
            }

            return scaledDistance * BurstMulti.Value;
        }

        public static float FullAutoTimePerShot(int bFirerate)
        {
            float roundspersecond = bFirerate / 60;

            float secondsPerShot = 1f / roundspersecond;

            return secondsPerShot;
        }

        public static float InverseScaleWithLogisticFunction(float originalValue, float k, float x0 = 20f)
        {
            float scaledValue = 1f - 1f / (1f + Mathf.Exp(k * (originalValue - x0)));
            return (float)System.Math.Round(scaledValue, 3);
        }
    }
}