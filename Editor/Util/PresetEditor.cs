using EFT;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms.VisualStyles;
using UnityEngine;
using UnityEngine.UIElements;
using static HBAO_Core;

namespace SAIN.Editor
{
    internal class PresetEditor
    {
        public static void Init()
        {
            foreach(var type in BotTypeOptions)
            {
                string check = type.ToLower();
                if (check.Contains("boss"))
                {
                    Bosses.Add(type);
                }
                else if (check.Contains("follower"))
                {
                    Followers.Add(type);
                }
                if (check.Contains("bigpipe") || check.Contains("birdeye") || check.Contains("knight"))
                {
                    Goons.Add(type);
                }
            }
        }

        private static bool SelectSingleOpen = false;
        private static bool SelectGroupOpen = false;
        public static string[] BotTypeOptions => SAINBotPresetManager.WildSpawnTypes;
        public static string[] BotDifficultyOptions => SAINBotPresetManager.Difficulties;

        public static int SelectedType = 0;
        public static int SelectedDifficulty = 0;

        private static SAINBotPreset PresetInEdit;

        private static readonly List<SAINBotPreset> BotPresetList = new List<SAINBotPreset>();

        private static readonly List<string> PMCs = new List<string> { "sptUsec", "sptBear" };
        private static readonly List<string> Scavs = new List<string> { "assault", "cursedAssault" };
        private static readonly List<string> Followers = new List<string>();
        private static readonly List<string> Bosses = new List<string>();
        private static readonly List<string> Goons = new List<string>();

        private static Vector2 SelectScrollPos = Vector2.zero;
        private static Vector2 PresetScrollPos = Vector2.zero;
        public static void PresetSelectionMenu()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select Single Bot Preset", GUILayout.Height(35f)))
            {
                SelectSingleOpen = !SelectSingleOpen;
                SelectGroupOpen = false;
            }
            if (GUILayout.Button("Select Bot Preset Group", GUILayout.Height(35f)))
            {
                SelectSingleOpen = false;
                SelectGroupOpen = !SelectGroupOpen;
            }
            if (GUILayout.Button("Load Preset", GUILayout.Height(35f), GUILayout.Width(200f)))
            {
                GetPresetsList(SelectGroupOpen, LoadAllDifficulties);
                SelectSingleOpen = false;
                SelectGroupOpen = false;
            }
            if (GUILayout.Button("X", GUILayout.Height(35f), GUILayout.Width(35f)))
            {
                SelectSingleOpen = false;
                SelectGroupOpen = false;
            }
            GUILayout.EndHorizontal(); 

            SelectSinglePreset(); 
            SelectGroupPreset();

            GUILayout.EndVertical();

            PresetScrollPos = GUILayout.BeginScrollView(PresetScrollPos, GUILayout.ExpandHeight(true));

            PresetEditWindow();

            GUILayout.EndScrollView();
        }

        private static void SelectSinglePreset()
        {
            if (SelectSingleOpen)
            {
                GUILayout.Space(5);

                GUILayout.Box("Bot Type:");

                float dropdownHeight = DropDownHeight(BotTypeOptions.Length, 8);
                SelectScrollPos = GUILayout.BeginScrollView(SelectScrollPos, GUILayout.Height(dropdownHeight));

                SelectedType = GUILayout.SelectionGrid(SelectedType, BotTypeOptions, 2);

                GUILayout.EndScrollView();

                GetDifficulty(false);
            }
        }

        private static int GroupSelection = 0;
        private static readonly string[] GroupSelectionArray = { "Scavs", "PMCs", "Goons", "Bosses", "Followers" };

        private static List<string> GetGroup(int i)
        {
            if (i == 0)
            {
                return Scavs;
            }
            if (i == 1)
            {
                return PMCs;
            }
            if (i == 2)
            {
                return Goons;
            }
            if (i == 3)
            {
                return Bosses;
            }
            if (i == 4)
            {
                return Followers;
            }
            return null;
        }

        private static void SelectGroupPreset()
        {
            if (SelectGroupOpen)
            {
                GUILayout.Space(5);

                GUILayout.Box("Preset Groups:");

                float dropdownHeight = DropDownHeight(GroupSelectionArray.Length, 4);
                SelectScrollPos = GUILayout.BeginScrollView(SelectScrollPos, GUILayout.Height(dropdownHeight));

                GroupSelection = GUILayout.SelectionGrid(GroupSelection, GroupSelectionArray, 2);

                GUILayout.EndScrollView();

                GetDifficulty(true);
            }
        }

        private static float DropDownHeight(int optionsLength, int optionsShown, float buttonHeight = 20f)
        {
            return Mathf.Min(optionsLength * buttonHeight, optionsShown * buttonHeight);
        }

        private static void GetDifficulty(bool group)
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Box("Difficulty:");
            if (GUILayout.Button("All: " + LoadAllDifficulties.ToString(), GUILayout.Width(100f)))
            {
                LoadAllDifficulties = !LoadAllDifficulties;
            }

            GUILayout.EndHorizontal();
            SelectedDifficulty = GUILayout.SelectionGrid(SelectedDifficulty, BotDifficultyOptions, 2);
            GUILayout.Space(5);
            string baseText = group ? GroupSelectionArray[GroupSelection] : BotTypeOptions[SelectedType];
            string addText = LoadAllDifficulties ? "All" : BotDifficultyOptions[SelectedDifficulty];
            GUILayout.Label("Selected = [" + baseText + "] Difficulty: [" + addText + "]");
        }

        private static bool LoadAllDifficulties = false;

        private static void PresetEditWindow()
        {
            if (BotPresetList.Count > 0)
            {
                SAINBotPreset preset = BotPresetList[0];
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();

                //GUILayout.Box("Editing: " + PresetInEdit.BotType.ToString() + " " + PresetInEdit.Difficulty.ToString(), GUILayout.Height(35f));
                if (GUILayout.Button("Save", GUILayout.Height(35f), GUILayout.Width(200f)))
                {
                    SAINBotPresetManager.SavePreset(BotPresetList);
                    BotPresetList.Clear();
                }
                if (GUILayout.Button("X", GUILayout.Height(35f), GUILayout.Width(35f)))
                {
                    BotPresetList.Clear();
                }

                GUILayout.EndHorizontal();

                GUILayout.Space(10);

                CreateOptions(preset);

                GUILayout.EndVertical();
            }
        }

        private static void CreateOptions(SAINBotPreset preset)
        {
            PropertyInfo[] properties = preset.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                Type propertyType = property.PropertyType;
                if (propertyType == typeof(SAINProperty<float>))
                {
                    var floatProperty = (SAINProperty<float>)property.GetValue(preset);
                    CreatePropertySlider(floatProperty);
                }
                else if (propertyType == typeof(SAINProperty<int>))
                {
                    var intProperty = (SAINProperty<int>)property.GetValue(preset);
                    CreatePropertySlider(intProperty);
                }
                else if (propertyType == typeof(SAINProperty<bool>))
                {
                    var boolProperty = (SAINProperty<bool>)property.GetValue(preset);
                    BuilderUtil.CreateButtonOption(boolProperty);
                }
            }
        }

        private static void CreatePropertySlider(SAINProperty<float> property)
        {
            BuilderUtil.HorizSlider(property);
        }

        private static void CreatePropertySlider(SAINProperty<int> property)
        {
            BuilderUtil.HorizSlider(property.Name, property.Value, property.Min, property.Max, 1f, property.Description);
        }

        private static SAINBotPreset LoadSelectedPreset()
        {
            WildSpawnType convertedType = GetType(BotTypeOptions[SelectedType]);
            BotDifficulty convertedDiff = GetDiff(BotDifficultyOptions[SelectedDifficulty]);
            return SAINBotPresetManager.LoadPreset(convertedType, convertedDiff);
        }

        private static List<SAINBotPreset> GetPresetsList(bool group, bool allDiff)
        {
            BotPresetList.Clear();
            if (!group)
            {
                if (!allDiff)
                {
                    BotPresetList.Add(LoadSelectedPreset());
                }
                else
                {
                    LoadAllDiffPresets(BotTypeOptions[SelectedType]);
                }
            }
            else
            {
                var list = GetGroup(GroupSelection);
                string difficulty = null;
                if (!allDiff)
                {
                    difficulty = BotDifficultyOptions[SelectedDifficulty];
                }
                LoadGroupPresets(list, difficulty);
            }
            return BotPresetList;
        }

        private static SAINBotPreset LoadSinglePreset(string type, string diff)
        {
            return SAINBotPresetManager.LoadPreset(GetType(type), GetDiff(diff));
        }

        private static SAINBotPreset LoadSinglePreset(WildSpawnType type, string diff)
        {
            return SAINBotPresetManager.LoadPreset(type, GetDiff(diff));
        }

        private static SAINBotPreset LoadSinglePreset(string type, BotDifficulty diff)
        {
            return SAINBotPresetManager.LoadPreset(GetType(type), diff);
        }

        private static WildSpawnType GetType(string type)
        {
            return (WildSpawnType)Enum.Parse(typeof(WildSpawnType), type);
        }

        private static BotDifficulty GetDiff(string diff)
        {
            return (BotDifficulty)Enum.Parse(typeof(BotDifficulty), diff);
        }

        private static List<SAINBotPreset> LoadAllDiffPresets(string type)
        {
            BotPresetList.Clear();
            WildSpawnType typeEnum = GetType(type);
            foreach (var difficulty in BotDifficultyOptions)
            {
                var preset = LoadSinglePreset(typeEnum, difficulty);
                BotPresetList.Add(preset);
            }
            return BotPresetList;
        }

        private static List<SAINBotPreset> LoadGroupPresets(List<string> group, string diff = null)
        {
            BotPresetList.Clear();
            WildSpawnType typeEnum;
            BotDifficulty diffEnum;
            SAINBotPreset preset;
            foreach (string groupItem in group)
            {
                if (diff != null)
                {
                    typeEnum = GetType(groupItem);
                    diffEnum = GetDiff(diff);
                    preset = SAINBotPresetManager.LoadPreset(typeEnum, diffEnum);
                    BotPresetList.Add(preset);
                }
                else
                {
                    foreach (var difficulty in BotDifficultyOptions)
                    {
                        preset = LoadSinglePreset(groupItem, difficulty);
                        BotPresetList.Add(preset);
                    }
                }
            }
            return BotPresetList;
        }
    }
}
