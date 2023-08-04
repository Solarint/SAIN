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

        [Name("HeadShot Protection")]
        [Description("Experimental, will kick bot's aiming target if it ends up on the player's head.")]
        [DefaultValue(false)]
        public bool HeadShotProtection = false;

        [DefaultValue(false)]
        [IsAdvanced(true)]
        public bool DrawDebugGizmos = false;
    }
}
