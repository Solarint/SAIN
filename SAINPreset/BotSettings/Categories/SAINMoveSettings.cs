

using Newtonsoft.Json;
using SAIN.SAINPreset.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.BotSettings.Categories
{
    public class SAINMoveSettings
    {
        [IsHidden(true)]
        [DefaultValue(0f)]
        public float RUN_IF_GAOL_FAR_THEN = 0f;
        [IsHidden(true)]
        [DefaultValue(100f)]
        public float CHANCE_TO_RUN_IF_NO_AMMO_0_100 = 100f;
        [IsHidden(true)]
        [DefaultValue(0f)]
        public float SEC_TO_CHANGE_TO_RUN = 0f;
        [IsHidden(true)]
        [DefaultValue(0f)]
        public float RUN_TO_COVER_MIN = 0f;

        [Name("Turn Speed Base")]
        [DefaultValue(265f)]
        [Minimum(100f)]
        [Maximum(500f)]
        [Rounding(1)]
        public float BASE_ROTATE_SPEED = 265f;

        [IsHidden(true)]
        [DefaultValue(275f)]
        public float FIRST_TURN_SPEED = 275f;

        [IsHidden(true)]
        [DefaultValue(300f)]
        public float FIRST_TURN_BIG_SPEED = 300f;

        [Name("Turn Speed Sprint")]
        [DefaultValue(320f)]
        [Minimum(100f)]
        [Maximum(500f)]
        [Rounding(1)]
        public float TURN_SPEED_ON_SPRINT = 320f;
    }
}
