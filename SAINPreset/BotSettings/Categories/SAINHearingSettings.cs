using EFT;
using Newtonsoft.Json;
using SAIN.SAINPreset.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.BotSettings.Categories
{
    public class SAINHearingSettings
    {
        [Name("Max Footstep Audio Distance")]
        [Description("The Maximum Range that a bot can hear footsteps, in meters.")]
        [DefaultValue(50f)]
        [Minimum(5f)]
        [Maximum(100f)]
        [Rounding(1)]
        public float MaxFootstepAudioDistance = 50f;
    }
}
