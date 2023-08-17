using SAIN.Attributes;
using System.ComponentModel;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public class SAINMoveSettings
    {
        [NameAndDescription("Turn Speed Base")]
        [DefaultValue(265f)]
        [MinMax(100f, 500f)]
        public float BASE_ROTATE_SPEED = 265f;

        [DefaultValue(275f)]
        [Advanced(IAdvancedOption.IsAdvanced)]
        public float FIRST_TURN_SPEED = 275f;

        [DefaultValue(300f)]
        [Advanced(IAdvancedOption.IsAdvanced)]
        public float FIRST_TURN_BIG_SPEED = 300f;

        [NameAndDescription("Turn Speed Sprint")]
        [DefaultValue(320f)]
        [MinMax(100f, 500f)]
        public float TURN_SPEED_ON_SPRINT = 320f;

        [Advanced(IAdvancedOption.Hidden)] public float RUN_IF_GAOL_FAR_THEN = 0f;
        [Advanced(IAdvancedOption.Hidden)] public float CHANCE_TO_RUN_IF_NO_AMMO_0_100 = 100f;
        [Advanced(IAdvancedOption.Hidden)] public float SEC_TO_CHANGE_TO_RUN = 0f;
        [Advanced(IAdvancedOption.Hidden)] public float RUN_TO_COVER_MIN = 0f;
    }
}