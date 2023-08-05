using EFT;
using Newtonsoft.Json;
using SAIN.SAINPreset.Attributes;
using System.ComponentModel;
using System.Reflection;

namespace SAIN.BotSettings.Categories
{
    public class SAINHearingSettings
    {
        [NameAndDescription(
            "Max Footstep Audio Distance",
            "The Maximum Range that a bot can hear footsteps, in meters.")]
        [DefaultValue(50f)]
        [MinMaxRound(5f, 100f, 1f)]
        public float MaxFootstepAudioDistance = 50f;
    }
}
