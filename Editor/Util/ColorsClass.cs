using Newtonsoft.Json;
using SAIN.Editor.Abstract;
using SAIN.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SAIN.Editor.Util
{
    public class ColorsClass : EditorAbstract, IEditorCache
    {
        public ColorsClass(SAINEditor editor) : base(editor)
        {
        }

        public void CreateCache()
        {
            if (ColorSchemeDictionary.Count == 0)
            {
                AddColor(ColorNames.White, Color.white);
                AddColor(ColorNames.Black, Color.black);
                AddColor(ColorNames.Clear, Color.clear);

                AddColor(ColorNames.LightRed, new Color(0.8f, 0.35f, 0.35f));
                AddColor(ColorNames.MidRed, new Color(0.7f, 0.25f, 0.25f));
                AddColor(ColorNames.DarkRed, new Color(0.6f, 0.15f, 0.15f));
                AddColor(ColorNames.VeryDarkRed, new Color(0.8f, 0.35f, 0.35f));
                AddColor(ColorNames.LightGray, Gray(0.3f));
                AddColor(ColorNames.MidGray, Gray(0.2f));
                AddColor(ColorNames.DarkGray, Gray(0.1f));
                AddColor(ColorNames.VeryDarkGray, Gray(0.05f));
                AddColor(ColorNames.VeryVeryDarkGray, Gray(0.025f));
                AddColor(ColorNames.LightBlue, new Color(0.4f, 0.4f, 0.9f));
                AddColor(ColorNames.MidBlue, new Color(0.3f, 0.3f, 0.8f));
                AddColor(ColorNames.DarkBlue, new Color(0.2f, 0.2f, 0.6f));
                AddColor(ColorNames.VeryDarkBlue, new Color(0.1f, 0.1f, 0.5f));
                AddColor(ColorNames.Gold, new Color(1f, 0.85f, 0f));
            }
        }

        public void ClearCache()
        {
            ListHelpers.ClearCache(ColorSchemeDictionary);
        }

        public readonly string SchemeName;

        public readonly Dictionary<ColorNames, Color> ColorSchemeDictionary = new Dictionary<ColorNames, Color>();

        public Color GetColor(ColorNames name)
        {
            if (ColorSchemeDictionary.ContainsKey(name))
            {
                return ColorSchemeDictionary[name];
            }
            return Color.green;
        }

        public void AddColor(ColorNames name, Color color)
        {
            if (!ColorSchemeDictionary.ContainsKey(name))
            {
                ColorSchemeDictionary.Add(name, color);
            }
            else
            {
                ColorSchemeDictionary[name] = color;
            }
        }

        private static Color Gray(float brightness)
        {
            return new Color(brightness, brightness, brightness);
        }
    }
}