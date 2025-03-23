using EFT;
using Newtonsoft.Json;
using SAIN.Attributes;
using SAIN.Helpers;
using SAIN.SAINComponent.Classes.WeaponFunction;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings.Categories
{
    public class MindSettings : SAINSettingsBase<MindSettings>, ISAINSettings
    {
        public override void Update()
        {
        }

        [Category("Enemy Sniper Reaction")]
        [Description("If a bot thinks it is under fire from a sniper, they will always want to sprint to cover.")]
        [Name("Always Sprint")]
        public bool ENEMYSNIPER_ALWAYS_SPRINT_COVER = true;

        [Category("Enemy Sniper Reaction")]
        [Description("If a bot thinks it is under fire from a sniper, they will always sprint while seeking that sniper.")]
        [Name("Always Sprint")]
        public bool ENEMYSNIPER_ALWAYS_SPRINT_SEARCH = true;

        [Category("Enemy Sniper Reaction")]
        [Name("Distance Under Fire")]
        [Description("If an enemy is further than this distance, in meters. They will be considered a sniper if shooting at a bot.")]
        [MinMax(30f, 250f, 1f)]
        public float ENEMYSNIPER_DISTANCE = 85f;

        [Category("Enemy Sniper Reaction")]
        [Name("Distance Enemy is no longer Sniper")]
        [Description("If an enemy is closer than this distance, in meters. They will no longer be considered a sniper if they previously were.")]
        [MinMax(30f, 250f, 1f)]
        public float ENEMYSNIPER_DISTANCE_END = 75f;

        //[Category("Enemy Sniper Reaction")]
        //[Name("Underfire by Sniper Status Duration")]
        //[Description("If a bot thinks it is underfire from a sniper, they will enter this state for X seconds.")]
        //[MinMax(0f, 120f, 1f)]
        //public float ENEMYSNIPER_STATUS_DURATION = 30f;

        [Name("Force Single Personality For All Bots")]
        [Description("All Spawned SAIN bots will be assigned the selected Personality, if any are set to true, no matter what.")]
        [Category("Personality")]
        public Dictionary<EPersonality, bool> ForcePersonality = new Dictionary<EPersonality, bool>()
        {
            { EPersonality.Wreckless, false},
            { EPersonality.GigaChad, false },
            { EPersonality.Chad, false },
            { EPersonality.SnappingTurtle, false},
            { EPersonality.Rat, false },
            { EPersonality.Coward, false },
            { EPersonality.Timmy, false},
            { EPersonality.Normal, false},
        };

        [Name("Boss Personalities")]
        [Description("Sets the pesonality that a boss will always use.")]
        [Category("Personality")]
        [Hidden]
        public Dictionary<WildSpawnType, EPersonality> PERS_BOSSES = new Dictionary<WildSpawnType, EPersonality>() {
            { WildSpawnType.bossKilla, EPersonality.Wreckless},
            { WildSpawnType.bossTagilla, EPersonality.Wreckless},
            { WildSpawnType.bossKolontay, EPersonality.Wreckless},

            { WildSpawnType.bossKnight, EPersonality.GigaChad},
            { WildSpawnType.followerBigPipe, EPersonality.GigaChad},

            { WildSpawnType.followerBirdEye, EPersonality.SnappingTurtle},
            { WildSpawnType.bossGluhar, EPersonality.SnappingTurtle},

            { WildSpawnType.bossKojaniy, EPersonality.Rat},
            { WildSpawnType.bossPartisan, EPersonality.Rat},

            { WildSpawnType.bossBully, EPersonality.Coward},
            { WildSpawnType.bossSanitar, EPersonality.Coward},
            { WildSpawnType.bossBoar, EPersonality.Coward},
        };

        [MinMax(0.1f, 5f, 100f)]
        [Category("Personality")]
        [Name("Global Aggression")]
        [Description("Higher = More aggressive bots, less time before seeking enemies. 2x = half the wait time.")]
        public float GlobalAggression = 1f;

        [Name("Bots can use Stealth Search")]
        [Description("If a bot thinks he was not heard, and isn't currently fighting an enemy, they can decide to be stealthy while they seek out an enemy, if they are inside a building.")]
        [Category("Personality")]
        public bool SneakyBots = true;

        [Name("Only Sneaky Personalities can be Stealthy")]
        [Description("Only allow sneaky personality types (rat, snapping turtle) to be stealthy while searching for an enemy, ignored if Stealth Search is disabled above")]
        [Category("Personality")]
        public bool OnlySneakyPersonalitiesSneaky = true;

        [Description("The distance from a bot's search destination that they will begin to be stealthy, if enabled.")]
        [Category("Personality")]
        [Advanced]
        [MinMax(5f, 200f, 10f)]
        public float MaximumDistanceToBeSneaky = 80f;

        [Name("Bot Suppression")]
        [Description("Toggles whether bots get suppressed or not. If disabled, all options below will do nothing.")]
        [Category("Suppression")]
        public bool SUPP_TOGGLE = true;

        [Name("Suppression Distance Scale Start")]
        [Description("The distance between the bullet, and a bot's head to receive full suppression effect. In Meters.")]
        [Category("Suppression")]
        [MinMax(1f, 30f, 100f)]
        [Advanced]
        public float SUPP_DISTANCE_SCALE_START = 4f;

        [Name("Suppression Distance Scale End")]
        [Description("The maximum distance between the bullet, and a bot's head to be considered Suppressing fire. In Meters. Scales linearly between Scale End and Scale Start.")]
        [Category("Suppression")]
        [MinMax(1f, 30f, 100f)]
        [Advanced]
        public float SUPP_DISTANCE_SCALE_END = 10f;

        [Name("Suppression Distance Amplify Distance")]
        [Description("If a bullet is closer than this distance, in meters, to the bot's head. Amplify the amount of suppression.")]
        [Category("Suppression")]
        [MinMax(0f, 5f, 100f)]
        [Advanced]
        public float SUPP_DISTANCE_AMP_DIST = 0.5f;

        [Name("Suppression Distance Amplify Amount")]
        [Description("If a bullet is closer than Amplify Distance to the bot's head. Amplify the amount of suppression by this multiplier.")]
        [Category("Suppression")]
        [MinMax(1f, 3f, 100f)]
        [Advanced]
        public float SUPP_DISTANCE_AMP_AMOUNT = 1.5f;

        [Description("The maximum distance between the bullet, and a bot's head to be considered under active enemy fire.")]
        [MinMax(0.1f, 20f, 100f)]
        [Category("Suppression")]
        [Advanced]
        public float MaxUnderFireDistance = 2f;

        [Hidden]
        [Name("Suppression States")]
        [Description("Configure each tier of suppression.")]
        [MinMax(0.01f, 10f, 100f)]
        [Category("Suppression")]
        [Advanced]
        public Dictionary<ESuppressionState, SuppressionConfig> SUPPRESSION_STATES = new Dictionary<ESuppressionState, SuppressionConfig>()
        {
            {ESuppressionState.Light, new SuppressionConfig {
                Threshold = 1f,
                PrecisionSpeedCoef = 1.15f,
                AccuracySpeedCoef = 1.2f,
                GainSightCoef = 1.2f,
                ScatteringCoef = 1.35f,
                VisibleDistCoef = 0.85f,
                HearingDistCoef = 0.8f,
                }
            },
            {ESuppressionState.Medium, new SuppressionConfig {
                Threshold = 6f,
                PrecisionSpeedCoef = 1.5f,
                AccuracySpeedCoef = 1.5f,
                GainSightCoef = 1.5f,
                ScatteringCoef = 1.75f,
                VisibleDistCoef = 0.6f,
                HearingDistCoef = 0.6f,
                }
            },
            {ESuppressionState.Heavy, new SuppressionConfig {
                Threshold = 15f,
                PrecisionSpeedCoef = 2f,
                AccuracySpeedCoef = 2f,
                GainSightCoef = 1.65f,
                ScatteringCoef = 2.5f,
                VisibleDistCoef = 0.5f,
                HearingDistCoef = 0.4f,
                }
            },
            {ESuppressionState.Extreme, new SuppressionConfig {
                Threshold = 25f,
                PrecisionSpeedCoef = 3f,
                AccuracySpeedCoef = 3f,
                GainSightCoef = 2f,
                ScatteringCoef = 3f,
                VisibleDistCoef = 0.33f,
                HearingDistCoef = 0.25f,
                }
            },
        };

        [Name("Amount Multiplier")]
        [Description("Linearly increase or decrease the amount of suppression points bots receive from 1 bullet. Higher = Bots get suppressed more easily.")]
        [MinMax(0.01f, 5f, 100f)]
        [Category("Suppression")]
        public float SUPP_AMOUNT_MULTI = 1f;

        [Name("Strength Multiplier")]
        [Description("Linearly increase or decrease the strength of suppression effects on bots. Higher = Suppression has more effect on bot stats.")]
        [MinMax(0.01f, 5f, 100f)]
        [Category("Suppression")]
        public float SUPP_STRENGTH_MULTI = 1f;

        [Advanced]
        [Name("Decay Tick Amount")]
        [Description("How much suppression to remove per update tick.")]
        [MinMax(0.01f, 5f, 100f)]
        [Category("Suppression")]
        public float SUP_DECAY_AMOUNT = 0.25f;

        [Advanced]
        [Name("Decay Tick Frequency")]
        [Description("How often to tick decay per second. 0.25 = 4 per second")]
        [MinMax(0.01f, 1f, 100f)]
        [Category("Suppression")]
        public float SUP_DECAY_FREQ = 0.25f;

        [Advanced]
        [Name("State Update Tick Frequency")]
        [Description("How often to check suppression state per second. 0.5 = 2 per second")]
        [MinMax(0.01f, 1f, 100f)]
        [Category("Suppression")]
        public float SUP_CHECK_FREQ = 0.5f;

        [Advanced]
        [Name("Suppression Amounts Per Caliber")]
        [Description("For each bullet that flies by a bot, add this number to their suppression counter, which decays constantly and linearly.")]
        [MinMax(0.1f, 20f, 100f)]
        [Category("Suppression")]
        [DefaultDictionary(nameof(SUPP_AMOUNTS_DEFAULT))]
        public Dictionary<ECaliber, float> SUPP_AMOUNTS = new Dictionary<ECaliber, float> {
            { ECaliber.Caliber9x18PM, 1f },
            { ECaliber.Caliber9x19PARA, 1.1f },
            { ECaliber.Caliber46x30, 1.2f },
            { ECaliber.Caliber9x21, 1.25f },
            { ECaliber.Caliber57x28, 1.3f },
            { ECaliber.Caliber762x25TT, 1.4f },
            { ECaliber.Caliber1143x23ACP, 1.5f },
            { ECaliber.Caliber9x33R, 1.5f },
            { ECaliber.Caliber545x39, 2.1f },
            { ECaliber.Caliber556x45NATO, 2f },
            { ECaliber.Caliber9x39, 2.5f },
            { ECaliber.Caliber762x35, 2.4f },
            { ECaliber.Caliber762x39, 2.5f },
            { ECaliber.Caliber366TKM, 2.5f },
            { ECaliber.Caliber68x51, 2.5f },
            { ECaliber.Caliber762x51, 2.65f },
            { ECaliber.Caliber127x55, 2.7f },
            { ECaliber.Caliber762x54R, 2.75f },
            { ECaliber.Caliber20g, 3f },
            { ECaliber.Caliber12g, 3f },
            { ECaliber.Caliber23x75, 3f },
            { ECaliber.Caliber26x75, 3f },
            { ECaliber.Caliber30x29, 3f },
            { ECaliber.Caliber40x46, 3f },
            { ECaliber.Caliber40mmRU, 3f },
            { ECaliber.Caliber86x70, 5f },
            { ECaliber.Caliber127x108, 5f },
            { ECaliber.Default, 2f },
        };

        [JsonIgnore]
        [Hidden]
        public static readonly Dictionary<ECaliber, float> SUPP_AMOUNTS_DEFAULT = new Dictionary<ECaliber, float> {
            { ECaliber.Caliber9x18PM, 1f },
            { ECaliber.Caliber9x19PARA, 1.1f },
            { ECaliber.Caliber46x30, 1.2f },
            { ECaliber.Caliber9x21, 1.25f },
            { ECaliber.Caliber57x28, 1.3f },
            { ECaliber.Caliber762x25TT, 1.4f },
            { ECaliber.Caliber1143x23ACP, 1.5f },
            { ECaliber.Caliber9x33R, 1.5f },
            { ECaliber.Caliber545x39, 2.1f },
            { ECaliber.Caliber556x45NATO, 2f },
            { ECaliber.Caliber9x39, 2.5f },
            { ECaliber.Caliber762x35, 2.4f },
            { ECaliber.Caliber762x39, 2.5f },
            { ECaliber.Caliber366TKM, 2.5f },
            { ECaliber.Caliber68x51, 2.5f },
            { ECaliber.Caliber762x51, 2.65f },
            { ECaliber.Caliber127x55, 2.7f },
            { ECaliber.Caliber762x54R, 2.75f },
            { ECaliber.Caliber86x70, 5f },
            { ECaliber.Caliber20g, 3f },
            { ECaliber.Caliber12g, 3f },
            { ECaliber.Caliber23x75, 3f },
            { ECaliber.Caliber26x75, 3f },
            { ECaliber.Caliber30x29, 3f },
            { ECaliber.Caliber40x46, 3f },
            { ECaliber.Caliber40mmRU, 3f },
            { ECaliber.Caliber127x108, 5f },
            { ECaliber.Default, 2f },
        };

        [Advanced]
        [Name("Max Suppression Number")]
        [Description("Suppression caps at this number.")]
        [MinMax(0.01f, 50f, 100f)]
        [Category("Suppression")]
        public float SUPP_MAX_NUM = 30f;

        public override void Init(List<ISAINSettings> list)
        {
            list.Add(this);
        }
    }
}