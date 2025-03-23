using EFT.UI;
using SAIN.Attributes;
using SAIN.Components;
using SAIN.Editor.GUISections;
using SAIN.Helpers;
using SAIN.Plugin;
using SAIN.Preset;
using UnityEngine;
using static SAIN.Editor.SAINLayout;

namespace SAIN.Editor
{
    public static class GUITabs
    {
        public static void CreateTabs(EEditorTab selectedTab)
        {
            EditTabsClass.BeginScrollView();
            switch (selectedTab) {
                case EEditorTab.Home:
                    Home(); break;

                case EEditorTab.BotSettings:
                    BotSettings(); break;

                case EEditorTab.Personalities:
                    Personality(); break;

                case EEditorTab.EquipmentStealth:
                    Stealth(); break;

                case EEditorTab.Advanced:
                    Advanced(); break;

                default: break;
            }
            EditTabsClass.EndScrollView();
        }

        public static void Home()
        {
            PresetSelection.PresetSelectionMenu();
            Space(20f);

            BotSettingsEditor.ShowAllSettingsGUI(
                SAINPlugin.LoadedPreset.GlobalSettings,
                out bool newEdit,
                "Global Settings",
                $"SAIN/Presets/{SAINPlugin.LoadedPreset.Info.Name}",
                35f,
                out bool saved);

            if (saved) {
                SAINPresetClass.ExportAll(SAINPlugin.LoadedPreset);
                ConfigEditingTracker.Clear();
            }
        }

        public static void BotSettings()
        {
            BotSelectionClass.Menu();
        }

        public static void Personality()
        {
            BotPersonalityEditor.PersonalityMenu();
        }

        private static void Stealth()
        {
            BeginVertical();

            BeginHorizontal();
            if (ConfigEditingTracker.UnsavedChanges) {
                BuilderClass.Alert(
                    "Click Save to export changes, and send changes to bots if in-game",
                    "YOU HAVE UNSAVED CHANGES!",
                    35f, ColorNames.DarkRed);
            }
            else {
                BuilderClass.Alert(null, null, 25f, null);
            }

            if (Button(
                "Save and Export",
                ConfigEditingTracker.GetUnsavedValuesString(),
                EUISoundType.InsuranceInsured,
                Height(25f))) {
                SAINPresetClass.ExportAll(SAINPlugin.LoadedPreset);
            }

            EndHorizontal();

            AttributesGUI.EditAllStealthValues(SAINPlugin.LoadedPreset.GearStealthValuesClass);
            EndVertical();
        }

        private static void forceDecisions(int spacing)
        {
            Space(spacing);

            _forceDecisionMenuOpen = BuilderClass.ExpandableMenu("Force SAIN Bot Decisions", _forceDecisionMenuOpen);
            if (_forceDecisionMenuOpen) {
                Space(spacing);

                ForceSoloOpen = BuilderClass.ExpandableMenu("Force Solo Decision", ForceSoloOpen);
                if (ForceSoloOpen) {
                    Space(spacing / 2f);

                    if (Button("Reset"))
                        SAINPlugin.ForceSoloDecision = ECombatDecision.None;

                    Space(spacing / 2f);

                    SAINPlugin.ForceSoloDecision = BuilderClass.SelectionGrid(
                        SAINPlugin.ForceSoloDecision,
                        EnumValues.GetEnum<ECombatDecision>());
                }

                Space(spacing);

                ForceSquadOpen = BuilderClass.ExpandableMenu("Force Squad Decision", ForceSquadOpen);
                if (ForceSquadOpen) {
                    Space(spacing / 2f);

                    if (Button("Reset"))
                        SAINPlugin.ForceSquadDecision = ESquadDecision.None;

                    Space(spacing / 2f);

                    SAINPlugin.ForceSquadDecision =
                        BuilderClass.SelectionGrid(SAINPlugin.ForceSquadDecision,
                        EnumValues.GetEnum<ESquadDecision>());
                }

                Space(spacing);

                ForceSelfOpen = BuilderClass.ExpandableMenu("Force Self Decision", ForceSelfOpen);
                if (ForceSelfOpen) {
                    Space(spacing / 2f);

                    if (Button("Reset"))
                        SAINPlugin.ForceSelfDecision = ESelfDecision.None;

                    Space(spacing / 2f);

                    SAINPlugin.ForceSelfDecision = BuilderClass.SelectionGrid(
                        SAINPlugin.ForceSelfDecision,
                        EnumValues.GetEnum<ESelfDecision>());
                }
            }
        }

        public static void Advanced()
        {
            AttributesGUI.EditAllValuesInObj(PresetHandler.EditorDefaults, out bool newEdit);
            if (newEdit) {
                PresetHandler.ExportEditorDefaults();
            }

            if (!SAINPlugin.DebugMode) {
                return;
            }

            const int spacing = 4;
            forceDecisions(spacing);
            forceTalk(spacing);
        }

        private static void forceTalk(int spacing)
        {
            Space(spacing);
            _forceTalkMenuOpen = BuilderClass.ExpandableMenu("Force Bots to Say Phrase", _forceTalkMenuOpen);
            if (_forceTalkMenuOpen) {
                Space(5);
                _forceTagStatusToggle = Toggle(_forceTagStatusToggle, "Force ETagStatus for Phrase");
                if (_forceTagStatusToggle) {
                    ETagStatus[] statuses = EnumValues.GetEnum<ETagStatus>();
                    for (int i = 0; i < statuses.Length; i++) {
                        if (Toggle(_forcedTagStatus == statuses[i], statuses[i].ToString())) {
                            if (_forcedTagStatus != statuses[i]) {
                                _forcedTagStatus = statuses[i];
                            }
                        }
                    }
                }
                Space(5);
                _withGroupDelay = Toggle(_withGroupDelay, "With Group Delay?");
                Space(5);
                Label("Say Phrase");
                EPhraseTrigger[] triggers = EnumValues.GetEnum<EPhraseTrigger>();
                for (int i = 0; i < triggers.Length; i++) {
                    if (Button(triggers[i].ToString())) {
                        if (SAINBotController.Instance?.Bots != null) {
                            foreach (var bot in SAINBotController.Instance.Bots.Values) {
                                if (bot != null) {
                                    if (_forceTagStatusToggle) {
                                        bot.Talk.Say(triggers[i], _forcedTagStatus, _withGroupDelay);
                                    }
                                    else {
                                        bot.Talk.Say(triggers[i], null, _withGroupDelay);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool _withGroupDelay;
        private static bool _forceTagStatusToggle;
        private static ETagStatus _forcedTagStatus;
        private static bool _forceTalkMenuOpen;
        private static bool _forceDecisionMenuOpen;
        private static bool ForceSoloOpen;
        private static bool ForceSquadOpen;
        private static bool ForceSelfOpen;
    }
}