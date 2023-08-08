using Comfort.Common;
using EFT.UI;
using System.Diagnostics;

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
            Singleton<GUISounds>.Instance.PlayUISound(soundType);
            if (SAINPlugin.DebugModeEnabled)
            {
                Logger.LogDebug(soundType, typeof(Sounds), true);
            }
        }
    }
}
