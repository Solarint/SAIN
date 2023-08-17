using EFT.UI;
using SAIN.Editor.GUISections;
using SAIN.Helpers;
using SAIN.Plugin;
using static Mono.Security.X509.X520;
using System.ComponentModel;
using static SAIN.Editor.SAINLayout;
using UnityEngine;
using static EFT.SpeedTree.TreeWind;
using SAIN.Preset.GlobalSettings;
using static GClass1711;
using SAIN.Attributes;

namespace SAIN.Editor
{
    public static class GUITabs
    {
        public static void CreateTabs(EEditorTab selectedTab)
        {
            EditTabsClass.BeginScrollView();
            switch (selectedTab)
            {
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

        public static void Home()
        {
            ModDetection.ModDetectionGUI();
            Space(15f);
            PresetSelection.Menu();
        }

        public static void GlobalSettings()
        {
            BotSettingsEditor.ShowAllSettingsGUI(
                SAINPlugin.LoadedPreset.GlobalSettings, 
                out bool newEdit,
                "Global Settings", 
                $"SAIN/Presets/{SAINPlugin.LoadedPreset.Info.Name}", 
                35f, 
                GlobalSettingsWereEdited, 
                out bool saved);

            if (newEdit)
            {
                GlobalSettingsWereEdited = true;
            }
            if (saved)
            {
                SAINPlugin.LoadedPreset.ExportGlobalSettings();
            }
        }

        public static bool GlobalSettingsWereEdited;

        public static void BotSettings()
        {
            //BeginArea(SAINEditor.OpenTabRect);
            BotSelectionClass.Menu();
            //EndArea();
        }

        public static void Personality()
        {
            BotPersonalityEditor.PersonalityMenu();
        }

        public static void Advanced()
        {
            const int spacing = 4;

            AttributesGUI.EditAllValuesInObj(PresetHandler.EditorDefaults, out bool newEdit);
            if (newEdit)
            {
                PresetHandler.ExportEditorDefaults();
            }

            if (!PresetHandler.EditorDefaults.GlobalDebugMode)
            {
                return;
            }

            Space(spacing);

            ForceDecisionOpen = BuilderClass.ExpandableMenu("Force SAIN Bot Decisions", ForceDecisionOpen);

            if (ForceDecisionOpen)
            {
                Space(spacing);

                ForceSoloOpen = BuilderClass.ExpandableMenu("Force Solo Decision", ForceSoloOpen);
                if (ForceSoloOpen)
                {
                    Space(spacing / 2f);

                    if (Button("Reset"))
                        SAINPlugin.ForceSoloDecision = SoloDecision.None;

                    Space(spacing / 2f);

                    SAINPlugin.ForceSoloDecision = BuilderClass.SelectionGrid(
                        SAINPlugin.ForceSoloDecision,
                        EnumValues.GetEnum<SoloDecision>());
                }

                Space(spacing);

                ForceSquadOpen = BuilderClass.ExpandableMenu("Force Squad Decision", ForceSquadOpen);
                if (ForceSquadOpen)
                {
                    Space(spacing / 2f);

                    if (Button("Reset"))
                        SAINPlugin.ForceSquadDecision = SquadDecision.None;

                    Space(spacing / 2f);

                    SAINPlugin.ForceSquadDecision =
                        BuilderClass.SelectionGrid(SAINPlugin.ForceSquadDecision,
                        EnumValues.GetEnum<SquadDecision>());
                }

                Space(spacing);

                ForceSelfOpen = BuilderClass.ExpandableMenu("Force Self Decision", ForceSelfOpen);
                if (ForceSelfOpen)
                {
                    Space(spacing / 2f);

                    if (Button("Reset"))
                        SAINPlugin.ForceSelfDecision = SelfDecision.None;

                    Space(spacing / 2f);

                    SAINPlugin.ForceSelfDecision = BuilderClass.SelectionGrid(
                        SAINPlugin.ForceSelfDecision,
                        EnumValues.GetEnum<SelfDecision>());
                }
            }
        }

        private static bool ForceDecisionOpen;
        private static bool ForceSoloOpen;
        private static bool ForceSquadOpen;
        private static bool ForceSelfOpen;
    }
}