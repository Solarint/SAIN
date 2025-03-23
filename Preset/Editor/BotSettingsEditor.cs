using EFT.UI;
using SAIN.Attributes;
using SAIN.Editor.Util;
using SAIN.Helpers;
using SAIN.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static SAIN.Editor.SAINLayout;

namespace SAIN.Editor.GUISections
{
    public static class BotSettingsEditor
    {
        private static readonly StringBuilder _stringBuilder = new StringBuilder();

        public static void ShowAllSettingsGUI(object settings, out bool wasEdited, string name, string savePath, float height, out bool Saved)
        {
            BeginHorizontal();

            Box(name, Height(height));

            Space(10);

            Label("Search", Width(125f), Height(height));

            var container = SettingsContainers.GetContainer(settings.GetType(), name);
            container.SearchPattern = TextField(
                container.SearchPattern,
                null,
                Width(250),
                Height(height));

            if (Button(
                "Clear",
                EUISoundType.MenuContextMenu,
                Width(80),
                Height(height))) {
                container.SearchPattern = string.Empty;
            }

            Space(10);

            if (ConfigEditingTracker.UnsavedChanges) {
                BuilderClass.Alert(
                    "Click Save to export changes, and send changes to bots if in-game",
                    "YOU HAVE UNSAVED CHANGES!",
                    height, ColorNames.DarkRed);
            }
            else {
                BuilderClass.Alert(null, null, height, null);
            }

            Saved = Button(
                "Save and Export",
                ConfigEditingTracker.GetUnsavedValuesString(),
                EUISoundType.InsuranceInsured,
                Height(height));

            EndHorizontal();

            container.Scroll = BeginScrollView(container.Scroll);

            CategoryOpenable(container.Categories, settings, out wasEdited, container.SearchPattern);

            EndScrollView();
        }

        public static bool CheckIfOpen(SettingsContainer container, float height = 30f)
        {
            BeginHorizontal();
            container.Open = BuilderClass.ExpandableMenu(container.Name, container.Open, null, height);
            if (Button("Clear", "Clear Selected Options in this Menu",
                EFT.UI.EUISoundType.MenuDropdownSelect,
                Width(100), Height(height))) {
                container.SelectedCategories.Clear();
                foreach (var category in container.Categories) {
                    category.SelectedList.Clear();
                }
            }
            EndHorizontal();
            return container.Open;
        }

        public static bool WasEdited;

        private static void CategoryOpenable(List<Category> categories, object settingsObject, out bool wasEdited, string search = null)
        {
            wasEdited = false;
            foreach (var categoryClass in categories) {
                if (categoryClass.OptionCount(out int notUsed) == 0) {
                    continue;
                }

                var attributes = categoryClass.CategoryInfo;
                object categoryObject = categoryClass.GetValue(settingsObject);

                BeginHorizontal(30);

                bool open = true;
                if (string.IsNullOrEmpty(search)) {
                    categoryClass.Open = BuilderClass.ExpandableMenu(
                        attributes.Name, categoryClass.Open, attributes.Description, EntryConfig.EntryHeight);
                    open = categoryClass.Open;
                }
                else {
                    Box(attributes.Name, attributes.Description, Height(PresetHandler.EditorDefaults.ConfigEntryHeight));
                }

                EndHorizontal(30);

                if (open) {
                    AttributesGUI.EditAllValuesInObj(categoryClass, categoryObject, out bool newEdit, search);
                    if (newEdit) {
                        wasEdited = true;
                    }
                }
            }
        }

        private static readonly GUIEntryConfig EntryConfig = new GUIEntryConfig();
    }
}