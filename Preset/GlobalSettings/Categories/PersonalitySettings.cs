using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings
{
    public class PersonalitySettings
    {
        [DefaultValue(false)]
        public bool AllChads = false;
        [DefaultValue(false)]
        public bool AllGigaChads = false;
        [DefaultValue(false)]
        public bool AllRats = false;
    }
}