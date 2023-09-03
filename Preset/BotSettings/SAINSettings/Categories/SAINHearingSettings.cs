using EFT;
using Newtonsoft.Json;
using SAIN.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public class SAINHearingSettings
    {
        [NameAndDescription(
            "Max Footstep Audio Distance",
            "The Maximum Range that a bot can hear footsteps, in meters.")]
        [Default(50f)]
        [MinMax(5f, 100f, 1f)]
        public float MaxFootstepAudioDistance = 50f;
    }
}