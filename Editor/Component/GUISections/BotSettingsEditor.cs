using EFT;
using SAIN.BotPresets;
using SAIN.Classes;
using SAIN.Components.BotSettings;
using SAIN.Editor.Abstract;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static SAIN.Classes.SAINBotSettingsClass;

namespace SAIN.Editor.GUISections
{
    public class BotSettingsEditor : EditorAbstract
    {
        public BotSettingsEditor(SAINEditor editor) : base(editor)
        {
            BotSettingsDictionary = BotSettingsManager.BotSettingsDictionary;
            GetSettingsLists();
        }

        void GetSettingsLists()
        {
            for (int i = 0; i < SettingsTypes.Length; i++)
            {
                Type type = SettingsTypes[i];
                var FieldsList = new List<FieldInfo>();
                foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    Type fieldType = field.FieldType;
                    if (fieldType == typeof(float) || fieldType == typeof(int) || fieldType == typeof(bool))
                    {
                        FieldsList.Add(field);
                    }
                }
                BotSettings.Add(type.Name, FieldsList);
            }
        }

        readonly Dictionary<string, bool> SubMenus = new Dictionary<string, bool>();

        public void CreateEditMenu()
        {
            Space(25f);

            ScrollViewEditMenu = BeginScrollView(ScrollViewEditMenu);
            BeginVertical();

            

            foreach (var settingsCategory in BotSettings)
            {
                string name = settingsCategory.Key;
                if (!SubMenus.ContainsKey(name))
                {
                    SubMenus.Add(name, false);
                }
                SubMenus[name] = Builder.ExpandableMenu(name, SubMenus[name]);
                if (!SubMenus[name])
                {
                    continue;
                }

                foreach (var field in settingsCategory.Value)
                {
                    BeginHorizontal();
                    Box(field.Name, "Variable");
                    EndHorizontal();
                }
            }

            EndVertical();
            EndScrollView();
        }

        Vector2 ScrollViewEditMenu = Vector2.zero;

        public Dictionary<Tuple<WildSpawnType, BotDifficulty>, SAINBotSettingsClass> BotSettingsDictionary;

        public readonly Dictionary<string, List<FieldInfo>> BotSettings = new Dictionary<string, List<FieldInfo>>();
        public readonly Type[] SettingsTypes =
        {
            typeof(BotCoreSettings),
            typeof(BotGlobalLayData),
            typeof(BotGlobalAimingSettings),
            typeof(BotGlobalLookData),
            typeof(BotGlobalShootData),
            typeof(BotGlobalsMoveSettings),
            typeof(BotGlobalsGrenadeSettings),
            typeof(BotGlobalsChangeSettings),
            typeof(BotGlobalsCoverSettings),
            typeof(BotGlobalPatrolSettings),
            typeof(BotGlobasHearingSettings),
            typeof(BotGlobalsMindSettings),
            typeof(BotGlobalsBossSettings),
            typeof(BotGlobalsScatteringSettings)
        };
    }
}
