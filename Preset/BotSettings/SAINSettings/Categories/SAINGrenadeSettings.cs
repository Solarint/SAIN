
using Newtonsoft.Json;
using SAIN.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public class SAINGrenadeSettings
    {
        [DefaultValue(100f)]
        [Advanced(IAdvancedOption.Hidden)]
        public float CHANCE_TO_NOTIFY_ENEMY_GR_100 = 100f;

        [DefaultValue(0.0f)]
        [Advanced(IAdvancedOption.Hidden)]
        public float DELTA_GRENADE_START_TIME = 0.0f;

        [DefaultValue(3)]
        [Advanced(IAdvancedOption.Hidden)]
        public int BEWARE_TYPE = 3;
    }
}
