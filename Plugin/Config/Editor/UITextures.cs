using UnityEngine;
using static SAIN.Editor.BuilderUtil;
using static SAIN.Editor.EditorGUI;
using static SAIN.Editor.EditorParameters;

namespace SAIN.Editor
{
    internal class UITextures
    {
        public static Texture2D CreateTexture(int width, int height, int borderSize = 5, Color fillColor = default, Color borderColor = default)
        {
            fillColor = fillColor == default ? Color.white : fillColor;
            borderColor = borderColor == default ? Color.red : borderColor;

            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = texture.GetPixels();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x < borderSize || x >= width - borderSize || y < borderSize || y >= height - borderSize)
                    {
                        pixels[y * width + x] = borderColor;
                    }
                    else
                    {
                        pixels[y * width + x] = fillColor;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        public static void ApplyTextures(GUIStyle style, Texture2D normal, Texture2D active = null)
        {
            active = active ?? normal;
            style.normal.background = normal;
            style.active.background = active;
            style.hover.background = active;
            style.onHover.background = active;
            style.onActive.background = active;
            style.focused.background = active;
            style.onFocused.background = active;
        }

        public static Rect DrawSliderBackGrounds(float progress)
        {
            Rect sliderRect = GUILayoutUtility.GetRect(GUIContent.none, SliderStyle, GUILayout.Width(SliderWidth.Value));
            float lineHeight = 5f; // Adjust the line height as desired
            float filledY = sliderRect.y + (sliderRect.height - lineHeight * 2f) * 0.5f;
            float sliderY = sliderRect.y + (sliderRect.height - lineHeight) * 0.5f;
            Rect Filled = new Rect(sliderRect.x, filledY, sliderRect.width * progress, lineHeight * 2f);
            Rect Background = new Rect(sliderRect.x, sliderY, sliderRect.width, lineHeight);

            GUI.DrawTexture(Background, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Color.white, 0, 0);
            GUI.DrawTexture(Filled, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Color.red, 0, 0);
            return sliderRect;
        }
    }
}
