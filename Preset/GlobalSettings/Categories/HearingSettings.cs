using SAIN.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings
{
    public class HearingSettings
    {
        [NameAndDescription(
            "Suppressed Sound Modifier",
            "Audible Gun Range is multiplied by this number when using a suppressor")]
        [DefaultValue(0.6f)]
        [MinMaxRound(0.1f, 0.95f, 100f)]
        public float SuppressorModifier = 0.6f;

        [NameAndDescription(
            "Subsonic Sound Modifier",
            "Audible Gun Range is multiplied by this number when using a suppressor and subsonic ammo")]
        [DefaultValue(0.25f)]
        [MinMaxRound(0.1f, 0.95f, 100f)]
        public float SubsonicModifier = 0.25f;

        [List]
        [AdvancedOptions(true)]
        public AudibleRangesClass AudibleRanges = new AudibleRangesClass();
    }

    public class AmmoSettingDictionary
    {
        public AmmoSettingDictionary(Dictionary<Caliber, float> valueDictionary)
        {
            ValueDictionary = valueDictionary;
        }

        public void Set(Caliber caliber, float value)
        {
            ValueDictionary[caliber] = value;
        }

        public float Get(Caliber caliber)
        {
            return ValueDictionary[caliber];
        }

        public Dictionary<Caliber, float> ValueDictionary;
    }

    public class WeaponSettingsDictionary
    {
        public WeaponSettingsDictionary(Dictionary<WeaponClass, float> valueDictionary)
        {
            ValueDictionary = valueDictionary;
        }

        public void Set(WeaponClass caliber, float value)
        {
            ValueDictionary[caliber] = value;
        }

        public float Get(WeaponClass caliber)
        {
            return ValueDictionary[caliber];
        }

        public Dictionary<WeaponClass, float> ValueDictionary;
    }
}