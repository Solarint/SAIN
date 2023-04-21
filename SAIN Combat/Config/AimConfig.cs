using BepInEx.Configuration;

namespace SAIN.Combat.Configs
{
    internal class AimingConfig
    {

        // recoil
        public static ConfigEntry<float> ScatterMultiplier { get; private set; }
        public static ConfigEntry<float> MaxScatter { get; private set; }
        public static ConfigEntry<float> AddRecoil { get; private set; }
        public static ConfigEntry<float> HorizontalRecoilCoef { get; private set; }

        // aim
        public static ConfigEntry<float> TIME_COEF_IF_MOVE { get; private set; }
        public static ConfigEntry<float> PANIC_COEF { get; private set; }
        public static ConfigEntry<float> COEF_FROM_COVER { get; private set; }
        public static ConfigEntry<float> BOTTOM_COEF { get; private set; }
        public static ConfigEntry<float> AccuratySpeed { get; private set; }
        public static ConfigEntry<float> MAX_AIM_TIME { get; private set; }

        // Scatter
        public static ConfigEntry<float> PANIC_ACCURATY_COEF { get; private set; }
        public static ConfigEntry<float> HARD_AIM { get; private set; }
        public static ConfigEntry<float> XZ_COEF { get; private set; }
        public static ConfigEntry<float> BASE_SHIEF { get; private set; }
        public static ConfigEntry<float> SCATTERING_HAVE_DAMAGE_COEF { get; private set; }
        public static ConfigEntry<float> SCATTERING_DIST_MODIF { get; private set; }
        public static ConfigEntry<float> SCATTERING_DIST_MODIF_CLOSE { get; private set; }
        public static ConfigEntry<float> DIST_TO_SHOOT_TO_CENTER { get; private set; }
        public static ConfigEntry<float> COEF_IF_MOVE { get; private set; }
        public static ConfigEntry<float> NEXT_SHOT_MISS_Y_OFFSET { get; private set; }
        public static ConfigEntry<float> Y_TOP_OFFSET_COEF { get; private set; }
        public static ConfigEntry<float> Y_BOTTOM_OFFSET_COEF { get; private set; }
        public static ConfigEntry<int> BAD_SHOOTS_MIN { get; private set; }
        public static ConfigEntry<int> BAD_SHOOTS_MAX { get; private set; }
        public static ConfigEntry<float> BAD_SHOOTS_OFFSET { get; private set; }
        public static ConfigEntry<float> BAD_SHOOTS_MAIN_COEF { get; private set; }
        public static ConfigEntry<int> Difficulty { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string recoil = "1. Recoil";

            ScatterMultiplier = Config.Bind(recoil, "Scatter Multiplier", 1f,
                new ConfigDescription("",
                new AcceptableValueRange<float>(0.01f, 2.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4 }));

            MaxScatter = Config.Bind(recoil, "Max Scatter", 2f,
                new ConfigDescription("",
                new AcceptableValueRange<float>(0.5f, 5.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 3 }));

            AddRecoil = Config.Bind(recoil, "Add Recoil Scatter", 0f,
                new ConfigDescription("",
                new AcceptableValueRange<float>(-0.5f, 0.5f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 3 }));


            string aim = "1. Aim";

            TIME_COEF_IF_MOVE = Config.Bind(aim, "TIME_COEF_IF_MOVE", 1.5f,
                new ConfigDescription("Translate: It will take so long to aim if the bot shoots immediately",
                new AcceptableValueRange<float>(0.01f, 5.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4, Browsable = false }));

            PANIC_COEF = Config.Bind(aim, "PANIC_COEF", 3.5f,
                new ConfigDescription("Translate: Aim time is multiplied by this factor if the char is panicking",
                new AcceptableValueRange<float>(0.01f, 5.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4, Browsable = false }));

            COEF_FROM_COVER = Config.Bind(aim, "COEF_FROM_COVER", 0.85f,
                new ConfigDescription("Translate: increased targeting when looking out of cover",
                new AcceptableValueRange<float>(0.01f, 1.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4, Browsable = false }));

            BOTTOM_COEF = Config.Bind(aim, "BOTTOM_COEF", 0.3f,
                new ConfigDescription("Translate: Base aiming time. Added to the results obtained by the formula",
                new AcceptableValueRange<float>(0.01f, 1.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4, Browsable = false }));

            AccuratySpeed = Config.Bind(aim, "AccuratySpeed", 0.3f,
                new ConfigDescription("",
                new AcceptableValueRange<float>(0.01f, 2.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4, Browsable = false }));

            MAX_AIM_TIME = Config.Bind(aim, "MAX_AIM_TIME", 1.5f,
                new ConfigDescription("",
                new AcceptableValueRange<float>(0.01f, 10.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4, Browsable = false }));


            string scatter = "1. Scatter";

            BAD_SHOOTS_MAIN_COEF = Config.Bind(scatter, "BAD_SHOOTS_MAIN_COEF", 1f,
              new ConfigDescription("Translate: Basic coefficient from the formula == N N*ln(x/5+1.2)",
                new AcceptableValueRange<float>(0.01f, 3.0f),
                new ConfigurationManagerAttributes { IsAdvanced = false, Order = 4, Browsable = false }));

            BASE_SHIEF = Config.Bind(scatter, "BASE_SHIFT", 1f,
              new ConfigDescription("Translate: Base shift in meters for aiming (example: BASE_SHIEF=5 => means at a distance of 20 meters it will aim like 20+5=25",
                new AcceptableValueRange<float>(0.01f, 3.0f),
              new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1, Browsable = false }));

            SCATTERING_DIST_MODIF_CLOSE = Config.Bind(scatter, "SCATTERING_DIST_MODIF_CLOSE", 0.6f,
              new ConfigDescription("Translate: The modifier of the dependence of aiming on the distance is not linear (another parameter is responsible for the linearity). //recommended values ​​0.2..1.3. //Less than 1 means the farther away the more accurate the linear dependence will be. More - more",
                new AcceptableValueRange<float>(0.01f, 3.0f),
              new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1, Browsable = false }));

            SCATTERING_DIST_MODIF = Config.Bind(scatter, "SCATTERING_DIST_MODIF", 0.8f,
              new ConfigDescription("Translate: The modifier of the dependence of aiming on the distance is not linear (another parameter is responsible for the linearity). //recommended values ​​0.2..1.3. //Less than 1 means the farther away the more accurate the linear dependence will be. More - more",
                new AcceptableValueRange<float>(0.01f, 3.0f),
              new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1, Browsable = false }));

            XZ_COEF = Config.Bind(scatter, "XZ_COEF", 0.15f,
              new ConfigDescription("Translate: Coefficient of dependence of aiming in the horizontal plane depending on the angle to the target",
                new AcceptableValueRange<float>(0.01f, 1.0f),
              new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1, Browsable = false }));

            NEXT_SHOT_MISS_Y_OFFSET = Config.Bind(scatter, "NEXT_SHOT_MISS_Y_OFFSET", 1f,
              new ConfigDescription("Translate: How much higher will the bot shooting it wants to miss",
                new AcceptableValueRange<float>(0.01f, 2.0f),
              new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1, Browsable = false }));

            BAD_SHOOTS_OFFSET = Config.Bind(scatter, "BAD_SHOOTS_OFFSET", 1f,
              new ConfigDescription("Translate: If we shoot past, then the bot will move its sight so far from the target in meters",
                new AcceptableValueRange<float>(0.01f, 2.0f),
              new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1, Browsable = false }));

            PANIC_ACCURATY_COEF = Config.Bind(scatter, "PANIC_ACCURATY_COEF", 3f,
                new ConfigDescription("Translate: ",
                new AcceptableValueRange<float>(0.01f, 1.0f),
              new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1, Browsable = false }));

            HARD_AIM = Config.Bind(scatter, "HARD_AIM", 0.75f,
                new ConfigDescription("Translate: Improved aiming factor",
                new AcceptableValueRange<float>(0.25f, 2.0f),
              new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1, Browsable = false }));

            COEF_IF_MOVE = Config.Bind(scatter, "COEF_IF_MOVE", 1.5f,
                new ConfigDescription("Translate: There will be so much more expansion if the bot shoots immediately",
                new AcceptableValueRange<float>(0.01f, 5.0f),
              new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1, Browsable = false }));

            Y_TOP_OFFSET_COEF = Config.Bind(scatter, "Y_TOP_OFFSET_COEF", 0.2f,
                new ConfigDescription("Translate: Aiming sphere top compression level",
                new AcceptableValueRange<float>(0.01f, 1.0f),
              new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1, Browsable = false }));

            Y_BOTTOM_OFFSET_COEF = Config.Bind(scatter, "Y_BOTTOM_OFFSET_COEF", 0.2f,
                new ConfigDescription("Translate: Scope bottom compression level",
                new AcceptableValueRange<float>(0.01f, 1.0f),
              new ConfigurationManagerAttributes { IsAdvanced = false, Order = 1, Browsable = false }));
        }
    }
}
/*
public static ConfigEntry<float> MAX_AIM_PRECICING { get; private set; }
public static ConfigEntry<float> BETTER_PRECICING_COEF { get; private set; }
public static ConfigEntry<float> RECLC_Y_DIST { get; private set; }
public static ConfigEntry<float> RECALC_DIST { get; private set; }
public static ConfigEntry<float> RECALC_SQR_DIST;
public static ConfigEntry<float> COEF_FROM_COVER { get; private set; }
public static ConfigEntry<float> PANIC_COEF { get; private set; }
public static ConfigEntry<float> PANIC_ACCURATY_COEF { get; private set; }
public static ConfigEntry<float> HARD_AIM { get; private set; }
public static ConfigEntry<int> HARD_AIM_CHANCE_100 { get; private set; }
public static ConfigEntry<float> PANIC_TIME { get; private set; }
public static ConfigEntry<int> RECALC_MUST_TIME { get; private set; }
public static ConfigEntry<int> RECALC_MUST_TIME_MIN { get; private set; }
public static ConfigEntry<int> RECALC_MUST_TIME_MAX { get; private set; }
public static ConfigEntry<float> DAMAGE_PANIC_TIME { get; private set; }
public static ConfigEntry<float> DANGER_UP_POINT { get; private set; }
public static ConfigEntry<float> MAX_AIMING_UPGRADE_BY_TIME { get; private set; }
public static ConfigEntry<float> DAMAGE_TO_DISCARD_AIM_0_100 { get; private set; }
public static ConfigEntry<float> MIN_TIME_DISCARD_AIM_SEC { get; private set; }
public static ConfigEntry<float> MAX_TIME_DISCARD_AIM_SEC { get; private set; }
public static ConfigEntry<float> XZ_COEF { get; private set; }
public static ConfigEntry<float> XZ_COEF_STATIONARY_BULLET { get; private set; }
public static ConfigEntry<float> XZ_COEF_STATIONARY_GRENADE { get; private set; }
public int SHOOT_TO_CHANGE_PRIORITY { get; private set; }
public static ConfigEntry<float> FIRST_CONTACT_ADD_SEC { get; private set; }
public static ConfigEntry<float> FIRST_CONTACT_ADD_CHANCE_100 { get; private set; }
public static ConfigEntry<float> BASE_HIT_AFFECTION_DELAY_SEC { get; private set; }
public static ConfigEntry<float> BASE_HIT_AFFECTION_MIN_ANG { get; private set; }
public static ConfigEntry<float> BASE_HIT_AFFECTION_MAX_ANG { get; private set; }
public static ConfigEntry<float> BASE_SHIEF { get; private set; }
public static ConfigEntry<float> BASE_SHIEF_STATIONARY_BULLET { get; private set; }
public static ConfigEntry<float> BASE_SHIEF_STATIONARY_GRENADE { get; private set; }
public static ConfigEntry<float> SCATTERING_HAVE_DAMAGE_COEF { get; private set; }
public static ConfigEntry<float> SCATTERING_DIST_MODIF { get; private set; }
public static ConfigEntry<float> SCATTERING_DIST_MODIF_CLOSE { get; private set; }
public static ConfigEntry<int> AIMING_TYPE { get; private set; }
public static ConfigEntry<float> DIST_TO_SHOOT_TO_CENTER { get; private set; }
public static ConfigEntry<float> DIST_TO_SHOOT_NO_OFFSET { get; private set; }
public static ConfigEntry<float> SHPERE_FRIENDY_FIRE_SIZE { get; private set; }
public static ConfigEntry<float> COEF_IF_MOVE { get; private set; }
public static ConfigEntry<float> TIME_COEF_IF_MOVE { get; private set; }
public static ConfigEntry<float> BOT_MOVE_IF_DELTA { get; private set; }
public static ConfigEntry<float> NEXT_SHOT_MISS_CHANCE_100 { get; private set; }
public static ConfigEntry<float> NEXT_SHOT_MISS_Y_OFFSET { get; private set; }
public static ConfigEntry<float> ANYTIME_LIGHT_WHEN_AIM_100 { get; private set; }
public static ConfigEntry<float> ANY_PART_SHOOT_TIME { get; private set; }
public static ConfigEntry<float> WEAPON_ROOT_OFFSET { get; private set; }
public static ConfigEntry<float> MIN_DAMAGE_TO_GET_HIT_AFFETS { get; private set; }
public static ConfigEntry<float> MAX_AIM_TIME { get; private set; }
public static ConfigEntry<float> OFFSET_RECAL_ANYWAY_TIME { get; private set; }
public static ConfigEntry<float> Y_TOP_OFFSET_COEF { get; private set; }
public static ConfigEntry<float> Y_BOTTOM_OFFSET_COEF { get; private set; }
public static ConfigEntry<float> STATIONARY_LEAVE_HALF_DEGREE { get; private set; }
/*
            MAX_AIM_PRECICING = Config.Bind(settings, "MAX_AIM_PRECICING", 0.7f);
            BETTER_PRECICING_COEF = 0.7f;
        RECLC_Y_DIST = 1.2f;
        RECALC_DIST = 0.7f;
        RECALC_SQR_DIST;
        COEF_FROM_COVER = 0.85f;
        PANIC_COEF = 3.5f;
        PANIC_ACCURATY_COEF = 3f;
        HARD_AIM = 0.75f;
        HARD_AIM_CHANCE_100 = 100;
        PANIC_TIME = 6f;
        RECALC_MUST_TIME = 3;
        RECALC_MUST_TIME_MIN = 1;
        RECALC_MUST_TIME_MAX = 2;
        DAMAGE_PANIC_TIME = 25f;
        DANGER_UP_POINT = 1.3f;
        MAX_AIMING_UPGRADE_BY_TIME = 0.7f;
        DAMAGE_TO_DISCARD_AIM_0_100 = 86f;
        MIN_TIME_DISCARD_AIM_SEC = 0.3f;
        MAX_TIME_DISCARD_AIM_SEC = 1.3f;
        XZ_COEF = 0.15f;
        XZ_COEF_STATIONARY_BULLET = 0.15f;
        XZ_COEF_STATIONARY_GRENADE = 0.25f;
        SHOOT_TO_CHANGE_PRIORITY = 5525;
        BOTTOM_COEF = 0.3f;
        FIRST_CONTACT_ADD_SEC = 0.1f;
        FIRST_CONTACT_ADD_CHANCE_100 = 80f;
        BASE_HIT_AFFECTION_DELAY_SEC = 0.77f;
        BASE_HIT_AFFECTION_MIN_ANG = 4f;
        BASE_HIT_AFFECTION_MAX_ANG = 18f;
        BASE_SHIEF = 1f;
        BASE_SHIEF_STATIONARY_BULLET = 0.05f;
        BASE_SHIEF_STATIONARY_GRENADE = 0.05f;
        SCATTERING_HAVE_DAMAGE_COEF = 2f;
        SCATTERING_DIST_MODIF = 0.8f;
        SCATTERING_DIST_MODIF_CLOSE = 0.6f;
        AIMING_TYPE = 2;
        DIST_TO_SHOOT_TO_CENTER = 3f;
        DIST_TO_SHOOT_NO_OFFSET = 3f;
        SHPERE_FRIENDY_FIRE_SIZE = -1f;
        COEF_IF_MOVE = 1.5f;
        TIME_COEF_IF_MOVE = 1.5f;
        BOT_MOVE_IF_DELTA = 0.01f;
        NEXT_SHOT_MISS_CHANCE_100 = 100f;
        NEXT_SHOT_MISS_Y_OFFSET = 1f;
        ANYTIME_LIGHT_WHEN_AIM_100 = -1f;
        ANY_PART_SHOOT_TIME = 900f;
        WEAPON_ROOT_OFFSET = 0.35f;
        MIN_DAMAGE_TO_GET_HIT_AFFETS = 1f;
        MAX_AIM_TIME = 1.5f;
        OFFSET_RECAL_ANYWAY_TIME = 1f;
        Y_TOP_OFFSET_COEF = 0.2f;
        Y_BOTTOM_OFFSET_COEF = 0.2f;
        STATIONARY_LEAVE_HALF_DEGREE = 45f;
        BAD_SHOOTS_MIN;
        BAD_SHOOTS_MAX;
        BAD_SHOOTS_OFFSET = 1f;
        BAD_SHOOTS_MAIN_COEF = 1f;
        START_TIME_COEF = 1f;
            */