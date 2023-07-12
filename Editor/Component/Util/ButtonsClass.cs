using UnityEngine;
using static SAIN.Editor.Sounds;
using static SAIN.Editor.ConfigValues;
using static SAIN.Editor.ToolTips;
using Colors = SAIN.Editor.Util.ColorsClass;
using BepInEx.Configuration;
using SAIN.BotPresets;
using static SAIN.Editor.Names.StyleNames;
using SAIN.Editor.Abstract;

namespace SAIN.Editor
{
    public class ButtonsClass : EditorAbstract
    {
        public ButtonsClass(GameObject gameObject) : base(gameObject) { }

        private const float InfoWidth = 35f;

        public void InfoBox(string description, bool extended = false)
        {
            Box("?", 25f, extended);
            CheckMouse(description);
        }

        public bool ButtonConfigEntry(ConfigEntry<bool> entry)
        {
            InfoBox(entry.Description.Description);

            Box(entry.Definition.Key, 200f);

            Space(25);

            entry.Value = CustomToggle(entry.Value);

            Space(25);

            ResetButton(entry);

            return entry.Value;
        }

        public bool ButtonProperty(SAINProperty<bool> entry, BotDifficulty difficulty)
        {
            bool value = (bool)entry.GetValue(difficulty);

            InfoBox(entry.Description);

            Box(entry.Name, 200f);

            Space(25);

            value = CustomToggle(value);

            Space(25);

            ResetButton(entry);

            entry.SetValue(difficulty, value);
            return value;
        }

        private const float ResetWidth = 60f;

        public void ResetButton<T>(ConfigEntry<T> entry)
        {
            if (Button("Reset", ResetWidth))
            {
                MenuClickSound();
                DefaultValue(entry);
            }
        }
        public void ResetButton<T>(SAINProperty<T> entry)
        {
            if (Button("Reset", ResetWidth))
            {
                MenuClickSound();
                if (entry is ConfigEntry<float> floatEntry)
                {
                    floatEntry.Value = (float)floatEntry.DefaultValue;
                }
                if (entry is ConfigEntry<int> intEntry)
                {
                    intEntry.Value = (int)intEntry.DefaultValue;
                }
                if (entry is ConfigEntry<bool> boolEntry)
                {
                    boolEntry.Value = (bool)boolEntry.DefaultValue;
                }
            }
        }

        public bool CustomToggle(bool value, string name = null, float? height = null, float? width = null)
        {
            bool oldValue = value;
            name = name ?? ToggleOnOff(value);

            if (width != null)
            {
                value = Toggle(value, name, width.Value);
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
