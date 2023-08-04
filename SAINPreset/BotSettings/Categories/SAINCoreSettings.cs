using EFT;
using Newtonsoft.Json;
using SAIN.SAINPreset.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.BotSettings.Categories
{
    public class SAINCoreSettings
    {
        [DefaultValue(160f)]
        [Minimum(45f)]
        [Maximum(180f)]
        [Rounding(1)]
        public float VisibleAngle = 160f;

        [DefaultValue(150f)]
        [Minimum(50f)]
        [Maximum(500f)]
        [Rounding(1)]
        public float VisibleDistance = 150f;

        [DefaultValue(0.2f)]
        [Minimum(0.05f)]
        [Maximum(0.95f)]
        [Rounding(100)]
        [IsAdvanced(true)]
        public float GainSightCoef = 0.2f;

        [DefaultValue(0.08f)]
        [Minimum(0.01f)]
        [Maximum(0.5f)]
        [Rounding(100)]
        [IsAdvanced(true)]
        public float ScatteringPerMeter = 0.08f;

        [DefaultValue(0.12f)]
        [Minimum(0.01f)]
        [Maximum(0.5f)]
        [Rounding(100)]
        [IsAdvanced(true)]
        public float ScatteringClosePerMeter = 0.12f;

        [DefaultValue(1.2f)]
        [Minimum(1f)]
        [Maximum(2f)]
        [Rounding(100)]
        [IsAdvanced(true)]
        public float DamageCoeff = 1.2f;

        [Name("Audible Range Multiplier")]
        [Description("Modifies the distance that this bot can hear sounds")]
        [DefaultValue(1f)]
        [Minimum(0.1f)]
        [Maximum(3f)]
        [Rounding(100)]
        public float HearingSense = 1f;

        [DefaultValue(true)]
        [IsAdvanced(true)]
        [IsHidden(true)]
        public bool CanRun = true;

        [DefaultValue(true)]
        public bool CanGrenade = true;
    }
}
