using SAIN.Attributes;

namespace SAIN.Preset.GlobalSettings
{
    public class GeneralSettings
    {
        [Name("Bot Grenades")]
        [Default(true)]
        public bool BotsUseGrenades = true;

        [Name("Global Difficulty Modifier")]
        [Description("Higher number = harder bots. Affects bot accuracy, recoil, fire-rate, full auto burst lenght, scatter, reaction-time")]
        [Default(1f)]
        [MinMax(0.1f, 5f, 100f)]
        public float GlobalDifficultyModifier = 1f;

        [Name("No Bush ESP")]
        [Description("Adds extra vision check for bots to help prevent bots seeing or shooting through foliage.")]
        [Default(true)]
        public bool NoBushESPToggle = true;

        [Name("Enhanced Cover Finding - Experimental")]
        [Description("CAN REDUCE PERFORMANCE. Improves bot reactions in a fight by decreasing the time it takes to find cover, can help with bots standing still occasionally before running for cover. Comes at the cost of some reduced performance overall.")]
        [Default(false)]
        public bool EnhancedCoverFinding = false;

        [Name("No Bush ESP Enhanced Raycasts")]
        [Description("Experimental: Increased Accuracy and extra checks")]
        [Default(false)]
        [Advanced]
        public bool NoBushESPEnhanced = false;

        [Name("No Bush ESP Enhanced Raycast Frequency p/ Second")]
        [Description("Experimental: How often to check for foliage vision blocks")]
        [Default(0.1f)]
        [MinMax(0f, 1f, 100f)]
        [Advanced]
        public float NoBushESPFrequency = 0.1f;

        [Name("No Bush ESP Enhanced Raycasts Ratio")]
        [Description("Experimental: Increased Accuracy and extra checks. " +
            "Sets the ratio of visible to not visible body parts to not block vision. " +
            "0.5 means half the body parts of the player must be visible to not block vision.")]
        [Default(0.5f)]
        [MinMax(0.2f, 1f, 10f)]
        [Advanced]
        public float NoBushESPEnhancedRatio = 0.5f;

        [Name("No Bush ESP Debug")]
        [Default(false)]
        [Advanced]
        public bool NoBushESPDebugMode = false;

        [Name("HeadShot Protection")]
        [Description("Experimental, will move bot's aiming target if it ends up on the player's head. NOT FOOLPROOF. It's more of a strong suggestion rather than a hard limit. If you find you are dying to headshots too frequently still, I recommend increasing your head health with another mod.")]
        [Default(false)]
        public bool HeadShotProtection = false;
    }
}