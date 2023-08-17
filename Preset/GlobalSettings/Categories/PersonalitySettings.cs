using Comfort.Common;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings
{
    public class PersonalitySettings
    {
        [DefaultValue(false)] public bool AllGigaChads = false;
        [DefaultValue(false)] public bool AllChads = false;
        [DefaultValue(false)] public bool AllRats = false;

        public bool CheckForForceAllPers(out SAINPersonality result)
        {
            result = SAINPersonality.Normal;
            if (AllGigaChads)
            {
                result = SAINPersonality.GigaChad;
                return true;
            }
            if (AllChads)
            {
                result = SAINPersonality.Chad;
                return true;
            }
            if (AllRats)
            {
                result = SAINPersonality.Rat;
                return true;
            }
            return false;
        }

        public void Update()
        {
            if (AllGigaChads)
            {
                AllChads = false;
                AllRats = false;
            }
            if (AllChads)
            {
                AllGigaChads = false;
                AllRats = false;
            }
            if (AllRats)
            {
                AllGigaChads = false;
                AllChads = false;
            }
        }
    }
}