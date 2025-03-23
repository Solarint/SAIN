using EFT;
using EFT.UI;
using SAIN.Editor;
using SAIN.Editor.GUISections;
using SAIN.Editor.Util;
using SAIN.Helpers;
using SAIN.Plugin;
using SAIN.Preset;
using SAIN.Preset.GearStealthValues;
using SAIN.Preset.GlobalSettings;
using SAIN.Preset.GlobalSettings.Categories;
using SAIN.Preset.Personalities;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.Classes.WeaponFunction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static SAIN.Editor.SAINLayout;

namespace SAIN.Attributes
{
    public class AttributesGUI
    {
        public static ConfigInfoClass GetAttributeInfo(MemberInfo member)
        {
            string name = member.Name + member.DeclaringType.Name;
            AddAttributesToDictionary(name, member);
            if (_attributeClasses.TryGetValue(name, out var value)) {
                return value;
            }
            return null;
        }

        private static void AddAttributesToDictionary(string name, MemberInfo member)
        {
            if (!_attributeClasses.ContainsKey(name) && !_failedAdds.Contains(name)) {
                var attributes = new ConfigInfoClass(member);
                if (attributes.ValueType != null) {
                    _attributeClasses.Add(name, attributes);
                }
                else {
                    _failedAdds.Add(name);
                }
            }
        }

        public static object EditValue(ref object value, object settingsObject, ConfigInfoClass attributes, out bool wasEdited, int listDepth, GUIEntryConfig config = null, string search = null)
        {
            checkEditValue(ref value, settingsObject, attributes, out wasEdited, listDepth, config, search);
            if (wasEdited) {
                if (value is ISAINSettings || value is ISettingsGroup) {
                    // is not
                }
                else {
                    ConfigEditingTracker.Add(attributes.Name, value);
                }
            }
            return value;
        }

        private static object checkEditValue(ref object value, object settingsObject, ConfigInfoClass info, out bool wasEdited, int listDepth, GUIEntryConfig config = null, string search = null)
        {
            wasEdited = false;
            if (value != null && info != null && !info.DoNotShowGUI) {
                config = config ?? _defaultEntryConfig;

                if (value is string stringValue) {
                    DisplayString(stringValue, listDepth, config, info);
                    return value;
                }

                if (value is float || value is bool || value is int) {
                    value = EditFloatBoolInt(ref value, settingsObject, info, config, listDepth, out wasEdited);
                    return value;
                }

                if (value is EHeardFromPeaceBehavior peaceBehavior) {
                    return value;
                }

                if (!ExpandableList(info, config.EntryHeight + 3, listDepth++, config)) {
                    return value;
                }
                if (value is ISAINSettings settings) {
                    EditAllValuesInObj(settings, out wasEdited, search, config, listDepth++);
                    return value;
                }
                if (value is ISettingsGroup group) {
                    EditAllValuesInObj(group, out wasEdited, search, config, listDepth++);
                    return value;
                }
                value = FindListTypeAndEdit(ref value, settingsObject, info, listDepth, out wasEdited, config, search);
            }
            return value;
        }

        private static void editSuppressionDict(Dictionary<ESuppressionState, SuppressionConfig> suppDict, out bool wasEdited)
        {
            wasEdited = false;
            CreateLabelStyle();

            BeginVertical(5f);
            foreach (KeyValuePair<ESuppressionState, SuppressionConfig> kvp in suppDict) {
                BeginHorizontal(150f);
                string suppStateString = $"Suppression State: {kvp.Key}";
                if (ExpandableList(suppStateString, null, PresetHandler.EditorDefaults.ConfigEntryHeight, 1, _defaultEntryConfig)) {
                }
                EndHorizontal(150f);
            }
            EndVertical(5f);
        }

        public static void DisplayString(string value, float listDepth, GUIEntryConfig entryConfig, ConfigInfoClass info)
        {
            if (value != null &&
                info != null &&
                !info.DoNotShowGUI) {
                if (entryConfig == null) {
                    entryConfig = _defaultEntryConfig;
                }
                startConfigEntry(listDepth, entryConfig, info);
                Label($"{info.Name}: ", Width(80), Height(PresetHandler.EditorDefaults.ConfigEntryHeight));
                Box(value, Height(PresetHandler.EditorDefaults.ConfigEntryHeight));
                EndHorizontal(100f);
            }
        }

        public static void DisplayString(string value, float listDepth, GUIEntryConfig entryConfig, float heightOverride = -1)
        {
            if (value != null) {
                if (entryConfig == null) {
                    entryConfig = _defaultEntryConfig;
                }
                startConfigEntry(listDepth, entryConfig, null);
                if (heightOverride < 0) {
                    heightOverride = entryConfig.EntryHeight;
                }
                Box(value, Height(heightOverride));
                EndHorizontal(100f);
            }
        }

        public static object FindListTypeAndEdit(ref object value, object settingsObject, ConfigInfoClass info, int listDepth, out bool wasEdited, GUIEntryConfig config = null, string search = null)
        {
            wasEdited = false;
            CreateLabelStyle();

            if (value is Dictionary<ELocation, DifficultySettings> locationDict) {
                editLocationDict(locationDict, settingsObject, info, listDepth, config, out wasEdited, search);
                return value;
            }

            if (value is Dictionary<ESuppressionState, SuppressionConfig> suppDict) {
                editSuppressionDict(suppDict, out wasEdited);
                return value;
            }

            if (value is Dictionary<string, EPersonality> nicknamePersDict) {
                createPersonalityDict(nicknamePersDict, out wasEdited);
                return value;
            }

            if (value is Dictionary<WildSpawnType, EPersonality> bossPersDict) {
                createPersonalityDict(bossPersDict, config, out wasEdited);
                return value;
            }

            if (value is Dictionary<ECaliber, float>) {
                EditFloatDictionary<ECaliber>(value, info, out wasEdited);
                return value;
            }

            if (value is Dictionary<SAINSoundType, float>) {
                EditFloatDictionary<SAINSoundType>(value, info, out wasEdited);
                return value;
            }

            if (value is Dictionary<EWeaponClass, float>) {
                EditFloatDictionary<EWeaponClass>(value, info, out wasEdited);
                return value;
            }

            if (value is Dictionary<ESoundDispersionType, DispersionValues> dispDict) {
                EditDispersionDictionary(dispDict, settingsObject, info, out wasEdited);
                return value;
            }

            if (value is Dictionary<AILimitSetting, float> aiLimitDict) {
                EditAILimitDictionary(aiLimitDict, settingsObject, info, out wasEdited);
                return value;
            }

            if (value is Dictionary<EPersonality, bool> boolDict) {
                EditBoolDictionary<EPersonality>(boolDict, info, out wasEdited);
                return value;
            }

            if (value is List<WildSpawnType> wildList) {
                ModifyLists.AddOrRemove(wildList, out wasEdited);
                return value;
            }

            if (value is List<BotType> botList) {
                ModifyLists.AddOrRemove(botList, out wasEdited);
                return value;
            }

            if (value is List<Brain> brainList) {
                ModifyLists.AddOrRemove(brainList, out wasEdited);
                return value;
            }

            return value;
        }

        private static void createPersonalityDict(Dictionary<string, EPersonality> persDictionary, out bool wasEdited)
        {
            CreateLabelStyle();
            BeginVertical(5f);

            wasEdited = false;

            foreach (KeyValuePair<string, EPersonality> kvp in persDictionary) {
                BeginHorizontal(150f);
                string outputNickname = TextArea(kvp.Key, null, Width(300f), Height(PresetHandler.EditorDefaults.ConfigEntryHeight));
                string outputPers = TextArea(kvp.Value.ToString(), null, Width(300f), Height(PresetHandler.EditorDefaults.ConfigEntryHeight));
                EndHorizontal(150f);
            }
            EndVertical(5f);
        }

        private static void createPersonalityDict(Dictionary<WildSpawnType, EPersonality> persDictionary, GUIEntryConfig entryConfig, out bool wasEdited)
        {
            CreateLabelStyle();
            _tempBossPersDict.Clear();
            _tempBossPersDict.AddRange(persDictionary);
            wasEdited = false;

            BeginVertical(5f);

            foreach (KeyValuePair<WildSpawnType, EPersonality> kvp in _tempBossPersDict) {
                BeginHorizontal(150f);
                string bossPerString = $"Boss Personality: {kvp.Key}";
                if (ExpandableList(bossPerString, null, 25f, 1, entryConfig)) {
                    EPersonality newSelection = selectPersonality(kvp.Value, entryConfig);
                    if (newSelection != kvp.Value) {
                        persDictionary[kvp.Key] = newSelection;
                    }
                }
                EndHorizontal(150f);
            }

            EndVertical(5f);

            _tempBossPersDict.Clear();
        }

        private static Dictionary<string, bool> openedSelections = new Dictionary<string, bool>();
        private static Dictionary<WildSpawnType, EPersonality> _tempBossPersDict = new Dictionary<WildSpawnType, EPersonality>();

        private static EPersonality selectPersonality(EPersonality selected, GUIEntryConfig entryConfig)
        {
            const float gridOptionHeight = 25f;
            int selectedId = 0;
            EPersonality[] allPersonalities = EnumValues.GetEnum<EPersonality>();

            // Get all possible personalities and create a string array of all options
            if (personalities_strings == null) {
                List<string> personalitiesstringList = new List<string>();
                for (int i = 0; i < allPersonalities.Length; i++) {
                    personalitiesstringList.Add(allPersonalities[i].ToString());
                }
                personalities_strings = personalitiesstringList.ToArray();
            }

            // find which one we have selected
            for (int i = 0; i < personalities_strings.Length; i++) {
                if (personalities_strings[i] == selected.ToString()) {
                    selectedId = i;
                    break;
                }
            }

            BeginVertical(5f);

            // select from string array
            EPersonality newPersonality = BotPersonalityEditor.SelectPersonality(selected, gridOptionHeight, 3);
            //int newSelection = GUILayout.SelectionGrid(selectedId, personalities_strings, 4, GetStyle(Style.selectionGrid), Width(gridWidth), Height(gridHeight));
            if (newPersonality != selected) {
                selected = newPersonality;
                SAIN.Editor.Sounds.PlaySound(EUISoundType.MenuCheckBox, 0.5f);
            };

            EndVertical(10f);

            return selected;
        }

        private static string[] personalities_strings;

        private static void CreateLabelStyle()
        {
            if (_labelStyle == null) {
                GUIStyle boxstyle = GetStyle(Style.box);
                _labelStyle = new GUIStyle(GetStyle(Style.label)) {
                    alignment = TextAnchor.MiddleLeft,
                    margin = boxstyle.margin,
                    padding = boxstyle.padding
                };
            }
        }

        private static void startConfigEntry(float listDepth, GUIEntryConfig entryConfig, ConfigInfoClass info)
        {
            float horizDepth = listDepth * entryConfig.SubList_Indent_Horizontal;
            if (info != null && (info.AdvancedOption || info.DeveloperOption)) {
                BeginHorizontal(25f);
                var oldAlignment = _labelStyle.alignment;
                _labelStyle.alignment = TextAnchor.MiddleCenter;
                Space(horizDepth);
                Box(info.AdvancedOption ? "Advanced" : "Developer",
                    _labelStyle,
                    Width(70f),
                    Height(PresetHandler.EditorDefaults.ConfigEntryHeight));
                _labelStyle.alignment = oldAlignment;
            }
            else {
                BeginHorizontal(100f + horizDepth);
            }
        }

        public static object EditFloatBoolInt(ref object value, object settingsObject, ConfigInfoClass info, GUIEntryConfig entryConfig, int listDepth, out bool wasEdited, bool showLabel = true, bool beginHoriz = true)
        {
            if (value == null) {
                wasEdited = false;
                return null;
            }

            if (beginHoriz) {
                startConfigEntry(listDepth, entryConfig, info);
            }

            GUILayoutOption[] layoutParams;
            bool useSimpleLayout = info.SimpleValueEdit || !PresetHandler.EditorDefaults.SliderToggle;

            if (showLabel) {
                CreateLabelStyle();
                if (useSimpleLayout) {
                    layoutParams = new GUILayoutOption[]
                    {
                        GUILayout.Width(450f),
                        GUILayout.Height(PresetHandler.EditorDefaults.ConfigEntryHeight)
                    };
                }
                else {
                    layoutParams = new GUILayoutOption[]
                    {
                        GUILayout.Height(PresetHandler.EditorDefaults.ConfigEntryHeight)
                    };
                }
                Box(new GUIContent(
                    info.Name,
                    info.Description),
                    _labelStyle,
                    layoutParams
                    );
            }

            object originalValue = value;
            string result = string.Empty;

            if (info.ValueType == typeof(bool)) {
                if (!useSimpleLayout) {
                    value = Toggle((bool)value, (bool)value ? "On" : "Off", EUISoundType.MenuCheckBox, entryConfig.Toggle);
                }
                result = value.ToString();
            }
            else if (info.ValueType == typeof(float)) {
                float flValue = (float)value;
                if (!useSimpleLayout) {
                    flValue = BuilderClass.CreateSlider(flValue, info.Min, info.Max, info.Rounding, entryConfig.Toggle);
                }
                value = flValue;
                result = flValue.Round(info.Rounding).ToString();
            }

            if (useSimpleLayout) {
                layoutParams = new GUILayoutOption[]
                {
                    GUILayout.Width(100),
                    GUILayout.Height(PresetHandler.EditorDefaults.ConfigEntryHeight)
                };
            }
            else {
                layoutParams = entryConfig.Result;
            }

            if (useSimpleLayout && info.ValueType == typeof(bool)) {
                value = Toggle((bool)value, (bool)value ? "On" : "Off", EUISoundType.MenuCheckBox, layoutParams);
            }
            else {
                string dirtyString = TextField(result, null, layoutParams);
                if (dirtyString != result) {
                    value = BuilderClass.CleanString(dirtyString, value);
                }
                if (value is int || value is float) {
                    value = info.Clamp(value);
                }
            }

            if (useSimpleLayout) {
                layoutParams = new GUILayoutOption[]
                {
                    GUILayout.Width(100),
                    GUILayout.Height(PresetHandler.EditorDefaults.ConfigEntryHeight)
                };
            }
            else {
                layoutParams = entryConfig.Reset;
            }

            var defaultValue = info.GetDefault(settingsObject);
            if (defaultValue != null) {
                if (Button("Reset", "Reset To Default Value", EUISoundType.ButtonClick, layoutParams)) {
                    value = defaultValue;
                    ConfigEditingTracker.Remove(info);
                }
            }
            else {
                Box(" ", "No Default Value is assigned to this option.", layoutParams);
            }

            if (beginHoriz)
                EndHorizontal(100f);

            wasEdited = originalValue.ToString() != value.ToString();
            return value;
        }

        public static void EditAllStealthValues(GearStealthValuesClass stealthClass)
        {
            BeginVertical(5f);

            var possibleTypes = EnumValues.GetEnum<EEquipmentType>();
            int count = possibleTypes.Length;
            var values = stealthClass.ItemStealthValues;
            var defaults = stealthClass.Defaults;

            for (int i = 0; i < count; i++) {
                var type = possibleTypes[i];
                if (values.TryGetValue(type, out var list)) {
                    if (!ExpandableList(type.ToString(), string.Empty, _defaultEntryConfig.EntryHeight + 5, 0, _defaultEntryConfig)) {
                        continue;
                    }
                    editStealthValueList(list, defaults);
                }
            }

            EndVertical(5f);
        }

        private static void editStealthValueList(List<ItemStealthValue> list, List<ItemStealthValue> defaults)
        {
            if (list.Count == 0) return;
            BeginVertical(10f);
            for (int i = 0; i < list.Count; i++) {
                ItemStealthValue value = list[i];
                ItemStealthValue defaultValue = getDefault(value, defaults);
                editStealthValue(value, defaultValue);
            }
            EndVertical(10f);
        }

        private static ItemStealthValue getDefault(ItemStealthValue value, List<ItemStealthValue> defaults)
        {
            if (!defaults.Contains(value))
                return null;

            foreach (ItemStealthValue value2 in defaults) {
                if (value2.Name == value.Name)
                    return value2;
            }
            return null;
        }

        private static void editStealthValue(ItemStealthValue stealthValue, ItemStealthValue defaultValue)
        {
            BeginHorizontal(150);
            string name = stealthValue.Name;
            string description = $"The Stealth Value for {name}";
            float fvalue = stealthValue.StealthValue;
            float min = 0.1f;
            float max = 2;

            fvalue = slider(name, description, fvalue, min, max, 1000f);
            if (defaultValue != null &&
                resetButton()) {
                fvalue = defaultValue.StealthValue;
            }

            if (fvalue != stealthValue.StealthValue) {
                stealthValue.StealthValue = fvalue;
                ConfigEditingTracker.Add(name, fvalue);
            }
            EndHorizontal(150);
        }

        private static bool ExpandableList(ConfigInfoClass info, float height, int listDepth, GUIEntryConfig config)
        {
            return ExpandableList(info.Name, info.Description, height, listDepth, config);
        }

        private static bool ExpandableList(string name, string description, float height, int listDepth, GUIEntryConfig config)
        {
            BeginHorizontal(100f + (listDepth * config.SubList_Indent_Horizontal));

            if (!_listOpen.ContainsKey(name)) {
                _listOpen.Add(name, false);
            }
            bool isOpen = _listOpen[name];
            isOpen = BuilderClass.ExpandableMenu(name, isOpen, description, height);
            _listOpen[name] = isOpen;

            EndHorizontal(100f);
            return isOpen;
        }

        public static void EditBoolDictionary<T>(object dictValue, ConfigInfoClass info, out bool edited) where T : Enum
        {
            edited = false;

            BeginVertical(5f);

            var defaultDictionary = info.DefaultDictionary as Dictionary<T, bool>;
            var dictionary = dictValue as Dictionary<T, bool>;
            List<T> list = dictionary.Keys.ToList();

            CreateLabelStyle();

            for (int i = 0; i < list.Count; i++) {
                BeginHorizontal(150f);

                var item = list[i];
                var name = item.ToString();
                Box(new GUIContent(name), _labelStyle, Height(PresetHandler.EditorDefaults.ConfigEntryHeight));
                if (Toggle(dictionary[item], dictionary[item] ? "On" : "Off", EUISoundType.MenuCheckBox, _defaultEntryConfig.Toggle)) {
                    // Option was selected, set all other values to false, other than the 1 selected
                    for (int j = 0; j < list.Count; j++) {
                        var item2 = list[j];
                        bool selected = item2.ToString() == name;

                        // Set all other options to false
                        if (!selected &&
                            dictionary[item2] != false) {
                            dictionary[item2] = false;
                            edited = true;
                        }

                        // Set the selected option to true
                        if (selected &&
                            dictionary[item2] != true) {
                            dictionary[item2] = true;
                            edited = true;
                        }
                    }
                }
                // Option was set to true, but is now set to false
                else if (dictionary[item] != false) {
                    // Option deselected
                    dictionary[item] = false;
                    edited = true;
                }
                EndHorizontal(150f);
            }

            list.Clear();
            EndVertical(5f);
        }

        public static void EditDispersionDictionary(Dictionary<ESoundDispersionType, DispersionValues> dictionary, object settingsObject, ConfigInfoClass info, out bool wasEdited)
        {
            BeginVertical(5f);

            var defaultDictionary = info.GetDefault(settingsObject) as Dictionary<ESoundDispersionType, DispersionValues>;
            ESoundDispersionType[] array = EnumValues.GetEnum<ESoundDispersionType>();
            wasEdited = false;

            for (int i = 0; i < array.Length; i++) {
                var soundType = array[i];
                if (!dictionary.TryGetValue(soundType, out DispersionValues values)) {
                    continue;
                }
                editDispStruct(values, soundType, defaultDictionary, out bool newEdit);
                if (newEdit)
                    wasEdited = true;
            }
            EndVertical(5f);
        }

        private static void editLocationDict(Dictionary<ELocation, DifficultySettings> dictionary, object settingsObject, ConfigInfoClass info, int listDepth, GUIEntryConfig config, out bool wasEdited, string search = null)
        {
            BeginVertical(5f);

            var defaultDictionary = info.GetDefault(settingsObject) as Dictionary<ELocation, DifficultySettings>;
            ELocation[] array = EnumValues.GetEnum<ELocation>();
            wasEdited = false;

            for (int i = 0; i < array.Length; i++) {
                var location = array[i];
                if (!dictionary.TryGetValue(location, out DifficultySettings originalValue)) {
                    continue;
                }
                string name = location.ToString();

                if (!ExpandableList(name, string.Empty, PresetHandler.EditorDefaults.ConfigEntryHeight + 3, listDepth, config)) {
                    continue;
                }

                BeginHorizontal(100f + (listDepth * config.SubList_Indent_Horizontal));
                Label(name, Height(PresetHandler.EditorDefaults.ConfigEntryHeight));
                EndHorizontal(100f);

                int subListDepth = listDepth + 1;
                EditAllValuesInObj(originalValue, out bool newEdit, search, config, subListDepth);
                if (newEdit) {
                    wasEdited = true;
                }
            }
            EndVertical(5f);
        }

        public static void EditAILimitDictionary(Dictionary<AILimitSetting, float> dictionary, object settingsObject, ConfigInfoClass info, out bool wasEdited)
        {
            BeginVertical(5f);

            var defaultDictionary = info.GetDefault(settingsObject) as Dictionary<AILimitSetting, float>;
            AILimitSetting[] array = EnumValues.GetEnum<AILimitSetting>();
            wasEdited = false;

            for (int i = 0; i < array.Length; i++) {
                var limitSetting = array[i];
                if (!dictionary.TryGetValue(limitSetting, out float originalValue)) {
                    continue;
                }
                BeginHorizontal(200f);

                string name = limitSetting.ToString();
                string description = "";
                float min = 5f;
                float max = 800f;

                float newValue = slider(name, description, originalValue, min, max, 10f);
                if (resetButton()) {
                    newValue = defaultDictionary[limitSetting];
                    dictionary[limitSetting] = newValue;
                    //ConfigEditingTracker.Remove(attributes);
                }

                if (dictionary[limitSetting] != newValue) {
                    dictionary[limitSetting] = newValue;
                    wasEdited = true;
                }
                EndHorizontal(200f);
            }
            EndVertical(5f);
        }

        private static void editDispStruct(DispersionValues values, ESoundDispersionType soundType, Dictionary<ESoundDispersionType, DispersionValues> defaultDictionary, out bool wasEdited)
        {
            wasEdited = false;

            BeginVertical(2f);
            BeginHorizontal(150);

            Box(new GUIContent(soundType.ToString()), _labelStyle, Height(PresetHandler.EditorDefaults.ConfigEntryHeight));

            EndHorizontal(150);
            BeginHorizontal(200f);

            string name = nameof(values.DistanceModifier);
            string description = "How much to randomize the distance that a bot thinks a sound originated from.";
            float fvalue = values.DistanceModifier;
            float min = 0f;
            float max = 20f;

            fvalue = slider(name, description, fvalue, min, max, 100f);
            if (resetButton())
                fvalue = defaultDictionary[soundType].DistanceModifier;

            if (fvalue != values.DistanceModifier) {
                values.DistanceModifier = fvalue;
                wasEdited = true;
            }

            EndHorizontal(200f);
            BeginHorizontal(200f);

            name = nameof(values.MinAngle);
            description = "";
            fvalue = values.MinAngle;
            min = 0f;
            max = 180;

            fvalue = slider(name, description, fvalue, min, max, 100f);
            if (resetButton())
                fvalue = defaultDictionary[soundType].MinAngle;

            if (fvalue != values.MinAngle) {
                values.MinAngle = fvalue;
                wasEdited = true;
            }

            EndHorizontal(200f);
            BeginHorizontal(200f);

            name = nameof(values.MaxAngle);
            description = "";
            fvalue = values.MaxAngle;
            min = 0f;
            max = 180;

            fvalue = slider(name, description, fvalue, min, max, 100f);
            if (resetButton())
                fvalue = defaultDictionary[soundType].MaxAngle;

            if (fvalue != values.MaxAngle) {
                values.MaxAngle = fvalue;
                wasEdited = true;
            }

            EndHorizontal(200f);
            BeginHorizontal(200f);

            name = nameof(values.VerticalModifier);
            description = "";
            fvalue = values.VerticalModifier;
            min = 0f;
            max = 0.5f;

            fvalue = slider(name, description, fvalue, min, max, 100f);
            if (resetButton())
                fvalue = defaultDictionary[soundType].VerticalModifier;

            if (fvalue != values.VerticalModifier) {
                values.VerticalModifier = fvalue;
                wasEdited = true;
            }

            EndHorizontal(200f);

            EndVertical(2f);
        }

        private static bool resetButton()
        {
            return Button("Reset", EUISoundType.ButtonClick, _defaultEntryConfig.Reset);
        }

        private static float slider(string name, string description, float value, float min, float max, float rounding)
        {
            Box(new GUIContent(name, description), _labelStyle, Height(PresetHandler.EditorDefaults.ConfigEntryHeight));
            value = BuilderClass.CreateSlider(value, min, max, rounding, _defaultEntryConfig.Toggle).Round(100f);
            Box(value.Round(rounding).ToString(), _defaultEntryConfig.Result);
            return value;
        }

        public static void EditFloatDictionary<T>(object dictValue, ConfigInfoClass info, out bool wasEdited) where T : Enum
        {
            BeginVertical(5f);

            float min = info.Min;
            float max = info.Max;
            float rounding = info.Rounding;

            var defaultDictionary = info.DefaultDictionary as Dictionary<T, float>;
            var dictionary = dictValue as Dictionary<T, float>;

            T[] array = EnumValues.GetEnum<T>();
            if (array != null && array.Length > 0) {
                for (int i = 0; i < array.Length; i++) {
                    //Logger.LogInfo(array[i]);
                }
            }
            List<T> list = new List<T>();
            foreach (var entry in dictionary) {
                if (entry.Key.ToString() == "Default") {
                    continue;
                }
                list.Add(entry.Key);
            }

            CreateLabelStyle();

            wasEdited = false;
            for (int i = 0; i < list.Count; i++) {
                BeginHorizontal(150f);

                var item = list[i];
                float originalValue = dictionary[item];
                float floatValue = originalValue;

                Box(new GUIContent(item.ToString()), _labelStyle, Height(PresetHandler.EditorDefaults.ConfigEntryHeight));
                floatValue = BuilderClass.CreateSlider(floatValue, min, max, rounding, _defaultEntryConfig.Toggle);
                Box(floatValue.Round(rounding).ToString(), _defaultEntryConfig.Result);

                if (resetButton())
                    floatValue = defaultDictionary[item];

                if (floatValue != originalValue) {
                    wasEdited = true;
                    dictionary[item] = floatValue;
                }
                EndHorizontal(150f);
            }
            list.Clear();
            EndVertical(5f);
        }

        public static void EditAllValuesInObj(object obj, out bool wasEdited, string search = null, GUIEntryConfig entryConfig = null, int listDepth = 0)
        {
            ConfigParams configParams = new ConfigParams {
                SettingsObject = obj,
                Search = search,
                EntryConfig = entryConfig,
                ListDepth = listDepth
            };
            EditAllValuesInObj(configParams, out wasEdited);
        }

        public static void EditAllValuesInObj(ConfigParams configParams, out bool wasEdited)
        {
            float indent = getIndentValue(configParams.EntryConfig);
            BeginVertical(0f);
            List<ConfigInfoClass> attributeInfos = new List<ConfigInfoClass>();
            getAllAttributeInfos(configParams.SettingsObject, attributeInfos, configParams.Search);
            displayOptionsByCategory(configParams, attributeInfos, out wasEdited);
            attributeInfos.Clear();
            EndVertical(0f);
        }

        private static float getIndentValue(GUIEntryConfig entryConfig)
        {
            const float defaultIndent = 5f;
            float indent;
            if (entryConfig != null) {
                indent = entryConfig.SubList_Indent_Vertical;
            }
            else {
                indent = defaultIndent;
            }
            return indent;
        }

        private static void displayCategory(ConfigParams configParams, List<ConfigInfoClass> attributeInfos, string category, out bool wasEdited)
        {
            wasEdited = false;
            bool categoryDrawn = false;

            int count = 0;

            // Display Non-Advanced Settings first, thats why there are 3 loops here. Probably a better way to do this.
            foreach (ConfigInfoClass attributes in attributeInfos) {
                if (attributes.AdvancedOption || attributes.DeveloperOption) {
                    continue;
                }
                if (attributes.DoNotShowGUI) {
                    continue;
                }
                if (attributes.Category != category) {
                    continue;
                }
                if (!categoryDrawn) {
                    categoryDrawn = true;
                    drawCategory(configParams, attributes, category);
                }
                displayConfigGUI(attributes, configParams, count++, out bool newEdit);
                if (newEdit) {
                    wasEdited = true;
                }
            }
            foreach (ConfigInfoClass attributes in attributeInfos) {
                if (attributes.AdvancedOption == false) {
                    continue;
                }
                if (attributes.DoNotShowGUI) {
                    continue;
                }
                if (attributes.Category != category) {
                    continue;
                }
                if (!categoryDrawn) {
                    categoryDrawn = true;
                    drawCategory(configParams, attributes, category);
                }
                displayConfigGUI(attributes, configParams, count++, out bool newEdit);
                if (newEdit) {
                    wasEdited = true;
                }
            }
            foreach (ConfigInfoClass attributes in attributeInfos) {
                if (attributes.DeveloperOption == false) {
                    continue;
                }
                if (attributes.DoNotShowGUI) {
                    continue;
                }
                if (attributes.Category != category) {
                    continue;
                }
                if (!categoryDrawn) {
                    categoryDrawn = true;
                    drawCategory(configParams, attributes, category);
                }
                displayConfigGUI(attributes, configParams, count++, out bool newEdit);
                if (newEdit) {
                    wasEdited = true;
                }
            }
            if (count > 0) {
                Space(10f);
            }
        }

        private static void drawCategory(ConfigParams configParams, ConfigInfoClass configInfo, string category)
        {
            if (category == "None") {
                return;
            }
            BeginHorizontal(0f);
            DisplayString($"    Category: {category}    ", configParams.ListDepth, configParams.EntryConfig, 15f);
            FlexibleSpace();
            EndHorizontal();
        }

        private static void displayOptionsByCategory(ConfigParams configParams, List<ConfigInfoClass> configInfos, out bool wasEdited)
        {
            wasEdited = false;
            List<string> categoriesList = new List<string>();
            getCategories(configInfos, categoriesList);
            for (int i = 0; i < categoriesList.Count; i++) {
                displayCategory(configParams, configInfos, categoriesList[i], out bool newEdit);
                if (newEdit) {
                    wasEdited = true;
                }
            }
            categoriesList.Clear();
        }

        private static void getCategories(List<ConfigInfoClass> configInfos, List<string> outputList)
        {
            outputList.Clear();
            // Get all categories that exist on this settings page, populate list with unique ones.
            for (int i = 0; i < configInfos.Count; i++) {
                ConfigInfoClass configInfo = configInfos[i];
                string category = configInfo.Category;
                if (category.IsNullOrEmpty() ||
                    outputList.Contains(category)) {
                    continue;
                }
                outputList.Add(category);
            }
        }

        public struct ConfigParams
        {
            public object SettingsObject;
            public string Search;
            public GUIEntryConfig EntryConfig;
            public int ListDepth;
        }

        private static void displayConfigGUI(ConfigInfoClass configInfo, ConfigParams configParams, int count, out bool edited)
        {
            object oldValue = getConfigValue(configInfo, configParams.SettingsObject);
            object newValue = EditValue(ref oldValue, configParams.SettingsObject, configInfo, out bool newEdit, configParams.ListDepth, configParams.EntryConfig, configParams.Search);
            if (newEdit) {
                setConfigValue(newValue, configInfo.MemberInfo, configParams.SettingsObject);
                edited = true;
                return;
            }
            edited = false;
        }

        private static object getConfigValue(ConfigInfoClass configInfo, object obj)
        {
            MemberInfo memberInfo = configInfo.MemberInfo;
            switch (memberInfo.MemberType) {
                case MemberTypes.Field:
                    return (memberInfo as FieldInfo).GetValue(obj);

                case MemberTypes.Property:
                    return (memberInfo as PropertyInfo).GetValue(obj);

                default:
                    return null;
            }
        }

        private static void setConfigValue(object value, MemberInfo memberInfo, object obj)
        {
            switch (memberInfo.MemberType) {
                case MemberTypes.Field:
                    (memberInfo as FieldInfo).SetValue(obj, value);
                    return;

                case MemberTypes.Property:
                    (memberInfo as PropertyInfo).SetValue(obj, value);
                    return;

                default:
                    return;
            }
        }

        private static void getAllAttributeInfos(object obj, List<ConfigInfoClass> outputList, string search)
        {
            outputList.Clear();
            FieldInfo[] fieldInfos = obj.GetType().GetFields();
            foreach (FieldInfo field in fieldInfos) {
                ConfigInfoClass configInfo = GetAttributeInfo(field);
                if (SkipForSearch(configInfo, search)) {
                    continue;
                }
                outputList.Add(configInfo);
            }
        }

        public static void EditAllValuesInObj(Category category, object categoryObject, out bool wasEdited, string search = null)
        {
            EditAllValuesInObj(categoryObject, out wasEdited, search);
            //BeginVertical(5);
            //wasEdited = false;
            //foreach (var fieldAtt in category.FieldAttributesList) {
            //    if (SkipForSearch(fieldAtt, search) || fieldAtt.Advanced) {
            //        continue;
            //    }
            //    object value = fieldAtt.GetValue(categoryObject);
            //    object newValue = EditValue(ref value, categoryObject, fieldAtt, out bool newEdit, 0, null, search);
            //    if (newEdit) {
            //        fieldAtt.SetValue(categoryObject, newValue);
            //        wasEdited = true;
            //    }
            //}
            //
            //foreach (var fieldAtt in category.FieldAttributesList) {
            //    if (SkipForSearch(fieldAtt, search) || !fieldAtt.Advanced) {
            //        continue;
            //    }
            //    object value = fieldAtt.GetValue(categoryObject);
            //    object newValue = EditValue(ref value, categoryObject, fieldAtt, out bool newEdit, 0, null, search);
            //    if (newEdit) {
            //        fieldAtt.SetValue(categoryObject, newValue);
            //        wasEdited = true;
            //    }
            //}
            //
            //EndVertical(5);
        }

        public static bool SkipForSearch(ConfigInfoClass attributes, string searchQuerry)
        {
            return !string.IsNullOrEmpty(searchQuerry) &&
                (attributes.Name?.ToLower().Contains(searchQuerry) == false &&
                attributes.Description?.ToLower().Contains(searchQuerry) == false &&
                attributes.Category?.ToLower().Contains(searchQuerry) == false);
        }

        private static readonly Dictionary<string, bool> _listOpen = new Dictionary<string, bool>();
        private static readonly List<string> _failedAdds = new List<string>();
        private static readonly GUIEntryConfig _defaultEntryConfig = new GUIEntryConfig();
        private static GUIStyle _labelStyle;
        private static readonly Dictionary<string, ConfigInfoClass> _attributeClasses = new Dictionary<string, ConfigInfoClass>();
    }
}