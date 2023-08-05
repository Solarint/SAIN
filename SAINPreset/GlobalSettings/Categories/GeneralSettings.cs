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
        [NameAndDescription(
            "No Bush ESP",
            "Adds extra vision check for bots to help prevent bots seeing or shooting through foliage.")]
        [DefaultValue(true)]
        public bool NoBushESPToggle = true;

        [NameAndDescription(
            "No Bush ESP Enhanced Raycast Frequency p/ Second", 
            "How often to check for foliage vision blocks")]
        [DefaultValue(0.1f)]
        [MinMaxRound(0f, 1f, 100f)]
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
        [MinMaxRound(0.2f, 1f, 10f)]
        public float NoBushESPEnhancedRatio = 0.5f;

        [NameAndDescription(
            "No Bush ESP Debug")]
        [DefaultValue(false)]
        [AdvancedOptions(true)]
        public bool NoBushESPDebugMode = false;

        [NameAndDescription(
            "HeadShot Protection", 
            "Experimental, will kick bot's aiming target if it ends up on the player's head.")]
        [DefaultValue(false)]
        public bool HeadShotProtection = false;

        [DefaultValue(false)]
        [AdvancedOptions(true)]
        public bool DrawDebugGizmos = false;
    }
}
