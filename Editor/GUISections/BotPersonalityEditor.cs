using SAIN.Editor.Abstract;
using System;
using System.Collections.Generic;
using UnityEngine;
using static SAIN.Helpers.Reflection;
using static SAIN.Attributes.AttributesGUI;
using SAIN.Preset.Personalities;

namespace SAIN.Editor.GUISections
{
    public class BotPersonalityEditor : EditorAbstract
    {
        public BotPersonalityEditor(SAINEditor editor) : base(editor)
        {
        }

        public void PersonalityMenu()
        {
            PersonScroll = Builder.BeginScrollView(PersonScroll);

            foreach (var personality in SAINPlugin.LoadedPreset.PersonalityManager.Personalities.Values)
            {
                string name = personality.Name;
                if (!OpenPersMenus.ContainsKey(name))
                {
                    OpenPersMenus.Add(name, false);
                }
                OpenPersMenus[name] = Builder.ExpandableMenu(name, OpenPersMenus[name], personality.Description);
                if (!OpenPersMenus[name])
                {
                    continue;
                }
                EditAllValuesInObj(personality.Variables, out int optionsCount, true);
            }
            Builder.EndScrollView();
        }

        private readonly Dictionary<string, bool> OpenPersMenus = new Dictionary<string, bool>();
        private Vector2 PersonScroll = Vector2.zero;
    }
}