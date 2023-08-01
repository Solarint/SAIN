using SAIN.BotSettings.Categories.Util;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.BotSettings.Categories
{
    internal class InitDictionary
    {
        static readonly Type Float = typeof(float);
        static readonly Type Int = typeof(int);
        static readonly Type Bool = typeof(bool);

        public static void Init()
        {
            BotAimingSettingsInit();
            BotChangeSettingsInit();
            BotCoreSettingsInit();
            BotGrenadeSettingsInit();
            BotHearingSettingsInit();
            BotLaySettingsInit();
            BotLookSettingsInit();
            BotMoveSettingsInit();
            BotMindSettingsInit();
            BotPatrolSettingsInit();
            BotScatterSettingsInit();
            BotShootSettingsInit();
        }

        public static void BotAimingSettingsInit()
        {
            var aim = new SAINAimingSettings();
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(aim.MAX_AIMING_UPGRADE_BY_TIME),
                    Type = Float,
                    SAINDefault = aim.MAX_AIMING_UPGRADE_BY_TIME,
                    Max = 1f,
                    Min = 0.1f,
                    Rounding = 100f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(SAINAimingSettings.MAX_AIM_TIME),
                    Type = Float,
                    SAINDefault = aim.MAX_AIM_TIME,
                    Max = 10f,
                    Min = 0.1f,
                    Rounding = 10f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(SAINAimingSettings.AIMING_TYPE),
                    Type = Int,
                    SAINDefault = aim.AIMING_TYPE,
                    Max = 6,
                    Min = 1
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(SAINAimingSettings.SHPERE_FRIENDY_FIRE_SIZE),
                    Type = Float,
                    SAINDefault = aim.SHPERE_FRIENDY_FIRE_SIZE,
                    Max = 3f,
                    Min = 0f,
                    Rounding = 100f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(SAINAimingSettings.RECALC_MUST_TIME),
                    Type = Int,
                    SAINDefault = aim.RECALC_MUST_TIME,
                    Max = 4,
                    Min = 1
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(SAINAimingSettings.RECALC_MUST_TIME_MIN),
                    Type = Int,
                    SAINDefault = aim.RECALC_MUST_TIME_MIN,
                    Max = 4,
                    Min = 1
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(SAINAimingSettings.RECALC_MUST_TIME_MAX),
                    Type = Int,
                    SAINDefault = aim.RECALC_MUST_TIME_MAX,
                    Max = 4,
                    Min = 1
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(SAINAimingSettings.BASE_HIT_AFFECTION_DELAY_SEC),
                    Type = Float,
                    SAINDefault = aim.BASE_HIT_AFFECTION_DELAY_SEC,
                    Max = 1f,
                    Min = 0f,
                    Rounding = 10f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(SAINAimingSettings.BASE_HIT_AFFECTION_MIN_ANG),
                    Type = Float,
                    SAINDefault = aim.BASE_HIT_AFFECTION_MIN_ANG,
                    Max = 15f,
                    Min = 0f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(SAINAimingSettings.BASE_HIT_AFFECTION_MAX_ANG),
                    Type = Float,
                    SAINDefault = aim.BASE_HIT_AFFECTION_MAX_ANG,
                    Max = 15f,
                    Min = 0f
                });
        }

        public static void BotChangeSettingsInit()
        {
        }

        public static void BotCoreSettingsInit()
        {
            var core = new SAINCoreSettings();
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(core.VisibleAngle),
                    DisplayName = "Vision Cone Angle Max",
                    Description = "The Maximum Vision Cone for a Bot",
                    Type = Float,
                    SAINDefault = core.VisibleAngle,
                    Max = 180,
                    Min = 50
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(core.VisibleDistance),
                    DisplayName = "Max Visible Distance",
                    Description = "The Maximum Distance a Bot Can See an Enemy.",
                    Type = Float,
                    SAINDefault = core.VisibleDistance,
                    Max = 500,
                    Min = 50
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(core.GainSightCoef),
                    DisplayName = "Gain Sight Coef",
                    Description = "Controls how quickly bots notice targets, Lower means faster vision speed.",
                    Type = Float,
                    SAINDefault = core.GainSightCoef,
                    Max = 0.5f,
                    Min = 0.05f,
                    Rounding = 100f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(core.ScatteringPerMeter),
                    DisplayName = "Scattering Per Meter",
                    Description = "How much scatter to add to a bot for every meter between them and their aiming target.",
                    Type = Float,
                    SAINDefault = core.ScatteringPerMeter,
                    Max = 0.5f,
                    Min = 0f,
                    Rounding = 100f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(core.ScatteringClosePerMeter),
                    DisplayName = "Scattering Close Per Meter",
                    Description = "How much scatter to add to a bot for every meter between them and their aiming target.",
                    Type = Float,
                    SAINDefault = core.ScatteringClosePerMeter,
                    Max = 0.5f,
                    Min = 0f,
                    Rounding = 100f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(core.DamageCoeff),
                    DisplayName = "Damage Coeff",
                    Description = "How Much to Increase Scatter when a bot's arms are injured",
                    Type = Float,
                    SAINDefault = core.DamageCoeff,
                    Max = 5f,
                    Min = 1f,
                    Rounding = 100f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(core.CanGrenade),
                    DisplayName = "Can Use Grenades",
                    Description = "",
                    Type = Bool,
                    SAINDefault = core.CanGrenade,
                });
        }

        public static void BotGrenadeSettingsInit()
        {
            var grenade = new SAINGrenadeSettings();
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(grenade.DELTA_GRENADE_START_TIME),
                    Type = Float,
                    SAINDefault = grenade.DELTA_GRENADE_START_TIME,
                    Max = 1f,
                    Min = 0f,
                    Rounding = 100f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(grenade.CHANCE_TO_NOTIFY_ENEMY_GR_100),
                    Type = Float,
                    SAINDefault = grenade.CHANCE_TO_NOTIFY_ENEMY_GR_100,
                    Max = 100f,
                    Min = 0f,
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(grenade.BEWARE_TYPE),
                    Type = Int,
                    SAINDefault = grenade.BEWARE_TYPE,
                    Max = 3,
                    Min = 1,
                });
        }

        public static void BotHearingSettingsInit()
        {
        }

        public static void BotLaySettingsInit()
        {
        }

        public static void BotLookSettingsInit()
        {
            var look = new SAINLookSettings();
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(look.CAN_USE_LIGHT),
                    Type = Bool,
                    SAINDefault = look.CAN_USE_LIGHT
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(look.FULL_SECTOR_VIEW),
                    Type = Bool,
                    SAINDefault = look.FULL_SECTOR_VIEW
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(look.MAX_DIST_CLAMP_TO_SEEN_SPEED),
                    Type = Float,
                    SAINDefault = look.MAX_DIST_CLAMP_TO_SEEN_SPEED,
                    Max = 1000f,
                    Min = 10f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(look.NIGHT_VISION_ON),
                    Type = Float,
                    SAINDefault = look.NIGHT_VISION_ON,
                    Max = 1000f,
                    Min = 10f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(look.NIGHT_VISION_DIST),
                    Type = Float,
                    SAINDefault = look.NIGHT_VISION_DIST,
                    Max = 1000f,
                    Min = 10f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(look.VISIBLE_ANG_NIGHTVISION),
                    Type = Float,
                    SAINDefault = look.VISIBLE_ANG_NIGHTVISION,
                    Max = 180f,
                    Min = 10f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(look.LOOK_THROUGH_PERIOD_BY_HIT),
                    Type = Float,
                    SAINDefault = look.LOOK_THROUGH_PERIOD_BY_HIT,
                    Max = 10f,
                    Min = 0f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(look.LightOnVisionDistance),
                    Type = Float,
                    SAINDefault = look.LightOnVisionDistance,
                    Max = 100f,
                    Min = 5f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(look.VISIBLE_ANG_LIGHT),
                    Type = Float,
                    SAINDefault = look.VISIBLE_ANG_LIGHT,
                    Max = 180f,
                    Min = 5f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(look.VISIBLE_DISNACE_WITH_LIGHT),
                    Type = Float,
                    SAINDefault = look.VISIBLE_DISNACE_WITH_LIGHT,
                    Max = 100f,
                    Min = 5f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(look.GOAL_TO_FULL_DISSAPEAR),
                    Type = Float,
                    SAINDefault = look.GOAL_TO_FULL_DISSAPEAR,
                    Max = 2,
                    Min = 0.1f,
                    Rounding = 100f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(look.GOAL_TO_FULL_DISSAPEAR_GREEN),
                    Type = Float,
                    SAINDefault = look.GOAL_TO_FULL_DISSAPEAR_GREEN,
                    Max = 2,
                    Min = 0.01f,
                    Rounding = 100f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(look.GOAL_TO_FULL_DISSAPEAR_SHOOT),
                    Type = Float,
                    SAINDefault = look.GOAL_TO_FULL_DISSAPEAR_SHOOT,
                    Max = 2,
                    Min = 0.01f,
                    Rounding = 100f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(look.MAX_VISION_GRASS_METERS),
                    Type = Float,
                    SAINDefault = look.MAX_VISION_GRASS_METERS,
                    Max = 30,
                    Min = 1f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(look.MAX_VISION_GRASS_METERS_OPT),
                    Type = Float,
                    SAINDefault = look.MAX_VISION_GRASS_METERS_OPT,
                    Max = 30,
                    Min = 1f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(look.MAX_VISION_GRASS_METERS_FLARE),
                    Type = Float,
                    SAINDefault = look.MAX_VISION_GRASS_METERS_FLARE,
                    Max = 30,
                    Min = 1f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(look.MAX_VISION_GRASS_METERS_FLARE_OPT),
                    Type = Float,
                    SAINDefault = look.MAX_VISION_GRASS_METERS_FLARE_OPT,
                    Max = 30,
                    Min = 0.1f,
                    Rounding = 10f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(look.NO_GRASS_DIST),
                    Type = Float,
                    SAINDefault = look.NO_GRASS_DIST,
                    Max = 30,
                    Min = 1f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(look.NO_GREEN_DIST),
                    Type = Float,
                    SAINDefault = look.NO_GREEN_DIST,
                    Max = 30,
                    Min = 0.1f,
                    Rounding = 10f
                });
        }

        public static void BotMoveSettingsInit()
        {
            var move = new SAINMoveSettings();
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(move.RUN_IF_GAOL_FAR_THEN),
                    Type = Float,
                    SAINDefault = move.RUN_IF_GAOL_FAR_THEN
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(move.CHANCE_TO_RUN_IF_NO_AMMO_0_100),
                    Type = Float,
                    SAINDefault = move.CHANCE_TO_RUN_IF_NO_AMMO_0_100
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(move.SEC_TO_CHANGE_TO_RUN),
                    Type = Float,
                    SAINDefault = move.SEC_TO_CHANGE_TO_RUN
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(move.RUN_TO_COVER_MIN),
                    Type = Float,
                    SAINDefault = move.RUN_TO_COVER_MIN
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(move.BASE_ROTATE_SPEED),
                    Type = Float,
                    SAINDefault = move.BASE_ROTATE_SPEED,
                    Max = 500f,
                    Min = 100f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(move.FIRST_TURN_SPEED),
                    Type = Float,
                    SAINDefault = move.FIRST_TURN_SPEED,
                    Max = 500f,
                    Min = 100f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(move.FIRST_TURN_BIG_SPEED),
                    Type = Float,
                    SAINDefault = move.FIRST_TURN_BIG_SPEED,
                    Max = 500f,
                    Min = 100f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(move.TURN_SPEED_ON_SPRINT),
                    Type = Float,
                    SAINDefault = move.TURN_SPEED_ON_SPRINT,
                    Max = 500f,
                    Min = 100f
                });
        }

        public static void BotMindSettingsInit()
        {
            var mind = new SAINMindSettings();
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(mind.TIME_TO_FORGOR_ABOUT_ENEMY_SEC),
                    DisplayName = "Time to Forget About Enemy",
                    Type = Float,
                    SAINDefault = mind.TIME_TO_FORGOR_ABOUT_ENEMY_SEC,
                    Max = 1000f,
                    Min = 30f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(mind.CHANCE_FUCK_YOU_ON_CONTACT_100),
                    Type = Float,
                    SAINDefault = mind.CHANCE_FUCK_YOU_ON_CONTACT_100,
                    Max = 100f,
                    Min = 0f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(mind.SURGE_KIT_ONLY_SAFE_CONTAINER),
                    Type = Float,
                    SAINDefault = mind.SURGE_KIT_ONLY_SAFE_CONTAINER
                });


            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(mind.SEC_TO_MORE_DIST_TO_RUN),
                    Type = Float,
                    SAINDefault = mind.SEC_TO_MORE_DIST_TO_RUN
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(mind.DIST_TO_STOP_RUN_ENEMY),
                    Type = Float,
                    SAINDefault = mind.DIST_TO_STOP_RUN_ENEMY
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(mind.NO_RUN_AWAY_FOR_SAFE),
                    Type = Bool,
                    SAINDefault = mind.NO_RUN_AWAY_FOR_SAFE
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(mind.CAN_USE_MEDS),
                    Type = Bool,
                    SAINDefault = mind.CAN_USE_MEDS
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(mind.CAN_USE_FOOD_DRINK),
                    Type = Bool,
                    SAINDefault = mind.CAN_USE_FOOD_DRINK
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(mind.GROUP_ANY_PHRASE_DELAY),
                    Type = Float,
                    SAINDefault = mind.GROUP_ANY_PHRASE_DELAY
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Hidden = true,
                    Key = nameof(mind.GROUP_EXACTLY_PHRASE_DELAY),
                    Type = Float,
                    SAINDefault = mind.GROUP_EXACTLY_PHRASE_DELAY
                });
        }

        public static void BotPatrolSettingsInit()
        {
        }

        public static void BotScatterSettingsInit()
        {
            var scatter = new SAINScatterSettings();
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(scatter.MinScatter),
                    DisplayName = "Min Scatter",
                    Type = Float,
                    SAINDefault = scatter.MinScatter,
                    Max = 1f,
                    Min = 0.01f,
                    Rounding = 100f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(scatter.MaxScatter),
                    DisplayName = "Max Scatter",
                    Type = Float,
                    SAINDefault = scatter.MaxScatter,
                    Max = 1f,
                    Min = 0.01f,
                    Rounding = 100f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(scatter.WorkingScatter),
                    DisplayName = "Working Scatter",
                    Type = Float,
                    SAINDefault = scatter.WorkingScatter,
                    Max = 1f,
                    Min = 0.01f,
                    Rounding = 100f
                });
        }

        public static void BotShootSettingsInit()
        {
            var shoot = new SAINShootSettings();
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(shoot.CAN_STOP_SHOOT_CAUSE_ANIMATOR),
                    Type = Bool,
                    SAINDefault = shoot.CAN_STOP_SHOOT_CAUSE_ANIMATOR
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(shoot.CHANCE_TO_CHANGE_TO_AUTOMATIC_FIRE_100),
                    DisplayName = "Chance To Change to Fullauto",
                    Type = Float,
                    SAINDefault = shoot.CHANCE_TO_CHANGE_TO_AUTOMATIC_FIRE_100,
                    Max = 100f,
                    Min = 0f,
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(shoot.AUTOMATIC_FIRE_SCATTERING_COEF),
                    DisplayName = "Fullauto Scatter Multiplier",
                    Type = Float,
                    SAINDefault = shoot.AUTOMATIC_FIRE_SCATTERING_COEF,
                    Max = 5f,
                    Min = 1f,
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(shoot.BASE_AUTOMATIC_TIME),
                    DisplayName = "Base Fullauto Time",
                    Type = Float,
                    SAINDefault = shoot.BASE_AUTOMATIC_TIME,
                    Max = 5f,
                    Min = 0.1f,
                    Rounding = 10f
                });
            BotSettingsDictionary.Add(
                new SettingsWrapper
                {
                    Key = nameof(shoot.RECOIL_DELTA_PRESS),
                    DisplayName = "RECOIL_DELTA_PRESS",
                    Description = "How much time must pass before a bot is affected by their weapon's recoil when they start shooting.",
                    Type = Float,
                    SAINDefault = shoot.RECOIL_DELTA_PRESS,
                    Max = 0.5f,
                    Min = 0f,
                });
        }
    }
}
