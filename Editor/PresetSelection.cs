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
            BeginVertical();

            const float LabelHeight = 25f;
            SAINPresetDefinition selectedPreset = SAINPlugin.LoadedPreset.Info;
            var alertStyle = GetStyle(Style.alert);
            if (selectedPreset.SAINVersion != AssemblyInfo.SAINVersion)
            {
                Box(new GUIContent(
                        $"Selected Preset was made for SAIN Version {selectedPreset.SAINVersion} " +
                        $"but you are running {AssemblyInfo.SAINVersion}, you may experience issues."), 
                    alertStyle, Height(LabelHeight));
            }
            BeginHorizontal();
            Box("Installed Presets", Height(LabelHeight));
            Box("Select an installed preset for SAIN Settings", Height(LabelHeight));
            if (Button("Refresh", "Refresh installed Presets", null, Height(LabelHeight)))
            {
                PresetHandler.LoadPresetOptions();
            }
            EndHorizontal();

            BeginHorizontal();
            const float InstalledHeight = 30;
            float endHeight = InstalledHeight + LabelHeight;

            int presetSpacing = 0;
            for (int i = 0; i < PresetHandler.PresetOptions.Count; i++)
            {
                presetSpacing++;
                var preset = PresetHandler.PresetOptions[i];
                bool selected = selectedPreset.Name == preset.Name;
                if (Toggle(selected, $"{preset.Name} for SAIN {preset.SAINVersion}", preset.Description, null, Height(InstalledHeight)))
                {
                    selectedPreset = preset;
                }
                if (preset.SAINVersion != AssemblyInfo.SAINVersion)
                {
                    Box(new GUIContent(
                            "!", 
                            $"Selected Preset was made for SAIN Version {preset.SAINVersion} " +
                            $"but you are running {AssemblyInfo.SAINVersion}, you may experience issues."), 
                        alertStyle, Height(InstalledHeight), Width(20));
                }
                if (presetSpacing >= 4)
                {
                    endHeight += InstalledHeight;
                    presetSpacing = 0;
                    EndHorizontal();
                    BeginHorizontal();
                }
            }
            EndHorizontal();

            EndVertical();

            Space(10);

            OpenNewPresetMenu = BuilderClass.ExpandableMenu("Create New Preset", OpenNewPresetMenu);
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
                    if (Toggle(selected, optionName, EFT.UI.EUISoundType.MenuCheckBox, Height(25)))
                    {
                        if (!selected)
                        {
                            CopyFrom = optionName;
                        }
                    }
                }

                Space(5);

                if (Button("Save and Export", EFT.UI.EUISoundType.InsuranceInsured, Height(50f)))
                {
                    var definition = new SAINPresetDefinition()
                    {
                        Name = NewName,
                        Description = NewDescription,
                        Creator = NewCreator,
                        SAINVersion = AssemblyInfo.SAINVersion,
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
            BeginHorizontal();

            Box(label, Width(225f));
            value = TextField(value);

            EndHorizontal();
            return Regex.Replace(value, @"[^\w \-]", "");
        }

        private static bool OpenNewPresetMenu;

        private static string NewName = "New Preset Name";
        private static string NewDescription = "Preset Description";
        private static string NewCreator = "Your Name Here";
        private static string CopyFrom = PresetHandler.DefaultPreset;
    }
}
