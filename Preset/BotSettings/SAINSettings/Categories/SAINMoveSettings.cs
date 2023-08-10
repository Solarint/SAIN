

using Newtonsoft.Json;
using SAIN.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public class SAINMoveSettings
    {

        [NameAndDescription("Turn Speed Base")]
        [DefaultValue(265f)]
        [MinMaxRound(100f, 500f)]
        public float BASE_ROTATE_SPEED = 265f;

        [DefaultValue(275f)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public float FIRST_TURN_SPEED = 275f;

        [DefaultValue(300f)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public float FIRST_TURN_BIG_SPEED = 300f;

        [NameAndDescription("Turn Speed Sprint")]
        [DefaultValue(320f)]
        [MinMaxRound(100f, 500f)]
        public float TURN_SPEED_ON_SPRINT = 320f;

        const string DONOTEDIT = "Do not Edit These";
        [Advanced(AdvancedEnum.Hidden)] public float RUN_IF_GAOL_FAR_THEN = 0f;
        [Advanced(AdvancedEnum.Hidden)] public float CHANCE_TO_RUN_IF_NO_AMMO_0_100 = 100f;
        [Advanced(AdvancedEnum.Hidden)] public float SEC_TO_CHANGE_TO_RUN = 0f;
        [Advanced(AdvancedEnum.Hidden)] public float RUN_TO_COVER_MIN = 0f;
    }
}
