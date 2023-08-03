using UnityEngine;
using static SAIN.Editor.Sounds;
using static SAIN.Editor.ConfigValues;
using Colors = SAIN.Editor.Util.ColorsClass;
using BepInEx.Configuration;
using SAIN.BotPresets;
using static SAIN.Editor.Names.StyleNames;
using SAIN.Editor.Abstract;

namespace SAIN.Editor
{
    public class ButtonsClass : EditorAbstract
    {
        public ButtonsClass(SAINEditor editor) : base(editor) { }

        private const float InfoWidth = 25f;

        public void InfoBox(string description, float height)
        {
            InfoBox(description, Width(InfoWidth), Height(height));
        }
        public void InfoBox(string description, float height, float width)
        {
            InfoBox(description, Width(width), Height(height));
        }

        public void InfoBox(string description, params GUILayoutOption[] options)
        {
            Box("?", description, options);
        }

        public bool ButtonConfigEntry(ConfigEntry<bool> entry)
        {
            InfoBox(entry.Description.Description);

            Box(entry.Definition.Key, Width(200f));

            Space(25);

            entry.BoxedValue = CustomToggle((bool)entry.BoxedValue);

            Space(25);

            ResetButton(entry);

            return entry.Value;
        }

        private const float ResetWidth = 60f;

        public void ResetButton<T>(ConfigEntry<T> entry, float height)
        {
            if (Button("Reset", Width(ResetWidth), Height(height)))
            {
                MenuClickSound();
                DefaultValue(entry);
            }
        }
        public void ResetButton<T>(ConfigEntry<T> entry)
        {
            if (Button("Reset", Width(ResetWidth)))
            {
                MenuClickSound();
                DefaultValue(entry);
            }
        }

        public bool CustomToggle(bool value, string name = null, float? height = null, float? width = null)
        {
            bool oldValue = value;
            name = name ?? ToggleOnOff(value);

            if (width != null)
            {
                value = Toggle(value, name, Width(width.Value));
            }
            else
            {
                value = Toggle(value, name);
            }
            if (oldValue != value)
            {
                MenuClickSound();
            }
            return value;
        }

        public string ToggleOnOff(bool value)
        {
            return Toggle(value, "On", "Off");
        }
        public string ToggleSelected(bool value)
        {
            return Toggle(value, "Selected", " ");
        }
        public string ToggleEnabledDisabled(bool value)
        {
            return Toggle(value, "Enabled", "Disabled");
        }
        public string ToggleTrueFalse(bool value)
        {
            return Toggle(value, "True", "False");
        }
        public string Toggle(bool value, string on, string off)
        {
            return value ? on : off;
        }
        public void SingleTextBool(string text, bool value)
        {
            string status = value ? ": Detected" : ": Not Detected";
            Box(text + status);
        }
    }
}
