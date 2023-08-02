using System.Collections.Generic;
using UnityEngine;
using static SAIN.Editor.Names.ColorNames;
using static SAIN.Editor.Names.StyleNames;
using Color = UnityEngine.Color;
using static SAIN.Editor.Util.ApplyToStyle;
using SAIN.Helpers;
using System;
using System.Collections;
using SAIN.Editor.Abstract;

namespace SAIN.Editor
{
    public class StyleOptions : EditorAbstract
    {
        public StyleOptions(SAINEditor editor) : base(editor)
        {
            CustomStyle = new CustomStyleClass(editor);
        }

        public Font CustomFont { get; private set; }
        public CustomStyleClass CustomStyle { get; private set; }

        public void ColorEditorMenu()
        {
            GUILayout.BeginVertical();
            Red = Builder.HorizSlider("Red", Red, 0f, 1f, 100f);
            Green = Builder.HorizSlider("Blue", Green, 0f, 1f, 100f);
            Blue = Builder.HorizSlider("Blue", Blue, 0f, 1f, 100f);
            Alpha = Builder.HorizSlider("Alpha", Alpha, 0f, 1f, 100f);

            if (Button("Save"))
            {
                CreateColor(Red, Green, Blue, Alpha);
            }
            if (AvailableColors.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                int count = 0;
                for (int i = 0; i < AvailableColors.Count; i++)
                {
                    count++;
                    if (count == 2)
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        count = 0;
                    }
                    Color color = AvailableColors[i];
                    var texture = Helpers.Textures.ModifyTexture.CreateTextureWithBorder(2, 2, 0, color);
                    GUIStyle style = new GUIStyle(GUI.skin.box);
                    style.normal.background = texture;
                    style.onNormal.background = texture;
                    style.onHover.background = texture;
                    style.hover.background = texture;
                    GUILayout.Box(color.ToString(), style, GUILayout.Width(50));
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        public GUIStyle GetFontStyleDynamic(string key, bool active)
        {
            return CustomStyle.GetFontStyleDynamic(key, active);
        }

        private static float Red = 0;
        private static float Green = 0;
        private static float Blue = 0;
        private static float Alpha = 1f;

        public static void CreateColor(float r, float g, float b, float a = 1f)
        {
            Color color = new Color(r, g, b, a);
            if (!AvailableColors.Contains(color))
            {
                AvailableColors.Add(color);
            }
        }

        public static List<Color> AvailableColors = new List<Color>();
    }

    public class CustomStyleClass
    {
        public CustomStyleClass(SAINEditor editor)
        {
            Editor = editor;
        }

        bool CacheCleared = true;

        public void Cache(bool value)
        {
            if (value)
            {
                CreateCache();
            }
            else
            {
                ClearCache();
            }
        }

        void ClearCache()
        {
            if (!CacheCleared)
            {
                StyleDictionary.Clear();
                CacheCleared = true;
            }
        }

        void CreateCache()
        {
            if (CacheCleared)
            {
                CreateStyles();
                SetTextSettings();
                CacheCleared = false;
            }
        }

        private readonly Dictionary<string, GUIStyle> StyleDictionary = new Dictionary<string, GUIStyle>();
        private readonly SAINEditor Editor;

        public GUIStyle GetStyle(string key)
        {
            if (StyleDictionary.ContainsKey(key))
            {
                return StyleDictionary[key];
            }
            else
            {
                StyleDictionary.Add(key, new GUIStyle());
                return StyleDictionary[key];
            }
        }

        public GUIStyle GetFontStyleDynamic(string key, bool active)
        {
            var style = GetStyle(key);
            style.fontStyle = active ? FontStyle.Bold : FontStyle.Normal;
            style.alignment = TextAnchor.LowerLeft;
            return style;
        }

        public void SaveStyle(string key, GUIStyle style)
        {
            if (StyleDictionary.ContainsKey(key))
            {
                StyleDictionary[key] = style;
            }
            else
            {
                StyleDictionary.Add(key, style);
            }
        }

        private void CreateStyles()
        {
            GUIStyle ButtonStyle = new GUIStyle(GUI.skin.button);
            GUIStyle BoxStyle = new GUIStyle(GUI.skin.box);
            GUIStyle ToggleStyle = new GUIStyle(GUI.skin.toggle);
            GUIStyle TextAreaStyle = new GUIStyle(GUI.skin.textArea);
            GUIStyle TextFieldStyle = new GUIStyle(GUI.skin.textField);
            GUIStyle WindowStyle = new GUIStyle(GUI.skin.window);
            GUIStyle VerticalScrollbarDownButtonStyle = new GUIStyle(GUI.skin.verticalScrollbarDownButton);
            GUIStyle VerticalScrollbarStyle = new GUIStyle(GUI.skin.verticalScrollbar);
            GUIStyle VerticalScrollbarThumbStyle = new GUIStyle(GUI.skin.verticalScrollbarThumb);
            GUIStyle VerticalScrollbarUpButtonStyle = new GUIStyle(GUI.skin.verticalScrollbarUpButton);
            GUIStyle LabelStyle = new GUIStyle(GUI.skin.label);
            GUIStyle HorizontalSliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
            GUIStyle HorizontalSliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
            GUIStyle VerticalSliderStyle = new GUIStyle(GUI.skin.verticalSlider);
            GUIStyle VerticalSliderThumbStyle = new GUIStyle(GUI.skin.verticalSliderThumb);
            GUIStyle ListStyle = new GUIStyle(GUI.skin.toggle);
            GUIStyle ToolTipStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(6, 6, 6, 6),
                border = new RectOffset(2, 2, 2, 2),
                wordWrap = true,
                clipping = TextClipping.Clip,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Normal
            };
            GUIStyle BlankBackgroundStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            BGAllStates(BlankBackgroundStyle, null);
            ApplyTextColorAllStates(BlankBackgroundStyle, Color.white, Color.white);

            var TexMidGray = GetColor(MidGray);
            var TexDarkGray = GetColor(DarkGray);
            var TexVeryDarkGray = GetColor(VeryDarkGray);
            var TexMidRed = GetColor(MidRed);
            var TexDarkRed = GetColor(DarkRed);

            BGAllStates(ToolTipStyle, TexVeryDarkGray);
            ApplyTextColorAllStates(ToolTipStyle, Color.white, Color.white);

            BGNormal(ListStyle, TexMidGray, TexDarkRed);
            BGActive(ListStyle, TexMidGray, TexDarkRed);
            BGHover(ListStyle, TexMidGray, TexDarkRed);
            BGFocused(ListStyle, TexMidGray, TexDarkRed);

            BGNormal(ToggleStyle, ButtonStyle, TexMidGray, TexDarkRed);
            BGActive(ToggleStyle, ButtonStyle, TexMidRed, TexDarkRed);
            BGHover(ToggleStyle, ButtonStyle, TexMidRed, TexDarkRed);
            BGFocused(ToggleStyle, ButtonStyle, TexMidRed, TexDarkRed);

            BGNormal(TextFieldStyle, TextAreaStyle, TexMidGray, TexMidRed);
            BGActive(TextFieldStyle, TextAreaStyle, TexDarkGray, TexMidRed);
            BGHover(TextFieldStyle, TextAreaStyle, TexDarkGray, TexMidRed);
            BGFocused(TextFieldStyle, TextAreaStyle, TexDarkGray, TexMidRed);

            BGAllStates(HorizontalSliderStyle, null);
            BGAllStates(HorizontalSliderThumbStyle, null);
            BGAllStates(WindowStyle, TexVeryDarkGray);
            BGAllStates(VerticalScrollbarStyle, TexDarkGray);
            BGAllStates(VerticalScrollbarThumbStyle, TexDarkRed);
            BGAllStates(VerticalScrollbarUpButtonStyle, TexDarkGray);
            BGAllStates(VerticalScrollbarDownButtonStyle, TexDarkGray);
            BGAllStates(BoxStyle, TexDarkGray);
            BGAllStates(LabelStyle, TexDarkGray);

            var offset = new RectOffset(10, 10, 1, 1);
            var selectGridStyle = new GUIStyle(ToggleStyle)
            {
                padding = offset,
                border = offset
            };

            SaveStyle(selectionGrid, selectGridStyle);
            SaveStyle(horizontalSliderThumb, HorizontalSliderThumbStyle);
            SaveStyle(button, ButtonStyle);
            SaveStyle(box, BoxStyle);
            SaveStyle(toggle, ToggleStyle);
            SaveStyle(textField, TextFieldStyle);
            SaveStyle(textArea, TextAreaStyle);
            SaveStyle(window, WindowStyle);
            SaveStyle(verticalScrollbarUpButton, VerticalScrollbarUpButtonStyle);
            SaveStyle(verticalScrollbarThumb, VerticalScrollbarThumbStyle);
            SaveStyle(verticalScrollbar, VerticalScrollbarStyle);
            SaveStyle(verticalScrollbarDownButton, VerticalScrollbarDownButtonStyle);
            SaveStyle(horizontalSlider, HorizontalSliderStyle);
            SaveStyle(label, LabelStyle);
            SaveStyle(list, ListStyle);
            SaveStyle(verticalSlider, VerticalSliderStyle);
            SaveStyle(verticalSliderThumb, VerticalSliderThumbStyle);
            SaveStyle(blankbox, BlankBackgroundStyle);
            SaveStyle(tooltip, ToolTipStyle);
        }

        private Texture2D GetColor(string key)
        {
            return Editor.TexturesClass.GetColor(key);
        }

        private void StyleTextAndSave(string key, Color color, Color? active = null, TextAnchor? anchor = null, FontStyle? fontStyle = null)
        {
            GUIStyle style = GetStyle(key);
            if (anchor != null)
            {
                style.alignment = anchor.Value;
            }
            if (fontStyle != null)
            {
                style.fontStyle = fontStyle.Value;
            }
            ApplyTextColorAllStates(style, color, active);
        }

        private void SetTextSettings()
        {
            var white = Color.white;
            StyleTextAndSave(box, white, white, TextAnchor.MiddleCenter, FontStyle.Bold);
            StyleTextAndSave(label, white, white, TextAnchor.MiddleLeft, FontStyle.Normal);

            Color ColorGold = Editor.Colors.GetColor(Gold);
            StyleTextAndSave(list, white, ColorGold, TextAnchor.MiddleLeft, FontStyle.Normal);
            StyleTextAndSave(button, white, ColorGold, TextAnchor.MiddleCenter, FontStyle.Bold);
            StyleTextAndSave(toggle, white, ColorGold, TextAnchor.MiddleCenter, FontStyle.Bold);
            StyleTextAndSave(textField, white, ColorGold, TextAnchor.MiddleLeft, FontStyle.Normal);
            StyleTextAndSave(textArea, white, ColorGold, TextAnchor.MiddleLeft, FontStyle.Normal);
            StyleTextAndSave(selectionGrid, white, ColorGold, TextAnchor.MiddleLeft, FontStyle.Bold);

            StyleTextAndSave(window, white);
        }
    }
}