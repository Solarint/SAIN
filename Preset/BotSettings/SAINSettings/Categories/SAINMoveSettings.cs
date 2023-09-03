using SAIN.Attributes;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public class SAINMoveSettings
    {
        [Name("Turn Speed Base")]
        [Default(265f)]
        [MinMax(100f, 500f)]
        public float BASE_ROTATE_SPEED = 265f;

        [Default(275f)]
        [Advanced]
        public float FIRST_TURN_SPEED = 275f;

        [Default(300f)]
        [Advanced]
        public float FIRST_TURN_BIG_SPEED = 300f;

        [Name("Turn Speed Sprint")]
        [Default(320f)]
        [MinMax(100f, 500f)]
        public float TURN_SPEED_ON_SPRINT = 320f;

        [Hidden] public float RUN_IF_GAOL_FAR_THEN = 0f;
        [Hidden] public float CHANCE_TO_RUN_IF_NO_AMMO_0_100 = 100f;
        [Hidden] public float SEC_TO_CHANGE_TO_RUN = 0f;
        [Hidden] public float RUN_TO_COVER_MIN = 0f;
    }
}