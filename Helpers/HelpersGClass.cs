using Aki.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using static EFT.SpeedTree.TreeWind;
using SAIN.SAINPreset;
using Aki.Reflection.CodeWrapper;
using System.Security.Policy;
using Newtonsoft.Json;
using static EFT.Player;

namespace SAIN.Helpers
{
    internal class HelpersGClass
    {
        static HelpersGClass()
        {
            _InventoryControllerProp = AccessTools.Property(typeof(Player), "GClass2659_0");
        }

        static readonly PropertyInfo _InventoryControllerProp;

        public static InventoryControllerClass GetInventoryController(Player player)
        {
            return (InventoryControllerClass)_InventoryControllerProp.GetValue(player);
        }

        public static void LoadSettings()
        {
            GClass564.Load();
        }

        public static GClass570<BotDifficulty, WildSpawnType, GClass566> AllSettings => GClass564.AllSettings;

        public DateTime UTCNow => GClass1292.UtcNow;
        public static GClass563 Core => GClass564.Core;
        public static float LAY_DOWN_ANG_SHOOT => Core.LAY_DOWN_ANG_SHOOT;
        public static float Gravity => Core.G;
        public static float SMOKE_GRENADE_RADIUS_COEF => Core.SMOKE_GRENADE_RADIUS_COEF;

        public static GClass635 SoundPlayer => Singleton<GClass635>.Instance;

        public static void PlaySound(IAIDetails player, Vector3 pos, float range, AISoundType soundtype)
        {
            SoundPlayer?.PlaySound(player, pos, range, soundtype);
        }
    }

    public class BotStatModifiers
    {
        public BotStatModifiers(float precision, float accuracySpeed, float gainSight, float scatter, float priorityScatter)
        {
            Modifiers = new GClass561
            {
                PrecicingSpeedCoef = precision,
                AccuratySpeedCoef = accuracySpeed,
                GainSightCoef = gainSight,
                ScatteringCoef = scatter,
                PriorityScatteringCoef = priorityScatter,
            };
        }

        public GClass561 Modifiers;
    }

    public class EFTCoreSettings
    {
        public static EFTCoreSettings GetCore()
        {
            UpdateCoreSettings();
            return new EFTCoreSettings
            {
                Core = GClass564.Core,
            };
        }

        public static void UpdateCoreSettings()
        {
            var core = GClass564.Core;
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
            GClass564.Core = newCore.Core;
        }

        public GClass563 Core;
    }

    public class EFTBotSettings
    {
        [JsonConstructor]
        public EFTBotSettings() { }

        public EFTBotSettings(string name, WildSpawnType type, BotDifficulty[] difficulties)
        {
            Name = name;
            WildSpawnType = type;
            foreach (BotDifficulty diff in difficulties)
            {
                Settings.Add(diff, GClass564.GetSettings(diff, type));
            }
        }

        public readonly string Name;
        public readonly WildSpawnType WildSpawnType;
        public readonly Dictionary<BotDifficulty, GClass566> Settings = new Dictionary<BotDifficulty, GClass566>();
    }
}