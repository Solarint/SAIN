using EFT.Interactive;
using EFT.UI;
using SAIN.Editor.Abstract;
using SAIN.Editor.GUISections;
using SAIN.Helpers;
using SAIN.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace SAIN.Editor
{
    public class GUITabs : EditorAbstract
    {
        public GUITabs(SAINEditor editor) : base(editor)
        {
            PresetSelection = new PresetSelection(editor);
            BotSelection = new BotSelectionClass(editor);
            SettingsEditor = new BotSettingsEditor(editor);
            BotPersonalityEditor = new BotPersonalityEditor(editor);
        }

        public void ClearCache()
        {
            SettingsEditor.ClearCache();
        }

        public PresetSelection PresetSelection { get; private set; }
        public BotSelectionClass BotSelection { get; private set; }
        public BotPersonalityEditor BotPersonalityEditor { get; private set; }
        public BotSettingsEditor SettingsEditor { get; private set; }

        public void CreateTabs(EditorTabs selectedTab)
        {
            EditTabsClass.BeginScrollView();

            switch (selectedTab)
            {
                case EditorTabs.None:
                    break;

                case EditorTabs.Home:
                    Home(); break;

                case EditorTabs.GlobalSettings:
                    GlobalSettings(); break;

                case EditorTabs.BotSettings:
                    BotSettings(); break;

                case EditorTabs.Personalities:
                    Personality(); break;

                case EditorTabs.Advanced:
                    Advanced(); break;

                default: break;
            }

            EditTabsClass.EndScrollView();
        }

        public void Home()
        {
            ModDetection.ModDetectionGUI();
            Space(5f);
            PresetSelection.Menu();
        }

        public void GlobalSettings()
        {
            if (Button("Save",
                $"Apply Values set below to GlobalSettings. Exports edited values to SAIN/Presets/{SAINPlugin.LoadedPreset.Info.Name} folder",
                EUISoundType.InsuranceInsured, Height(30)))
            {
                SAINPlugin.LoadedPreset.ExportGlobalSettings();
                PresetHandler.UpdateExistingBots();
            }
            SettingsEditor.SettingsMenu(SAINPlugin.LoadedPreset.GlobalSettings);
        }

        public void BotSettings()
        {
            BeginArea(Editor.OpenTabRect);
            BotSelection.Menu();
            EndArea();
        }

        public void Personality()
        {
            if (Button("Save", 
                $"Apply Values set below to personalities. Exports edited values to SAIN/Presets/{SAINPlugin.LoadedPreset.Info.Name}/Personalities folder", 
                EUISoundType.InsuranceInsured, Height(30)))
            {
                SAINPlugin.LoadedPreset.ExportPersonalities();
                PresetHandler.UpdateExistingBots();
            }
            BotPersonalityEditor.PersonalityMenu();
        }

        public void Advanced()
        {
            bool wasEnabled = Editor.AdvancedOptionsEnabled;

            Editor.AdvancedOptionsEnabled = Builder.Toggle(Editor.AdvancedOptionsEnabled, 
                "Advanced Bot Configs", "Edit at your own risk.", 
                EUISoundType.MenuCheckBox, 
                Builder.Height(40f));

            if (wasEnabled != Editor.AdvancedOptionsEnabled)
            {
                PresetHandler.SaveEditorDefaults();
            }

            SAINPlugin.DebugModeEnabled = Builder.Toggle(SAINPlugin.DebugModeEnabled, 
                "Global Debug Mode", 
                EUISoundType.MenuCheckBox, 
                Builder.Height(40f));
        }
    }
}
