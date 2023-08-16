using SAIN.Attributes;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings
{
    public class GeneralSettings
    {
        [NameAndDescription(
            "Global Difficulty Modifier",
            "Higher number = harder bots. Affects bot accuracy, recoil, fire-rate, full auto burst lenght, scatter, reaction-time")]
        [DefaultValue(1f)]
        [MinMax(0.1f, 5f, 100f)]
        public float GlobalDifficultyModifier = 1f;

        [NameAndDescription(
            "No Bush ESP",
            "Adds extra vision check for bots to help prevent bots seeing or shooting through foliage.")]
        [DefaultValue(true)]
        public bool NoBushESPToggle = true;

        [NameAndDescription(
            "No Bush ESP Enhanced Raycast Frequency p/ Second",
            "How often to check for foliage vision blocks")]
        [DefaultValue(0.1f)]
        [MinMax(0f, 1f, 100f)]
        public float NoBushESPFrequency = 0.1f;

        [NameAndDescription(
            "No Bush ESP Enhanced Raycasts",
            "Experimental: Increased Accuracy and extra checks")]
        [DefaultValue(false)]
        public bool NoBushESPEnhanced = false;

        [NameAndDescription(
            "No Bush ESP Enhanced Raycasts Ratio",
            "Experimental: Increased Accuracy and extra checks. Sets the ratio of visible to not visible body parts to block vision. " +
            "0.5 means half the body parts of the player must be visible to block vision.")]
        [DefaultValue(0.5f)]
        [MinMax(0.2f, 1f, 10f)]
        public float NoBushESPEnhancedRatio = 0.5f;

        [NameAndDescription(
            "No Bush ESP Debug")]
        [DefaultValue(false)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public bool NoBushESPDebugMode = false;

        [NameAndDescription(
            "HeadShot Protection",
            "Experimental, will kick bot's aiming target if it ends up on the player's head.")]
        [DefaultValue(false)]
        public bool HeadShotProtection = false;
    }
}