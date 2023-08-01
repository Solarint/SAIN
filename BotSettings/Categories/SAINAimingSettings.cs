using Newtonsoft.Json;
using SAIN.SAINPreset.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.BotSettings.Categories
{
    public class SAINAimingSettings
    {
        [JsonIgnore]
        public static readonly FieldInfo[] Fields = typeof(SAINAimingSettings).GetFields(BindingFlags.Instance | BindingFlags.Public);

        [Key(nameof(MAX_AIMING_UPGRADE_BY_TIME))]
        [Name("Aiming Upgrade By Time")]
        [Description(null)]
        [DefaultValue(0.8f)]
        [Minimum(0.1f)]
        [Maximum(0.95f)]
        [Rounding(100)]
        public float MAX_AIMING_UPGRADE_BY_TIME = 0.8f;

        [Key(nameof(MAX_AIM_TIME))]
        [Name("Max Aim Time")]
        [Description(null)]
        [DefaultValue(0.8f)]
        [Minimum(0.1f)]
        [Maximum(0.95f)]
        [Rounding(100)]
        public float MAX_AIM_TIME = 2f;

        [Key(nameof(AIMING_TYPE))]
        [Name("Aim Type")]
        [Description(null)]
        [DefaultValue(4)]
        [Minimum(1)]
        [Maximum(6)]
        public int AIMING_TYPE = 4;

        [Key(nameof(SHPERE_FRIENDY_FIRE_SIZE))]
        [Name("Frieldly Fire Spherecast Size")]
        [Description(null)]
        [DefaultValue(0.15f)]
        [Minimum(0f)]
        [Maximum(0.5f)]
        [Rounding(100)]
        public float SHPERE_FRIENDY_FIRE_SIZE = 0.15f;

        [Key(nameof(RECALC_MUST_TIME))]
        [DefaultValue(1)]
        [IsHidden(true)]
        public int RECALC_MUST_TIME = 1;

        [Key(nameof(RECALC_MUST_TIME_MIN))]
        [DefaultValue(1)]
        [IsHidden(true)]
        public int RECALC_MUST_TIME_MIN = 1;

        [Key(nameof(RECALC_MUST_TIME_MAX))]
        [DefaultValue(2)]
        [IsHidden(true)]
        public int RECALC_MUST_TIME_MAX = 2;

        [Key(nameof(BASE_HIT_AFFECTION_DELAY_SEC))]
        [Name("Aiming Upgrade By Time")]
        [Description(null)]
        [DefaultValue(0.8f)]
        [Minimum(0.1f)]
        [Maximum(0.95f)]
        [Rounding(100)]
        public float BASE_HIT_AFFECTION_DELAY_SEC = 0.5f;

        [Key(nameof(BASE_HIT_AFFECTION_MIN_ANG))]
        [Name("Aiming Upgrade By Time")]
        [Description(null)]
        [DefaultValue(0.8f)]
        [Minimum(0.1f)]
        [Maximum(0.95f)]
        [Rounding(100)]
        public float BASE_HIT_AFFECTION_MIN_ANG = 3f;

        [Key(nameof(BASE_HIT_AFFECTION_MAX_ANG))]
        [Name("Aiming Upgrade By Time")]
        [Description(null)]
        [DefaultValue(0.8f)]
        [Minimum(0.1f)]
        [Maximum(0.95f)]
        [Rounding(100)]
        public float BASE_HIT_AFFECTION_MAX_ANG = 5f;
    }
}