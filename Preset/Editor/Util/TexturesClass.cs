using SAIN.Editor.Util;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Editor;

public static class TexturesClass
{
    public static void CreateCache()
    {
        if (ColorTextures.Count == 0)
        {
            foreach (var color in ColorsClass.ColorSchemeDictionary)
            {
                if (color.Key == ColorNames.Clear)
                {
                    ColorTextures.Add(color.Key.ToString(), null);
                }
                else
                {
                    ColorTextures.Add(color.Key.ToString(), NewTexture(color.Value));
                }
            }
            foreach (var color in ColorsClass.GrayColorScheme)
            {
                ColorTextures.Add(color.Key.ToString(), NewTexture(color.Value));
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

    private static readonly Dictionary<string, Texture2D> RandomColors = new();

    public static Texture2D GetTexture<T>(T name)
    {
        if (ColorTextures.TryGetValue(name.ToString(), out var texture))
        {
            return texture;
        }
        return Texture2D.redTexture;
    }

    public static Texture2D GetCustom(ColorNames name)
    {
        if (CustomTextures.TryGetValue(name.ToString(), out var texture))
        {
            return texture;
        }
        return Texture2D.redTexture;
    }

    public static readonly Dictionary<string, Texture2D> ColorTextures = new();

    public static readonly Dictionary<string, Texture2D> CustomTextures = new();

    public static Texture2D NewTexture(Color color, int width = 2, int height = 2)
    {
        Texture2D texture = new(width, height);
        Color[] colorApply = new Color[texture.width * texture.height];
        for (int i = 0; i < colorApply.Length; i++)
        {
            colorApply[i] = color;
        }
        texture.SetPixels(colorApply);
        texture.Apply();
        return texture;
    }

    public static Rect DrawSliderBackGrounds(float progressRatio, Rect lastRect)
    {
        float lineHeight = 5f;
        bool mouseHoveringInSlider = MouseFunctions.IsMouseInside(lastRect);
        drawTexture(lastRect, mouseHoveringInSlider ? EGraynessLevel.Dark : EGraynessLevel.Darker, null);

        Rect background = lastRect;
        background.height = lineHeight;
        background.center = lastRect.center;
        drawTexture(background, mouseHoveringInSlider ? EGraynessLevel.Mid : EGraynessLevel.Dark, null);

        Rect filledPortion = lastRect;
        filledPortion.height = lineHeight * 2f;
        filledPortion.center = lastRect.center;
        filledPortion.width = Mathf.Lerp(0f, lastRect.width, progressRatio);
        drawTexture(filledPortion, null, mouseHoveringInSlider ? ColorNames.MidRed : ColorNames.DarkRed);

        drawSliderThumb(filledPortion, lastRect, mouseHoveringInSlider, progressRatio);
        return lastRect;
    }

    private static void drawSliderThumb(Rect Filled, Rect lastRect, bool mouseInsideSlider, float progressRatio)
    {
        Rect thumbRect = lastRect;
        thumbRect.width = 12;
        thumbRect.x = Mathf.Lerp(lastRect.x, lastRect.x + lastRect.width, progressRatio);
        if (thumbRect.x + thumbRect.width > lastRect.x + lastRect.width)
        {
            thumbRect.x = lastRect.x + lastRect.width - thumbRect.width;
        }
        getThumbColor(mouseInsideSlider, thumbRect, out EGraynessLevel? gray, out ColorNames? color);
        drawTexture(thumbRect, gray, color);
    }

    private static void getThumbColor(bool mouseInSlider, Rect Thumb, out EGraynessLevel? gray, out ColorNames? color)
    {
        if (MouseFunctions.IsMouseInside(Thumb))
        {
            color = null;
            gray = EGraynessLevel.VeryLight;
            return;
        }
        if (mouseInSlider)
        {
            color = null;
            gray = EGraynessLevel.BrightMid;
            return;
        }
        color = null;
        gray = EGraynessLevel.DarkMid;
        return;
    }

    private static Color color(ColorNames name)
    {
        return ColorsClass.GetColor(name);
    }

    private static Color color(EGraynessLevel name)
    {
        return ColorsClass.GetColor(name);
    }

    private static void drawTexture(Rect rect, EGraynessLevel? gray, ColorNames? colorName)
    {
        if (gray != null)
        {
            drawTexture(rect, color(gray.Value));
            return;
        }
        if (colorName != null)
        {
            drawTexture(rect, color(colorName.Value));
            return;
        }
    }

    private static void drawTexture(Rect rect, Color color)
    {
        GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, color, 0, 0);
    }
}