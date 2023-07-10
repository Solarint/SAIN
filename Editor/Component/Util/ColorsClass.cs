using System.Collections.Generic;
using UnityEngine;
using static SAIN.Editor.Names.ColorNames;
using SAIN.Editor.Abstract;

namespace SAIN.Editor.Util
{
    public class ColorsClass : EditorAbstract
    {
        public readonly Dictionary<string, Color> ColorScheme = new Dictionary<string, Color>();
        public readonly Dictionary<string, Color> CustomColors = new Dictionary<string, Color>();

        public ColorsClass(GameObject go) : base(go)
        {
            AddToScheme(LightRed, 0.8f, 0.4f, 0.4f);
            AddToScheme(MidRed, 0.7f, 0.3f, 0.3f);
            AddToScheme(DarkRed, 0.6f, 0.2f, 0.2f);
            AddToScheme(VeryDarkRed, 0.5f, 0.1f, 0.1f);

            AddToScheme(LightGray, CreateGray(0.4f));
            AddToScheme(MidGray, CreateGray(0.25f));
            AddToScheme(DarkGray, CreateGray(0.15f));
            AddToScheme(VeryDarkGray, CreateGray(0.065f));

            AddToScheme(LightBlue, 0.4f, 0.4f, 0.9f);
            AddToScheme(MidBlue, 0.3f, 0.3f, 0.8f);
            AddToScheme(DarkBlue, 0.2f, 0.2f, 0.6f);
            AddToScheme(VeryDarkBlue, 0.1f, 0.1f, 0.5f);

            AddToScheme(Gold, 1f, 0.85f, 0f);
        }

        public Color GetColor(string name)
        {
            if (ColorScheme.ContainsKey(name))
            {
                return ColorScheme[name];
            }
            return Color.green;
        }

        public Color AddToScheme(string name, float R, float G, float B)
        {
            Color color = new Color(R, G, B);
            AddToScheme(name, color);
            return color;
        }

        public Color AddToScheme(string name, Color color)
        {
            ColorScheme.Add(name, color);
            return color;
        }

        public Color AddCustom(string name, float R, float G, float B)
        {
            Color color = new Color(R, G, B);
            AddCustom(name, color);
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