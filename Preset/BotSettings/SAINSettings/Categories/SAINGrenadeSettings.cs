
using Newtonsoft.Json;
using SAIN.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public class SAINGrenadeSettings
    {
        [Hidden]
        public float CHANCE_TO_NOTIFY_ENEMY_GR_100 = 100f;

        [Hidden]
        public float DELTA_GRENADE_START_TIME = 0.0f;

        [Hidden]
        public int BEWARE_TYPE = 3;
    }
}