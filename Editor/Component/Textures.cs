using UnityEngine;
using System.IO;
using System.Collections.Generic;
using SAIN.Editor.Component;
using SAIN.Helpers.Textures;

namespace SAIN.Editor
{
    public class Textures : EditorAbstract
    {
        public Textures(GameObject editor) : base(editor)
        {
            Load.Textures(UIElementsFolder, UIElements);
            Load.Textures(ColorTexturesFolder, ColorTextures);
            Load.Textures(CustomTexturesFolder, CustomTextures);
            LoadBackgrounds();
        }

        public void Init()
        {
            CreateTextures();
            Save.Textures(ColorTexturesFolder, ColorTextures);
            Save.Textures(BackgroundsPath, "background", Backgrounds);
        }

        private const int BackGroundCount = 4;
        private readonly Recolor BackgroundRecolor = new Recolor(-0.08f, -0.18f, -0.18f);

        public Texture2D[] Backgrounds { get; private set; }

        public readonly Dictionary<string, Texture2D> UIElements = new Dictionary<string, Texture2D>();

        public readonly Dictionary<string, Texture2D> ColorTextures = new Dictionary<string, Texture2D>();

        public readonly Dictionary<string, Texture2D> CustomTextures = new Dictionary<string, Texture2D>();

        private void CreateTextures()
        {
            var colors = Editor.Colors.ColorScheme;
            foreach (var color in colors)
            {
                AddTexture(color.Key, color.Value, ColorTextures);
            }
        }

        public Texture2D AddTexture(string name, Color color, Dictionary<string, Texture2D> dict)
        {
            Texture2D texture = NewTexture(color);
            if (!dict.ContainsKey(name))
            {
                dict.Add(name, texture);
            }
            return texture;
        }

        public void AddCustom(string name, Color color)
        {
            AddTexture(name, color, CustomTextures);
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

        private void LoadBackgrounds()
        {
            Backgrounds = Load.Textures(BackgroundsPath);
            if (Backgrounds == null || Backgrounds.Length == 0)
            {
                Backgrounds = Modify.CreateNewBackgrounds(BackgroundRecolor, BackgroundsPath, BackGroundCount);
            }
        }

        private const string TexturesPath = "BepInEx/plugins/SAIN/UI/Textures/";
        private const string UIElementsFolder = TexturesPath + "UIElements";
        private const string ColorTexturesFolder = TexturesPath + "Colors";
        private const string CustomTexturesFolder = TexturesPath + "Custom";
        private const string BackgroundsPath = TexturesPath + "Backgrounds";
        private const string OriginalBackgroundPath = BackgroundsPath + "/original.png";
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
