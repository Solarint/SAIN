using SAIN.Editor.Util;
using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Editor
{
    public static class TexturesClass
    {
        public static void CreateCache()
        {
            if (ColorTextures.Count == 0)
            {
                var dictionary = ColorsClass.ColorSchemeDictionary;
                foreach (var color in dictionary)
                {
                    if (color.Key == ColorNames.Clear)
                    {
                        ColorTextures.Add(color.Key, null);
                    }
                    else
                    {
                        ColorTextures.Add(color.Key, NewTexture(color.Value));
                    }
                }
            }
        }

        public static Texture2D GetRandomGray(string key)
        {
            if (!RandomColors.ContainsKey(key))
            {
                var texture = NewTexture(ColorsClass.GetRandomColor(key));
                RandomColors.Add(key, texture);
            }
            return RandomColors[key];
        }

        private static readonly Dictionary<string, Texture2D> RandomColors = new Dictionary<string, Texture2D>();

        public static Texture2D GetTexture(ColorNames name)
        {
            if (ColorTextures.ContainsKey(name))
            {
                return ColorTextures[name];
            }
            return Texture2D.redTexture;
        }

        public static Texture2D GetCustom(ColorNames name)
        {
            if (CustomTextures.ContainsKey(name))
            {
                return CustomTextures[name];
            }
            return Texture2D.redTexture;
        }

        public static readonly Dictionary<ColorNames, Texture2D> ColorTextures = new Dictionary<ColorNames, Texture2D>();

        public static readonly Dictionary<ColorNames, Texture2D> CustomTextures = new Dictionary<ColorNames, Texture2D>();

        public static Texture2D NewTexture(Color color)
        {
            Texture2D texture = new Texture2D(2, 2);
            Color[] colorApply = new Color[texture.width * texture.height];
            for (int i = 0; i < colorApply.Length; i++)
            {
                colorApply[i] = color;
            }
            texture.SetPixels(colorApply);
            texture.Apply();
            return texture;
        }

        public static Rect DrawSliderBackGrounds(float progress, Rect lastRect)
        {
            float lineHeight = 5f;
            float filledY = lastRect.y + (lastRect.height - lineHeight * 2f) * 0.5f;
            float sliderY = lastRect.y + (lastRect.height - lineHeight) * 0.5f;
            Rect Filled = new Rect(lastRect.x, filledY, lastRect.width * progress, lineHeight * 2f);
            Rect Background = new Rect(lastRect.x, sliderY, lastRect.width, lineHeight);

            Filled.y -= 0.5f;
            Background.y -= 0.5f;

            Rect Thumb = new Rect(Filled.x + Filled.width - 4f, lastRect.y, 12, lastRect.height);
            GUI.DrawTexture(Background, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, ColorsClass.GetColor(ColorNames.LightGray), 0, 0);
            GUI.DrawTexture(Filled, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, ColorsClass.GetColor(ColorNames.LightRed), 0, 0);
            GUI.DrawTexture(Thumb, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, ColorsClass.GetColor(ColorNames.MidGray), 0, 0);
            // new Color(1f, 0.25f, 0.25f)
            return lastRect;
        }
    }
}