using EFT;
using Newtonsoft.Json;
using SAIN.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public class SAINCoreSettings
    {
        [DefaultValue(160f)]
        [MinMaxRound(45f, 180f)]
        public float VisibleAngle = 160f;

        [DefaultValue(150f)]
        [MinMaxRound(50f, 500f)]
        [AdvancedOptions(false, false, true)]
        public float VisibleDistance = 150f;

        [DefaultValue(0.2f)]
        [MinMaxRound(0.05f, 0.95f, 100f)]
        [AdvancedOptions(true, false, true)]
        public float GainSightCoef = 0.2f;

        [DefaultValue(0.08f)]
        [MinMaxRound(0.01f, 0.5f, 100f)]
        [AdvancedOptions(true, false, true)]
        public float ScatteringPerMeter = 0.08f;

        [DefaultValue(0.12f)]
        [MinMaxRound(0.01f, 0.5f, 100f)]
        [AdvancedOptions(true, false, true)]
        public float ScatteringClosePerMeter = 0.12f;

        [NameAndDescription(
            "Injury Scatter Multiplier",
            "Increase scatter when a bots arms are injured.")]
        [DefaultValue(1.33f)]
        [MinMaxRound(1f, 2f, 100f)]
        [AdvancedOptions(true)]
        public float DamageCoeff = 1.33f;

        [NameAndDescription(
            "Audible Range Multiplier",
            "Modifies the distance that this bot can hear sounds")]
        [DefaultValue(1f)]
        [MinMaxRound(0.1f, 3f, 100f)]
        public float HearingSense = 1f;

        [DefaultValue(true)]
        [AdvancedOptions(true, true)]
        public bool CanRun = true;

        [DefaultValue(true)]
        public bool CanGrenade = true;
    }
}
