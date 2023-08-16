using SAIN.Attributes;
using SAIN.Editor.Util;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static SAIN.Editor.SAINLayout;

namespace SAIN.Editor.GUISections
{
    public static class BotSettingsEditor
    {
        public static void ShowAllSettingsGUI(object settings, out bool wasEdited)
        {
            wasEdited = false;
            BeginVertical();

            var container = SettingsContainers.GetContainer(settings.GetType());

            Space(5);
            if (CheckIfOpen(container))
            {
                Space(5);
                container.Scroll = BeginScrollView(container.Scroll);
                string search = BuilderClass.SearchBox(container);
                try
                {
                    Space(5);
                    CategoryOpenable(container.Categories, settings, out bool newEdit, search);
                    if (newEdit)
                    {
                        wasEdited = true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
                EndScrollView();
            }

            Space(5);
            EndVertical();
        }

        public static bool CheckIfOpen(SettingsContainer container)
        {
            BeginHorizontal();
            container.Open = BuilderClass.ExpandableMenu(container.Name, container.Open, null, 30, false);
            if (Button("Clear", "Clear Selected Options in this Menu",
                EFT.UI.EUISoundType.MenuDropdownSelect,
                Width(30), Height(30)))
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
                var attInfo = categoryClass.CategoryAttributes;
                object categoryObject = categoryClass.Field.GetValue(settingsObject);

                BeginHorizontal();

                if (string.IsNullOrEmpty(search))
                {
                    categoryClass.Open = BuilderClass.ExpandableMenu(attInfo.Name, categoryClass.Open, attInfo.Description, EntryConfig.EntryHeight, false);
                }
                else
                {
                    Label(attInfo.Name, attInfo.Description, Height(EntryConfig.EntryHeight));
                }

                string labelText = $"Options Count: [{categoryClass.OptionsCount}]";
                string advanced = SAINEditor.AdvancedBotConfigs ?
                    " Advanced Options are on, so the reason for this section being empty is that the values are hidden because they SHOULD NOT be changed under any circumstance."
                    : " Advanced Options in the Advanced Tab may show more options.";
                string toolTip = $"The Number of Options available in this section. {advanced}";

                Label(labelText, toolTip, Height(EntryConfig.EntryHeight), Width(150));

                if (Button("Clear", "Clear Selected Options in this Category",
                    EFT.UI.EUISoundType.MenuDropdownSelect,
                    Width(30), Height(30)))
                {
                    categoryClass.SelectedList.Clear();
                }

                EndHorizontal();

                if (categoryClass.Open)
                {
                    AttributesGUI.EditAllValuesInObj(categoryClass, categoryObject, out bool newEdit, search);
                    if (newEdit)
                    {
                        wasEdited = true;
                    }
                }
            }
        }

        private static readonly GUIEntryConfig EntryConfig = new GUIEntryConfig();
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

        public static void UpdateCache()
        {
            foreach (var container in Containers.Values) 
            { 
                container.UpdateCache(); 
            }
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
                if (!attributes.DoNotShowGUI)
                {
                    Categories.Add(new Category(attributes));
                }
            }
        }

        public void UpdateCache()
        {
            foreach (var category in Categories)
            {
                category.UpdateCache();
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
        public Category(FieldInfo field)
        {
            CategoryAttributes = new AttributesInfoClass(field);
            GetFields(CategoryAttributes.ValueType);
        }

        public Category(AttributesInfoClass attributes)
        {
            CategoryAttributes = attributes;
            GetFields(attributes.ValueType);
        }

        public void UpdateCache()
        {
            GetFields(CategoryAttributes.ValueType);
        }

        private void GetFields(Type type)
        {
            foreach (FieldInfo subField in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var attInfo = AttributesGUI.GetAttributeInfo(subField);
                if (!attInfo.DoNotShowGUI)
                {
                    FieldAttributes.Add(attInfo);
                }
            }
        }

        public FieldInfo Field => CategoryAttributes.Field;
        public readonly AttributesInfoClass CategoryAttributes;

        public readonly List<AttributesInfoClass> FieldAttributes = new List<AttributesInfoClass>();
        public readonly List<AttributesInfoClass> SelectedList = new List<AttributesInfoClass>();

        public bool Open = false;
        public Vector2 Scroll = Vector2.zero;

        public int OptionsCount => FieldAttributes.Count;
    }
}