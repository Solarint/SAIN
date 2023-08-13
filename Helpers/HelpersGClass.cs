using Comfort.Common;
using EFT;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using EFTCore = GClass563;
using EFTFileSettings = GClass564;
using EFTSettingsGroup = GClass566;
using EFTSoundPlayer = GClass635;
using EFTStatModifiersClass = GClass561;
using EFTTime = GClass1292;
using EFTSearchPoint = GClass273;

namespace SAIN.Helpers
{

    internal class HelpersGClass
    {
        static HelpersGClass()
        {
            InventoryControllerProp = AccessTools.Property(typeof(Player), "GClass2659_0");
            EFTBotSettingsProp = AccessTools.Property(typeof(BotDifficultySettingsClass), "FileSettings");
        }

        public static readonly PropertyInfo EFTBotSettingsProp;
        public static readonly PropertyInfo InventoryControllerProp;

        public static InventoryControllerClass GetInventoryController(Player player)
        {
            return (InventoryControllerClass)InventoryControllerProp.GetValue(player);
        }

        public static EFTSettingsGroup GetEFTSettings(WildSpawnType type, BotDifficulty difficulty)
        {
            return (EFTSettingsGroup)SAINPlugin.LoadedPreset.BotSettings.GetEFTSettings(type, difficulty);
        }

        public DateTime UTCNow => EFTTime.UtcNow;
        public static EFTCoreSettings EFTCore => SAINPlugin.LoadedPreset.GlobalSettings.EFTCoreSettings;
        public static float LAY_DOWN_ANG_SHOOT => EFTCore.Core.LAY_DOWN_ANG_SHOOT;
        public static float Gravity => EFTCore.Core.G;
        public static float SMOKE_GRENADE_RADIUS_COEF => EFTCore.Core.SMOKE_GRENADE_RADIUS_COEF;

        public static void PlaySound(IAIDetails player, Vector3 pos, float range, AISoundType soundtype)
        {
            Singleton<EFTSoundPlayer>.Instance?.PlaySound(player, pos, range, soundtype);
        }
    }

    public class TemporaryStatModifiers
    {
        public TemporaryStatModifiers(float precision, float accuracySpeed, float gainSight, float scatter, float priorityScatter)
        {
            Modifiers = new EFTStatModifiersClass
            {
                PrecicingSpeedCoef = precision,
                AccuratySpeedCoef = accuracySpeed,
                GainSightCoef = gainSight,
                ScatteringCoef = scatter,
                PriorityScatteringCoef = priorityScatter,
            };
        }

        public EFTStatModifiersClass Modifiers;
    }

    public class SearchPoint
    {
        public EFTSearchPoint Point;
    }

    public class EFTCoreSettings
    {
        public static EFTCoreSettings GetCore()
        {
            UpdateCoreSettings();
            return new EFTCoreSettings
            {
                Core = EFTFileSettings.Core,
            };
        }

        public static void UpdateCoreSettings()
        {
            var core = EFTFileSettings.Core;
            core.SCAV_GROUPS_TOGETHER = false;
            core.DIST_NOT_TO_GROUP = 50f;
            core.DIST_NOT_TO_GROUP_SQR = 50f * 50f;
            core.MIN_DIST_TO_STOP_RUN = 0f;
            core.CAN_SHOOT_TO_HEAD = false;
            core.ARMOR_CLASS_COEF = 7f;
            core.SHOTGUN_POWER = 40f;
            core.RIFLE_POWER = 50f;
            core.PISTOL_POWER = 20f;
            core.SMG_POWER = 60f;
            core.SNIPE_POWER = 5f;
        }

        public static void UpdateCoreSettings(EFTCoreSettings newCore)
        {
            EFTFileSettings.Core = newCore.Core;
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
            foreach (BotDifficulty diff in difficulties)
            {
                Settings.Add(diff, EFTFileSettings.GetSettings(diff, type));
            }
        }

        public string Name;
        public WildSpawnType WildSpawnType;
        public Dictionary<BotDifficulty, EFTSettingsGroup> Settings = new Dictionary<BotDifficulty, EFTSettingsGroup>();
    }
}