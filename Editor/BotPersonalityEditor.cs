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

            foreach (var personality in SAINPlugin.LoadedPreset.PersonalityManager.Personalities.Values)
            {
                string name = personality.Name;
                if (!OpenPersMenus.ContainsKey(name))
                {
                    OpenPersMenus.Add(name, false);
                }

                BeginHorizontal(80f);
                OpenPersMenus[name] = BuilderClass.ExpandableMenu(name, OpenPersMenus[name], personality.Description);
                EndHorizontal(80f);

                if (OpenPersMenus[name])
                {
                    EditAllValuesInObj(personality.Variables, out bool newEdit);
                    if (newEdit)
                    {
                        PersonalitiesWereEdited = true;
                    }
                }
            }
        }

        public static bool PersonalitiesWereEdited;

        private static readonly Dictionary<string, bool> OpenPersMenus = new Dictionary<string, bool>();
    }
}