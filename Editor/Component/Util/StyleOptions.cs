using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;
using static SAIN.Editor.Util.ApplyToStyle;
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

        public GUIStyle GetFontStyleDynamic(Style key, bool active)
        {
            return CustomStyle.GetFontStyleDynamic(key, active);
        }

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

        private readonly Dictionary<Style, GUIStyle> StyleDictionary = new Dictionary<Style, GUIStyle>();
        private readonly SAINEditor Editor;

        public GUIStyle GetStyle(Style key)
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

        public GUIStyle GetFontStyleDynamic(Style key, bool active)
        {
            var style = GetStyle(key);
            style.fontStyle = active ? FontStyle.Bold : FontStyle.Normal;
            style.alignment = TextAnchor.LowerLeft;
            return style;
        }

        public void SaveStyle(Style key, GUIStyle style)
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

            var TexMidGray = GetColor(ColorNames.MidGray);
            var TexDarkGray = GetColor(ColorNames.DarkGray);
            var TexVeryDarkGray = GetColor(ColorNames.VeryDarkGray);
            var TexMidRed = GetColor(ColorNames.MidRed);
            var TexDarkRed = GetColor(ColorNames.DarkRed);

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

            LabelStyle.margin = BoxStyle.margin;
            LabelStyle.padding = BoxStyle.margin;

            SaveStyle(Style.selectionGrid, selectGridStyle);
            SaveStyle(Style.horizontalSliderThumb, HorizontalSliderThumbStyle);
            SaveStyle(Style.button, ButtonStyle);
            SaveStyle(Style.box, BoxStyle);
            SaveStyle(Style.toggle, ToggleStyle);
            SaveStyle(Style.textField, TextFieldStyle);
            SaveStyle(Style.textArea, TextAreaStyle);
            SaveStyle(Style.window, WindowStyle);
            SaveStyle(Style.verticalScrollbarUpButton, VerticalScrollbarUpButtonStyle);
            SaveStyle(Style.verticalScrollbarThumb, VerticalScrollbarThumbStyle);
            SaveStyle(Style.verticalScrollbar, VerticalScrollbarStyle);
            SaveStyle(Style.verticalScrollbarDownButton, VerticalScrollbarDownButtonStyle);
            SaveStyle(Style.horizontalSlider, HorizontalSliderStyle);
            SaveStyle(Style.label, LabelStyle);
            SaveStyle(Style.list, ListStyle);
            SaveStyle(Style.verticalSlider, VerticalSliderStyle);
            SaveStyle(Style.verticalSliderThumb, VerticalSliderThumbStyle);
            SaveStyle(Style.blankbox, BlankBackgroundStyle);
            SaveStyle(Style.tooltip, ToolTipStyle);
        }

        private Texture2D GetColor(ColorNames key)
        {
            return Editor.TexturesClass.GetColor(key);
        }

        private void StyleTextAndSave(Style key, Color color, Color? active = null, TextAnchor? anchor = null, FontStyle? fontStyle = null)
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
            StyleTextAndSave(Style.box, white, white, TextAnchor.MiddleCenter, FontStyle.Bold);
            StyleTextAndSave(Style.label, white, white, TextAnchor.MiddleLeft, FontStyle.Normal);

            Color ColorGold = Editor.Colors.GetColor(ColorNames.Gold);
            StyleTextAndSave(Style.list, white, ColorGold, TextAnchor.MiddleLeft, FontStyle.Normal);
            StyleTextAndSave(Style.button, white, ColorGold, TextAnchor.MiddleCenter, FontStyle.Bold);
            StyleTextAndSave(Style.toggle, white, ColorGold, TextAnchor.MiddleCenter, FontStyle.Bold);
            StyleTextAndSave(Style.textField, white, ColorGold, TextAnchor.MiddleLeft, FontStyle.Normal);
            StyleTextAndSave(Style.textArea, white, ColorGold, TextAnchor.MiddleLeft, FontStyle.Normal);
            StyleTextAndSave(Style.selectionGrid, white, ColorGold, TextAnchor.MiddleLeft, FontStyle.Bold);

            StyleTextAndSave(Style.window, white);
        }
    }
}