using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAIN.SAINPreset.Settings;
using SAIN.SAINPreset.Settings.BotConfig;

namespace SAIN.SAINPreset
{
    public class SAINPresetClass
    {
        public SAINPresetDefinition Info = new SAINPresetDefinition();

        // Global Config Settings
        public GeneralSettings General = new GeneralSettings();

        public PersonalitySettings Personality = new PersonalitySettings();

        public HearingSettings Hearing = new HearingSettings();

        public ExtractSettings Extract = new ExtractSettings();

        public BotConfigSettings BotConfig = new BotConfigSettings();
    }
    public class SAINPresetDefinition
    {
        public string Key;
        public string DisplayName;
        public string Description;
        public string Creator;
        public string SAINVersion;
        public string DateCreated;
    }
}
