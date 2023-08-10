using EFT;
using EFT.UI;
using SAIN.Editor;
using SAIN.Helpers;
using SAIN.Preset;
using SAIN.Preset.GlobalSettings.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static Mono.Security.X509.X520;

namespace SAIN.Attributes
{
    public class AttributesGUI
    {
        public static AttributesInfoClass GetAttributeInfo(FieldInfo field)
        {
            string name = field.Name + field.DeclaringType.Name;
            if (!AttributesClasses.ContainsKey(name))
            {
                AttributesClasses.Add(name, new AttributesInfoClass(field));
            }
            return AttributesClasses[name];
        }

        public static AttributesInfoClass GetAttributeInfo(PropertyInfo property)
        {
            string name = property.Name + property.DeclaringType.Name;
            if (!AttributesClasses.ContainsKey(name))
            {
                AttributesClasses.Add(name, new AttributesInfoClass(property));
            }
            return AttributesClasses[name];
        }

        public static void ClearCache()
        {
            if (ListHelpers.ClearCache(AttributesClasses))
            {
                LabelStyle = null;
            }
        }

        private static readonly GUIEntryConfig DefaultEntryConfig = new GUIEntryConfig();

        private static GUIStyle LabelStyle;
        private static readonly Dictionary<string, AttributesInfoClass> AttributesClasses = new Dictionary<string, AttributesInfoClass>();

        public static object EditValue(object value, AttributesInfoClass attributes, GUIEntryConfig entryConfig = null)
        {
            if (value == null || attributes == null)
            {
                return value;
            }
            if (attributes.DoNotShowGUI)
            {
                return value;
            }

            if (entryConfig == null)
            {
                entryConfig = DefaultEntryConfig;
            }

            Type ValueType = attributes.ValueType;
            switch (attributes.ListTypeEnum)
            {
                case AttributeListType.None:
                    value = EditNormal(value, attributes, entryConfig);
                    break;

                case AttributeListType.List:
                    if (ExpandableList(attributes, entryConfig.EntryHeight))
                    {
                        if (ValueType == typeof(List<BotType>))
                        {
                            var list = (List<BotType>)value;
                            Builder.ModifyLists.AddOrRemove(list);
                            value = list;
                        }
                        else if (ValueType == typeof(List<WildSpawnType>))
                        {
                            var list = (List<WildSpawnType>)value;
                            Builder.ModifyLists.AddOrRemove(list);
                            value = list;
                        }
                        else if (ValueType == typeof(List<BigBrainConfigClass>))
                        {
                            var list = (List<BigBrainConfigClass>)value;
                            Builder.ModifyLists.AddOrRemove(list);
                            value = list;
                        }
                    }
                    break;

                case AttributeListType.Array:
                    break;

                case AttributeListType.Dictionary:
                    if (ValueType == typeof(Dictionary<,>))
                    {
                        break;
                    }
                    else
                    {
                        if (ExpandableList(attributes, entryConfig.EntryHeight))
                        {
                            EditAllValuesInObj(value);
                        }
                        break;
                    }

                default:
                    break;
            }
            return value;
        }

        private static object EditNormal(object value, AttributesInfoClass attributes, GUIEntryConfig entryConfig)
        {
            Builder.BeginHorizontal();

            var labelHeight = Builder.Height(entryConfig.EntryHeight);
            Buttons.InfoBox(attributes.Description, entryConfig.Info);

            if (LabelStyle == null)
            {
                GUIStyle boxstyle = Buttons.GetStyle(Style.box);
                LabelStyle = new GUIStyle(Buttons.GetStyle(Style.label))
                {
                    alignment = TextAnchor.MiddleLeft,
                    margin = boxstyle.margin,
                    padding = boxstyle.padding
                };
            }

            Builder.Box(new GUIContent(attributes.Name), LabelStyle, labelHeight);

            float min = default;
            float max = default;
            bool showResult = false;

            if (attributes.ValueType == typeof(bool))
            {
                showResult = true;

                value = Builder.Toggle((bool)value, (bool)value ? "On" : "Off", EUISoundType.MenuCheckBox, entryConfig.Toggle);
            }
            else if (attributes.ValueType == typeof(float))
            {
                showResult = true;

                if (!MinMaxFloat(out min, out max, attributes))
                {
                    min = Mathf.Round(((float)value / 10f) * 10f) / 10f;
                    max = Mathf.Round(((float)value * 5f) * 10f) / 10f;
                }

                Builder.MinValueBox(min, entryConfig.Info);

                float flValue = Builder.CreateSlider((float)value, min, max, entryConfig.Slider);
                float rounding = attributes.Rounding == null ? 10f : attributes.Rounding.Value;
                value = Mathf.Round(flValue * rounding) / rounding;

                Builder.MaxValueBox(max, entryConfig.Info);
            }
            else if (attributes.ValueType == typeof(int))
            {
                showResult = true;

                if (!MinMaxInt(out int intMin, out int intMax, attributes))
                {
                    min = Mathf.RoundToInt((int)value / 10f);
                    max = Mathf.RoundToInt((int)value * 5f);
                }
                else
                {
                    min = intMin;
                    max = intMax;
                }

                Builder.MinValueBox(min, entryConfig.Info);

                float floatvalue = Builder.CreateSlider((int)value, min, max, entryConfig.Slider);
                value = Mathf.RoundToInt(floatvalue);

                Builder.MaxValueBox(max, entryConfig.Info);
            }
            if (showResult && value != null)
            {
                string dirtyString = Builder.TextField(value.ToString(), null, entryConfig.Result);
                value = Builder.CleanString(dirtyString, value);
                value = Clamp(value, min, max, attributes.ValueType);

                if (attributes.Default != null)
                {
                    if (Builder.Button("Reset", "Reset To Default Value", EUISoundType.ButtonClick, entryConfig.Reset))
                        value = attributes.Default;
                }
                else
                {
                    Builder.Box(" ", "No Default Value is assigned to this option.", entryConfig.Reset);
                }
            }

            Builder.EndHorizontal();

            if (attributes.ValueType == typeof(int))
            {
                //value = RoundObjectToInt(value);
            }
            return value;
        }

        private static int RoundObjectToInt(object value)
        {
            return Mathf.RoundToInt((float)value);
        }

        private static bool ExpandableList(AttributesInfoClass attributes, float height)
        {
            Builder.BeginHorizontal();

            string name = attributes.Name;
            if (!ListIsOpen.ContainsKey(name))
            {
                ListIsOpen.Add(name, false);
            }
            bool isOpen = ListIsOpen[name];
            isOpen = Builder.ExpandableMenu(name, isOpen, attributes.Description, height, 30f, false);
            ListIsOpen[name] = isOpen;

            Builder.EndHorizontal();
            return isOpen;
        }

        private static readonly Dictionary<string, bool> ListIsOpen = new Dictionary<string, bool>();

        private static object Clamp(object value, float min, float max, Type valueType)
        {
            float floatVal;
            if (valueType == typeof(float))
            {
                floatVal = (float)value;
                value = Mathf.Clamp(floatVal, min, max);
            }
            if (valueType == typeof(int))
            {
                int intVal = (int)value;
                floatVal = intVal;
                value = Mathf.RoundToInt(Mathf.Clamp(floatVal, min, max));
            }
            return value;
        }

        private static bool MinMaxFloat(out float min, out float max, AttributesInfoClass attributes)
        {
            min = default;
            max = default;
            if (attributes.Min != null)
            {
                min = attributes.Min.Value;

                if (attributes.Max != null)
                {
                    max = attributes.Max.Value;
                    return true;
                }
            }
            return false;
        }

        private static bool MinMaxInt(out int min, out int max, AttributesInfoClass attributes)
        {
            min = default;
            max = default;
            if (attributes.Min != null)
            {
                min = Mathf.RoundToInt(attributes.Min.Value);

                if (attributes.Max != null)
                {
                    max = Mathf.RoundToInt(attributes.Max.Value);
                    return true;
                }
            }
            return false;
        }

        public static object EditValue(object value, FieldInfo field, GUIEntryConfig entryConfig = null)
        {
            return EditValue(value, GetAttributeInfo(field), entryConfig);
        }

        public static void EditAllValuesInObj(object obj)
        {
            Builder.BeginVertical();

            foreach (var field in obj.GetType().GetFields())
            {
                object value = field.GetValue(obj);
                object newValue = EditValue(value, field);
                if (value.ToString() != newValue.ToString())
                {
                    field.SetValue(obj, newValue);
                }
            }

            Builder.EndVertical();
        }

        public static bool HideConfigOption(PropertyInfo property)
        {
            return HideConfigOption(property.GetCustomAttribute<AdvancedAttribute>());
        }

        public static bool HideConfigOption(FieldInfo fieldInfo)
        {
            return HideConfigOption(fieldInfo.GetCustomAttribute<AdvancedAttribute>());
        }

        public static bool HideConfigOption(AdvancedAttribute advancedAttribute)
        {
            if (advancedAttribute != null)
            {
                if (advancedAttribute.Options.Contains(AdvancedEnum.Hidden))
                {
                    return true;
                }
                if (advancedAttribute.Options.Contains(AdvancedEnum.IsAdvanced) && !SAINPlugin.Editor.AdvancedOptionsEnabled)
                {
                    return true;
                }
            }
            return false;
        }

        private static ButtonsClass Buttons => SAINPlugin.Editor.Buttons;
        private static BuilderClass Builder => SAINPlugin.Editor.Builder;
    }
}