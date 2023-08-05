using SAIN.Editor.Abstract;
using SAIN.Plugin;
using SAIN.SAINPreset;
using UnityEngine;

namespace SAIN.Editor.GUISections
{
    public class PresetSelection : EditorAbstract
    {
        public PresetSelection(SAINEditor editor) : base(editor) { }

        public bool Menu()
        {
            BeginVertical();

            BeginHorizontal();
            const float LabelHeight = 25f;
            Box("Installed Presets", Height(LabelHeight));
            Box("Select an installed preset for SAIN Settings", Height(LabelHeight));
            bool refresh = Button("Refresh", "Refresh installed Presets", Height(LabelHeight));
            LoadPresetOptions(refresh);
            EndHorizontal();

            BeginHorizontal();
            const float InstalledHeight = 30;
            float endHeight = InstalledHeight + LabelHeight;

            int presetSpacing = 0;
            SAINPresetDefinition selectedPreset = SAINPlugin.LoadedPreset.Definition;
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

            if (selectedPreset != SAINPlugin.LoadedPreset.Definition)
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
