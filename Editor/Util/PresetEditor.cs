using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
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
                else if (check.Contains("sptusec") || check.Contains("sptbear"))
                {
                    PMCs.Add(type);
                }
                else if (check.Contains("assault"))
                {
                    Scavs.Add(type);
                }
                else
                {
                    Other.Add(type);
                }
            }
        }

        private static bool SelectSingleOpen = false;
        private static bool SelectGroupOpen = false;
        public static string[] BotTypeOptions => SAINBotPresetManager.WildSpawnTypes;
        public static string[] BotDifficultyOptions => SAINBotPresetManager.Difficulties;

        public static int SelectedType = 0;
        public static int SelectedDifficulty = 0;

        private static readonly List<SAINBotPreset> BotPresetList = new List<SAINBotPreset>();

        private static readonly List<string> PMCs = new List<string>();
        private static readonly List<string> Scavs = new List<string>();
        private static readonly List<string> Followers = new List<string>();
        private static readonly List<string> Bosses = new List<string>();
        private static readonly List<string> Goons = new List<string>();
        private static readonly List<string> Other = new List<string>();

        private static Vector2 SelectScrollPos = Vector2.zero;
        private static Vector2 PresetScrollPos = Vector2.zero;
        public static void PresetSelectionMenu()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            if (SelectSingleOpen = Buttons.Button("Select Single Bot Preset", SelectSingleOpen, 35f))
            {
                SelectGroupOpen = false;
            }
            if (SelectGroupOpen = Buttons.Button("Select Bot Preset Group", SelectGroupOpen, 35f))
            {
                SelectSingleOpen = false;
            }
            if (Buttons.Button("Load Preset", null, 35f, 200f))
            {
                GetPresetsList();
                SelectSingleOpen = false;
                SelectGroupOpen = false;
            }
            if (Buttons.Button("X", null, 35f, 35f))
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

                LoadAllTypes = LabelAndAll("Bot Type", LoadAllTypes);
                if (!LoadAllTypes)
                {
                    float dropdownHeight = DropDownHeight(BotTypeOptions.Length, 8);
                    SelectScrollPos = GUILayout.BeginScrollView(SelectScrollPos, GUILayout.Height(dropdownHeight));

                    SelectedType = GUILayout.SelectionGrid(SelectedType, BotTypeOptions, 2);

                    GUILayout.EndScrollView();
                }

                GetDifficulty(false);
            }
        }

        private static int GroupSelection = 0;
        private static readonly string[] GroupSelectionArray = { "Scavs", "PMCs", "Goons", "Bosses", "Followers", "Other" };

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
            if (i == 5)
            {
                return Other;
            }
            return null;
        }

        private static void SelectGroupPreset()
        {
            if (SelectGroupOpen)
            {
                GUILayout.Space(5);

                LoadAllTypes = LabelAndAll("Preset Groups", LoadAllTypes);
                if (!LoadAllTypes)
                {
                    float dropdownHeight = DropDownHeight(GroupSelectionArray.Length, 4);
                    SelectScrollPos = GUILayout.BeginScrollView(SelectScrollPos, GUILayout.Height(dropdownHeight));

                    GroupSelection = GUILayout.SelectionGrid(GroupSelection, GroupSelectionArray, 2);

                    GUILayout.EndScrollView();
                }

                GetDifficulty(true);
            }
        }

        private static bool LabelAndAll(string labelName, bool value)
        {
            GUILayout.BeginHorizontal();

            labelName += ": ";
            GUILayout.Box(labelName);

            string name = "All: " + value.ToString();
            value = Buttons.Button(name, value);

            GUILayout.EndHorizontal();

            return value;
        }

        private static bool LoadAllTypes = false;

        private static float DropDownHeight(int optionsLength, int optionsShown, float buttonHeight = 20f)
        {
            return Mathf.Min(optionsLength * buttonHeight, optionsShown * buttonHeight);
        }

        private static void GetDifficulty(bool group)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Space(5);

            LoadAllDifficulties = LabelAndAll("Difficulty", LoadAllDifficulties);

            GUILayout.EndHorizontal();

            if (!LoadAllDifficulties)
            {
                SelectedDifficulty = GUILayout.SelectionGrid(SelectedDifficulty, BotDifficultyOptions, 2);
            }

            GUILayout.Space(5);
        }

        private static bool LoadAllDifficulties = false;

        private static void PresetEditWindow()
        {
            if (BotPresetList.Count == 0)
            {
                GUILayout.Label("No Presets Loaded");
                return;
            }
            if (BotPresetList.Count > 0)
            {
                string baseText;
                if (LoadAllTypes)
                {
                    baseText = "All Bots!";
                }
                else
                {
                    baseText = GroupLoaded ? GroupSelectionArray[GroupSelection] : BotTypeOptions[SelectedType];
                }
                string addText = LoadAllDifficulties ? "All" : BotDifficultyOptions[SelectedDifficulty];
                GUILayout.Label("Selected = [" + baseText + "] Difficulty: [" + addText + "]");

                SAINBotPreset preset = BotPresetList[0];
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();

                //GUILayout.Box("Editing: " + PresetInEdit.BotType.ToString() + " " + PresetInEdit.Difficulty.ToString(), GUILayout.Height(35f));
                if (GUILayout.Button("Save", GUILayout.Height(35f), GUILayout.Width(200f)))
                {
                    SAINBotPresetManager.ClonePresetList(BotPresetList, preset);
                    BotPresetList.Clear();
                }
                if (GUILayout.Button("Discard", GUILayout.Height(35f)))
                {
                    BotPresetList.Clear();
                }

                GUILayout.FlexibleSpace();

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
                    float value = CreatePropertySlider(floatProperty);
                }
                else if (propertyType == typeof(SAINProperty<int>))
                {
                    var intProperty = (SAINProperty<int>)property.GetValue(preset);
                    int value = CreatePropertySlider(intProperty);
                }
                else if (propertyType == typeof(SAINProperty<bool>))
                {
                    var boolProperty = (SAINProperty<bool>)property.GetValue(preset);
                    bool value = BuilderUtil.CreateButtonOption(boolProperty);
                }
            }
        }

        private static void ApplyValueToList<T>(List<SAINBotPreset> presets, SAINProperty<T> property)
        {

        }

        private static float CreatePropertySlider(SAINProperty<float> property)
        {
            return BuilderUtil.HorizSlider(property);
        }

        private static int CreatePropertySlider(SAINProperty<int> property)
        {
            float value = BuilderUtil.HorizSlider(property.Name, property.Value, property.Min, property.Max, 1f, property.Description);
            return Mathf.RoundToInt(value);
        }

        private static SAINBotPreset LoadSelectedPreset()
        {
            WildSpawnType convertedType = GetType(BotTypeOptions[SelectedType]);
            BotDifficulty convertedDiff = GetDiff(BotDifficultyOptions[SelectedDifficulty]);
            return SAINBotPresetManager.LoadPreset(convertedType, convertedDiff);
        }

        private static void GetPresetsList()
        {
            BotPresetList.Clear();
            if (!SelectGroupOpen && !SelectGroupOpen)
            {
                return;
            }

            string difficulty = null;
            if (!LoadAllDifficulties)
            {
                difficulty = BotDifficultyOptions[SelectedDifficulty];
            }

            if (LoadAllTypes)
            {
                LoadGroupPresets(BotTypeOptions, difficulty);
            }
            else if (SelectGroupOpen)
            {
                GroupLoaded = true;
                LoadGroupPresets(GetGroup(GroupSelection), difficulty);
            }
            else if (SelectSingleOpen)
            {
                GroupLoaded = false;
                if (!LoadAllDifficulties)
                {
                    BotPresetList.Add(LoadSelectedPreset());
                }
                else
                {
                    LoadAllDiffPresets(BotTypeOptions[SelectedType]);
                }
            }
        }

        private static bool GroupLoaded = false;

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

        private static void LoadAllDiffPresets(string type)
        {
            WildSpawnType typeEnum = GetType(type);
            foreach (var difficulty in BotDifficultyOptions)
            {
                var preset = LoadSinglePreset(typeEnum, difficulty);
                BotPresetList.Add(preset);
            }
        }

        private static void LoadGroupPresets(List<string> group, string diff = null)
        {
            foreach (string groupItem in group)
            {
                AddPresetToList(groupItem, diff);
            }
        }

        private static void LoadGroupPresets(string[] group, string diff = null)
        {
            foreach (string groupItem in group)
            {
                AddPresetToList(groupItem, diff);
            }
        }

        private static void AddPresetToList(string type, string diff = null)
        {
            WildSpawnType typeEnum = GetType(type);
            BotDifficulty diffEnum;
            SAINBotPreset preset;
            if (diff != null)
            {
                diffEnum = GetDiff(diff);
                preset = SAINBotPresetManager.LoadPreset(typeEnum, diffEnum);
                BotPresetList.Add(preset);
            }
            else
            {
                foreach (var difficulty in BotDifficultyOptions)
                {
                    preset = LoadSinglePreset(typeEnum, difficulty);
                    BotPresetList.Add(preset);
                }
            }
        }
    }
}
