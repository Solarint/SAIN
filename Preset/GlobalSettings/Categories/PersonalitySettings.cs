using SAIN.Attributes;

namespace SAIN.Preset.GlobalSettings
{
    public class PersonalitySettings
    {
        [Default(false)]
        public bool AllGigaChads = false;

        [Default(false)]
        public bool AllChads = false;

        [Default(false)]
        public bool AllRats = false;

        public bool CheckForForceAllPers(out IPersonality result)
        {
            result = IPersonality.Normal;
            if (AllGigaChads)
            {
                result = IPersonality.GigaChad;
                return true;
            }
            if (AllChads)
            {
                result = IPersonality.Chad;
                return true;
            }
            if (AllRats)
            {
                result = IPersonality.Rat;
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