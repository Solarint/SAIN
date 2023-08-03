using Newtonsoft.Json;
using SAIN.Editor.Abstract;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SAIN.Editor.Util
{
    public class ColorsClass : EditorAbstract
    {
        public ColorsClass(SAINEditor editor) : base(editor)
        {
            BaseColorScheme = new BaseColorSchemeClass(nameof(SAIN));
        }

        public Dictionary<ColorNames, Color> ColorScheme => BaseColorScheme.ColorSchemeDictionary;

        private readonly BaseColorSchemeClass BaseColorScheme;

        public Color GetColor(ColorNames name)
        {
            return BaseColorScheme.GetColor(name);
        }

        public void AddorUpdateColorScheme(ColorNames name, float R, float G, float B)
        {
            AddorUpdateColorScheme(name, new Color(R, G, B));
        }

        public void AddorUpdateColorScheme(ColorNames name, Color color)
        {
            BaseColorScheme.AddorUpdateColorScheme(name, color);
        }
    }

    public class BaseColorSchemeClass
    {
        [JsonConstructor]
        public BaseColorSchemeClass()
        {
            BaseColorSchemeDictionary = ColorSchemeDictionary;
        }

        public BaseColorSchemeClass(string schemeName)
        {
            SchemeName = schemeName;
            ColorSchemeDictionary = new Dictionary<ColorNames, Color>();
            CustomColorsDictionary = new Dictionary<ColorNames, Color>();

            AddorUpdateColorScheme(ColorNames.LightRed, new Color(0.8f, 0.35f, 0.35f));
            AddorUpdateColorScheme(ColorNames.MidRed, new Color(0.7f, 0.25f, 0.25f));
            AddorUpdateColorScheme(ColorNames.DarkRed, new Color(0.6f, 0.15f, 0.15f));
            AddorUpdateColorScheme(ColorNames.VeryDarkRed, new Color(0.8f, 0.35f, 0.35f));
            AddorUpdateColorScheme(ColorNames.LightGray, Gray(0.3f));
            AddorUpdateColorScheme(ColorNames.MidGray, Gray(0.2f));
            AddorUpdateColorScheme(ColorNames.DarkGray, Gray(0.1f));
            AddorUpdateColorScheme(ColorNames.VeryDarkGray, Gray(0.05f));
            AddorUpdateColorScheme(ColorNames.VeryVeryDarkGray, Gray(0.025f));
            AddorUpdateColorScheme(ColorNames.Black, Color.black);
            AddorUpdateColorScheme(ColorNames.LightBlue, new Color(0.4f, 0.4f, 0.9f));
            AddorUpdateColorScheme(ColorNames.MidBlue, new Color(0.3f, 0.3f, 0.8f));
            AddorUpdateColorScheme(ColorNames.DarkBlue, new Color(0.2f, 0.2f, 0.6f));
            AddorUpdateColorScheme(ColorNames.VeryDarkBlue, new Color(0.1f, 0.1f, 0.5f));
            AddorUpdateColorScheme(ColorNames.Gold, new Color(1f, 0.85f, 0f));

            BaseColorSchemeDictionary = ColorSchemeDictionary;
        }

        [JsonProperty]
        public readonly string SchemeName;

        [JsonProperty]
        public readonly Dictionary<ColorNames, Color> ColorSchemeDictionary;

        [JsonProperty]
        public readonly Dictionary<ColorNames, Color> CustomColorsDictionary;

        public readonly Dictionary<ColorNames, Color> BaseColorSchemeDictionary;

        public Color GetColor(ColorNames name)
        {
            return GetColorFromDictionary(name, ColorSchemeDictionary);
        }

        public Color GetBaseColor(ColorNames name)
        {
            return GetColorFromDictionary(name, BaseColorSchemeDictionary);
        }

        public Color GetCustom(ColorNames name)
        {
            return GetColorFromDictionary(name, CustomColorsDictionary);
        }

        private Color GetColorFromDictionary(ColorNames name, Dictionary<ColorNames, Color> dictionary)
        {
            bool dictNull = dictionary == null;
            bool inDict = dictionary?.ContainsKey(name) == true;
            if (!dictNull && inDict)
            {
                return dictionary[name];
            }
            return Color.green;
        }

        public void AddorUpdateDictionary(ColorNames name, Color color, Dictionary<ColorNames, Color> dictionary)
        {
            if (!dictionary.ContainsKey(name))
            {
                dictionary.Add(name, color);
            }
            else
            {
                dictionary[name] = color;
            }
        }

        public void AddorUpdateColorScheme(ColorNames name, Color color)
        {
            AddorUpdateDictionary(name, color, ColorSchemeDictionary);
        }

        public void AddorUpdateCustom(ColorNames name, Color color)
        {
            AddorUpdateDictionary(name, color, CustomColorsDictionary);
        }

        public void AddorUpdateColorScheme(ColorNames name, float R, float G, float B)
        {
            AddorUpdateColorScheme(name, new Color(R, G, B));
        }

        private static Color Gray(float brightness)
        {
            return new Color(brightness, brightness, brightness);
        }
    }
}