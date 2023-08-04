using BepInEx.Configuration;
using EFT.InventoryLogic;
using SAIN.Editor;
using SAIN.SAINPreset.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using static Mono.Security.X509.X520;

namespace SAIN.SAINPreset.Settings
{
    public class HearingSettings
    {
        [Name("Suppressed Sound Modifier")]
        [Description("Audible Gun Range is multiplied by this number when using a suppressor")]
        [DefaultValue(0.6f)]
        [Minimum(0.1f)]
        [Maximum(0.95f)]
        [Rounding(100f)]
        public float SuppressorModifier = 0.6f;

        [Name("Subsonic Sound Modifier")]
        [Description("Audible Gun Range is multiplied by this number when using a suppressor and subsonic ammo")]
        [DefaultValue(0.25f)]
        [Minimum(0.1f)]
        [Maximum(0.95f)]
        [Rounding(100f)]
        public float SubsonicModifier = 0.25f;

        public AmmoSettingDictionary AudibleRanges =
            new AmmoSettingDictionary(
                new Dictionary<Caliber, float>
                {
                    {Caliber.Caliber9x18PM, 125f},
                    {Caliber.Caliber9x19PARA, 125f},
                    {Caliber.Caliber46x30, 135},
                    {Caliber.Caliber9x21, 130},
                    {Caliber.Caliber57x28, 140},
                    {Caliber.Caliber762x25TT, 140},
                    {Caliber.Caliber1143x23ACP, 140},
                    {Caliber.Caliber9x33R, 130},
                    {Caliber.Caliber545x39, 180},
                    {Caliber.Caliber556x45NATO, 180},
                    {Caliber.Caliber9x39, 180},
                    {Caliber.Caliber762x35, 180},
                    {Caliber.Caliber762x39, 200},
                    {Caliber.Caliber366TKM, 200},
                    {Caliber.Caliber762x51, 225},
                    {Caliber.Caliber127x55, 225},
                    {Caliber.Caliber762x54R, 275},
                    {Caliber.Caliber86x70, 300},
                    {Caliber.Caliber20g, 225},
                    {Caliber.Caliber12g, 225},
                    {Caliber.Caliber23x75, 210},
                    {Caliber.Caliber26x75, 50},
                    {Caliber.Caliber30x29, 50},
                    {Caliber.Caliber40x46, 50},
                    {Caliber.Caliber40mmRU, 50}
                }
        );
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

    public enum Caliber
    {
        Caliber9x18PM,
        Caliber9x19PARA,
        Caliber46x30,
        Caliber9x21,
        Caliber57x28,
        Caliber762x25TT,
        Caliber1143x23ACP,
        Caliber9x33R,
        Caliber545x39,
        Caliber556x45NATO,
        Caliber9x39,
        Caliber762x35,
        Caliber762x39,
        Caliber366TKM,
        Caliber762x51,
        Caliber127x55,
        Caliber762x54R,
        Caliber86x70,
        Caliber20g,
        Caliber12g,
        Caliber23x75,
        Caliber26x75,
        Caliber30x29,
        Caliber40x46,
        Caliber40mmRU
    }

    public enum WeaponClass
    {
        AssaultRifle,
        AssaultCarbine,
        Machinegun,
        SMG,
        Pistol,
        MarksmanRifle,
        SniperRifle,
        Shotgun,
        GrenadeLauncher,
        SpecialWeapon
    }

}