
using Newtonsoft.Json;
using SAIN.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public class SAINGrenadeSettings
    {
        [DefaultValue(100f)]
        [AdvancedOptions(true, true)]
        public float CHANCE_TO_NOTIFY_ENEMY_GR_100 = 100f;

        [DefaultValue(0.0f)]
        [AdvancedOptions(true, true)]
        public float DELTA_GRENADE_START_TIME = 0.0f;

        [DefaultValue(3)]
        [AdvancedOptions(true, true)]
        public int BEWARE_TYPE = 3;
    }
}
