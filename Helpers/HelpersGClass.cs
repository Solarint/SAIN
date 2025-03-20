using EFT;
using HarmonyLib;
using Newtonsoft.Json;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFTCore = GClass598;
using EFTStatModifiersClass = GClass595;

////////
// Fixed some GClass References here, but classes were renamed in the deobfuscation, so much of this isn't necessary anymore. Need to clean this up
////////

namespace SAIN.Helpers
{
    internal class HelpersGClass
    {
        static HelpersGClass()
        {
            InventoryControllerProp = AccessTools.Field(typeof(Player), "_inventoryController");
            EFTBotSettingsProp = AccessTools.Property(typeof(BotDifficultySettingsClass), "FileSettings");
            RefreshSettingsMethod = AccessTools.Method(typeof(BotDifficultySettingsClass), "method_0");
            PathControllerField = AccessTools.Field(typeof(BotMover), "_pathController");
            AimDataType = PatchConstants.EftTypes.Single(x => x.GetProperty("LastSpreadCount") != null && x.GetProperty("LastAimTime") != null);
        }

        public static readonly Type AimDataType;

        public static void RefreshSettings(BotDifficultySettingsClass settings)
        {
            RefreshSettingsMethod.Invoke(settings, null);
        }

        private static readonly MethodInfo RefreshSettingsMethod;

        public static readonly PropertyInfo EFTBotSettingsProp;
        public static readonly FieldInfo InventoryControllerProp;
        public static readonly FieldInfo PathControllerField;

        public static BotSettingsComponents GetEFTSettings(WildSpawnType type, BotDifficulty difficulty)
        {
            return (BotSettingsComponents)SAINPlugin.LoadedPreset.BotSettings.GetEFTSettings(type, difficulty);
        }

        public static PathControllerClass GetPathControllerClass(BotMover botMover)
        {
            return (PathControllerClass)PathControllerField.GetValue(botMover);
        }

        public static float LAY_DOWN_ANG_SHOOT => EFTCore.Core.LAY_DOWN_ANG_SHOOT;
        public static float Gravity => EFTCore.Core.G;
        public static float SMOKE_GRENADE_RADIUS_COEF => EFTCore.Core.SMOKE_GRENADE_RADIUS_COEF;
    }

    public class TemporaryStatModifiers
    {
        public TemporaryStatModifiers(float precision = 1f, float accuracySpeed = 1f, float gainSight = 1f, float scatter = 1f, float priorityScatter = 1f, float visibleDistance = 1f)
        {
            Modifiers = new EFTStatModifiersClass {
                PrecicingSpeedCoef = precision,
                AccuratySpeedCoef = accuracySpeed,
                GainSightCoef = gainSight,
                ScatteringCoef = scatter,
                PriorityScatteringCoef = priorityScatter,
                VisibleDistCoef = visibleDistance
            };
        }

        public EFTStatModifiersClass Modifiers;
    }

    public class EFTCoreSettings
    {
        public static void UpdateCoreSettings()
        {
            var core = EFTCore.Core;

            core.SCAV_GROUPS_TOGETHER = false;
            core.DIST_NOT_TO_GROUP = 50f;
            core.DIST_NOT_TO_GROUP_SQR = core.DIST_NOT_TO_GROUP.Sqr();

            //core.MIN_DIST_TO_STOP_RUN = 0f;

            core.CAN_SHOOT_TO_HEAD = true;

            core.SOUND_DOOR_OPEN_METERS = 40f;
            core.SOUND_DOOR_BREACH_METERS = 70f;
            core.JUMP_SPREAD_DIST = 65f;
            core.BASE_WALK_SPEREAD2 = 65f;
            core.GRENADE_PRECISION = 10;

            core.PRONE_POSE = 1f;
            core.MOVE_COEF = 1f;
            core.LOWER_POSE = 0f;
            core.MAX_POSE = 1f;

            core.FLARE_POWER = 1.75f;
            core.FLARE_TIME = 2.5f;

            core.SHOOT_TO_CHANGE_RND_PART_DELTA = 2f;

            ModDetection.UpdateArmorClassCoef();
        }

        public static void UpdateArmorClassCoef(float coef)
        {
            EFTCore.Core.ARMOR_CLASS_COEF = coef;
        }

        public EFTCore Core;
    }

    public class EFTBotSettings
    {
        [JsonConstructor]
        public EFTBotSettings()
        { }

        public EFTBotSettings(string name, WildSpawnType type, BotDifficulty[] difficulties)
        {
            Name = name;
            WildSpawnType = type;
            foreach (BotDifficulty diff in difficulties) {
                Settings.Add(diff, EFTCore.GetSettings(diff, type));
            }
        }

        [JsonProperty]
        public string Name;

        [JsonProperty]
        public WildSpawnType WildSpawnType;

        [JsonProperty]
        public Dictionary<BotDifficulty, BotSettingsComponents> Settings = new Dictionary<BotDifficulty, BotSettingsComponents>();
    }
}