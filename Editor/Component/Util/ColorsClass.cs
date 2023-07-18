using Newtonsoft.Json;
using SAIN.Editor.Abstract;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static SAIN.Editor.Names.ColorNames;

namespace SAIN.Editor.Util
{
    public class ColorsClass : EditorAbstract
    {
        public ColorsClass(SAINEditor editor) : base(editor)
        {
            BaseColorScheme = JsonUtility.Load.LoadColorScheme();
        }

        public Dictionary<string, ColorWrapper> ColorScheme => BaseColorScheme.ColorSchemeDictionary;

        private readonly BaseColorSchemeClass BaseColorScheme;

        public Color GetColor(string name)
        {
            return BaseColorScheme.GetColor(name);
        }

        public void AddorUpdateColorScheme(string name, float R, float G, float B)
        {
            AddorUpdateColorScheme(name, new Color(R, G, B));
        }

        public void AddorUpdateColorScheme(string name, Color color, bool overwrite = false)
        {
            BaseColorScheme.AddorUpdateColorScheme(name, color, overwrite);
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
            ColorSchemeDictionary = new Dictionary<string, ColorWrapper>();
            CustomColorsDictionary = new Dictionary<string, ColorWrapper>();

            AddorUpdateColorScheme(LightRed, new Color(0.8f, 0.35f, 0.35f));
            AddorUpdateColorScheme(MidRed, new Color(0.7f, 0.25f, 0.25f));
            AddorUpdateColorScheme(DarkRed, new Color(0.6f, 0.15f, 0.15f));
            AddorUpdateColorScheme(VeryDarkRed, new Color(0.8f, 0.35f, 0.35f));

            AddorUpdateColorScheme(LightGray, Gray(0.3f));
            AddorUpdateColorScheme(MidGray, Gray(0.2f));
            AddorUpdateColorScheme(DarkGray, Gray(0.1f));
            AddorUpdateColorScheme(VeryDarkGray, Gray(0.05f));
            AddorUpdateColorScheme(VeryVeryDarkGray, Gray(0.025f));

            AddorUpdateColorScheme(Black, Color.black);

            AddorUpdateColorScheme(LightBlue, new Color(0.4f, 0.4f, 0.9f));
            AddorUpdateColorScheme(MidBlue, new Color(0.3f, 0.3f, 0.8f));
            AddorUpdateColorScheme(DarkBlue, new Color(0.2f, 0.2f, 0.6f));
            AddorUpdateColorScheme(VeryDarkBlue, new Color(0.1f, 0.1f, 0.5f));

            AddorUpdateColorScheme(Gold, new Color(1f, 0.85f, 0f));

            BaseColorSchemeDictionary = ColorSchemeDictionary;
        }

        [JsonProperty]
        public readonly string SchemeName;

        [JsonProperty]
        public readonly Dictionary<string, ColorWrapper> ColorSchemeDictionary;

        [JsonProperty]
        public readonly Dictionary<string, ColorWrapper> CustomColorsDictionary;

        public readonly Dictionary<string, ColorWrapper> BaseColorSchemeDictionary;

        public Color GetColor(string name)
        {
            return GetColorFromDictionary(name, ColorSchemeDictionary);
        }

        public Color GetBaseColor(string name)
        {
            return GetColorFromDictionary(name, BaseColorSchemeDictionary);
        }

        public Color GetCustom(string name)
        {
            return GetColorFromDictionary(name, CustomColorsDictionary);
        }

        private Color GetColorFromDictionary(string name, Dictionary<string, ColorWrapper> dictionary)
        {
            bool dictNull = dictionary == null;
            bool inDict = dictionary?.ContainsKey(name) == true;
            if (!dictNull && inDict)
            {
                return dictionary[name].Color;
            }
            Logger.LogWarning($"Color Key Exists?: [{inDict}] || Dictionary == null?: [{dictNull}]", GetType(), true);
            return Color.green;
        }

        public void AddorUpdateDictionary(string name, Color color, Dictionary<string, ColorWrapper> dictionary, bool overwrite = false)
        {
            if (!dictionary.ContainsKey(name))
            {
                dictionary.Add(name, new ColorWrapper(name, color));
            }
            else if (overwrite)
            {
                dictionary[name] = new ColorWrapper(name, color);
            }
            else
            {
                name += Guid.NewGuid().ToString();
                Log("Color Key already Exists");
                if (!dictionary.ContainsKey(name))
                {
                    Log("Adding: " + name);
                    dictionary.Add(name, new ColorWrapper(name, color));
                }
            }
        }

        public void AddorUpdateColorScheme(string name, Color color, bool overwrite = false)
        {
            AddorUpdateDictionary(name, color, ColorSchemeDictionary, overwrite);
        }

        public void AddorUpdateCustom(string name, Color color, bool overwrite = false)
        {
            AddorUpdateDictionary(name, color, CustomColorsDictionary, overwrite);
        }

        public void AddorUpdateColorScheme(string name, float R, float G, float B, bool overwrite = false)
        {
            AddorUpdateColorScheme(name, new Color(R, G, B), overwrite);
        }

        private static void Log(string message)
        {
            Logger.LogDebug(message, typeof(BaseColorSchemeClass));
        }

        private static Color Gray(float brightness)
        {
            return new Color(brightness, brightness, brightness);
        }
    }

    [Serializable]
    public class ColorWrapper
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public ColorWrapper(string name, Color color)
        {
            Name = name;
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
            Color = color;
        }

        public string Name { get; private set; }

        [JsonConstructor]
        public ColorWrapper()
        {
            Color = new Color(r, g, b, a);
        }

        public Color Color { get; private set; }
    }

    public class ColorNames
    {
        [JsonConstructor]
        public ColorNames()
        {
        }

        public ColorNames(string schemeName)
        {
            SchemeName = schemeName;
            SetNames();
        }

        private void SetNames()
        {
            LightRed = nameof(LightRed);
            MidRed = nameof(MidRed);
            DarkRed = nameof(DarkRed);
            VeryDarkRed = nameof(VeryDarkRed);
            LightGray = nameof(LightGray);
            MidGray = nameof(MidGray);
            DarkGray = nameof(DarkGray);
            VeryDarkGray = nameof(VeryDarkGray);
            VeryVeryDarkGray = nameof(VeryVeryDarkGray);
            Black = nameof(Black);
            LightBlue = nameof(LightBlue);
            MidBlue = nameof(MidBlue);
            DarkBlue = nameof(DarkBlue);
            VeryDarkBlue = nameof(VeryDarkBlue);
            Gold = nameof(Gold);
        }

        [JsonProperty]
        public readonly string SchemeName;

        [JsonProperty]
        public static string LightRed { get; private set; }
        [JsonProperty]
        public static string MidRed { get; private set; }
        [JsonProperty]
        public static string DarkRed { get; private set; }
        [JsonProperty]
        public static string VeryDarkRed { get; private set; }
        [JsonProperty]
        public static string LightGray { get; private set; }
        [JsonProperty]
        public static string MidGray { get; private set; }
        [JsonProperty]
        public static string DarkGray { get; private set; }
        [JsonProperty]
        public static string VeryDarkGray { get; private set; }
        [JsonProperty]
        public static string VeryVeryDarkGray { get; private set; }
        [JsonProperty]
        public static string Black { get; private set; }
        [JsonProperty]
        public static string LightBlue { get; private set; }
        [JsonProperty]
        public static string MidBlue { get; private set; }
        [JsonProperty]
        public static string DarkBlue { get; private set; }
        [JsonProperty]
        public static string VeryDarkBlue { get; private set; }
        [JsonProperty]
        public static string Gold { get; private set; }
    }
}