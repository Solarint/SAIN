using BepInEx.Configuration;
using EFT.InventoryLogic;
using System.Collections.Generic;

namespace SAIN.SAINPreset.Settings
{
    public class HearingSettings
    {
        public static float AudibleRangeMultiplier;
        public static float MaxFootstepAudioDistance;

        public static AmmoSettingDictionary AudibleRanges =
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

    public class WeaponAmmoClassSettings
    {
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

        public AmmoSettingDictionary AmmoShootability =
            new AmmoSettingDictionary(
                new Dictionary<Caliber, float>
                {
                    {Caliber.Caliber9x18PM, 0.2f},
                    {Caliber.Caliber9x19PARA, 0.25f},
                    {Caliber.Caliber46x30, 0.3f},
                    {Caliber.Caliber9x21, 0.3f},
                    {Caliber.Caliber57x28, 0.35f},
                    {Caliber.Caliber762x25TT, 0.4f},
                    {Caliber.Caliber1143x23ACP, 0.4f},
                    {Caliber.Caliber9x33R, 0.65f},
                    {Caliber.Caliber545x39, 0.5f},
                    {Caliber.Caliber556x45NATO, 0.5f},
                    {Caliber.Caliber9x39, 0.55f},
                    {Caliber.Caliber762x35, 0.55f},
                    {Caliber.Caliber762x39, 0.65f},
                    {Caliber.Caliber366TKM, 0.65f},
                    {Caliber.Caliber762x51, 0.7f},
                    {Caliber.Caliber127x55, 0.75f},
                    {Caliber.Caliber762x54R, 0.8f},
                    {Caliber.Caliber86x70, 1.0f},
                    {Caliber.Caliber20g, 0.65f},
                    {Caliber.Caliber12g, 0.7f},
                    {Caliber.Caliber23x75, 0.75f},
                    {Caliber.Caliber26x75, 1f},
                    {Caliber.Caliber30x29, 1f},
                    {Caliber.Caliber40x46, 1f},
                    {Caliber.Caliber40mmRU, 1f}
                }
        );

        public WeaponSettingsDictionary WeaponShootability =
            new WeaponSettingsDictionary(
                new Dictionary<WeaponClass, float>
                {
                    {WeaponClass.AssaultRifle, 0.25f},
                    {WeaponClass.AssaultCarbine, 0.3f},
                    {WeaponClass.Machinegun, 0.25f},
                    {WeaponClass.SMG, 0.2f},
                    {WeaponClass.Pistol, 0.4f},
                    {WeaponClass.MarksmanRifle, 0.5f},
                    {WeaponClass.SniperRifle, 0.75f},
                    {WeaponClass.Shotgun, 0.5f},
                    {WeaponClass.GrenadeLauncher, 1f},
                    {WeaponClass.SpecialWeapon, 1f}
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

        public Dictionary<WeaponClass, float> WeaponShootability = new Dictionary<WeaponClass, float>()
        {
            {WeaponClass.AssaultRifle, 0.25f},
            {WeaponClass.AssaultCarbine, 0.3f},
            {WeaponClass.Machinegun, 0.25f},
            {WeaponClass.SMG, 0.2f},
            {WeaponClass.Pistol, 0.4f},
            {WeaponClass.MarksmanRifle, 0.5f},
            {WeaponClass.SniperRifle, 0.75f}, // Note: you may want to handle the VSS and VAL exception separately
            {WeaponClass.Shotgun, 0.5f},
            {WeaponClass.GrenadeLauncher, 1f},
            {WeaponClass.SpecialWeapon, 1f}
        };
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