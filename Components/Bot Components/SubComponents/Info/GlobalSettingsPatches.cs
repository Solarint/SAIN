using EFT;

namespace SAIN.Classes
{
    public class GlobalSettings
    {
        public static void Update(BotOwner Botowner)
        {
            var fileSettings = Botowner.Settings.FileSettings;
            Update(fileSettings.Mind);
            Update(fileSettings.Aiming);
            Update(fileSettings.Shoot);
            Update(fileSettings.Scattering);
            Update(fileSettings.Move);
            Update(fileSettings.Grenade);
            Update(fileSettings.Look);
        }
        public static void Update(BotGlobalsMindSettings settings)
        {
            settings.TIME_TO_FORGOR_ABOUT_ENEMY_SEC = 240f;
            settings.SEC_TO_MORE_DIST_TO_RUN = 0f;
            settings.DIST_TO_STOP_RUN_ENEMY = 0f;
            settings.CHANCE_FUCK_YOU_ON_CONTACT_100 = 0f;
            settings.DIST_TO_STOP_RUN_ENEMY = 0f;
            settings.NO_RUN_AWAY_FOR_SAFE = true;
            settings.SURGE_KIT_ONLY_SAFE_CONTAINER = false;
            settings.CAN_USE_MEDS = true;
            settings.CAN_USE_FOOD_DRINK = true;
            settings.GROUP_ANY_PHRASE_DELAY = 5f;
            settings.GROUP_EXACTLY_PHRASE_DELAY = 5f;
        }
        public static void Update(BotGlobalAimingSettings settings)
        {
            settings.AIMING_TYPE = 4;
            settings.SHPERE_FRIENDY_FIRE_SIZE = 0.15f;
            //fileSettings.RECALC_MUST_TIME = 1;
            //fileSettings.RECALC_MUST_TIME_MIN = 1;
            //fileSettings.RECALC_MUST_TIME_MAX = 2;
            settings.BASE_HIT_AFFECTION_DELAY_SEC = 0.5f;
            settings.BASE_HIT_AFFECTION_MIN_ANG = 3f;
            settings.BASE_HIT_AFFECTION_MAX_ANG = 5f;
        }
        public static void Update(BotGlobalShootData settings)
        {
            settings.CAN_STOP_SHOOT_CAUSE_ANIMATOR = true;
            settings.CHANCE_TO_CHANGE_TO_AUTOMATIC_FIRE_100 = 100f;
            settings.AUTOMATIC_FIRE_SCATTERING_COEF = 1.5f;
            settings.BASE_AUTOMATIC_TIME = 0.5f;
            settings.RECOIL_DELTA_PRESS = 0.0f;
        }
        public static void Update(BotGlobalsScatteringSettings settings)
        {
            settings.MinScatter *= 0.9f;
            settings.WorkingScatter *= 0.9f;
            settings.MaxScatter *= 0.9f;
        }
        public static void Update(BotGlobalsMoveSettings settings)
        {
            settings.RUN_IF_GAOL_FAR_THEN = 0f;
            settings.CHANCE_TO_RUN_IF_NO_AMMO_0_100 = 100f;
            settings.SEC_TO_CHANGE_TO_RUN = 0f;
            settings.RUN_TO_COVER_MIN = 0f;
            settings.BASE_ROTATE_SPEED = 265f;
            settings.FIRST_TURN_SPEED = 275f;
            settings.FIRST_TURN_BIG_SPEED = 300f;
            //fileSettings.TURN_SPEED_ON_SPRINT = 320f;
        }
        public static void Update(BotGlobalsGrenadeSettings settings)
        {
            settings.CHANCE_TO_NOTIFY_ENEMY_GR_100 = 100f;
            settings.DELTA_GRENADE_START_TIME = 0.0f;
            settings.BEWARE_TYPE = 3;
        }
        public static void Update(BotGlobalLookData settings)
        {
            settings.CAN_USE_LIGHT = true;
            settings.FULL_SECTOR_VIEW = false;

            settings.MAX_DIST_CLAMP_TO_SEEN_SPEED = 500f;

            settings.NIGHT_VISION_ON = 75f;
            settings.NIGHT_VISION_OFF = 125f;
            settings.NIGHT_VISION_DIST = 125f;
            settings.VISIBLE_ANG_NIGHTVISION = 90f;

            settings.LOOK_THROUGH_PERIOD_BY_HIT = 0f;

            settings.LightOnVisionDistance = 40f;
            settings.VISIBLE_ANG_LIGHT = 30f;
            settings.VISIBLE_DISNACE_WITH_LIGHT = 50f;

            settings.GOAL_TO_FULL_DISSAPEAR = 0.25f;
            settings.GOAL_TO_FULL_DISSAPEAR_GREEN = 0.15f;
            settings.GOAL_TO_FULL_DISSAPEAR_SHOOT = 0.01f;

            settings.MAX_VISION_GRASS_METERS = 1f;
            settings.MAX_VISION_GRASS_METERS_OPT = 1f;
            settings.MAX_VISION_GRASS_METERS_FLARE = 4f;
            settings.MAX_VISION_GRASS_METERS_FLARE_OPT = 0.25f;

            settings.NO_GREEN_DIST = 3f;
            settings.NO_GRASS_DIST = 3f;
        }
    }
}
