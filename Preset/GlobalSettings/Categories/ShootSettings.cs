using EFT.InventoryLogic;
using Newtonsoft.Json;
using SAIN.Attributes;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings
{
    public class ShootSettings
    {
        [Name(
            "Global EFT Scatter Multiplier"
            )]
        [Description(
            "Higher = more scattering. Modifies EFT's default scatter feature. 1.5 = 1.5x more scatter"
            )]
        [Default(1f)]
        [MinMax(0.1f, 10f, 100f)]
        public float GlobalScatterMultiplier = 1f;

        [Name(
            "Global SAIN Recoil Scatter Multiplier"
            )]
        [Description(
            "Higher = more recoil. Modifies SAIN's new recoil scatter feature. 1.5 = 1.5x more scatter from recoil"
            )]
        [Default(1f)]
        [MinMax(0.1f, 5f, 100f)]
        public float GlobalRecoilMultiplier = 1f;

        [Name(
            "Max Recoil Per Shot"
            )]
        [Description(
            "Maximum Impulse force from a single shot for a bot."
            )]
        [Default(1.5f)]
        [MinMax(0.1f, 10f, 100f)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public float MaxRecoil = 1.5f;

        [Name(
            "Add or Subtract Recoil"
            )]
        [Description(
            "Linearly add or subtract from the final recoil result"
            )]
        [Default(-1f)]
        [MinMax(-10f, 10f, 100f)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public float AddRecoil = -1f;

        [Name(
            "Recoil Decay p/frame"
            )]
        [Description(
            "How much to decay the recoil impulse per frame. 0.75 means 25% of the recoil will be removed per frame."
            )]
        [Default(0.75f)]
        [MinMax(0.1f, 0.99f, 100f)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public float RecoilDecay = 0.75f;

        [Name(
            "Ammo Shootability"
            )]
        [Description(
            "Lower is BETTER. " +
            "How Shootable this ammo type is, affects semi auto firerate and full auto burst length." +
            "Value is scaled but roughly gives a plus or minus 20% to firerate depending on the value set here." +
            "For Example. 9x19 will shoot about 20% faster fire-rate on semi-auto at 50 meters" +
            ", and fire 20% longer bursts when on full auto"
            )]
        [MinMax(0.01f, 1f, 100f)]
        [Advanced]
        [Dictionary(typeof(ICaliber), typeof(float))]
        public Dictionary<ICaliber, float> AmmoShootability = new Dictionary<ICaliber, float>(AmmoShootabilityDefaults);

        [JsonIgnore]
        [Advanced(AdvancedEnum.Hidden)]
        public static readonly Dictionary<ICaliber, float> AmmoShootabilityDefaults = new Dictionary<ICaliber, float>()
        {
            { ICaliber.Caliber9x18PM, 0.2f },
            { ICaliber.Caliber9x19PARA, 0.25f },
            { ICaliber.Caliber46x30, 0.3f },
            { ICaliber.Caliber9x21, 0.3f },
            { ICaliber.Caliber57x28, 0.35f },
            { ICaliber.Caliber762x25TT, 0.4f },
            { ICaliber.Caliber1143x23ACP, 0.4f },
            { ICaliber.Caliber9x33R, 0.65f },
            { ICaliber.Caliber545x39, 0.5f },
            { ICaliber.Caliber556x45NATO, 0.5f },
            { ICaliber.Caliber9x39, 0.55f },
            { ICaliber.Caliber762x35, 0.55f },
            { ICaliber.Caliber762x39, 0.65f },
            { ICaliber.Caliber366TKM, 0.65f },
            { ICaliber.Caliber762x51, 0.7f },
            { ICaliber.Caliber127x55, 0.75f },
            { ICaliber.Caliber762x54R, 0.8f },
            { ICaliber.Caliber86x70, 1.0f },
            { ICaliber.Caliber20g, 0.65f },
            { ICaliber.Caliber12g, 0.7f },
            { ICaliber.Caliber23x75, 0.75f },
            { ICaliber.Caliber26x75, 1f },
            { ICaliber.Caliber30x29, 1f },
            { ICaliber.Caliber40x46, 1f },
            { ICaliber.Caliber40mmRU, 1f },
            { ICaliber.Caliber127x108, 0.25f },
            { ICaliber.Default, 0.5f },
        };

        [Name(
            "Weapon Shootability"
            )]
        [Description(
            "Lower is BETTER. " +
            "How Shootable this weapon type is, affects semi auto firerate and full auto burst length." +
            "Value is scaled but roughly gives a plus or minus 20% to firerate depending on the value set here." +
            "For Example. SMGs will shoot about 20% faster fire-rate on semi-auto at 50 meters" +
            ", and fire 20% longer bursts when on full auto"
            )]
        [MinMax(0.01f, 1f, 100f)]
        [Advanced]
        [Dictionary(typeof(IWeaponClass), typeof(float))]
        public Dictionary<IWeaponClass, float> WeaponShootability = new Dictionary<IWeaponClass, float>(WeaponShootabilityDefaults);

        [JsonIgnore]
        [Advanced(AdvancedEnum.Hidden)]
        public static readonly Dictionary<IWeaponClass, float> WeaponShootabilityDefaults = new Dictionary<IWeaponClass, float>()
        {
            { IWeaponClass.Default, 0.4f },
            { IWeaponClass.assaultCarbine, 0.35f },
            { IWeaponClass.assaultRifle, 0.4f },
            { IWeaponClass.machinegun, 0.3f },
            { IWeaponClass.smg, 0.2f },
            { IWeaponClass.pistol, 0.4f },
            { IWeaponClass.marksmanRifle, 0.75f },
            { IWeaponClass.sniperRifle, 1f },
            { IWeaponClass.shotgun, 0.75f },
            { IWeaponClass.grenadeLauncher, 1f },
            { IWeaponClass.specialWeapon, 1f },
        };

        [Name(
            "Bot Preferred Shoot Distances"
            )]
        [Description(
            "The distances that a bot prefers to shoot a particular weapon class. " +
            "Bots will try to close the distance if further than this."
            )]
        [MinMax(10f, 250f, 1f)]
        [Dictionary(typeof(IWeaponClass), typeof(float))]
        [Advanced]
        public Dictionary<IWeaponClass, float> EngagementDistance = new Dictionary<IWeaponClass, float>(EngagementDistanceDefaults);

        [JsonIgnore]
        [Advanced(AdvancedEnum.Hidden)]
        public static readonly Dictionary<IWeaponClass, float> EngagementDistanceDefaults = new Dictionary<IWeaponClass, float>()
        {
            { IWeaponClass.Default, 75f },
            { IWeaponClass.assaultCarbine, 75f },
            { IWeaponClass.assaultRifle, 100f },
            { IWeaponClass.machinegun, 85f },
            { IWeaponClass.smg, 40f },
            { IWeaponClass.pistol, 25f },
            { IWeaponClass.marksmanRifle, 110f },
            { IWeaponClass.sniperRifle, 175f },
            { IWeaponClass.shotgun, 25f },
            { IWeaponClass.grenadeLauncher, 65f },
            { IWeaponClass.specialWeapon, 50f },
        };

        [JsonIgnore]
        [Advanced(AdvancedEnum.Hidden)]
        private const string Shootability = "Affects Weapon Shootability Calculations. ";

        [Description(Shootability)]
        [Advanced]
        [Default(0.35f)]
        [Percentage0to1(0.01f, 0.65f)]
        public float WeaponClassScaling = 0.35f;

        [Description(Shootability)]
        [Advanced]
        [Default(0.25f)]
        [Percentage0to1(0.01f, 0.65f)]
        public float RecoilScaling = 0.25f;

        [Description(Shootability)]
        [Advanced]
        [Default(0.1f)]
        [Percentage0to1(0.01f, 0.65f)]
        public float ErgoScaling = 0.1f;

        [Description(Shootability)]
        [Advanced]
        [Default(0.25f)]
        [Percentage0to1(0.01f, 0.65f)]
        public float AmmoCaliberScaling = 0.25f;

        [Description(Shootability)]
        [Advanced]
        [Default(0.4f)]
        [Percentage0to1(0.01f, 0.65f)]
        public float WeaponProficiencyScaling = 0.4f;

        [Description(Shootability)]
        [Advanced]
        [Default(0.35f)]
        [Percentage0to1(0.01f, 0.65f)]
        public float DifficultyScaling = 0.35f;
    }
}