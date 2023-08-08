using SAIN.Editor.Abstract;
using SAIN.Editor.GUISections;
using SAIN.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace SAIN.Editor
{
    public class GUITabs : EditorAbstract
    {
        public GUITabs(SAINEditor editor) : base(editor)
        {
        }

        public void None()
        {

        }

        public void Home()
        {
            ModDetection.ModDetectionGUI();

            Builder.Space(5f);

            Editor.PresetSelection.Menu();
        }

        public void GlobalSettings()
        {
            if (Builder.Button("Save"))
            {
                PresetHandler.UpdateExistingBots();
                PresetHandler.SaveLoadedPreset();
            }

            Editor.SettingsEditor.SettingsMenu(SAINPlugin.LoadedPreset.GlobalSettings);
        }

        public void BotSettings()
        {

        }

        public void Personality()
        {

        }

        public void Advanced()
        {

        }
    }
}
