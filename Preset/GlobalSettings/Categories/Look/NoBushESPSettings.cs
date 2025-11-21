using SAIN.Attributes;

namespace SAIN.Preset.GlobalSettings;

public class NoBushESPSettings : SAINSettingsBase<NoBushESPSettings>, ISAINSettings
{
    [Name("No Bush ESP")]
    [Description("Adds extra vision check for bots to help prevent bots seeing or shooting through foliage.")]
    public bool NoBushESPToggle = true;

    [Name("No Bush ESP Enhanced Raycasts")]
    [Description("Experimental: Increased Accuracy and extra checks")]
    public bool NoBushESPEnhanced = false;

    [Name("No Bush ESP Enhanced Raycast Frequency p/ Second")]
    [Description("Experimental: How often to check for foliage vision blocks")]
    [MinMax(0f, 1f, 100f)]
    [Advanced]
    public float NoBushESPFrequency = 0.1f;

    [Name("No Bush ESP Enhanced Raycasts Ratio")]
    [Description("Experimental: Increased Accuracy and extra checks. " +
        "Sets the ratio of visible to not visible body parts to not block vision. " +
        "0.75 means half the body parts of the player must be visible to not block vision.")]
    [MinMax(0.1f, 1f, 100f)]
    [Advanced]
    public float NoBushESPEnhancedRatio = 0.75f;

    [Name("No Bush ESP Debug")]
    [Advanced]
    public bool NoBushESPDebugMode = false;
}