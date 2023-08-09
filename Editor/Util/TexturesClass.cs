using SAIN.Editor.Abstract;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Editor
{
    public class TexturesClass : EditorAbstract, IEditorCache
    {
        public TexturesClass(SAINEditor editor) : base(editor)
        {
        }

        public void CreateCache()
        {
            if (ColorTextures.Count == 0)
            {
                var dictionary = Editor.Colors.ColorSchemeDictionary;
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

        public void ClearCache()
        {
            if (ColorTextures.Count > 0)
            {
                ColorTextures.Clear();
            }
        }


        public Texture2D GetColor(ColorNames name)
        {
            if (ColorTextures.ContainsKey(name))
            {
                return ColorTextures[name];
            }
            return Texture2D.redTexture;
        }

        public Texture2D GetCustom(ColorNames name)
        {
            if (CustomTextures.ContainsKey(name))
            {
                return CustomTextures[name];
            }
            return Texture2D.redTexture;
        }

        public readonly Dictionary<ColorNames, Texture2D> ColorTextures = new Dictionary<ColorNames, Texture2D>();

        public readonly Dictionary<ColorNames, Texture2D> CustomTextures = new Dictionary<ColorNames, Texture2D>();

        public Texture2D NewTexture(Color color)
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

        public Rect DrawSliderBackGrounds(float progress)
        {
            Rect lastRect = GUILayoutUtility.GetLastRect();
            float lineHeight = 5f;
            float filledY = lastRect.y + (lastRect.height - lineHeight * 2f) * 0.5f;
            float sliderY = lastRect.y + (lastRect.height - lineHeight) * 0.5f;
            Rect Filled = new Rect(lastRect.x, filledY, lastRect.width * progress, lineHeight * 2f);
            Rect Background = new Rect(lastRect.x, sliderY, lastRect.width, lineHeight);

            GUI.DrawTexture(Background, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Color.white, 0, 0);
            GUI.DrawTexture(Filled, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(1f, 0.25f, 0.25f), 0, 0);
            return lastRect;
        }
    }

    public sealed class Recolor
    {
        public Recolor(float r, float g, float b, bool multiply = false)
        {
            Multiply = multiply;

            float Min = multiply ? 0f : -1f;
            float Max = multiply ? 10f : 1f;

            R = Mathf.Clamp(r, Min, Max);
            G = Mathf.Clamp(g, Min, Max);
            B = Mathf.Clamp(b, Min, Max);
        }

        public readonly float R;
        public readonly float G;
        public readonly float B;
        public readonly bool Multiply;
    }
}