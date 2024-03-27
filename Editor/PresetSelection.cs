using SAIN.Plugin;
using SAIN.Preset;
using System;
using System.Text.RegularExpressions;
using UnityEngine;
using static SAIN.Editor.SAINLayout;

namespace SAIN.Editor.GUISections
{
    public static class PresetSelection
    {
        public static bool Menu()
        {
            const float LabelHeight = 35f;

            BeginHorizontal(100f);
            Box("Installed Presets", Height(LabelHeight));
            Box("Select an installed preset for SAIN Settings", Height(LabelHeight));
            if (Button("Refresh", "Refresh installed Presets", null, Height(LabelHeight)))
            {
                PresetHandler.LoadPresetOptions();
            }
            EndHorizontal(100f);

            SAINPresetDefinition selectedPreset = SAINPlugin.LoadedPreset.Info;
            if (selectedPreset.SAINVersion != AssemblyInfoClass.SAINPresetVersion)
            {
                Box(new GUIContent(
                        $"Selected Preset Version: {selectedPreset.SAINVersion} " +
                        $"but current SAIN Preset Version is: {AssemblyInfoClass.SAINPresetVersion}, you may experience issues."),
                    GetStyle(Style.alert), Height(LabelHeight + 5));
            }

            BeginHorizontal(true);

            const float InstalledHeight = 40;
            const int OptionsPerLine = 4;
            float width = 1860f / OptionsPerLine;
            float spacing = 15f / OptionsPerLine - 1;
            const float AlertWidth = 25f;

            int presetSpacing = 0;
            for (int i = 0; i < PresetHandler.PresetOptions.Count; i++)
            {
                presetSpacing++;
                var preset = PresetHandler.PresetOptions[i];
                bool badVersion = preset.SAINVersion != AssemblyInfoClass.SAINPresetVersion;
                float toggleWidth = badVersion ? width - AlertWidth : width;

                bool selected = selectedPreset.Name == preset.Name;
                if (Toggle(selected, 
                    $"{preset.Name} for SAIN {preset.SAINVersion}", 
                    preset.Description, 
                    null, 
                    Height(InstalledHeight), 
                    Width(toggleWidth)))
                {
                    if (!selected)
                    {
                        selectedPreset = preset;
                    }
                }
                if (badVersion)
                {
                    Box(new GUIContent(
                            "!", 
                            $"Selected Preset Version {preset.SAINVersion} " +
                            $"but current SAIN Preset Version is: {AssemblyInfoClass.SAINPresetVersion}, " +
                            $"you may experience issues."),
                        GetStyle(Style.alert), 
                        Height(InstalledHeight), 
                        Width(AlertWidth));
                }
                if (presetSpacing >= OptionsPerLine)
                {
                    presetSpacing = 0;
                    EndHorizontal(true);
                    BeginHorizontal(true);
                }
                else
                {
                    Space(spacing);
                }
            }
            EndHorizontal(true);

            Space(10);

            OpenNewPresetMenu = BuilderClass.ExpandableMenu("Create New Preset", OpenNewPresetMenu, null, 40f);
            if (OpenNewPresetMenu)
            {
                Space(5);

                NewName = LabeledTextField(NewName, "Preset Name");
                NewDescription = LabeledTextField(NewDescription, "Preset Description");
                NewCreator = LabeledTextField(NewCreator, "Creator Name");

                Space(5);

                Box("Select Preset to Copy From");
                Space(2);
                for (int i = 0; i < PresetHandler.PresetOptions.Count; i++)
                {
                    string optionName = PresetHandler.PresetOptions[i].Name;
                    bool selected = CopyFrom == optionName;
                    BeginHorizontal(150f);
                    if (Toggle(selected, optionName, EFT.UI.EUISoundType.MenuCheckBox, Height(25)))
                    {
                        if (!selected)
                        {
                            CopyFrom = optionName;
                        }
                    }
                    EndHorizontal(150f);
                }

                Space(5);

                if (Button("Save and Export", EFT.UI.EUISoundType.InsuranceInsured, Height(50f)))
                {
                    var definition = new SAINPresetDefinition()
                    {
                        Name = NewName,
                        Description = NewDescription,
                        Creator = NewCreator,
                        SAINVersion = AssemblyInfoClass.SAINPresetVersion,
                        DateCreated = DateTime.Today.ToString()
                    };
                    PresetHandler.PresetOptions.Add(definition);
                    PresetHandler.SavePresetDefinition(definition);
                    PresetHandler.InitPresetFromDefinition(definition);
                }
            }

            if (selectedPreset.Name != SAINPlugin.LoadedPreset.Info.Name)
            {
                PresetHandler.InitPresetFromDefinition(selectedPreset);
                return true;
            }
            return false;
        }

        private static string LabeledTextField(string value, string label)
        {
            BeginHorizontal(25f);

            Box(label, Width(225f));
            Space(5);
            value = TextField(value);

            EndHorizontal(25f);
            return Regex.Replace(value, @"[^\w \-]", "");
        }

        private static bool OpenNewPresetMenu;

        private static string NewName = "New Preset Name";
        private static string NewDescription = "Preset Description";
        private static string NewCreator = "Your Name Here";
        private static string CopyFrom = PresetHandler.DefaultPreset;
    }
}
