using UnityEngine;
using static SAIN.Editor.Sounds;
using static SAIN.Editor.EditorParameters;
using static SAIN.Editor.ConfigValues;
using static SAIN.Editor.Styles;
using static SAIN.Editor.ToolTips;
using static SAIN.Editor.BuilderUtil;
using BepInEx.Configuration;
using SAIN.Plugin.Config;

namespace SAIN.Editor
{
    internal class Buttons
    {
        public static void SingleTextBool(string text, bool value)
        {
            Color old = GUI.backgroundColor;
            SetBackground(value);
            string status = value ? ": Detected" : ": Not Detected";
            GUILayout.Box(text + status);
            GUI.backgroundColor = old;
        }

        private static void SetBackground(bool value, Color? on = null, Color? off = null)
        {
            Color? color = value ? on ?? Color.red : off ?? Color.gray;
            GUI.backgroundColor = color.Value;
        }

        public static void InfoBox(string description)
        {
            if (description == null)
            {
                GUILayout.Space(30);
                return;
            }
            GUILayout.Box("?", TextStyle, GUILayout.Width(20));
            CheckMouse(description);
        }

        public static bool Button(ConfigEntry<bool> entry, string name, string description = null)
        {
            InfoBox(description);
            CreateNameLabel(name);
            GUILayout.FlexibleSpace();
            entry.Value = Button(entry.Value, 420f);
            GUILayout.FlexibleSpace();
            ResetButton(entry);
            return entry.Value;
        }

        public static bool Button(SAINProperty<bool> entry)
        {
            InfoBox(entry.Description);
            CreateNameLabel(entry.Name);
            GUILayout.FlexibleSpace();
            entry.Value = Button(entry.Value, 420f);
            GUILayout.FlexibleSpace();
            ResetButton(entry);
            return entry.Value;
        }

        public static void ResetButton<T>(ConfigEntry<T> entry)
        {
            if (GUILayout.Button("Reset", GUILayout.MaxWidth(50f)))
            {
                MenuClickSound();
                DefaultValue(entry);
            }
        }
        public static void ResetButton<T>(SAINProperty<T> entry)
        {
            if (GUILayout.Button("Reset", GUILayout.MaxWidth(50f)))
            {
                MenuClickSound();
                DefaultValue(entry);
            }
        }

        public static bool Button(bool value, float width)
        {
            Color old = GUI.backgroundColor;
            SetBackground(value);
            if (GUILayout.Button(Toggle(value), GUILayout.Width(width)))
            {
                MenuClickSound();
                value = !value;
            }
            GUI.backgroundColor = old;
            return value;
        }

        public static int Button(string name, int value, int target, float width = 300f)
        {
            Color old = GUI.backgroundColor;
            SetBackground(target == value);
            if (GUILayout.Button(name, GUILayout.Width(width)))
            {
                MenuClickSound();
                value = target;
            }
            GUI.backgroundColor = old;
            return value;
        }

        public static string Toggle(bool value)
        {
            return value ? "On" : "Off";
        }
    }
}
