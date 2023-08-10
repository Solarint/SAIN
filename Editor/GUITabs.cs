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
            if (Button("Save"))
            {
                PresetHandler.UpdateExistingBots();
                PresetHandler.SaveLoadedPreset();
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
            BotPersonalityEditor.PersonalityMenu();
        }

        public void Advanced()
        {
            Editor.AdvancedOptionsEnabled = Builder.Toggle(Editor.AdvancedOptionsEnabled, "Advanced Bot Configs", "Edit at your own risk.", null, Builder.Height(30f));
            SAINPlugin.DebugModeEnabled = Builder.Toggle(SAINPlugin.DebugModeEnabled, "Global Debug Mode", null, Builder.Height(30f));

            Space(5);

            Label("GUI Sounds Test");
            int count = 0;

            BeginHorizontal();
            foreach (var sound in EnumValues.GetEnum<EUISoundType>())
            {
                count++;
                Button(sound.ToString(), sound, Width(3 / 1800f));
                if (count == 3)
                {
                    count = 0;
                    EndHorizontal();
                    BeginHorizontal();
                }
            }
            EndHorizontal();
        }
    }
}
