using Comfort.Common;
using EFT.UI;

namespace SAIN.Editor
{
    internal class Sounds
    {
        public static void MenuClickSound()
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
        }
    }
}
