using BepInEx.Configuration;
using SAIN.SAINPreset.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.SAINPreset.Settings
{
    public class GeneralSettings
    {
        [Name("No Bush ESP")]
        [Description("Adds extra vision check for bots to help prevent bots seeing or shooting through foliage.")]
        [DefaultValue(true)]
        public bool NoBushESPToggle = true;

        [Name("No Bush ESP Enhanced Raycast Frequency p/ Second")]
        [Description("How often to check for foliage vision blocks")]
        [DefaultValue(0.1f)]
        [Minimum(0f)]
        [Maximum(1f)]
        [Rounding(100f)]
        public float NoBushESPFrequency = 0.1f;

        [Name("No Bush ESP Enhanced Raycasts")]
        [Description("Experimental: Increased Accuracy and extra checks")]
        [DefaultValue(false)]
        public bool NoBushESPEnhanced = false;

        [Name("No Bush ESP Enhanced Raycasts Ratio")]
        [Description("Experimental: Increased Accuracy and extra checks. Sets the ratio of visible to not visible body parts to block vision. 0.5 means half the body parts of the player must be visible to block vision.")]
        [DefaultValue(0.5f)]
        [Minimum(0.2f)]
        [Maximum(1f)]
        [Rounding(10f)]
        public float NoBushESPEnhancedRatio = 0.5f;

        [Name("No Bush ESP Debug")]
        [DefaultValue(false)]
        [IsAdvanced(true)]
        public bool NoBushESPDebugMode = false;

        [Name("HeadShot Protection")]
        [Description("Experimental, will kick bot's aiming target if it ends up on the player's head.")]
        [DefaultValue(false)]
        public bool HeadShotProtection = false;

        [DefaultValue(false)]
        [IsAdvanced(true)]
        public bool DrawDebugGizmos = false;
    }
}
