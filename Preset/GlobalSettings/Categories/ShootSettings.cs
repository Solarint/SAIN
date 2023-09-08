using Newtonsoft.Json;
using SAIN.Attributes;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings
{
    public class ShootSettings
    {
        [Name("Global EFT Scatter Multiplier")]
        [Description("Higher = more scattering. Modifies EFT's default scatter feature. 1.5 = 1.5x more scatter")]
        [Default(1f)]
        [MinMax(0.01f, 10f, 100f)]
        public float GlobalScatterMultiplier = 1f;

        [Name("Global SAIN Recoil Scatter Multiplier")]
        [Description("Higher = more recoil. Modifies SAIN's new recoil scatter feature. 1.5 = 1.5x more scatter from recoil")]
        [Default(1f)]
        [MinMax(0.1f, 5f, 100f)]
        public float GlobalRecoilMultiplier = 1f;

        [Name("Max Recoil Per Shot")]
        [Description("Maximum Impulse force from a single shot for a bot.")]
        [Default(1.5f)]
        [MinMax(0.1f, 10f, 100f)]
        [Advanced]
        public float MaxRecoil = 1.5f;

        [Name("Add or Subtract Recoil")]
        [Description("Linearly add or subtract from the final recoil result")]
        [Default(-1f)]
        [MinMax(-10f, 10f, 100f)]
        [Advanced]
        public float AddRecoil = -1f;

        [Name("Recoil Decay p/frame")]
        [Description("How much to decay the recoil impulse per frame. 0.75 means 25% of the recoil will be removed per frame.")]
        [Default(0.75f)]
        [Percentage01to99]
        [Advanced]
        public float RecoilDecay = 0.75f;

        [Name("Ammo Shootability" )]
        [Description(
            "Lower is BETTER. " +
            "How Shootable this ammo type is, affects semi auto firerate and full auto burst length." +
            "Value is scaled but roughly gives a plus or minus 20% to firerate depending on the value set here." +
            "For Example. 9x19 will shoot about 20% faster fire-rate on semi-auto at 50 meters" +
            ", and fire 20% longer bursts when on full auto"
            )]
        [Percentage0to1(0.01f)]
        [Advanced]
        [DefaultDictionary(nameof(AmmoCaliberShootabilityDefaults))]
        public Dictionary<ICaliber, float> AmmoCaliberShootability = new Dictionary<ICaliber, float>(AmmoCaliberShootabilityDefaults);

        [JsonIgnore]
        [Hidden]
        public static readonly Dictionary<ICaliber, float> AmmoCaliberShootabilityDefaults = new Dictionary<ICaliber, float>()
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

        [Name("Weapon Shootability")]
        [Description(
            "Lower is BETTER. " +
            "How Shootable this weapon type is, affects semi auto firerate and full auto burst length." +
            "Value is scaled but roughly gives a plus or minus 20% to firerate depending on the value set here." +
            "For Example. SMGs will shoot about 20% faster fire-rate on semi-auto at 50 meters" +
            ", and fire 20% longer bursts when on full auto"
            )]
        [Percentage0to1(0.01f)]
        [Advanced]
        [DefaultDictionary(nameof(WeaponClassShootabilityDefaults))]
        public Dictionary<IWeaponClass, float> WeaponClassShootability = new Dictionary<IWeaponClass, float>(WeaponClassShootabilityDefaults);

        [JsonIgnore]
        [Hidden]
        public static readonly Dictionary<IWeaponClass, float> WeaponClassShootabilityDefaults = new Dictionary<IWeaponClass, float>()
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

        [Name("Weapon Firerate Wait Time")]
        [Description(
            "HIGHER is BETTER. " +
            "This is the time to wait inbetween shots for every meter." +
            "the number is divided by the distance to their target, to get a wait period between shots." +
            "For Example. With a setting of 100: " +
            "if a target is 50m away, they will wait 0.5 sec between shots because 50 / 100 is 0.5." +
            "This number is later modified by the Shootability multiplier, to get a final fire-rate that gets sent to a bot."
            )]
        [MinMax(30f, 250f, 1f)]
        [Advanced]
        [DefaultDictionary(nameof(WeaponPerMeterDefaults))]
        public Dictionary<IWeaponClass, float> WeaponPerMeter = new Dictionary<IWeaponClass, float>(WeaponPerMeterDefaults);

        [JsonIgnore]
        [Hidden]
        public static readonly Dictionary<IWeaponClass, float> WeaponPerMeterDefaults = new Dictionary<IWeaponClass, float>()
        {
            { IWeaponClass.Default, 120f },
            { IWeaponClass.assaultCarbine, 140 },
            { IWeaponClass.assaultRifle, 130 },
            { IWeaponClass.machinegun, 135 },
            { IWeaponClass.smg, 160 },
            { IWeaponClass.pistol, 65 },
            { IWeaponClass.marksmanRifle, 75 },
            { IWeaponClass.sniperRifle, 50 },
            { IWeaponClass.shotgun, 60 },
            { IWeaponClass.grenadeLauncher, 75 },
            { IWeaponClass.specialWeapon, 80 },
        };

        [Name("Bot Preferred Shoot Distances")]
        [Description(
            "The distances that a bot prefers to shoot a particular weapon class. " +
            "Bots will try to close the distance if further than this."
            )]
        [MinMax(10f, 250f, 1f)]
        [Advanced]
        [DefaultDictionary(nameof(EngagementDistanceDefaults))]
        public Dictionary<IWeaponClass, float> EngagementDistance = new Dictionary<IWeaponClass, float>(EngagementDistanceDefaults);

        [JsonIgnore]
        [Hidden]
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
        [Hidden]
        private const string Shootability = "Affects Weapon Shootability Calculations. ";

        [Description(Shootability)]
        [Advanced]
        [Default(0.35f)]
        [Percentage01to99]
        public float WeaponClassScaling = 0.35f;

        [Description(Shootability)]
        [Advanced]
        [Default(0.25f)]
        [Percentage01to99]
        public float RecoilScaling = 0.25f;

        [Description(Shootability)]
        [Advanced]
        [Default(0.1f)]
        [Percentage01to99]
        public float ErgoScaling = 0.1f;

        [Description(Shootability)]
        [Advanced]
        [Default(0.25f)]
        [Percentage01to99]
        public float AmmoCaliberScaling = 0.25f;

        [Description(Shootability)]
        [Advanced]
        [Default(0.4f)]
        [Percentage01to99]
        public float WeaponProficiencyScaling = 0.4f;

        [Description(Shootability)]
        [Advanced]
        [Default(0.35f)]
        [Percentage01to99]
        public float DifficultyScaling = 0.35f;
    }
}