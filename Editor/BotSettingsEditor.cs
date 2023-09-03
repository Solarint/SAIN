using EFT.UI;
using SAIN.Attributes;
using SAIN.Editor.Util;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static SAIN.Editor.SAINLayout;

namespace SAIN.Editor.GUISections
{
    public static class BotSettingsEditor
    {
        public static void ShowAllSettingsGUI(object settings, out bool wasEdited, string name, string savePath, float height, bool unsavedChanges, out bool Saved)
        {
            BeginHorizontal();

            const float spacing = 3f;

            Box(name, Height(height));

            Space(spacing);

            string saveToolTip = $"Apply Values set below to {name}. " +
                $"Exports edited values to {savePath} folder";

            Saved = Button("Save and Export", saveToolTip, EUISoundType.InsuranceInsured, Height(height));

            const float alertWidth = 250f;

            if (unsavedChanges)
            {
                BuilderClass.Alert(
                    "Click Save to export changes, and send changes to bots if in-game",
                    "YOU HAVE UNSAVED CHANGES",
                    height, alertWidth, ColorNames.LightRed);
            }

            Space(spacing);

            Label("Search", Width(125f), Height(height));

            Space(spacing);

            var container = SettingsContainers.GetContainer(settings.GetType(), name);
            container.SearchPattern = TextField(container.SearchPattern, null, Width(250), Height(height));

            Space(spacing);

            if (Button("Clear", EUISoundType.MenuContextMenu, Width(80), Height(height)))
            {
                container.SearchPattern = string.Empty;
            }

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
                Width(100), Height(height)))
            {
                container.SelectedCategories.Clear();
                foreach (var category in container.Categories)
                {
                    category.SelectedList.Clear();
                }
            }
            EndHorizontal();
            return container.Open;
        }

        public static SettingsContainer SelectSettingsGUI(Type type, string name, out bool wasEdited)
        {
            wasEdited = false;
            var container = SettingsContainers.GetContainer(type, name);
            Space(5);
            if (CheckIfOpen(container))
            {
                Space(5);

                string search = BuilderClass.SearchBox(container);
                try
                {
                    ModifyLists.AddOrRemove(container, out bool newEdit, search);
                    if (newEdit)
                    {
                        wasEdited = true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
            }
            Space(5);
            return container;
        }

        public static bool WasEdited;

        private static void CategoryOpenable(List<Category> categories, object settingsObject, out bool wasEdited, string search = null)
        {
            wasEdited = false;
            foreach (var categoryClass in categories)
            {
                if (categoryClass.OptionCount(out int notUsed) == 0)
                {
                    continue;
                }

                var attributes = categoryClass.CategoryInfo;
                object categoryObject = categoryClass.GetValue(settingsObject);

                BeginHorizontal(30);

                bool open = true;
                if (string.IsNullOrEmpty(search))
                {
                    categoryClass.Open = BuilderClass.ExpandableMenu(
                        attributes.Name, categoryClass.Open, attributes.Description, EntryConfig.EntryHeight);
                    open = categoryClass.Open;
                }
                else
                {
                    Box(attributes.Name, attributes.Description, Height(EntryConfig.EntryHeight));
                }

                EndHorizontal(30);

                if (open)
                {
                    Space(3);
                    AttributesGUI.EditAllValuesInObj(categoryClass, categoryObject, out bool newEdit, search);
                    if (newEdit)
                    {
                        wasEdited = true;
                    }
                }
            }
        }

        private static readonly GUIEntryConfig EntryConfig = new GUIEntryConfig(30f);
    }
}

namespace SAIN.Editor
{
    public static class SettingsContainers
    {
        private static readonly Dictionary<Type, SettingsContainer> Containers = new Dictionary<Type, SettingsContainer>();

        public static SettingsContainer GetContainer(Type containerType, string name = null)
        {
            if (!Containers.ContainsKey(containerType))
            {
                Containers.Add(containerType, new SettingsContainer(containerType, name));
            }
            return Containers[containerType];
        }
    }

    public sealed class SettingsContainer
    {
        public SettingsContainer(Type settingsType, string name = null)
        {
            Name = name ?? settingsType.Name;
            foreach (FieldInfo field in settingsType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                AttributesInfoClass attributes = new AttributesInfoClass(field);
                if (!attributes.Hidden)
                {
                    var category = new Category(attributes);
                    category.OptionCount(out int realCount);
                    if (realCount > 0)
                    {
                        Categories.Add(category);
                    }
                }
            }
        }

        public readonly string Name;

        public readonly List<Category> Categories = new List<Category>();
        public readonly List<Category> SelectedCategories = new List<Category>();

        public string SearchPattern = string.Empty;

        public bool Open = false;
        public bool SecondOpen = false;
        public Vector2 Scroll = Vector2.zero;
        public Vector2 SecondScroll = Vector2.zero;
    }

    public sealed class Category
    {
        public Category(AttributesInfoClass attributes)
        {
            CategoryInfo = attributes;
            foreach (FieldInfo field in attributes.ValueType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var attInfo = AttributesGUI.GetAttributeInfo(field);
                if (attInfo != null && !attInfo.Hidden)
                {
                    FieldAttributesList.Add(attInfo);
                }
            }
        }

        public object GetValue(object obj)
        {
            return CategoryInfo.GetValue(obj);
        }

        public void SetValue(object obj, object value)
        {
            CategoryInfo.SetValue(obj, value);
        }

        public readonly AttributesInfoClass CategoryInfo;

        public readonly List<AttributesInfoClass> FieldAttributesList = new List<AttributesInfoClass>();
        public readonly List<AttributesInfoClass> SelectedList = new List<AttributesInfoClass>();

        public bool Open = false;
        public Vector2 Scroll = Vector2.zero;

        public int OptionCount(out int realCount)
        {
            realCount = 0;
            int count = 0;
            foreach (var option in FieldAttributesList)
            {
                if (!option.DoNotShowGUI)
                {
                    count++;
                }
                realCount++;
            }
            return count;
        }
    }
}