using SAIN.Editor.Component;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Editor.Util
{
    public class Colors : EditorAbstract
    {
        public readonly Dictionary<string, Color> ColorScheme = new Dictionary<string, Color>();
        public readonly Dictionary<string, Color> CustomColors = new Dictionary<string, Color>();

        public Colors(GameObject go) : base(go)
        {
            Add("LightRed", 0.8f, 0.4f, 0.4f);
            Add("MidRed", 0.7f, 0.3f, 0.3f);
            Add("DarkRed", 0.6f, 0.2f, 0.2f);
            Add("VeryDarkRed", 0.5f, 0.1f, 0.1f);

            Add("LightGray", CreateGray(0.5f));
            Add("MidGray", CreateGray(0.35f));
            Add("DarkGray", CreateGray(0.2f));
            Add("VeryDarkGray", CreateGray(0.1f));

            Add("LightBlue", 0.4f, 0.4f, 0.9f);
            Add("MidBlue", 0.3f, 0.3f, 0.8f);
            Add("DarkBlue", 0.2f, 0.2f, 0.6f);
            Add("VeryDarkBlue", 0.1f, 0.1f, 0.5f);
        }

        public Color Add(string name, float R, float G, float B)
        {
            Color color = new Color(R, G, B);
            ColorScheme.Add(name, color);
            return color;
        }

        public Color Add(string name, Color color)
        {
            ColorScheme.Add(name, color);
            return color;
        }

        public Color AddCustom(string name, float R, float G, float B)
        {
            Color color = new Color(R, G, B);
            CustomColors.Add(name, color);
            return color;
        }

        public Color AddCustom(string name, Color color)
        {
            CustomColors.Add(name, color);
            return color;
        }

        public static Color CreateGray(float brightness)
        {
            return new Color(brightness, brightness, brightness);
        }
    }
}