using EFT;
using Newtonsoft.Json;
using System.Reflection;

namespace SAIN.BotSettings.Categories
{
    public class SAINHearingSettings
    {
        static void InitHearing()
        {
            string name = "Audible Range Multiplier";
            string desc = "Modifies the distance that this bot can hear sounds.";
            string section = "Hearing";
            float def = 1f;
            float min = 0.25f;
            float max = 3f;
            float round = 100f;

            name = "Max Footstep Audio Distance";
            desc = "The Maximum Range that a bot can hear footsteps, in meters.";
            section = "Hearing";
            def = 50f;
            min = 5f;
            max = 100f;
            round = 1f;
        }
        public float AudibleRangeMultiplier = 1f;
        public float MaxFootstepAudioDistance = 50f;
    }
}
