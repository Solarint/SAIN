using EFT;
using Newtonsoft.Json;
using SAIN.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public class SAINCoreSettings
    {
        [Default(160f)]
        [MinMax(45f, 180f)]
        public float VisibleAngle = 160f;

        [Default(150f)]
        [MinMax(50f, 500f)]
        [CopyValue]
        public float VisibleDistance = 150f;

        [Default(0.2f)]
        [MinMax(0.01f, 0.95f, 100f)]
        [Advanced]
        [CopyValue]
        public float GainSightCoef = 0.2f;

        [Default(0.08f)]
        [MinMax(0.001f, 0.5f, 100f)]
        [Advanced]
        [CopyValue]
        public float ScatteringPerMeter = 0.08f;

        [Default(0.12f)]
        [MinMax(0.001f, 0.5f, 100f)]
        [Advanced]
        [CopyValue]
        public float ScatteringClosePerMeter = 0.12f;

        [NameAndDescription(
            "Injury Scatter Multiplier",
            "Increase scatter when a bots arms are injured.")]
        [Default(1.33f)]
        [MinMax(1f, 2f, 100f)]
        [Advanced]
        public float DamageCoeff = 1.33f;

        [NameAndDescription(
            "Audible Range Multiplier",
            "Modifies the distance that this bot can hear sounds")]
        [Default(1f)]
        [MinMax(0.1f, 3f, 100f)]
        public float HearingSense = 1f;

        [Default(true)]
        [Hidden]
        public bool CanRun = true;

        [Default(true)]
        public bool CanGrenade = true;
    }
}