using UnityEngine;
using static SAIN.Editor.Sounds;
using static SAIN.Editor.EditorParameters;
using static SAIN.Editor.ConfigValues;
using static SAIN.Editor.Styles;
using static SAIN.Editor.ToolTips;
using static SAIN.Editor.BuilderUtil;
using Colors = SAIN.Editor.Util.Colors;
using BepInEx.Configuration;

namespace SAIN.Editor
{
    internal class Buttons
    {
        public const float Height = 25f;

        private static void SetBackground(bool value, Color? on = null, Color? off = null)
        {
            Color? color = value ? on ?? Color.red : off ?? Color.gray;
            GUI.backgroundColor = color.Value;
        }

        private const float InfoWidth = 25f;

        public static void InfoBox(string description)
        {
            GUILayout.Box("?", Height(Height), Width(InfoWidth));
            CheckMouse(description);
        }

        public static bool ButtonConfigEntry(ConfigEntry<bool> entry, string name, string description = null)
        {
            InfoBox(description);
            GUILayout.Box(name, GUILayout.Height(Height));
            GUILayout.Space(25);
            entry.Value = Button(null, entry.Value, Height);
            GUILayout.Space(25);
            ResetButton(entry);
            return entry.Value;
        }

        public static bool ButtonProperty(SAINProperty<bool> entry)
        {
            InfoBox(entry.Description);
            GUILayout.Box(entry.Name, GUILayout.Height(Height));
            GUILayout.Space(25);
            entry.Value = Button(null, entry.Value, Height);
            GUILayout.Space(25);
            ResetButton(entry);
            return entry.Value;
        }

        private const float ResetWidth = 60f;

        public static void ResetButton<T>(ConfigEntry<T> entry)
        {
            if (GUILayout.Button("Reset", GUILayout.Width(ResetWidth), GUILayout.Height(Height)))
            {
                MenuClickSound();
                DefaultValue(entry);
            }
        }
        public static void ResetButton<T>(SAINProperty<T> entry)
        {
            if (GUILayout.Button("Reset", GUILayout.Width(ResetWidth), GUILayout.Height(Height)))
            {
                MenuClickSound();
                DefaultValue(entry);
            }
        }

        public static bool Button(string name = null, bool? value = null, float? height = null, float? width = null)
        {
            Texture2D old = GUI.skin.button.normal.background;
            if (value == true)
            {
                //GUI.skin.button.normal.background = Colors.TextureDarkRed;
            }
            name = name ?? ToggleOnOff(value.Value);

            bool button;
            if (height != null && width != null)
            {
                button = GUILayout.Button(name, Height(height), Width(width));
            }
            else if (height != null)
            {
                button = GUILayout.Button(name, Height(height));
            }
            else if (width != null)
            {
                button = GUILayout.Button(name, Width(width));
            }
            else
            {
                button = GUILayout.Button(name);
            }
            GUI.skin.button.normal.background = old;
            if (value != null)
            {
                if (button)
                {
                    MenuClickSound();
                    value = !value.Value;
                }
                return value.Value;
            }
            return button;
        }

        public static string ToggleOnOff(bool value)
        {
            return Toggle(value, "On", "Off");
        }
        public static string ToggleSelected(bool value)
        {
            return Toggle(value, "Selected", " ");
        }
        public static string ToggleEnabledDisabled(bool value)
        {
            return Toggle(value, "Enabled", "Disabled");
        }
        public static string ToggleTrueFalse(bool value)
        {
            return Toggle(value, "True", "False");
        }
        public static string Toggle(bool value, string on, string off)
        {
            return value ? on : off;
        }
        public static void SingleTextBool(string text, bool value)
        {
            Color old = GUI.backgroundColor;
            SetBackground(value);
            string status = value ? ": Detected" : ": Not Detected";
            GUILayout.Box(text + status);
            GUI.backgroundColor = old;
        }
    }
}
