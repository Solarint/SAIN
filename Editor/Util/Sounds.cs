using Comfort.Common;
using EFT.UI;
using System.Diagnostics;
using UnityEngine;

namespace SAIN.Editor
{
    internal class Sounds
    {
        public static void ButtonClick()
        {
            PlaySound(EUISoundType.ButtonClick);
        }

        public static void ResetClickSound()
        {
            PlaySound(EUISoundType.InsuranceInsured);
        }

        public static void PlaySound(EUISoundType soundType)
        {
            if (SoundLimiter < Time.time)
            {
                SoundLimiter = Time.time + 0.1f;
                Singleton<GUISounds>.Instance.PlayUISound(soundType);
                if (SAINPlugin.DebugModeEnabled)
                {
                    Logger.LogDebug(soundType);
                }
            }
        }
        private static float SoundLimiter;
    }
}
