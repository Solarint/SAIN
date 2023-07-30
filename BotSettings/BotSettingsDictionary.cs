using SAIN.BotSettings.Categories;
using SAIN.BotSettings.Categories.Util;
using SAIN.Editor;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SAIN.BotSettings
{
    public static class BotSettingsDictionary
    {
        static BotSettingsDictionary()
        {
            Settings = new SAINDictionary<SettingsWrapper>("DefaultBotSettings", "BotSettings");
        }

        public static void Test()
        {
            if (Settings.NewFileCreated)
            {
                InitDictionary.Init();
                Settings.Export();
            }
        }

        public static void Add(SettingsWrapper value)
        {
            Settings.Add(value.Key, value);
        }
        public static void Modify(SettingsWrapper value)
        {
            Settings.Modify(value.Key, value);
        }
        public static bool Get(string key, out SettingsWrapper value)
        {
            return Settings.Get(key, out value);
        }
        public static void Remove(string key)
        {
            Settings.Remove(key);
        }
        public static void Remove(SettingsWrapper value)
        {
            Settings.Remove(value.Key);
        }

        public static readonly SAINDictionary<SettingsWrapper> Settings;
    }
}
