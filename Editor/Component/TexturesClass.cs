using UnityEngine;
using System.IO;
using System.Collections.Generic;
using SAIN.Helpers.Textures;
using SAIN.Editor.Abstract;

namespace SAIN.Editor
{
    public class TexturesClass : EditorAbstract
    {
        public TexturesClass(SAINEditor editor) : base(editor)
        {
        }

        public Texture2D GetColor(string name)
        {
            if (ColorTextures.ContainsKey(name))
            {
                return ColorTextures[name];
            }
            return Texture2D.redTexture;
        }

        public Texture2D GetCustom(string name)
        {
            if (CustomTextures.ContainsKey(name))
            {
                return CustomTextures[name];
            }
            return Texture2D.redTexture;
        }

        public void Init()
        {
            CreateTextures();
        }

        private const int BackGroundCount = 4;
        //private readonly Recolor BackgroundRecolor = new Recolor(-0.08f, -0.18f, -0.18f);

        public readonly Dictionary<string, Texture2D> ColorTextures = new Dictionary<string, Texture2D>();

        public readonly Dictionary<string, Texture2D> CustomTextures = new Dictionary<string, Texture2D>();

        private void CreateTextures()
        {
            var dictionary = Editor.Colors.ColorScheme;
            foreach (var wrapper in dictionary.Values)
            {
                Color colorValue = wrapper.Color;
                Texture2D texture = NewTexture(colorValue);
                ColorTextures.Add(wrapper.Name, texture);
                //AddTexture(wrapper.Key, wrapper.Value, ColorTextures);
            }
        }

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

        //private const string TexturesPath = "BepInEx/plugins/SAIN/UI/Textures/";
        //private const string UIElementsFolder = TexturesPath + "UIElements";
        //private const string ColorTexturesFolder = TexturesPath + "Colors";
        //private const string CustomTexturesFolder = TexturesPath + "Custom";
        //private const string BackgroundsPath = TexturesPath + "Backgrounds/Custom";
        //private const string OriginalBackgroundPath = TexturesPath + "Backgrounds/original.png";
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
