using EFT.UI;
using SAIN.Editor.Abstract;
using SAIN.Editor.GUISections;
using SAIN.Helpers;
using SAIN.Plugin;
using static SAIN.Editor.SAINLayout;

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
            BotSelection.ClearCache();
        }

        public PresetSelection PresetSelection { get; private set; }
        public BotSelectionClass BotSelection { get; private set; }
        public BotPersonalityEditor BotPersonalityEditor { get; private set; }
        public BotSettingsEditor SettingsEditor { get; private set; }

        public void CreateTabs(EEditorTab selectedTab)
        {
            EditTabsClass.BeginScrollView();
            switch (selectedTab)
            {
                case EEditorTab.None:
                    break;

                case EEditorTab.Home:
                    Home(); break;

                case EEditorTab.GlobalSettings:
                    GlobalSettings(); break;

                case EEditorTab.BotSettings:
                    BotSettings(); break;

                case EEditorTab.Personalities:
                    Personality(); break;

                case EEditorTab.Advanced:
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
            string toolTip = $"Apply Values set below to GlobalSettings. " +
                $"Exports edited values to SAIN/Presets/{SAINPlugin.LoadedPreset.Info.Name} folder";
            if (Builder.SaveChanges(GlobalSettingsWereEdited, toolTip, 35))
            {
                SAINPlugin.LoadedPreset.ExportGlobalSettings();
            }

            SettingsEditor.ShowAllSettingsGUI(SAINPlugin.LoadedPreset.GlobalSettings, out bool newEdit);
            if (newEdit)
            {
                GlobalSettingsWereEdited = true;
            }
        }

        public bool GlobalSettingsWereEdited;

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
            bool oldValue = Editor.AdvancedBotConfigs;
            Editor.AdvancedBotConfigs = Editor.AdvancedBotConfigs.GUIToggle("Advanced Bot Configs", "Edit at your own risk.", EUISoundType.MenuCheckBox, Height(40));
            if (oldValue != Editor.AdvancedBotConfigs)
            {
                Builder.ModifyLists.ClearCache();
                SettingsEditor.ClearCache();
                PresetHandler.SaveEditorDefaults();
            }

            oldValue = SAINPlugin.DebugModeEnabled;
            SAINPlugin.DebugModeEnabled = SAINPlugin.DebugModeEnabled.GUIToggle("Global Debug Mode", EUISoundType.MenuCheckBox, Height(40));
            if (oldValue != SAINPlugin.DebugModeEnabled)
            {
                PresetHandler.SaveEditorDefaults();
            }

            oldValue = SAINPlugin.DrawDebugGizmos;
            SAINPlugin.DrawDebugGizmos = SAINPlugin.DrawDebugGizmos.GUIToggle("Draw Debug Gizmos", EUISoundType.MenuCheckBox, Height(40));
            if (oldValue != SAINPlugin.DrawDebugGizmos)
            {
                PresetHandler.SaveEditorDefaults();
            }

            ForceDecisionOpen = Builder.ExpandableMenu("Force SAIN Bot Decisions", ForceDecisionOpen);

            if (ForceDecisionOpen)
            {
                const int spacing = 5;
                Space(spacing);

                ForceSoloOpen = Builder.ExpandableMenu("Force Solo Decision", ForceSoloOpen);
                if (ForceSoloOpen)
                {
                    if (Button("Reset"))
                        SAINPlugin.ForceSoloDecision = SoloDecision.None;

                    SAINPlugin.ForceSoloDecision = Builder.SelectionGrid(
                        SAINPlugin.ForceSoloDecision,
                        EnumValues.GetEnum<SoloDecision>());
                }

                Space(spacing);

                ForceSquadOpen = Builder.ExpandableMenu("Force Squad Decision", ForceSquadOpen);
                if (ForceSquadOpen)
                {
                    if (Button("Reset"))
                        SAINPlugin.ForceSquadDecision = SquadDecision.None;

                    SAINPlugin.ForceSquadDecision =
                        Builder.SelectionGrid(SAINPlugin.ForceSquadDecision,
                        EnumValues.GetEnum<SquadDecision>());
                }

                Space(spacing);

                ForceSelfOpen = Builder.ExpandableMenu("Force Self Decision", ForceSelfOpen);
                if (ForceSelfOpen)
                {
                    if (Button("Reset"))
                        SAINPlugin.ForceSelfDecision = SelfDecision.None;

                    SAINPlugin.ForceSelfDecision = Builder.SelectionGrid(
                        SAINPlugin.ForceSelfDecision,
                        EnumValues.GetEnum<SelfDecision>());
                }
            }
        }

        private bool ForceDecisionOpen;
        private bool ForceSoloOpen;
        private bool ForceSquadOpen;
        private bool ForceSelfOpen;
    }
}