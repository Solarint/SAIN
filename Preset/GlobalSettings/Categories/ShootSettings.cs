using Newtonsoft.Json;
using SAIN.Attributes;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings;

// note for later: Need to find a way to remove the clunky duplicates of the dictionaries here, as its a hold over from a previous system of getting the default values for different config options.
public class ShootSettings : SAINSettingsBase<ShootSettings>, ISAINSettings
{
    [Name("Base Minimum Time Between Shots")]
    [Category("General")]
    [MinMax(0.0f, 2f, 100f)]
    public float MIN_FIRE_RATE_INTERVAL = 0.1f; // minimum time between shots

    [Name("Base Maximum Time Between Shots")]
    [Category("General")]
    [MinMax(0.0f, 8f, 100f)]
    public float MAX_FIRE_RATE_INTERVAL = 4f; // maximum time between shots

    [Name("Fullauto Wait Multiplier")]
    [Description("If a bot is on fullauto, the time between shots will be multiplied by this value")]
    [Category("General")]
    [MinMax(0.0f, 1f, 100f)]
    public float MAX_FIRE_RATE_COEF_FULLAUTO = 0.25f; // maximum time between shots

    [Name("Firerate Randomization")]
    [Description("Randomize the time a bot waits between shots to make it less robotic. A value of 0.25 means it will multiply it by a random value between 0.75x - 1.25x")]
    [Category("General")]
    [MinMax(0.0f, 1f, 100f)]
    public float FIRERATE_RANDOMIZATION_COEF = 0.25f; // randomization coefficient for firerate

    [Name("Bots only use Semi Auto")]
    [Description("If enabled, bots will never be able to use full auto at close/mid range.")]
    [Category("General")]
    public bool ONLY_SEMIAUTO_TOGGLE = false;

    [Name("Global Recoil Multiplier")]
    [Description("Higher = more recoil. Modifies SAIN's recoil scatter feature. 1.5 = 1.5x more recoilfrom a single gunshot")]
    [Category("Bot Recoil")]
    [MinMax(0.01f, 3f, 100f)]
    public float BOT_RECOIL_COEF = 0.5f;

    [Name("Add or Subtract Recoil")]
    [Description("Linearly add or subtract from the final recoil result")]
    [Category("Bot Recoil")]
    [MinMax(-20f, 20f, 100f)]
    [Advanced]
    public float BOT_RECOIL_ADD = 0f;

    [Name("Recoil Decay Coefficient")]
    [Description("Controls the speed that bots will recover from a weapon's recoil. Higher = faster decay")]
    [Category("Bot Recoil")]
    [MinMax(0.01f, 1f, 100f)]
    [Advanced]
    public float BOT_RECOIL_DECAY_COEF = 0.3f;

    [Name("Bot Weapon Recoil Baseline")]
    [Description("A Bot's weapon's recoil will get divided by this to calculate how much to rotate their view on each shot, so, for example, " +
        "with a baseline of 100 - if a bot has 250 total recoil, that number gets divided by this to produce 2.5. Higher = Less recoil for bots. " +
        "This is basically an average recoil of all possible weapons.")]
    [Category("Bot Recoil")]
    [MinMax(25f, 300f, 1f)]
    [Advanced]
    public float BOT_RECOIL_BASELINE = 100;

    [Name("Bot Weapon Recoil Baseline - Realism Mod")]
    [Description("A Bot's weapon's recoil will get divided by this to calculate how much to rotate their view on each shot. This value is used if Realism mod is on to reflect different recoil numbers.")]
    [Category("Bot Recoil")]
    [MinMax(25f, 300f, 1f)]
    [Advanced]
    public float BOT_RECOIL_BASELINE_REALISM = 125f;

    [Name("Ammo Shootability")]
    [Description(
        "Lower is BETTER. " +
        "How Shootable this ammo type is, affects semi auto firerate and full auto burst length." +
        "Value is scaled but roughly gives a plus or minus 20% to firerate depending on the value set here." +
        "For Example. 9x19 will shoot about 20% faster fire-rate on semi-auto at 50 meters" +
        ", and fire 20% longer bursts when on full auto"
        )]
    [Category("Bot Weapon Control")]
    [Percentage0to1(0.01f)]
    [Advanced]
    [DefaultDictionary(nameof(AmmoCaliberShootabilityDefaults))]
    public Dictionary<ECaliber, float> AmmoCaliberShootability = new()
    {
        { ECaliber.Caliber9x18PM, 0.225f },
        { ECaliber.Caliber9x19PARA, 0.275f },
        { ECaliber.Caliber46x30, 0.325f },
        { ECaliber.Caliber9x21, 0.325f },
        { ECaliber.Caliber57x28, 0.35f },
        { ECaliber.Caliber762x25TT, 0.425f },
        { ECaliber.Caliber1143x23ACP, 0.425f },
        { ECaliber.Caliber9x33R, 0.65f },
        { ECaliber.Caliber545x39, 0.525f },
        { ECaliber.Caliber556x45NATO, 0.525f },
        { ECaliber.Caliber9x39, 0.6f },
        { ECaliber.Caliber762x35, 0.575f },
        { ECaliber.Caliber762x39, 0.675f },
        { ECaliber.Caliber366TKM, 0.675f },
        { ECaliber.Caliber762x51, 0.725f },
        { ECaliber.Caliber127x55, 0.775f },
        { ECaliber.Caliber762x54R, 0.85f },
        { ECaliber.Caliber86x70, 1.0f },
        { ECaliber.Caliber20g, 0.7f },
        { ECaliber.Caliber12g, 0.725f },
        { ECaliber.Caliber23x75, 0.85f },
        { ECaliber.Caliber26x75, 1f },
        { ECaliber.Caliber30x29, 1f },
        { ECaliber.Caliber40x46, 1f },
        { ECaliber.Caliber40mmRU, 1f },
        { ECaliber.Caliber127x108, 0.5f },
        { ECaliber.Caliber68x51, 0.6f },
        { ECaliber.Default, 0.5f },
    };

    [JsonIgnore]
    [Hidden]
    public static readonly Dictionary<ECaliber, float> AmmoCaliberShootabilityDefaults = new()
    {
        { ECaliber.Caliber9x18PM, 0.225f },
        { ECaliber.Caliber9x19PARA, 0.275f },
        { ECaliber.Caliber46x30, 0.325f },
        { ECaliber.Caliber9x21, 0.325f },
        { ECaliber.Caliber57x28, 0.35f },
        { ECaliber.Caliber762x25TT, 0.425f },
        { ECaliber.Caliber1143x23ACP, 0.425f },
        { ECaliber.Caliber9x33R, 0.65f },
        { ECaliber.Caliber545x39, 0.525f },
        { ECaliber.Caliber556x45NATO, 0.525f },
        { ECaliber.Caliber9x39, 0.6f },
        { ECaliber.Caliber762x35, 0.575f },
        { ECaliber.Caliber762x39, 0.675f },
        { ECaliber.Caliber366TKM, 0.675f },
        { ECaliber.Caliber762x51, 0.725f },
        { ECaliber.Caliber127x55, 0.775f },
        { ECaliber.Caliber762x54R, 0.85f },
        { ECaliber.Caliber86x70, 1.0f },
        { ECaliber.Caliber20g, 0.7f },
        { ECaliber.Caliber12g, 0.725f },
        { ECaliber.Caliber23x75, 0.85f },
        { ECaliber.Caliber26x75, 1f },
        { ECaliber.Caliber30x29, 1f },
        { ECaliber.Caliber40x46, 1f },
        { ECaliber.Caliber40mmRU, 1f },
        { ECaliber.Caliber127x108, 0.5f },
        { ECaliber.Caliber68x51, 0.6f },
        { ECaliber.Default, 0.5f },
    };

    [Name("Max FullAuto Distances")]
    [Description("The maximum distance a bot using this caliber can fire it full auto. Not all values are used since some calibers don't have any full auto weapons that use it.")]
    [Category("Bot Weapon Control")]
    [MinMax(10f, 150f)]
    [Advanced]
    [DefaultDictionary(nameof(AmmoCaliberFullAutoMaxDistancesDefaults))]
    public Dictionary<ECaliber, float> AmmoCaliberFullAutoMaxDistances = new()
    {
        { ECaliber.Caliber9x18PM, 80f },
        { ECaliber.Caliber9x19PARA, 80f },
        { ECaliber.Caliber46x30, 70f },
        { ECaliber.Caliber9x21, 70f },
        { ECaliber.Caliber57x28, 70f },
        { ECaliber.Caliber762x25TT, 70f },
        { ECaliber.Caliber1143x23ACP, 75f },
        { ECaliber.Caliber9x33R, 75f },
        { ECaliber.Caliber545x39, 65f },
        { ECaliber.Caliber556x45NATO, 65f },
        { ECaliber.Caliber9x39, 60f },
        { ECaliber.Caliber762x35, 55f },
        { ECaliber.Caliber762x39, 50f },
        { ECaliber.Caliber366TKM, 45f },
        { ECaliber.Caliber762x51, 45f },
        { ECaliber.Caliber127x55, 50f },
        { ECaliber.Caliber762x54R, 50f },
        { ECaliber.Caliber86x70, 40f },
        { ECaliber.Caliber20g, 30f },
        { ECaliber.Caliber12g, 30f },
        { ECaliber.Caliber23x75, 30f },
        { ECaliber.Caliber26x75, 30f },
        { ECaliber.Caliber30x29, 30f },
        { ECaliber.Caliber40x46, 30f },
        { ECaliber.Caliber40mmRU, 30f },
        { ECaliber.Caliber127x108, 30f },
        { ECaliber.Caliber68x51, 50f },
        { ECaliber.Default, 55f },
    };

    [JsonIgnore]
    [Hidden]
    public static readonly Dictionary<ECaliber, float> AmmoCaliberFullAutoMaxDistancesDefaults = new()
    {
        { ECaliber.Caliber9x18PM, 80f },
        { ECaliber.Caliber9x19PARA, 80f },
        { ECaliber.Caliber46x30, 70f },
        { ECaliber.Caliber9x21, 70f },
        { ECaliber.Caliber57x28, 70f },
        { ECaliber.Caliber762x25TT, 70f },
        { ECaliber.Caliber1143x23ACP, 75f },
        { ECaliber.Caliber9x33R, 75f },
        { ECaliber.Caliber545x39, 65f },
        { ECaliber.Caliber556x45NATO, 65f },
        { ECaliber.Caliber9x39, 60f },
        { ECaliber.Caliber762x35, 55f },
        { ECaliber.Caliber762x39, 50f },
        { ECaliber.Caliber366TKM, 45f },
        { ECaliber.Caliber762x51, 45f },
        { ECaliber.Caliber127x55, 50f },
        { ECaliber.Caliber762x54R, 50f },
        { ECaliber.Caliber86x70, 40f },
        { ECaliber.Caliber20g, 30f },
        { ECaliber.Caliber12g, 30f },
        { ECaliber.Caliber23x75, 30f },
        { ECaliber.Caliber26x75, 30f },
        { ECaliber.Caliber30x29, 30f },
        { ECaliber.Caliber40x46, 30f },
        { ECaliber.Caliber40mmRU, 30f },
        { ECaliber.Caliber127x108, 30f },
        { ECaliber.Caliber68x51, 50f },
        { ECaliber.Default, 55f },
    };

    [Name("Weapon Shootability")]
    [Description(
        "Lower is BETTER. " +
        "How Shootable this weapon type is, affects semi auto firerate and full auto burst length." +
        "Value is scaled but roughly gives a plus or minus 20% to firerate depending on the value set here." +
        "For Example. SMGs will shoot about 20% faster fire-rate on semi-auto at 50 meters" +
        ", and fire 20% longer bursts when on full auto"
        )]
    [Category("Bot Weapon Control")]
    [Percentage0to1(0.01f)]
    [Advanced]
    [DefaultDictionary(nameof(WeaponClassShootabilityDefaults))]
    public Dictionary<EWeaponClass, float> WeaponClassShootability = new()
    {
        { EWeaponClass.Default, 0.425f },
        { EWeaponClass.assaultCarbine, 0.5f },
        { EWeaponClass.assaultRifle, 0.5f },
        { EWeaponClass.machinegun, 0.15f },
        { EWeaponClass.smg, 0.25f },
        { EWeaponClass.pistol, 0.4f },
        { EWeaponClass.marksmanRifle, 0.75f },
        { EWeaponClass.sniperRifle, 1f },
        { EWeaponClass.shotgun, 0.75f },
        { EWeaponClass.grenadeLauncher, 1f },
        { EWeaponClass.specialWeapon, 1f },
    };

    [JsonIgnore]
    [Hidden]
    public static readonly Dictionary<EWeaponClass, float> WeaponClassShootabilityDefaults = new()
    {
        { EWeaponClass.Default, 0.425f },
        { EWeaponClass.assaultCarbine, 0.5f },
        { EWeaponClass.assaultRifle, 0.5f },
        { EWeaponClass.machinegun, 0.15f },
        { EWeaponClass.smg, 0.25f },
        { EWeaponClass.pistol, 0.4f },
        { EWeaponClass.marksmanRifle, 0.75f },
        { EWeaponClass.sniperRifle, 1f },
        { EWeaponClass.shotgun, 0.75f },
        { EWeaponClass.grenadeLauncher, 1f },
        { EWeaponClass.specialWeapon, 1f },
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
    [Category("Bot Weapon Control")]
    [Advanced]
    [DefaultDictionary(nameof(WeaponPerMeterDefaults))]
    public Dictionary<EWeaponClass, float> WeaponPerMeter = new()
    {
        { EWeaponClass.Default, 130f },
        { EWeaponClass.assaultCarbine, 150 },
        { EWeaponClass.assaultRifle, 140 },
        { EWeaponClass.machinegun, 180 },
        { EWeaponClass.smg, 160 },
        { EWeaponClass.pistol, 75 },
        { EWeaponClass.marksmanRifle, 85 },
        { EWeaponClass.sniperRifle, 60 },
        { EWeaponClass.shotgun, 80 },
        { EWeaponClass.grenadeLauncher, 90 },
        { EWeaponClass.specialWeapon, 80 },
    };

    [JsonIgnore]
    [Hidden]
    public static readonly Dictionary<EWeaponClass, float> WeaponPerMeterDefaults = new()
    {
        { EWeaponClass.Default, 130f },
        { EWeaponClass.assaultCarbine, 150 },
        { EWeaponClass.assaultRifle, 140 },
        { EWeaponClass.machinegun, 180 },
        { EWeaponClass.smg, 160 },
        { EWeaponClass.pistol, 75 },
        { EWeaponClass.marksmanRifle, 85 },
        { EWeaponClass.sniperRifle, 60 },
        { EWeaponClass.shotgun, 80 },
        { EWeaponClass.grenadeLauncher, 90 },
        { EWeaponClass.specialWeapon, 80 },
    };

    [Name("Bot Preferred Shoot Distances")]
    [Description(
        "The distances that a bot prefers to shoot a particular weapon class. " +
        "Bots will try to close the distance if further than this."
        )]
    [MinMax(10f, 250f, 1f)]
    [Category("Bot Weapon Control")]
    [Advanced]
    [DefaultDictionary(nameof(EngagementDistanceDefaults))]
    public Dictionary<EWeaponClass, float> EngagementDistance = new()
    {
        { EWeaponClass.Default, 125f },
        { EWeaponClass.assaultCarbine, 125f },
        { EWeaponClass.assaultRifle, 150f },
        { EWeaponClass.machinegun, 125f },
        { EWeaponClass.smg, 70f },
        { EWeaponClass.pistol, 50f },
        { EWeaponClass.marksmanRifle, 175f },
        { EWeaponClass.sniperRifle, 300f },
        { EWeaponClass.shotgun, 50f },
        { EWeaponClass.grenadeLauncher, 100f },
        { EWeaponClass.specialWeapon, 100f },
    };

    [JsonIgnore]
    [Hidden]
    public static readonly Dictionary<EWeaponClass, float> EngagementDistanceDefaults = new()
    {
        { EWeaponClass.Default, 125f },
        { EWeaponClass.assaultCarbine, 125f },
        { EWeaponClass.assaultRifle, 150f },
        { EWeaponClass.machinegun, 125f },
        { EWeaponClass.smg, 70f },
        { EWeaponClass.pistol, 50f },
        { EWeaponClass.marksmanRifle, 175f },
        { EWeaponClass.sniperRifle, 300f },
        { EWeaponClass.shotgun, 50f },
        { EWeaponClass.grenadeLauncher, 100f },
        { EWeaponClass.specialWeapon, 100f },
    };

    [JsonIgnore]
    [Hidden]
    private const string Shootability = "Affects Weapon Shootability Calculations, which tells a bot how fast they should shoot, how long they should hold down full auto fire, and how far they will decide to fire full auto or swap to only semi-auto.. ";

    private const string Scaling = "Affects how much this type of effect has on how Shootable a bot's weapon is considered. Higher = More effect. So if you set RecoilScaling to 0, recoil will play no part in how fast or slow a bot shoots.";

    [Description(Shootability + Scaling)]
    [Category("Bot Weapon Control")]
    [Advanced]
    [Percentage01to99]
    public float WeaponClassScaling = 0.25f;

    [Description(Shootability + Scaling)]
    [Category("Bot Weapon Control")]
    [Advanced]
    [Percentage01to99]
    public float RecoilScaling = 0.35f;

    [Description(Shootability + Scaling)]
    [Category("Bot Weapon Control")]
    [Advanced]
    [Percentage01to99]
    public float ErgoScaling = 0.08f;

    [Description(Shootability + Scaling)]
    [Category("Bot Weapon Control")]
    [Advanced]
    [Percentage01to99]
    public float AmmoCaliberScaling = 0.3f;

    [Description(Shootability + Scaling)]
    [Category("Bot Weapon Control")]
    [Advanced]
    [Percentage01to99]
    public float WeaponProficiencyScaling = 0.3f;

    [Description(Shootability + Scaling)]
    [Category("Bot Weapon Control")]
    [Advanced]
    [Percentage01to99]
    public float DifficultyScaling = 0.3f;

    public override void Init(List<ISAINSettings> list)
    {
        list.Add(this);
    }
}