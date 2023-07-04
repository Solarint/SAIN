using Comfort.Common;
using EFT.UI;

namespace SAIN.Editor
{
    internal class Sounds
    {
        public static void MenuClickSound()
        {
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ButtonClick);
        }

        public static void ResetClickSound()
        {
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.InsuranceInsured);
        }
    }
}
