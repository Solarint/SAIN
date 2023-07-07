using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SAIN.Editor
{
    internal class PresetEditor
    {
        public static void Init()
        {
            SingleList = BotTypeOptions.ToList();
            AllBots = BotTypeOptions.ToList();
            foreach (var type in BotTypeOptions)
            {
                string check = type.ToLower();
                if (check.Contains("bigpipe") || check.Contains("birdeye") || check.Contains("knight"))
                {
                    Goons.Add(type);
                }
                else if (check.Contains("boss"))
                {
                    Bosses.Add(type);
                }
                else if (check.Contains("follower"))
                {
                    Followers.Add(type);
                }
                else if (check.Contains("usec") || check.Contains("bear"))
                {
                    PMCs.Add(type);
                }
                else if (check.Contains("scav") || check.Contains("cursed"))
                {
                    Scavs.Add(type);
                }
                else
                {
                    Other.Add(type);
                }
            }
        }

        private static bool SelectTypeOpen = false;
        public static string[] BotTypeOptions => SAINBotPresetManager.ConvertedWildSpawnTypes;
        public static string[] BotDifficultyOptions => SAINBotPresetManager.Difficulties;

        public static int SelectedType = 0;
        public static int SelectedIntDiff = 1;

        private static readonly List<SAINBotPreset> BotPresetList = new List<SAINBotPreset>();

        private static List<string> AllBots;
        private static List<string> SingleList;

        private static readonly List<string> PMCs = new List<string>();
        private static readonly List<string> Scavs = new List<string>();
        private static readonly List<string> Followers = new List<string>();
        private static readonly List<string> Bosses = new List<string>();
        private static readonly List<string> Goons = new List<string>();
        private static readonly List<string> Other = new List<string>();

        private static List<string> SelectedList;

        public static List<int> SelectedProperties = new List<int>();
        public static List<int> SelectedPresets = new List<int>();

        private static Vector2 PropertyScroll = Vector2.zero;

        private static void PropertySelectionMenu()
        {
            GUILayout.BeginVertical();
            PropertyScroll = GUILayout.BeginScrollView(PropertyScroll, GUILayout.Width(RectLayout.MainWindow.width), GUILayout.Height(200));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            var properties = SAINBotPresetManager.Properties;
            int spacing = 0;
            for (int i = 0; i < properties.Length; i++)
            {
                if (spacing == 3)
                {
                    spacing = 0;
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                }
                // Check if this index is currently selected
                bool selected = SelectedProperties.Contains(i);

                if (GUILayout.Toggle(selected, properties[i].Name, GUILayout.Height(25), GUILayout.Width(RectLayout.MainWindow.width / 3f)))
                {
                    // The button was pressed, update the selection state
                    if (!selected)
                    {
                        SelectedProperties.Add(i);
                    }
                }
                else
                {
                    if (selected)
                    {
                        SelectedProperties.Remove(i);
                    }
                }
                spacing++;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private static Vector2 PresetScroll = Vector2.zero;

        private static void PresetSelectionMenu()
        {
            GUILayout.BeginVertical();

            PresetScroll = GUILayout.BeginScrollView(PresetScroll, GUILayout.Width(RectLayout.MainWindow.width), GUILayout.Height(200));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            int spacing = 0;
            for (int i = 0; i < BotTypeOptions.Length; i++)
            {
                if (spacing == 3)
                {
                    spacing = 0;
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
                // Check if this index is currently selected
                bool selected = SelectedPresets.Contains(i);

                if (GUILayout.Toggle(selected, BotTypeOptions[i], GUILayout.Height(25), GUILayout.Width(RectLayout.MainWindow.width / 3f)))
                {
                    // The button was pressed, update the selection state
                    if (!selected)
                    {
                        SelectedPresets.Add(i);
                    }
                }
                else
                {
                    if (selected)
                    {
                        SelectedPresets.Remove(i);
                    }
                }
                spacing++;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private static bool ExpandPresetEditor = false;

        public static void PresetMenu()
        {
            if (ExpandPresetEditor = BuilderUtil.ExpandableMenu("Bot Preset Editor", ExpandPresetEditor, "Edit Values for particular bot types and difficulty settings"))
            {
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();

                SelectTypeOpen = Buttons.Button("Select Bot", SelectTypeOpen, 35f);
                if (Buttons.Button("Load Preset", null, 35f))
                {
                    GetPresetsList();
                    SelectTypeOpen = false;
                }
                if (Buttons.Button("X", null, 35f, 35f))
                {
                    SelectTypeOpen = false;
                }
                GUILayout.EndHorizontal();

                SelectGroupPreset();
                PresetEditWindow();
                GUILayout.EndVertical();
            }
        }

        private static void SelectGroupPreset()
        {
            if (SelectTypeOpen)
            {
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();
                SelectedList = SelectListButton(SingleList, SelectedList, "Single Type", "Select a single bot type to edit");
                SelectedList = SelectListButton(AllBots, SelectedList, "All", "All Bot Types.");
                SelectedList = SelectListButton(Scavs, SelectedList, nameof(Scavs));
                SelectedList = SelectListButton(PMCs, SelectedList, nameof(PMCs));
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();
                SelectedList = SelectListButton(Goons, SelectedList, nameof(Goons));
                SelectedList = SelectListButton(Bosses, SelectedList, nameof(Bosses));
                SelectedList = SelectListButton(Followers, SelectedList, nameof(Followers));
                SelectedList = SelectListButton(Other, SelectedList, nameof(Other));
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                if (SelectedList == SingleList)
                {
                    PresetSelectionMenu();
                }

                GUILayout.BeginHorizontal();

                GUILayout.Space(5);

                LoadAllDifficulties = LabelAndAll("Difficulty", LoadAllDifficulties);

                GUILayout.EndHorizontal();

                if (!LoadAllDifficulties)
                {
                    SelectedDifficulty = DifficultyTable(SelectedDifficulty);
                }
                else
                {
                    SelectedDifficulty = null;
                }
            }
        }

        private static readonly List<string> SelectedDifficulties = new List<string>();

        private static void DifficultyTable()
        {
            GUILayout.BeginHorizontal();
            string name;
            for (int i = 0; i < BotDifficultyOptions.Length; i++)
            {
                name = BotDifficultyOptions[i];
                bool selected = SelectedDifficulties.Contains(name);
                if (GUILayout.Toggle(selected, name, GUILayout.Height(25), GUILayout.Width(RectLayout.MainWindow.width / 4f)))
                {
                    // The button was pressed, update the selection state
                    if (!selected)
                    {
                        SelectedDifficulties.Add(name);
                    }
                }
                else
                {
                    if (selected)
                    {
                        SelectedDifficulties.Remove(name);
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private static List<string> SelectListButton(List<string> option, List<string> list, string name = null, string description = null, float height = 30f, float width = 185f)
        {
            name = name ?? nameof(option);

            if (Buttons.Button(name, option == list, height, width))
            {
                return option;
            }

            if (description == null)
            {
                ToolTips.CheckMouseTable(option);
            }
            else
            {
                ToolTips.CheckMouse(description);
            }

            return list;
        }

        private static bool LabelAndAll(string labelName, bool value)
        {
            const float height = 35;
            const float width = 250f;

            GUILayout.BeginHorizontal();

            labelName += ": ";
            GUILayout.Box(labelName, GUILayout.Height(height));

            string name = "All: " + Buttons.ToggleOnOff(value);
            value = Buttons.Button(name, value, height, width);

            GUILayout.EndHorizontal();

            return value;
        }

        private static string SelectedDifficulty = null;
        private static bool LoadAllDifficulties = false;

        private static void PresetEditWindow()
        {
            if (BotPresetList == null || BotPresetList.Count == 0)
            {
                GUILayout.Label("No Presets Loaded");
                return;
            }
            if (BotPresetList.Count > 0)
            {
                string addText = LoadAllDifficulties ? "All" : BotDifficultyOptions[SelectedIntDiff];
                GUILayout.Label($"Selected [{BotPresetList.Count}] Presets [ Type: {EditingStringName()} Difficulty: {addText}]");

                SAINBotPreset preset = BotPresetList[0];
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();

                //GUILayout.Box("Editing: " + PresetInEdit.BotType.ToString() + " " + PresetInEdit.Difficulty.ToString(), GUILayout.Height(35f));
                if (GUILayout.Button("Save", GUILayout.Height(35f), GUILayout.Width(200f)))
                {
                    if (!LoadAllDifficulties)
                    {
                        var enumDiff = SAINBotPresetManager.GetDiff(SelectedDifficulty);
                        SAINBotPresetManager.ClonePresetList(BotPresetList, preset, enumDiff);
                    }
                    else
                    {
                        foreach (var diff in BotDifficultyOptions)
                        {
                            var enumDiff = SAINBotPresetManager.GetDiff(diff);
                            SAINBotPresetManager.ClonePresetList(BotPresetList, preset, enumDiff);
                        }
                    }
                    BotPresetList.Clear();
                }
                if (GUILayout.Button("Discard", GUILayout.Height(35f)))
                {
                    BotPresetList.Clear();
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.EndVertical(); 

                PropertyEditMenu(BotPresetList[0]);
            }
        }

        private static void PropertyEditMenu(SAINBotPreset preset)
        {
            LoadAllProperties = LabelAndAll("Select Property", LoadAllProperties);

            if (LoadAllProperties)
            {
                CreateAllOptions(preset);
            }
            else
            {
                PropertySelectionMenu();
                for (int i = 0; i < SelectedProperties.Count; i++)
                {
                    CreatePropertyOption(SAINBotPresetManager.Properties[SelectedProperties[i]], preset);
                }
            }
        }

        private static string EditingStringName()
        {
            if (SelectedList == AllBots)
            {
                return "All Bots";
            }
            else if (SelectedList == Scavs)
            {
                return nameof(Scavs);
            }
            else if (SelectedList == Goons)
            {
                return nameof(Goons);
            }
            else if (SelectedList == Bosses)
            {
                return nameof(Bosses);
            }
            else if (SelectedList == Followers)
            {
                return nameof(Followers);
            }
            else
            {
                return BotTypeOptions[SelectedType];
            }
        }


        private static Vector2 PropScroll = Vector2.zero;

        private static void CreateAllOptions(SAINBotPreset preset)
        {
            PropertyInfo[] properties = SAINBotPresetManager.Properties;
            foreach (PropertyInfo property in properties)
            {
                CreatePropertyOption(property, preset);
            }
        }

        private static void CreatePropertyOption(PropertyInfo property, SAINBotPreset preset)
        {
            Type propertyType = property.PropertyType;
            if (propertyType == typeof(SAINProperty<float>))
            {
                var floatProperty = (SAINProperty<float>)property.GetValue(preset);
                BuilderUtil.HorizSlider(floatProperty, preset.Difficulty);
            }
            else if (propertyType == typeof(SAINProperty<bool>))
            {
                var boolProperty = (SAINProperty<bool>)property.GetValue(preset);
                BuilderUtil.CreateButtonOption(boolProperty, preset.Difficulty);
            }
        }

        private static void GetPresetsList()
        {
            BotPresetList.Clear();

            if (SelectedList == SingleList)
            {
                AddPresetToList(BotTypeOptions[SelectedType], SelectedDifficulty);
            }
            else
            {
                foreach (string groupItem in SelectedList)
                {
                    AddPresetToList(groupItem, SelectedDifficulty);
                }
            }
        }

        private static bool LoadAllProperties = false;

        private static void AddPresetToList(string type, string diff = null)
        {
            type = SAINBotPresetManager.GetConvertedName(type, true);
            WildSpawnType typeEnum = SAINBotPresetManager.GetType(type);
            if (diff != null)
            {
                BotPresetList.Add(SAINBotPresetManager.LoadPreset(typeEnum, diff));
            }
            else
            {
                foreach (var diffOption in BotDifficultyOptions)
                {
                    BotPresetList.Add(SAINBotPresetManager.LoadPreset(typeEnum, diffOption));
                }
            }
        }
    }
}