
using Newtonsoft.Json;
using SAIN.SAINPreset.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.BotSettings.Categories
{
    public class SAINGrenadeSettings
    {
        [DefaultValue(100f)]
        [Minimum(0.0f)]
        [Maximum(100)]
        [Rounding(1)]
        [IsAdvanced(true)]
        [IsHidden(true)]
        public float CHANCE_TO_NOTIFY_ENEMY_GR_100 = 100f;

        [DefaultValue(0.0f)]
        [Minimum(0.0f)]
        [Maximum(1f)]
        [IsAdvanced(true)]
        [IsHidden(true)]
        [Rounding(100)]
        public float DELTA_GRENADE_START_TIME = 0.0f;

        [DefaultValue(3)]
        [Minimum(1)]
        [Maximum(3)]
        [IsAdvanced(true)]
        [IsHidden(true)]
        public int BEWARE_TYPE = 3;
    }
}
