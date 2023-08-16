using SAIN.Editor.Abstract;
using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;
using static SAIN.Attributes.AttributesGUI;
using static SAIN.Editor.SAINLayout;

namespace SAIN.Editor.GUISections
{
    public class BotPersonalityEditor : EditorAbstract
    {
        public BotPersonalityEditor(SAINEditor editor) : base(editor)
        {
        }

        public void ClearCache()
        {
            ListHelpers.ClearCache(OpenPersMenus);
        }

        public void PersonalityMenu()
        {
            string toolTip = $"Apply Values set below to Personalities. " +
                $"Exports edited values to SAIN/Presets/{SAINPlugin.LoadedPreset.Info.Name}/Personalities folder";

            if (Builder.SaveChanges(PersonalitiesWereEdited, toolTip, 35))
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
                OpenPersMenus[name] = Builder.ExpandableMenu(name, OpenPersMenus[name], personality.Description);
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

        public bool PersonalitiesWereEdited;

        private readonly Dictionary<string, bool> OpenPersMenus = new Dictionary<string, bool>();
        private Vector2 PersonScroll = Vector2.zero;
    }
}