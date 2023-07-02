using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using static SAIN.Editor.EditorGUI;
using static SAIN.Editor.EditorParameters;

namespace SAIN.Editor
{
    internal class BuilderUtil
    {
        public static void OnGUI()
        {
            if (TooltipStyle == null)
            {
                CreateStyles();
            }
            if (OpenEditWindow)
            {
                EditWindow = GUI.Window(1, EditWindow, BuilderUtil.EditorWindow, "Editor Editor");
            }
        }

        private static List<GUIStyleState> States = new List<GUIStyleState> { GUIStyle.normal};

        public static GUIStyle SliderStyle;
        public static GUIStyle ThumbStyle;

        private static void CreateStyles()
        {
            SliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
            SliderStyle.normal.background = null; // Set the background to null to hide the default background
            SliderStyle.normal.textColor = Color.clear; // Set the text color

            var Thumb = CreateTexture(16, 24, 1, Color.white, Color.red);
            var ThumbActive = CreateTexture(16, 24, 2, Color.white, Color.red);

            ThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
            ThumbStyle.normal.background = Thumb; // Set the background to a white texture for the thumb
            ThumbStyle.active.background = ThumbActive;
            ThumbStyle.hover.background = ThumbActive;
            ThumbStyle.onHover.background = ThumbActive;
            ThumbStyle.onActive.background = ThumbActive;
            ThumbStyle.focused.background = ThumbActive;
            ThumbStyle.onFocused.background = ThumbActive;

            TooltipStyle = new GUIStyle(GUI.skin.box)
            {
                wordWrap = true,
                clipping = TextClipping.Clip,
                fixedWidth = 200f // Set the maximum width of the tooltip
            };
            Texture2D tooltiptex = CreateTexture(50, 50, 3, Color.gray, Color.red);
            TooltipStyle.normal.background = tooltiptex;
            TooltipStyle.active.background = tooltiptex;
            TooltipStyle.hover.background = tooltiptex;
            TooltipStyle.onHover.background = tooltiptex;
            TooltipStyle.onActive.background = tooltiptex;
            TooltipStyle.focused.background = tooltiptex;
            TooltipStyle.onFocused.background = tooltiptex;

            TooltipStyle.normal.background = Texture2D.whiteTexture; // Set the background to a solid white texture
            TooltipStyle.normal.textColor = Color.black; // Set the text color to black

            TextStyle = new GUIStyle(GUI.skin.textField)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
        }

        private static void ApplyTextures(GUIStyle style, Texture2D normal, Texture2D active = null)
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

        private static Rect EditWindow = new Rect(50, 50, 600, 650);

        public static void DrawSlider<T>(string name, ConfigEntry<T> entry, float min, float max, float rounding = 1f, string description = "")
        {
            GUILayout.BeginHorizontal();

            TextStyle.alignment = TextAnchor.MiddleCenter;

            GUILayout.Box("?", TextStyle);
            CheckDrawToolTip(description);

            GUILayout.Space(SliderSpaceWidth.Value);
            GUILayout.Box(name, TextStyle, GUILayout.Width(SliderLabelWidth.Value));

            GUILayout.Space(SliderSpaceWidth.Value);

            BuildSlider(entry, min, max, rounding);

            GUILayout.Box(entry.Value.ToString(), TextStyle, GUILayout.Width(SliderResultWidth.Value));

            DrawToolTip();
            GUILayout.EndHorizontal();
        }

        private static void BuildSlider<T>(ConfigEntry<T> entry, float min, float max, float rounding = 1f)
        {
            var defaultStyle = TextStyle.fontStyle;
            TextStyle.fontStyle = FontStyle.Normal;

            GUILayout.Box(min.ToString(), TextStyle);
            CheckDrawToolTip("Min");

            if (typeof(T) == typeof(float))
            {
                if (entry is ConfigEntry<float> floatEntry)
                {
                    SliderTextureDraw(floatEntry, min, max);
                    //floatEntry.Value = GUILayout.HorizontalSlider(floatEntry.Value, min, max, GUILayout.Width(SliderWidth.Value));
                    floatEntry.Value = Mathf.Round(floatEntry.Value * rounding) / rounding;
                }
            }
            else if (typeof(T) == typeof(int))
            {
                if (entry is ConfigEntry<int> intEntry)
                {
                    intEntry.Value = Mathf.RoundToInt(GUILayout.HorizontalSlider(intEntry.Value, min, max, GUILayout.Width(SliderWidth.Value)));
                }
            }

            GUILayout.Box(max.ToString(), TextStyle);
            CheckDrawToolTip("Max");

            TextStyle.fontStyle = defaultStyle;
        }

        public static Rect? ToolTip;

        private static void SliderTextureDraw(ConfigEntry<float> entry, float min, float max)
        {
            float sliderValue = entry.Value;
            float progress = (sliderValue - min) / (max - min); // Calculate the progress from 0 to 1
            var sliderRect = DrawSliderBackGrounds(progress);
            sliderValue = GUI.HorizontalSlider(sliderRect, sliderValue, min, max, SliderStyle, ThumbStyle);
            entry.Value = sliderValue;
        }

        private static void SliderTextureDraw(ConfigEntry<int> entry, float min, float max)
        {
            float sliderValue = entry.Value;
            float progress = (sliderValue - min) / (max - min); // Calculate the progress from 0 to 1
            var sliderRect = DrawSliderBackGrounds(progress);
            sliderValue = GUI.HorizontalSlider(sliderRect, sliderValue, min, max, SliderStyle, ThumbStyle);
            entry.Value = Mathf.RoundToInt(sliderValue);
        }

        private static void SliderTextureDraw<T>(ConfigEntry<T> entry, float min, float max)
        {
            float sliderValue = 1f;
            if (typeof(T) == typeof(float))
            {
                if (entry is ConfigEntry<float> floatEntry)
                {
                    sliderValue = floatEntry.Value;
                }
            }
            else if (typeof(T) == typeof(int))
            {
                if (entry is ConfigEntry<int> intEntry)
                {
                    sliderValue = intEntry.Value;
                }
            }
            float progress = (sliderValue - min) / (max - min); // Calculate the progress from 0 to 1
            var sliderRect = DrawSliderBackGrounds(progress);
            sliderValue = GUI.HorizontalSlider(sliderRect, sliderValue, min, max, SliderStyle, ThumbStyle); 
            AssignValue(entry, sliderValue);
        }

        private static void AssignValue<T>(ConfigEntry<T> entry, float value)
        {
            if (entry is ConfigEntry<float> floatEntry1)
            {
                floatEntry1.Value = value;
            }
            if (entry is ConfigEntry<int> intEntry1)
            {
                intEntry1.Value = Mathf.RoundToInt(value);
            }
        }

        private static Rect DrawSliderBackGrounds(float progress)
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

        private static Texture2D CreateTexture(int width, int height, int borderSize, Color fillColor, Color borderColor)
        {
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

        private static void CheckDrawToolTip(string text)
        {
            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && Event.current.type == EventType.Repaint)
            {
                GUI.tooltip = text;
                Vector2 tooltipSize = TooltipStyle.CalcSize(new GUIContent(GUI.tooltip));
                ToolTip = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y - tooltipSize.y, tooltipSize.x, tooltipSize.y);
            }
        }

        private static void DrawToolTip()
        {
            if (ToolTip != null)
            {
                GUI.Label(ToolTip.Value, GUI.tooltip, TooltipStyle);
                ToolTip = null;
            }
        }

        private static GUIStyle TooltipStyle;

        public static void EditorWindow(int TWCWindowID)
        {
            GUI.DragWindow(new Rect(0, 0, 600, 20));
            GUILayout.BeginArea(new Rect(25, 55, 550, 500));
            GUILayout.BeginVertical();

            GUILayout.Box("Slider Style Modification");

            EditorSlider(SliderLabelWidth);
            EditorSlider(SliderSpaceWidth);
            EditorSlider(SliderWidth);
            EditorSlider(SliderMinMaxWidth);
            EditorSlider(SliderResultWidth);

            if (GUILayout.Button("Close")) OpenEditWindow = false;

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private static void EditorSlider(ConfigEntry<float> configEntry, float min = 5f, float max = 300f)
        {
            GUILayout.Box(configEntry.Definition.Key + " : " + configEntry.Value.ToString());
            configEntry.Value = GUILayout.HorizontalSlider(configEntry.Value, min, max);
            configEntry.Value = Mathf.RoundToInt(configEntry.Value);
        }

        public static bool OpenEditWindow;
    }
}