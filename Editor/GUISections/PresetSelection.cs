using SAIN.Editor.Abstract;
using SAIN.Plugin;
using SAIN.Preset;
using UnityEngine;

namespace SAIN.Editor.GUISections
{
    public class PresetSelection : EditorAbstract
    {
        public PresetSelection(SAINEditor editor) : base(editor) { }

        public bool Menu()
        {
            BeginVertical();

            const float LabelHeight = 25f;
            SAINPresetDefinition selectedPreset = SAINPlugin.LoadedPreset.Info;
            if (selectedPreset.SAINVersion != AssemblyInfo.SAINVersion)
            {
                Box($"Selected Preset was made for SAIN Version {selectedPreset.SAINVersion} but you are running {AssemblyInfo.SAINVersion}, you may experience issues.", Height(LabelHeight));
            }
            BeginHorizontal();
            Box("Installed Presets", Height(LabelHeight));
            Box("Select an installed preset for SAIN Settings", Height(LabelHeight));
            bool refresh = Button("Refresh", "Refresh installed Presets", Height(LabelHeight));
            LoadPresetOptions(refresh);
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
                if (Toggle(selected, preset.Name, preset.Description, Height(InstalledHeight)))
                {
                    selectedPreset = preset;
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

            if (selectedPreset != SAINPlugin.LoadedPreset.Info)
            {
                PresetHandler.InitPresetFromDefinition(selectedPreset);
                return true;
            }
            return false;
        }

        private void LoadPresetOptions(bool refresh = false)
        {
            if (RecheckOptionsTimer < Time.time || refresh)
            {
                RecheckOptionsTimer = Time.time + 60f;
                PresetHandler.LoadPresetOptions();
            }
        }

        private float RecheckOptionsTimer;
    }
}
