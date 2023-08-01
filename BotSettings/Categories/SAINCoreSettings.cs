using EFT;
using SAIN.SAINPreset.Attributes;
using System.ComponentModel;

namespace SAIN.BotSettings.Categories
{
    public class SAINCoreSettings
    {
        [Key(nameof(VisibleAngle))]
        [DefaultValue(160f)]
        [Minimum(45f)]
        [Maximum(180f)]
        [Rounding(1)]
        public float VisibleAngle = 160f;

        [Key(nameof(VisibleDistance))]
        [DefaultValue(150f)]
        [Minimum(50f)]
        [Maximum(500f)]
        [Rounding(1)]
        public float VisibleDistance = 150f;

        [Key(nameof(GainSightCoef))]
        [DefaultValue(0.2f)]
        [Minimum(0.05f)]
        [Maximum(0.95f)]
        [Rounding(100)]
        public float GainSightCoef = 0.2f;

        [Key(nameof(ScatteringPerMeter))]
        [DefaultValue(0.08f)]
        [Minimum(0.01f)]
        [Maximum(0.5f)]
        [Rounding(100)]
        public float ScatteringPerMeter = 0.08f;

        [Key(nameof(ScatteringClosePerMeter))]
        [DefaultValue(0.12f)]
        [Minimum(0.01f)]
        [Maximum(0.5f)]
        [Rounding(100)]
        public float ScatteringClosePerMeter = 0.12f;

        [Key(nameof(DamageCoeff))]
        [DefaultValue(1.2f)]
        [Minimum(1f)]
        [Maximum(2f)]
        [Rounding(100)]
        public float DamageCoeff = 1.2f;

        [Key(nameof(HearingSense))]
        [DefaultValue(0.65f)]
        [Minimum(0.1f)]
        [Maximum(2f)]
        [Rounding(100)]
        public float HearingSense = 0.65f;

        [Key(nameof(CanRun))]
        [DefaultValue(true)]
        public bool CanRun = true;

        [Key(nameof(CanGrenade))]
        [DefaultValue(true)]
        public bool CanGrenade = true;
    }
}
