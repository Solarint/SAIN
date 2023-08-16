using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;
using static SAIN.Attributes.AttributesGUI;
using static SAIN.Editor.SAINLayout;

namespace SAIN.Editor.GUISections
{
    public static class BotPersonalityEditor
    {
        public static void ClearCache()
        {
            ListHelpers.ClearCache(OpenPersMenus);
        }

        public static void PersonalityMenu()
        {
            string toolTip = $"Apply Values set below to Personalities. " +
                $"Exports edited values to SAIN/Presets/{SAINPlugin.LoadedPreset.Info.Name}/Personalities folder";

            if (BuilderClass.SaveChanges(PersonalitiesWereEdited, toolTip, 35))
            {
                SAINPlugin.LoadedPreset.ExportPersonalities();
            }

            PersonScroll = BeginScrollView(PersonScroll);

            foreach (var personality in SAINPlugin.LoadedPreset.PersonalityManager.Personalities.Values)
            {
                string name = personality.Name;
                if (!OpenPersMenus.ContainsKey(name))
                {
                    OpenPersMenus.Add(name, false);
                }
                OpenPersMenus[name] = BuilderClass.ExpandableMenu(name, OpenPersMenus[name], personality.Description);
                if (OpenPersMenus[name])
                {
                    EditAllValuesInObj(personality.Variables, out bool newEdit);
                    if (newEdit)
                    {
                        PersonalitiesWereEdited = true;
                    }
                }
            }
            EndScrollView();
        }

        public static bool PersonalitiesWereEdited;

        private static readonly Dictionary<string, bool> OpenPersMenus = new Dictionary<string, bool>();
        private static Vector2 PersonScroll = Vector2.zero;
    }
}