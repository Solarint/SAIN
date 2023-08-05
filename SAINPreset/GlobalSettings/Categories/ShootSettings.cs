using BepInEx.Configuration;
using SAIN.SAINPreset.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mono.Security.X509.X520;
using static System.Collections.Specialized.BitVector32;

namespace SAIN.SAINPreset.Settings
{
    public class ShootSettings
    {
        [NameAndDescription(
            "Max Recoil Per Shot", 
            "Maximum Impulse force from a single shot for a bot.")]
        [DefaultValue(2f)]
        [MinMaxRound(0.1f,10f,100f)]
        [AdvancedOptions(true)]
        public float MaxRecoil = 2f;

        [NameAndDescription(
            "Add or Subtract Recoil",
            "Linearly add or subtract from the final recoil result")]
        [DefaultValue(0f)]
        [MinMaxRound(0.1f,5f,100f)]
        [AdvancedOptions(true)]
        public float AddRecoil = 0f;

        [NameAndDescription(
            "Recoil Decay p/frame",
            "How much to decay the recoil impulse per frame. 0.8 means 20% of the recoil will be removed per frame.")]
        [DefaultValue(0.8f)]
        [MinMaxRound(0.1f, 0.99f, 100f)]
        [AdvancedOptions(true)]
        public float RecoilDecay = 0.8f;

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
}
