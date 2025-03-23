using Newtonsoft.Json;
using SAIN.Attributes;
using SAIN.Helpers;
using SAIN.SAINComponent.Classes;
using System;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings
{
    public class HearingSettings : SAINSettingsBase<HearingSettings>, ISAINSettings
    {
        static HearingSettings()
        {
            // Hearing Dispersion
            HEAR_DISPERSION_VALUES_Defaults = new Dictionary<SAINSoundType, float>()
            {
                { SAINSoundType.Shot, 17.5f },
                { SAINSoundType.SuppressedShot, 13.5f },
                { SAINSoundType.FootStep, 12.5f },
            };
            const float defaultDispersion = 12.5f;
            Helpers.ListHelpers.PopulateKeys(HEAR_DISPERSION_VALUES_Defaults, defaultDispersion);

            // Gunfire Hearing Distances
            HearingDistancesDefaults = new Dictionary<ECaliber, float>()
            {
                { ECaliber.Caliber9x18PM, 110f },
                { ECaliber.Caliber9x19PARA, 110f },
                { ECaliber.Caliber46x30, 120f },
                { ECaliber.Caliber9x21, 120f },
                { ECaliber.Caliber57x28, 120f },
                { ECaliber.Caliber762x25TT, 120f },
                { ECaliber.Caliber1143x23ACP, 115f },
                { ECaliber.Caliber9x33R, 125 },
                { ECaliber.Caliber545x39, 160 },
                { ECaliber.Caliber556x45NATO, 160 },
                { ECaliber.Caliber9x39, 160 },
                { ECaliber.Caliber762x35, 175 },
                { ECaliber.Caliber762x39, 175 },
                { ECaliber.Caliber366TKM, 175 },
                { ECaliber.Caliber762x51, 200f },
                { ECaliber.Caliber127x55, 200f },
                { ECaliber.Caliber762x54R, 225f },
                { ECaliber.Caliber86x70, 250f },
                { ECaliber.Caliber20g, 185 },
                { ECaliber.Caliber12g, 185 },
                { ECaliber.Caliber23x75, 210 },
                { ECaliber.Caliber26x75, 50 },
                { ECaliber.Caliber30x29, 50 },
                { ECaliber.Caliber40x46, 50 },
                { ECaliber.Caliber40mmRU, 50 },
                { ECaliber.Caliber127x108, 300 },
                { ECaliber.Caliber68x51, 200f },
                { ECaliber.Default, 125 },
            };
            const float defaultDistance = 125;
            Helpers.ListHelpers.PopulateKeys(HearingDistancesDefaults, defaultDistance);
        }

        [Name("Rain Sound Multiplier - Outdoors")]
        [Description("If it is raining, reduce heard distances by up to X amount. Depending on intensity of rain. Scales linearly with rain value.")]
        [Category("Hearing Distance")]
        [MinMax(0.01f, 1f, 1000f)]
        public float RAIN_SOUND_COEF_OUTSIDE = 0.5f;

        [Name("Rain Sound Multiplier - Inside Building")]
        [Description("If it is raining, reduce heard distances by up to X amount. Depending on intensity of rain. Scales linearly with rain value.")]
        [Category("Hearing Distance")]
        [MinMax(0.01f, 1f, 1000f)]
        public float RAIN_SOUND_COEF_INSIDE = 0.75f;

        [Name("Max Footstep Audio Distance")]
        [Description("The Maximum Range that a bot can hear footsteps, sprinting, and jumping, turning, gear sounds, and any movement related sounds, in meters. " +
            "This is a theoretical max range, actual range heavily changes depending on conditions.")]
        [Category("Hearing Distance")]
        [MinMax(10f, 150f, 100f)]
        public float MaxFootstepAudioDistance = 70f;

        [Name("Max Footstep Audio Distance without Headphones")]
        [Description("The Maximum Range that a bot can hear footsteps, sprinting, and jumping, turning, gear sounds, and any movement related sounds, in meters. " +
            "This is a theoretical max range, actual range heavily changes depending on conditions.")]
        [Category("Hearing Distance")]
        [MinMax(10f, 150f, 100f)]
        public float MaxFootstepAudioDistanceNoHeadphones = 50f;

        [Name("Hearing Randomization and Estimation")]
        [Description(_dispersion_descr)]
        [Category("Position Randomization")]
        [Advanced]
        [MinMax(1f, 100f, 1000f)]
        [DefaultDictionary(nameof(HEAR_DISPERSION_VALUES_Defaults))]
        public Dictionary<SAINSoundType, float> HEAR_DISPERSION_VALUES = new Dictionary<SAINSoundType, float>
        {
            { SAINSoundType.Shot, 17.5f },
            { SAINSoundType.SuppressedShot, 13.5f },
            { SAINSoundType.FootStep, 12.5f },
        };

        [JsonIgnore]
        [Hidden]
        public static readonly Dictionary<SAINSoundType, float> HEAR_DISPERSION_VALUES_Defaults;

        [Name("Unheard Shot Bullet Fly-by Modifier")]
        [Description("When a bot has a bullet fly by them, the dispersion on the source of the gunshot will be X times more random, " +
            "so a value of 2 will mean that it will multiply the randomized position to be 2x as far as originally calculated.")]
        [Category("Position Randomization")]
        [Advanced]
        [MinMax(1f, 10f, 100f)]
        public float HEAR_DISPERSION_BULLET_FELT_MOD = 2f;

        [Name("Minimum Hearing Randomization")]
        [Description("Higher = More Randomization, less accuracy in position prediction. Minimum Dispersion of a bot's estimated position from a sound they heard. In Meters. ")]
        [Category("Position Randomization")]
        [Advanced]
        [MinMax(0.0f, 2f, 1000f)]
        public float HEAR_DISPERSION_MIN = 0.5f;

        [Name("No Randomization Distance")]
        [Description("If the distance to a sound is less and or equal to this number, a bot will perfectly predict the source position, so no randomization or dispersion at all. " +
            "A value of 0 will disable this.")]
        [Category("Position Randomization")]
        [Advanced]
        [MinMax(0f, 50f, 1000f)]
        public float HEAR_DISPERSION_MIN_DISTANCE_THRESH = 10f;

        [Name("Max Randomization Distance")]
        [Description("The max cap, in meters, that an estimated position can be from the real position that a sound is played from. ")]
        [Category("Position Randomization")]
        [Advanced]
        [MinMax(10f, 250f, 1000f)]
        public float HEAR_DISPERSION_MAX_DISPERSION = 50f;

        [Name("Hearing Randomization Angle - Maximum")]
        [Description(_hear_angle_descr)]
        [Category("Position Randomization")]
        [Advanced]
        [MinMax(0.1f, 3f, 1000f)]
        public float HEAR_DISPERSION_ANGLE_MULTI_MAX = 1.5f;

        [Name("Hearing Randomization Angle - Minimum")]
        [Description(_hear_angle_descr)]
        [Category("Position Randomization")]
        [Advanced]
        [MinMax(0.1f, 3f, 1000f)]
        public float HEAR_DISPERSION_ANGLE_MULTI_MIN = 0.5f;

        [Name("Bunker Audio Range")]
        [Description("Reduces audio range if a bot and an enemy are not in the same bunker")]
        [Category("Hearing Environment Modifiers")]
        [Advanced]
        [MinMax(0.01f, 1f, 1000f)]
        public float BUNKER_REDUCTION_COEF = 0.2f;

        [Name("Bunker Elevation Range")]
        [Description("Reduces audio range if a bot and an enemy are both in a bunker, but at different levels")]
        [Category("Hearing Environment Modifiers")]
        [Advanced]
        [MinMax(0.01f, 1f, 100f)]
        public float BUNKER_ELEV_DIFF_COEF = 0.66f;

        [Name("Gunshot Occlusion")]
        [Description("If an obstacle is inbetween a bot's head and the position of a sound, reduce its range by this amount")]
        [Category("Hearing Environment Modifiers")]
        [Advanced]
        [MinMax(0.01f, 1f, 1000f)]
        public float GUNSHOT_OCCLUSION_MOD = 0.8f;

        [Name("Suppressed Gunshot Occlusion")]
        [Description("If an obstacle is inbetween a bot's head and the position of a sound, reduce its range by this amount")]
        [Category("Hearing Environment Modifiers")]
        [Advanced]
        [MinMax(0.01f, 1f, 100f)]
        public float GUNSHOT_OCCLUSION_MOD_SUPP = 0.65f;

        [Name("Footstep Occlusion")]
        [Description("If an obstacle is inbetween a bot's head and the position of a sound, reduce its range by this amount")]
        [Category("Hearing Environment Modifiers")]
        [Advanced]
        [MinMax(0.01f, 1f, 1000f)]
        public float FOOTSTEP_OCCLUSION_MOD = 0.6f;

        [Name("Sprint Occlusion")]
        [Description("If an obstacle is inbetween a bot's head and the position of a sound, reduce its range by this amount")]
        [Category("Hearing Environment Modifiers")]
        [Advanced]
        [MinMax(0.01f, 1f, 100f)]
        public float FOOTSTEP_OCCLUSION_MOD_SPRINT = 0.8f;

        [Name("Other Occlusion")]
        [Description("If an obstacle is inbetween a bot's head and the position of a sound, reduce its range by this amount")]
        [Category("Hearing Environment Modifiers")]
        [Advanced]
        [MinMax(0.01f, 1f, 100f)]
        public float OTHER_OCCLUSION_MOD = 0.6f;

        [Name("Indoor / Outdoor Difference - Gunfire")]
        [Description("If bots are not in the same area as the source of a sound, reduce audio range by this amount")]
        [Category("Hearing Environment Modifiers")]
        [Advanced]
        [MinMax(0.01f, 1f, 1000f)]
        public float GUNSHOT_ENVIR_MOD = 0.65f;

        [Name("Indoor / Outdoor Difference - Footsteps/Other")]
        [Description("If bots are not in the same area as the source of a sound, reduce audio range by this amount")]
        [Category("Hearing Environment Modifiers")]
        [Advanced]
        [MinMax(0.01f, 1f, 100f)]
        public float FOOTSTEP_ENVIR_MOD = 0.7f;

        [Name("Environment Modifier Minimum")]
        [Description("")]
        [Category("Hearing Environment Modifiers")]
        [DeveloperOption]
        [MinMax(0.01f, 1f, 1000f)]
        public float MIN_ENVIRONMENT_MOD = 0.05f;

        [Name("No Headphones")]
        [Description("If a bot does not have headphones, reduce audible range of all sounds by this amount.")]
        [Category("Hearing Modifiers")]
        [MinMax(0.01f, 1f, 1000f)]
        public float HEAR_MODIFIER_NO_EARS = 0.6f;

        [Name("Heavy Helmet")]
        [Description("If a bot is wearing a heavy helmet, reduce audible range of all sounds by this amount.")]
        [Category("Hearing Modifiers")]
        [MinMax(0.01f, 1f, 100f)]
        public float HEAR_MODIFIER_HEAVY_HELMET = 0.8f;

        [Name("Dying")]
        [Description("If a bot is dying or seriously injured, reduce audible range of all sounds by this amount.")]
        [Category("Hearing Modifiers")]
        [MinMax(0.01f, 1f, 1000f)]
        public float HEAR_MODIFIER_DYING = 0.8f;

        [Name("Sprinting")]
        [Description("If a bot is sprinting, reduce audible range of all sounds by this amount.")]
        [Category("Hearing Modifiers")]
        [Advanced]
        [MinMax(0.01f, 1f, 1000f)]
        public float HEAR_MODIFIER_SPRINT = 0.85f;

        [Name("Heavy Breathing")]
        [Description("If a bot is breathing heavily, reduce audible range of all sounds by this amount.")]
        [Category("Hearing Modifiers")]
        [Advanced]
        [MinMax(0.01f, 1f, 1000f)]
        public float HEAR_MODIFIER_HEAVYBREATH = 0.65f;

        [Name("Minimum Hear Modifier")]
        [Description("Final Multiplier will not go below this value.")]
        [Category("Hearing Modifiers")]
        [DeveloperOption]
        [MinMax(0.01f, 1f, 1000f)]
        public float HEAR_MODIFIER_MIN_CLAMP = 0.01f;

        [Name("Maximum Hear Modifier")]
        [Description("Final Multiplier will not go above this value.")]
        [Category("Hearing Modifiers")]
        [DeveloperOption]
        [MinMax(1f, 5f, 1000f)]
        public float HEAR_MODIFIER_MAX_CLAMP = 5f;

        [Name("Minimum Hearing Modifier Distance")]
        [Description("Sounds that originate closer than this have a 100% chance of being heard.")]
        [Category("Hearing Modifiers")]
        [Advanced]
        [MinMax(0f, 50f, 100f)]
        public float HEAR_MODIFIER_MAX_AFFECT_DIST = 3f;

        [Name("Scale Start Distance - No Headphones")]
        [Description("Sounds that originate closer than this have a 100% chance of being heard.")]
        [Category("Hearing Chance")]
        [Advanced]
        [MinMax(0, 10, 100)]
        public float HEAR_CHANCE_MIN_DIST = 0.25f;

        [Name("Scale Start Distance - Headphones")]
        [Description("Sounds that originate closer than this have a 100% chance of being heard.")]
        [Category("Hearing Chance")]
        [Advanced]
        [MinMax(0, 100, 100)]
        public float HEAR_CHANCE_MIN_DIST_HEADPHONES = 1;

        [Name("Midrange Coefficient")]
        [Description("If the distance between a sound and the bot is X distance of its max range, increase its chance to hear slightly. So if a sound has a range of 50 meters, and the distance to a bot hears is 25 meters away, that would result in 25 / 50, so 0.5, which is below this mid range value.")]
        [Category("Hearing Chance")]
        [Advanced]
        [MinMax(0.00f, 1f, 1000f)]
        public float HEAR_CHANCE_MIDRANGE_COEF = 0.66f;

        [Name("Mid range Minimum Chance - Headphones")]
        [Description("If a sound is within mid-range. Increase the minimum chance by this amount")]
        [Category("Hearing Chance")]
        [Advanced]
        [MinMax(0, 100, 1)]
        public float HEAR_CHANCE_MIDRANGE_MINCHANCE_HEADPHONES = 3;

        [Name("Long range Minimum Chance - Headphones")]
        [Description("If a sound is further than mid-range. Increase the minimum chance by this amount")]
        [Category("Hearing Chance")]
        [Advanced]
        [MinMax(0, 100, 1)]
        public float HEAR_CHANCE_LONGRANGE_MINCHANCE_HEADPHONES = 1;

        [Name("Standing Still Velocity")]
        [Description("Boost Hearing chance slightly if a bot's velocity is under this value.")]
        [Category("Hearing Chance")]
        [Advanced]
        [MinMax(0.0f, 1f, 1000f)]
        public float HEAR_CHANCE_NOTMOVING_VELOCITY = 0.05f;

        [Name("Standing Still Min Chance - No Headphones")]
        [Description("If a bot is standing still, add X percent to a bot minimum hear chance for sounds that aren't gunshots.")]
        [Category("Hearing Chance")]
        [Advanced]
        [MinMax(0, 100, 1)]
        public float HEAR_CHANCE_NOTMOVING_MINCHANCE = 2;

        [Name("Standing Still Min Chance - Headphones")]
        [Description("If a bot is standing still, add X percent to a bot minimum hear chance for sounds that aren't gunshots.")]
        [Category("Hearing Chance")]
        [MinMax(0, 100, 1)]
        public float HEAR_CHANCE_NOTMOVING_MINCHANCE_HEADPHONES = 4;

        [Name("Other Sounds Min Chance - Headphones")]
        [Description("If the type of sound is not footsteps or gunfire, add X percent to a bot minimum hear chance.")]
        [Category("Hearing Chance")]
        [Advanced]
        [MinMax(0, 100, 1)]
        public float HEAR_CHANCE_HEADPHONES_OTHERSOUNDS = 3;

        [Name("Active Enemy Min Chance - No Headphones")]
        [Description("If the source of a sound is from a bot's active primary enemy, add X percent to a bot minimum hear chance for sounds that aren't gunshots.")]
        [Category("Hearing Chance")]
        [Advanced]
        [MinMax(0, 100, 1)]
        public float HEAR_CHANCE_CURRENTENEMY_MINCHANCE = 2;

        [Name("Active Enemy Min Chance - Headphones")]
        [Description("If the source of a sound is from a bot's active primary enemy, add X percent to a bot minimum hear chance for sounds that aren't gunshots.")]
        [Category("Hearing Chance")]
        [Advanced]
        [MinMax(0, 100, 1)]
        public float HEAR_CHANCE_CURRENTENEMY_MINCHANCE_HEADPHONES = 3;

        [MinMax(1f, 150f, 100f)]
        [Category("Hearing Distance")]
        [Name("Looting Sound")]
        [Advanced]
        public float BaseSoundRange_Looting = 40f;

        [MinMax(1f, 150f, 100f)]
        [Category("Hearing Distance")]
        [Name("Footstep Skid and Turn")]
        [Advanced]
        public float BaseSoundRange_MovementTurnSkid = 30f;

        [MinMax(1f, 150f, 100f)]
        [Category("Hearing Distance")]
        [Name("Grenade Pullout and Pin Pull")]
        [Advanced]
        public float BaseSoundRange_GrenadePinDraw = 35f;

        [MinMax(1f, 150f, 100f)]
        [Category("Hearing Distance")]
        [Name("Prone Sound")]
        [Advanced]
        public float BaseSoundRange_Prone = 50f;

        [MinMax(1f, 150f, 100f)]
        [Category("Hearing Distance")]
        [Name("Healing Sound")]
        [Advanced]
        public float BaseSoundRange_Healing = 40f;

        [MinMax(1f, 150f, 100f)]
        [Category("Hearing Distance")]
        [Name("Reload Sound")]
        [Advanced]
        public float BaseSoundRange_Reload = 30f;

        [MinMax(1f, 150f, 100f)]
        [Category("Hearing Distance")]
        [Name("Surgery Sound")]
        [Advanced]
        public float BaseSoundRange_Surgery = 55f;

        [MinMax(1f, 150f, 100f)]
        [Category("Hearing Distance")]
        [Name("Dryfire Sound")]
        [Advanced]
        public float BaseSoundRange_DryFire = 10f;

        [MinMax(1f, 150f, 100f)]
        [Category("Hearing Distance")]
        [Name("Fall Landing Sound")]
        [Advanced]
        public float MaxSoundRange_FallLanding = 70;

        [MinMax(1f, 150f, 100f)]
        [Category("Hearing Distance")]
        [Name("Aim Down Sights Sound")]
        [Advanced]
        public float BaseSoundRange_AimingandGearRattle = 35f;

        [MinMax(1f, 150f, 100f)]
        [Category("Hearing Distance")]
        [Name("Eat and Drink Sound")]
        [Advanced]
        public float BaseSoundRange_EatDrink = 40f;

        [MinMax(1f, 150f, 100f)]
        [Category("Hearing Distance")]
        [Name("Max Squad Communication Range - No Headphones")]
        public float MaxRangeToReportEnemyActionNoHeadset = 50f;

        [Name("Hearing Delay / Reaction Time with Active Enemy")]
        [MinMax(0.0f, 1f, 100f)]
        public float BaseHearingDelayWithEnemy = 0.2f;

        [Name("Hearing Delay / Reaction Time while At Peace")]
        [MinMax(0.0f, 1f, 100f)]
        public float BaseHearingDelayAtPeace = 0.35f;

        [Name("Global Gunshot Audible Range Multiplier")]
        [MinMax(0.1f, 2f, 100f)]
        [Category("Hearing Distance")]
        public float GunshotAudioMultiplier = 1f;

        [Name("Global Footstep Audible Range Multiplier")]
        [MinMax(0.1f, 2f, 100f)]
        [Category("Hearing Distance")]
        public float FootstepAudioMultiplier = 1f;

        [Name("Suppressed Sound Modifier")]
        [Description("Audible Gun Range is multiplied by this number when using a suppressor")]
        [MinMax(0.1f, 0.95f, 100f)]
        [Category("Hearing Distance")]
        public float SuppressorModifier = 0.6f;

        [Name("Subsonic Sound Modifier")]
        [Description("Audible Gun Range is multiplied by this number when using a suppressor and subsonic ammo")]
        [MinMax(0.1f, 0.95f, 100f)]
        [Category("Hearing Distance")]
        public float SubsonicModifier = 0.33f;

        [Name("Hearing Distances by Ammo Type")]
        [Description("How far a bot can hear a gunshot when fired from each specific caliber listed here.")]
        [Category("Hearing Distance")]
        [MinMax(30f, 400f, 10f)]
        [Advanced]
        [DefaultDictionary(nameof(HearingDistancesDefaults))]
        public Dictionary<ECaliber, float> HearingDistances = new Dictionary<ECaliber, float>
        {
            { ECaliber.Caliber9x18PM, 110f },
            { ECaliber.Caliber9x19PARA, 110f },
            { ECaliber.Caliber46x30, 120f },
            { ECaliber.Caliber9x21, 120f },
            { ECaliber.Caliber57x28, 120f },
            { ECaliber.Caliber762x25TT, 120f },
            { ECaliber.Caliber1143x23ACP, 115f },
            { ECaliber.Caliber9x33R, 125 },
            { ECaliber.Caliber545x39, 160 },
            { ECaliber.Caliber556x45NATO, 160 },
            { ECaliber.Caliber9x39, 160 },
            { ECaliber.Caliber762x35, 175 },
            { ECaliber.Caliber762x39, 175 },
            { ECaliber.Caliber366TKM, 175 },
            { ECaliber.Caliber762x51, 200f },
            { ECaliber.Caliber127x55, 200f },
            { ECaliber.Caliber762x54R, 225f },
            { ECaliber.Caliber86x70, 250f },
            { ECaliber.Caliber20g, 185 },
            { ECaliber.Caliber12g, 185 },
            { ECaliber.Caliber23x75, 210 },
            { ECaliber.Caliber26x75, 50 },
            { ECaliber.Caliber30x29, 50 },
            { ECaliber.Caliber40x46, 50 },
            { ECaliber.Caliber40mmRU, 50 },
            { ECaliber.Caliber127x108, 300 },
            { ECaliber.Caliber68x51, 200f },
            { ECaliber.Default, 125 },
        };

        [JsonIgnore]
        [Hidden]
        public static readonly Dictionary<ECaliber, float> HearingDistancesDefaults = new Dictionary<ECaliber, float>()
        {
            { ECaliber.Caliber9x18PM, 110f },
            { ECaliber.Caliber9x19PARA, 110f },
            { ECaliber.Caliber46x30, 120f },
            { ECaliber.Caliber9x21, 120f },
            { ECaliber.Caliber57x28, 120f },
            { ECaliber.Caliber762x25TT, 120f },
            { ECaliber.Caliber1143x23ACP, 115f },
            { ECaliber.Caliber9x33R, 125 },
            { ECaliber.Caliber545x39, 160 },
            { ECaliber.Caliber556x45NATO, 160 },
            { ECaliber.Caliber9x39, 160 },
            { ECaliber.Caliber762x35, 175 },
            { ECaliber.Caliber762x39, 175 },
            { ECaliber.Caliber366TKM, 175 },
            { ECaliber.Caliber762x51, 200f },
            { ECaliber.Caliber127x55, 200f },
            { ECaliber.Caliber762x54R, 225f },
            { ECaliber.Caliber86x70, 250f },
            { ECaliber.Caliber20g, 185 },
            { ECaliber.Caliber12g, 185 },
            { ECaliber.Caliber23x75, 210 },
            { ECaliber.Caliber26x75, 50 },
            { ECaliber.Caliber30x29, 50 },
            { ECaliber.Caliber40x46, 50 },
            { ECaliber.Caliber40mmRU, 50 },
            { ECaliber.Caliber127x108, 300 },
            { ECaliber.Caliber68x51, 200f },
            { ECaliber.Default, 125 },
        };

        public override void Init(List<ISAINSettings> list)
        {
            Helpers.ListHelpers.CloneEntries(HEAR_DISPERSION_VALUES_Defaults, HEAR_DISPERSION_VALUES);
            Helpers.ListHelpers.CloneEntries(HearingDistancesDefaults, HearingDistances);
            list.Add(this);
        }

        [JsonIgnore]
        [Hidden]
        private const string _dispersion_descr = "Higher = Less Randomization and more accuracy. " +
            "The distance to the sound's position, in meters, is divided by the number here. " +
            "Example: A unsuppressed gunshot is 150 meters away. And the dispersion value for unsuppressed gunfire is 20. So We divide 150 by 20 to result in 7.5, " +
            "so the randomized position that a bot thinks a gunshot came from is a position within 7.5 meters from the actual source of the gunshot. " +
            "Note: this randomized position must be somewhere that is walkable so that they can potentially be able the investigate it. " +
            "It is also not randomized in height to avoid bots having difficulty navigating to where they think a sound came from, " +
            "so imagine a flat plane around you that extends 7.5 meters away that includes all walkable space, " +
            "if you shoot - that bot 150 away will estimate you are somewhere within that 7.5 meter radius, on the same height level that you are on. " +
            "If there is no walkable space around you, it will find the closest walkable place.";

        [JsonIgnore]
        [Hidden]
        private const string _hear_angle_descr = "If a bot is looking at the source of a sound, they will be more accurate in their position prediction, up to the Minimum value here. " +
            "If it is directly behind them, randomization will be multiplied by the Maximumm value here. " +
            "It is a linear scale between these, so a sound directly to their right or left, will have the difference between the Maximumm and Minimum. " +
            "Setting both the Min and the Max to 1.0 will disable this system.";
    }
}