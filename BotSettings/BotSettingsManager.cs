using EFT;
using SAIN.Classes;
using SAIN.Helpers;
using System;
using System.Collections.Generic;

namespace SAIN.Components.BotSettings
{
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
                    SAINBotSettingsClass BotSettings = JsonUtility.Load.LoadBotSettings(type, diff);

                    if (BotSettings == null)
                    {
                        string text = GClass560.LoadDifficultyStringInternal(diff, type);
                        if (text != null)
                        {
                            var settings = GClass562.Create(text);
                            BotSettings = new SAINBotSettingsClass(settings);
                        }
                    }
                    if (BotSettings != null)
                    {
                        GlobalSettings.Update(BotSettings);
                        SaveBotSettings(BotSettings, type, diff);
                    }
                }
            }
        }

        public static SAINBotSettingsClass GetBotSettings(WildSpawnType type, BotDifficulty diff)
        {
            var tuple = GetBotTuple(type, diff);
            if (BotSettingsDictionary.ContainsKey(tuple))
            {
                return BotSettingsDictionary[tuple];
            }
            return null;
        }

        public static void SaveBotSettings(SAINBotSettingsClass settings, WildSpawnType type, BotDifficulty diff)
        {
            var tuple = GetBotTuple(type, diff);

            if (BotSettingsDictionary.ContainsKey(tuple))
            {
                BotSettingsDictionary[tuple] = settings;
            }
            else
            {
                BotSettingsDictionary.Add(tuple, settings);
            }
            JsonUtility.Save.SaveBotSettings(settings, diff, type);
        }

        public static Tuple<WildSpawnType, BotDifficulty> GetBotTuple(WildSpawnType type, BotDifficulty diff)
        {
            return Tuple.Create(type, diff);
        }

        public static Dictionary<Tuple<WildSpawnType, BotDifficulty>, SAINBotSettingsClass> BotSettingsDictionary = new Dictionary<Tuple<WildSpawnType, BotDifficulty>, SAINBotSettingsClass>();
    }
}
