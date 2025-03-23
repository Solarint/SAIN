using EFT.UI;
using SAIN.Helpers;
using SAIN.Plugin;
using SAIN.Preset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using static SAIN.Editor.SAINLayout;

namespace SAIN.Editor.GUISections
{
    public static class PresetSelection
    {
        private static readonly List<SAINPresetDefinition> defaultPresets = SAINDifficultyClass.DefaultPresetDefinitions.Values.ToList();

        private const float PRESET_LABEL_HEIGHT = 55f;
        private const float PRESET_OPTION_HEIGHT = 25f;
        private const float PRESET_OPTION_WIDTH = 500;
        private const float PRESET_BASE_OPTION_WIDTH = 150f;
        private const float PRESET_ALERT_HEIGHT = 30f;

        public static void PresetSelectionMenu()
        {
            SAINPresetDefinition selectedPreset = SAINPlugin.LoadedPreset.Info;
            checkCreateWarning(selectedPreset);

            /////
            BeginHorizontal();

            baseSelectionOptions();
            selectedPreset = selectDefault(selectedPreset);
            selectedPreset = selectCustom(selectedPreset);
            checkCreateNew();
            if (checkDeletePreset()) {
                selectedPreset = SAINPresetClass.Instance.Info;
            }
            FlexibleSpace();

            EndHorizontal();
            /////

            if (selectedPreset.Name != SAINPlugin.LoadedPreset.Info.Name) {
                PresetHandler.InitPresetFromDefinition(selectedPreset);
            }
        }

        private static void checkCreateWarning(SAINPresetDefinition selectedPreset)
        {
            string sainPresetV = selectedPreset.SAINPresetVersion;
            if (string.IsNullOrEmpty(sainPresetV)) {
                sainPresetV = selectedPreset.SAINVersion;
            }
            GUIContent content = new GUIContent(
                        $"Warning: The selected preset version is: [{sainPresetV}], " +
                        $"but current SAIN preset version is: [{AssemblyInfoClass.SAINPresetVersion}] (SAIN version [{AssemblyInfoClass.SAINVersion}]), default bot config values may be set incorrectly due to updates to SAIN. THIS DOESN'T MEAN YOUR GAME IS BROKEN, just be aware bots might not act as intended.");

            Rect rect = GUILayoutUtility.GetRect(content, GetStyle(Style.alert), Height(PRESET_ALERT_HEIGHT));
            if (selectedPreset.IsCustom && sainPresetV != AssemblyInfoClass.SAINPresetVersion) {
                GUI.Box(rect, content, GetStyle(Style.alert));
            }
            else {
                GUI.Box(rect, new GUIContent(""), GetStyle(Style.blankbox));
            }
        }

        private static void baseSelectionOptions()
        {
            BeginVertical();
            Box("Presets", "Select an Installed preset for SAIN Settings", Height(PRESET_LABEL_HEIGHT), Width(PRESET_BASE_OPTION_WIDTH));
            if (Button("Refresh", "Refresh installed Presets", EUISoundType.ButtonClick, Height(PRESET_LABEL_HEIGHT), Width(PRESET_BASE_OPTION_WIDTH))) {
                PresetHandler.LoadCustomPresetOptions();
            }

            _makeNewPresetMenuToggle = Toggle(
                _makeNewPresetMenuToggle,
                new GUIContent("Create New Preset"),
                EUISoundType.ButtonClick,
                Height(PRESET_LABEL_HEIGHT), Width(PRESET_BASE_OPTION_WIDTH));

            EndVertical();
        }

        private static SAINPresetDefinition selectDefault(SAINPresetDefinition selectedPreset)
        {
            BeginVertical();
            Label("Default Presets", Width(PRESET_OPTION_WIDTH));

            for (int i = 0; i < defaultPresets.Count; i++) {
                var preset = defaultPresets[i];
                if (SAINDifficultyClass.DefaultPresetDefinitions.TryGetKey(preset, out var sainDifficulty)) {
                    bool selected = SAINPlugin.EditorDefaults.SelectedDefaultPreset == sainDifficulty;

                    if (Toggle(
                        selected,
                        $"{preset.Name}",
                        preset.Description,
                        EUISoundType.MenuCheckBox,
                        Height(PRESET_OPTION_HEIGHT), Width(PRESET_OPTION_WIDTH)
                        )) {
                        if (!selected) {
                            SAINPlugin.EditorDefaults.SelectedDefaultPreset = sainDifficulty;
                            selectedPreset = preset;
                        }
                    }
                }
            }
            EndVertical();
            return selectedPreset;
        }

        private static SAINPresetDefinition selectCustom(SAINPresetDefinition selectedPreset)
        {
            BeginVertical();
            Label("Custom Presets", Width(PRESET_OPTION_WIDTH));
            for (int i = 0; i < PresetHandler.CustomPresetOptions.Count; i++) {
                var preset = PresetHandler.CustomPresetOptions[i];
                if (preset.IsCustom == true) {
                    bool selected = SAINPlugin.EditorDefaults.SelectedDefaultPreset == SAINDifficulty.none
                        && selectedPreset.Name == preset.Name;

                    if (Toggle(
                        selected,
                        $"{preset.Name}",
                        preset.Description,
                        EUISoundType.MenuCheckBox,
                        Height(PRESET_OPTION_HEIGHT), Width(PRESET_OPTION_WIDTH)
                        )) {
                        if (!selected) {
                            selectedPreset = preset;
                        }
                    }
                }
            }
            EndVertical();
            return selectedPreset;
        }

        private static void checkCreateNew()
        {
            if (_makeNewPresetMenuToggle) {
                BeginVertical();

                BeginHorizontal();
                Space(25);
                SAINPresetDefinition info = SAINPlugin.LoadedPreset.Info;
                if (info.CanEditName && Button("Save Info", "Update the selected presets name, description, and creator.", EFT.UI.EUISoundType.InsuranceInsured, Height(30f))) {
                    string oldName = info.Name;
                    var newInfo = info.Clone();

                    newInfo.Name = NewName;
                    newInfo.Description = NewDescription;
                    newInfo.Creator = NewCreator;

                    JsonUtility.DeletePreset(info);

                    PresetHandler.SavePresetDefinition(newInfo);
                    PresetHandler.InitPresetFromDefinition(newInfo, true);
                    PresetHandler.LoadCustomPresetOptions();
                }
                if (Button("Save A New Preset", EFT.UI.EUISoundType.InsuranceInsured, Height(30f))) {
                    SAINPresetDefinition newPreset = SAINPlugin.LoadedPreset.Info.Clone();

                    newPreset.Name = NewName;
                    newPreset.Description = NewDescription;
                    newPreset.Creator = NewCreator;
                    newPreset.SAINVersion = AssemblyInfoClass.SAINPresetVersion;
                    newPreset.DateCreated = DateTime.Today.ToString();

                    PresetHandler.SavePresetDefinition(newPreset);
                    PresetHandler.InitPresetFromDefinition(newPreset, true);
                }
                Space(25);
                EndHorizontal();

                Space(3);

                NewName = LabeledTextField(NewName, "Name");
                NewDescription = LabeledTextField(NewDescription, "Description");
                NewCreator = LabeledTextField(NewCreator, "Creator");

                EndVertical();
            }
        }

        private static bool checkDeletePreset()
        {
            if (SAINPresetClass.Instance.Info.IsCustom) {
                BeginVertical();
                _deletePresetConfirmation1 = Toggle(_deletePresetConfirmation1, "Delete Selected Preset", null, Height(30), Width(250f));
                if (_deletePresetConfirmation1) {
                    _deletePresetConfirmation2 = Toggle(_deletePresetConfirmation2, "Are you Sure?", null, Height(30), Width(250f));
                    if (_deletePresetConfirmation2) {
                        if (Button($"CONFIRM DELETE OF {SAINPresetClass.Instance.Info.Name} ?", Height(60), Width(250f))) {
                            var deletedInfo = SAINPresetClass.Instance.Info;
                            PresetHandler.loadDefault();
                            JsonUtility.DeletePreset(deletedInfo);
                            Sounds.PlaySound(EUISoundType.MalfunctionExamined);
                            PresetHandler.LoadCustomPresetOptions();
                            _deletePresetConfirmation2 = false;
                            _deletePresetConfirmation1 = false;
                            return true;
                        }
                    }
                }
                EndVertical();
            }
            return false;
        }

        private static bool _deletePresetConfirmation1 = false;
        private static bool _deletePresetConfirmation2 = false;

        private static string LabeledTextField(string value, string label)
        {
            BeginHorizontal();
            Box(label, Width(125f), Height(PresetHandler.EditorDefaults.ConfigEntryHeight));
            value = TextField(value, null, Width(350f), Height(PresetHandler.EditorDefaults.ConfigEntryHeight));
            EndHorizontal();

            return Regex.Replace(value, @"[^\w \-]", "");
        }

        private static bool _makeNewPresetMenuToggle;

        private static string NewName = "Enter Name Here";
        private static string NewDescription = "Enter Description Here";
        private static string NewCreator = "Your Name Here";
    }
}