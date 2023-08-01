using EFT;
using SAIN.Classes;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using SAIN.BotSettings.Categories;
using Newtonsoft.Json;
using System.Data;

namespace SAIN.BotSettings
{
    public class BotSettingsHandler
    {
        public static readonly FieldWrapper SAINSettingsFields = new FieldWrapper(typeof(SAINSettings));

        public static readonly FieldWrapper EFTSettingsFields = new FieldWrapper(typeof(BotDifficultySettingsClass), "FileSettings",
            "Aiming", "Boss", "Change", "Core", "Grenade", "Hearing", "Lay", "Look", "Mind", "Move", "Patrol", "Scattering", "Shoot");

        static readonly Dictionary<WildSpawnType, SAINDictionary<SAINBotSettingsClass>> GlobalSettingsDictionary = new Dictionary<WildSpawnType, SAINDictionary<SAINBotSettingsClass>>();

        static SAINDictionary<SAINBotSettingsClass> GetSettings(WildSpawnType type)
        {
            if (!GlobalSettingsDictionary.ContainsKey(type))
            {
                string currentPreset = JsonUtility.Save.SelectedPresetName;
                string[] path = JsonUtility.EFTBotConfigFolders(currentPreset);

                var BotSettings = new SAINDictionary<SAINBotSettingsClass>(type.ToString(), path);
                if (BotSettings.NewFileCreated)
                {
                    BotSettings.Add(BotDifficulty.easy, new SAINBotSettingsClass(type, BotDifficulty.easy));
                    BotSettings.Add(BotDifficulty.normal, new SAINBotSettingsClass(type, BotDifficulty.normal));
                    BotSettings.Add(BotDifficulty.hard, new SAINBotSettingsClass(type, BotDifficulty.hard));
                    BotSettings.Add(BotDifficulty.impossible, new SAINBotSettingsClass(type, BotDifficulty.impossible));
                    BotSettings.Export();
                }
                GlobalSettingsDictionary.Add(type, BotSettings);
            }
            return GlobalSettingsDictionary[type];
        }

        public static SAINBotSettingsClass GetSettings(BotOwner owner)
        {
            var type = owner.Profile.Info.Settings.Role;
            var diff = owner.Profile.Info.Settings.BotDifficulty;

            var Result = GetSettings(type).Get(diff);
            Result.BotDefaultValues.Init(owner);

            return Result;
        }
    }
    public class BotSettingsManager
    {
        static BotSettingsManager()
        {
            var botTypes = BotPresets.BotTypeDefinitions.BotTypes;
            var diffs = (BotDifficulty[])Enum.GetValues(typeof(BotDifficulty));
            foreach (var botType in botTypes)
            {
                var type = botType.WildSpawnType;
                foreach (var diff in diffs)
                {
                }
            }
        }
    }
}
