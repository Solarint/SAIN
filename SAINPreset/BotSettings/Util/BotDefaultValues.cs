using EFT;
using SAIN.Helpers;
using System.Collections.Generic;
using System.Reflection;

namespace SAIN.BotSettings.Categories.Util
{
    public class BotDefaultValues
    {
        public SAINSettings DefaultSettings = new SAINSettings();

        public void Init(BotOwner owner)
        {
            if (!Initialized)
            {
                Initialized = true;
                InitCore(owner.Settings);
                var settings = owner.Settings.FileSettings;
                Init(settings.Aiming);
                Init(settings.Grenade);
                Init(settings.Look);
                Init(settings.Mind);
                Init(settings.Move);
                Init(settings.Scattering);
                Init(settings.Shoot);
            }
        }

        bool Initialized = false;
        public void InitCore(BotDifficultySettingsClass settingsClass)
        {
            var settings = settingsClass.FileSettings.Core;
            var Core = DefaultSettings.Core;
            Core.VisibleAngle = settings.VisibleAngle;
            Core.VisibleDistance = settings.VisibleDistance;
            Core.GainSightCoef = settings.GainSightCoef;
            Core.ScatteringPerMeter = settings.ScatteringPerMeter;
            Core.DamageCoeff = settings.DamageCoeff;
            Core.CanRun = settings.CanRun;
            Core.CanGrenade = settings.CanGrenade;
        }
        public void Init(BotGlobalAimingSettings settings)
        {
            var Aiming = DefaultSettings.Aiming;
            Aiming.AIMING_TYPE = settings.AIMING_TYPE;
            Aiming.BASE_HIT_AFFECTION_DELAY_SEC = settings.BASE_HIT_AFFECTION_DELAY_SEC;
            Aiming.BASE_HIT_AFFECTION_MAX_ANG = settings.BASE_HIT_AFFECTION_MAX_ANG;
            Aiming.BASE_HIT_AFFECTION_MIN_ANG = settings.BASE_HIT_AFFECTION_MIN_ANG;
            Aiming.MAX_AIMING_UPGRADE_BY_TIME = settings.MAX_AIMING_UPGRADE_BY_TIME;
            Aiming.MAX_AIM_TIME = settings.MAX_AIM_TIME;
            Aiming.RECALC_MUST_TIME = settings.RECALC_MUST_TIME;
            Aiming.RECALC_MUST_TIME_MIN = settings.RECALC_MUST_TIME_MIN;
            Aiming.RECALC_MUST_TIME_MAX = settings.RECALC_MUST_TIME_MAX;
        }
        public void Init(BotGlobalsGrenadeSettings settings)
        {
            var Grenade = DefaultSettings.Grenade;
            Grenade.CHANCE_TO_NOTIFY_ENEMY_GR_100 = settings.CHANCE_TO_NOTIFY_ENEMY_GR_100;
            Grenade.DELTA_GRENADE_START_TIME = settings.DELTA_GRENADE_START_TIME;
            Grenade.BEWARE_TYPE = settings.BEWARE_TYPE;
        }
        public void Init(BotGlobalLookData settings)
        {
            var Look = DefaultSettings.Look;
            Look.CAN_USE_LIGHT = settings.CAN_USE_LIGHT;
            Look.FULL_SECTOR_VIEW = settings.FULL_SECTOR_VIEW;
            Look.MAX_DIST_CLAMP_TO_SEEN_SPEED = settings.MAX_DIST_CLAMP_TO_SEEN_SPEED;
            Look.NIGHT_VISION_ON = settings.NIGHT_VISION_ON;
            Look.NIGHT_VISION_OFF = settings.NIGHT_VISION_OFF;
            Look.NIGHT_VISION_DIST = settings.NIGHT_VISION_DIST;
            Look.VISIBLE_ANG_NIGHTVISION = settings.VISIBLE_ANG_NIGHTVISION;
            Look.LOOK_THROUGH_PERIOD_BY_HIT = settings.LOOK_THROUGH_PERIOD_BY_HIT;
            Look.LightOnVisionDistance = settings.LightOnVisionDistance;
            Look.VISIBLE_ANG_LIGHT = settings.VISIBLE_ANG_LIGHT;
            Look.VISIBLE_DISNACE_WITH_LIGHT = settings.VISIBLE_DISNACE_WITH_LIGHT;
            Look.GOAL_TO_FULL_DISSAPEAR = settings.GOAL_TO_FULL_DISSAPEAR;
            Look.GOAL_TO_FULL_DISSAPEAR_GREEN = settings.GOAL_TO_FULL_DISSAPEAR_GREEN;
            Look.GOAL_TO_FULL_DISSAPEAR_SHOOT = settings.GOAL_TO_FULL_DISSAPEAR_SHOOT;
            Look.MAX_VISION_GRASS_METERS = settings.MAX_VISION_GRASS_METERS;
            Look.MAX_VISION_GRASS_METERS_OPT = settings.MAX_VISION_GRASS_METERS_OPT;
            Look.MAX_VISION_GRASS_METERS_FLARE = settings.MAX_VISION_GRASS_METERS_FLARE;
            Look.MAX_VISION_GRASS_METERS_FLARE_OPT = settings.MAX_VISION_GRASS_METERS_FLARE_OPT;
            Look.NO_GREEN_DIST = settings.NO_GREEN_DIST;
            Look.NO_GRASS_DIST = settings.NO_GRASS_DIST;
        }
        public void Init(BotGlobalsMindSettings settings)
        {
            var Mind = DefaultSettings.Mind;
            Mind.TIME_TO_FORGOR_ABOUT_ENEMY_SEC = settings.TIME_TO_FORGOR_ABOUT_ENEMY_SEC;
            Mind.CHANCE_FUCK_YOU_ON_CONTACT_100 = settings.CHANCE_FUCK_YOU_ON_CONTACT_100;
            Mind.SURGE_KIT_ONLY_SAFE_CONTAINER = settings.SURGE_KIT_ONLY_SAFE_CONTAINER;
            Mind.SEC_TO_MORE_DIST_TO_RUN = settings.SEC_TO_MORE_DIST_TO_RUN;
            Mind.DIST_TO_STOP_RUN_ENEMY = settings.DIST_TO_STOP_RUN_ENEMY;
            Mind.NO_RUN_AWAY_FOR_SAFE = settings.NO_RUN_AWAY_FOR_SAFE;
            Mind.CAN_USE_MEDS = settings.CAN_USE_MEDS;
            Mind.CAN_USE_FOOD_DRINK = settings.CAN_USE_FOOD_DRINK;
            Mind.GROUP_ANY_PHRASE_DELAY = settings.GROUP_ANY_PHRASE_DELAY;
            Mind.GROUP_EXACTLY_PHRASE_DELAY = settings.GROUP_EXACTLY_PHRASE_DELAY;
        }
        public void Init(BotGlobalsMoveSettings settings)
        {
            var Move = DefaultSettings.Move;
            Move.RUN_IF_GAOL_FAR_THEN = settings.RUN_IF_GAOL_FAR_THEN;
            Move.CHANCE_TO_RUN_IF_NO_AMMO_0_100 = settings.CHANCE_TO_RUN_IF_NO_AMMO_0_100;
            Move.SEC_TO_CHANGE_TO_RUN = settings.SEC_TO_CHANGE_TO_RUN;
            Move.RUN_TO_COVER_MIN = settings.RUN_TO_COVER_MIN;
            Move.BASE_ROTATE_SPEED = settings.BASE_ROTATE_SPEED;
            Move.FIRST_TURN_SPEED = settings.FIRST_TURN_SPEED;
            Move.FIRST_TURN_BIG_SPEED = settings.FIRST_TURN_BIG_SPEED;
            Move.TURN_SPEED_ON_SPRINT = settings.TURN_SPEED_ON_SPRINT;
        }
        public void Init(BotGlobalsScatteringSettings settings)
        {
            var Scatter = DefaultSettings.Scattering;
            Scatter.MinScatter = settings.MinScatter;
            Scatter.WorkingScatter = settings.WorkingScatter;
            Scatter.MaxScatter = settings.MaxScatter;
        }
        public void Init(BotGlobalShootData settings)
        {
            var Shoot = DefaultSettings.Shoot;
            Shoot.CAN_STOP_SHOOT_CAUSE_ANIMATOR = settings.CAN_STOP_SHOOT_CAUSE_ANIMATOR;
            Shoot.CHANCE_TO_CHANGE_TO_AUTOMATIC_FIRE_100 = settings.CHANCE_TO_CHANGE_TO_AUTOMATIC_FIRE_100;
            Shoot.AUTOMATIC_FIRE_SCATTERING_COEF = settings.AUTOMATIC_FIRE_SCATTERING_COEF;
            Shoot.BASE_AUTOMATIC_TIME = settings.BASE_AUTOMATIC_TIME;
            Shoot.RECOIL_DELTA_PRESS = settings.RECOIL_DELTA_PRESS;
        }
    }
}
