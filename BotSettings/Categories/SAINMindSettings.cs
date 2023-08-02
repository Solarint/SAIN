using EFT;
using Newtonsoft.Json;
using SAIN.SAINPreset.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.BotSettings.Categories
{
    public class SAINMindSettings
    {
        [Name("Time To Forget About Enemy")]
        [Description("If a bot hasn't seen or heard their enemy after this amount of time, they will return to patrol")]
        [DefaultValue(240f)]
        [Minimum(30f)]
        [Maximum(1200f)]
        [Rounding(1)]
        public float TIME_TO_FORGOR_ABOUT_ENEMY_SEC = 240f;

        [Name("Midlle Finger Chance")]
        [Description("Chance this bot will flick you off when spotted")]
        [DefaultValue(0f)]
        [Minimum(0f)]
        [Maximum(100f)]
        [Rounding(1)]
        public float CHANCE_FUCK_YOU_ON_CONTACT_100 = 0f;

        [IsHidden(true)]
        [DefaultValue(false)]
        public bool SURGE_KIT_ONLY_SAFE_CONTAINER = false;

        [IsHidden(true)]
        [DefaultValue(0f)]
        public float SEC_TO_MORE_DIST_TO_RUN = 0f;

        [IsHidden(true)]
        [DefaultValue(0f)]
        public float DIST_TO_STOP_RUN_ENEMY = 0f;

        [IsHidden(true)]
        [DefaultValue(true)]
        public bool NO_RUN_AWAY_FOR_SAFE = true;

        [IsHidden(true)]
        [DefaultValue(true)]
        public bool CAN_USE_MEDS = true;

        [IsHidden(true)]
        [DefaultValue(true)]
        public bool CAN_USE_FOOD_DRINK = true;

        [IsHidden(true)]
        [DefaultValue(5f)]
        public float GROUP_ANY_PHRASE_DELAY = 5f;

        [IsHidden(true)]
        [DefaultValue(5f)]
        public float GROUP_EXACTLY_PHRASE_DELAY = 5f;
    }
}
