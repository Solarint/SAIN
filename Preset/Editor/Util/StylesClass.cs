using SAIN.Editor.Util;
using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;
using static SAIN.Editor.Util.ApplyToStyle;
using Color = UnityEngine.Color;

namespace SAIN.Editor
{
    public static class StylesClass
    {
        public static void CreateCache()
        {
            if (Styles.Count == 0) {
                CreateStyles();
            }
        }

        private static readonly Dictionary<Style, GUIStyle> Styles = new Dictionary<Style, GUIStyle>();

        public static GUIStyle GetStyle(Style key)
        {
            if (!Styles.ContainsKey(key)) {
                Styles.Add(key, new GUIStyle(GUI.skin.box));
            }
            return Styles[key];
        }

        public static GUIStyle GetFontStyleDynamic(Style key, bool active)
        {
            if (!DynamicStyles.ContainsKey(key)) {
                var originalStyle = GetStyle(key);
                var normalStyle = new GUIStyle(originalStyle) {
                    fontStyle = FontStyle.Normal,
                    alignment = TextAnchor.MiddleCenter
                };
                var activeStyle = new GUIStyle(originalStyle) {
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };

                var gold = ColorsClass.GetColor(ColorNames.Gold);
                TextColorAllStates(Color.white, normalStyle);
                TextColorAllStates(gold, activeStyle);

                DynamicStyles.Add(key,
                    new DynamicStyle {
                        Normal = normalStyle,
                        Active = activeStyle
                    }
                );
            }

            DynamicStyle dynamicStyle = DynamicStyles[key];
            return active ? dynamicStyle.Active : dynamicStyle.Normal;
        }

        private static readonly Dictionary<Style, DynamicStyle> DynamicStyles = new Dictionary<Style, DynamicStyle>();

        private static void CreateStyles()
        {
            GUIStyle LabelStyle =
                new GUIStyle(GUI.skin.label) {
                    padding = new RectOffset(4, 4, 0, 0),
                    margin = new RectOffset(4, 4, 4, 4),
                    border = new RectOffset(4, 4, 0, 0),
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Normal
                };

            GUIStyle ButtonStyle =
                new GUIStyle(GUI.skin.button) {
                    padding = LabelStyle.padding,
                    margin = LabelStyle.margin,
                    border = LabelStyle.border,
                    overflow = LabelStyle.overflow,
                    contentOffset = LabelStyle.contentOffset,
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Normal
                };

            GUIStyle BoxStyle =
                new GUIStyle(GUI.skin.box) {
                    padding = LabelStyle.padding,
                    margin = LabelStyle.margin,
                    border = LabelStyle.border,
                    overflow = LabelStyle.overflow,
                    contentOffset = LabelStyle.contentOffset,
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Normal
                };

            GUIStyle ToggleStyle =
                new GUIStyle(GUI.skin.toggle) {
                    padding = LabelStyle.padding,
                    margin = LabelStyle.margin,
                    border = LabelStyle.border,
                    overflow = LabelStyle.overflow,
                    contentOffset = LabelStyle.contentOffset,
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Normal,
                };

            GUIStyle TextAreaStyle =
                new GUIStyle(GUI.skin.textArea) {
                    padding = LabelStyle.padding,
                    margin = LabelStyle.margin,
                    border = LabelStyle.border,
                    overflow = LabelStyle.overflow,
                    contentOffset = LabelStyle.contentOffset,
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Normal,
                };

            GUIStyle TextFieldStyle =
                new GUIStyle(GUI.skin.textField) {
                    padding = LabelStyle.padding,
                    margin = LabelStyle.margin,
                    border = LabelStyle.border,
                    overflow = LabelStyle.overflow,
                    contentOffset = LabelStyle.contentOffset,
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Normal,
                };

            GUIStyle ScrollViewStyle =
                new GUIStyle(GUI.skin.scrollView) {
                    padding = LabelStyle.padding,
                    margin = LabelStyle.margin,
                    border = LabelStyle.border,
                    overflow = LabelStyle.overflow,
                    contentOffset = LabelStyle.contentOffset,
                };

            GUIStyle WindowStyle =
                new GUIStyle(GUI.skin.window);

            GUIStyle VerticalScrollbarDownButtonStyle =
                new GUIStyle(GUI.skin.verticalScrollbarDownButton);

            GUIStyle VerticalScrollbarStyle =
                new GUIStyle(GUI.skin.verticalScrollbar);

            GUIStyle VerticalScrollbarThumbStyle =
                new GUIStyle(GUI.skin.verticalScrollbarThumb);

            GUIStyle VerticalScrollbarUpButtonStyle =
                new GUIStyle(GUI.skin.verticalScrollbarUpButton);

            GUIStyle HorizontalSliderStyle =
                new GUIStyle(GUI.skin.horizontalSlider) {
                    padding = LabelStyle.padding,
                    margin = LabelStyle.margin,
                    border = LabelStyle.border,
                    overflow = LabelStyle.overflow,
                    contentOffset = LabelStyle.contentOffset,
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Normal
                };

            GUIStyle HorizontalSliderThumbStyle =
                new GUIStyle(GUI.skin.horizontalSliderThumb) {
                    padding = LabelStyle.padding,
                    margin = LabelStyle.margin,
                    border = LabelStyle.border,
                    overflow = LabelStyle.overflow,
                    contentOffset = LabelStyle.contentOffset,
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Normal
                };

            GUIStyle VerticalSliderStyle =
                new GUIStyle(GUI.skin.verticalSlider);

            GUIStyle VerticalSliderThumbStyle =
                new GUIStyle(GUI.skin.verticalSliderThumb);

            GUIStyle ListStyle =
                new GUIStyle(GUI.skin.toggle) {
                    padding = LabelStyle.padding,
                    margin = LabelStyle.margin,
                    border = LabelStyle.border,
                    overflow = LabelStyle.overflow,
                    contentOffset = LabelStyle.contentOffset,
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Normal
                };

            GUIStyle ToolTipStyle =
                new GUIStyle(GUI.skin.box) {
                    padding = new RectOffset(4, 4, 4, 4),
                    border = new RectOffset(4, 4, 4, 4),
                    wordWrap = true,
                    clipping = TextClipping.Clip,
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Normal,
                };

            GUIStyle BlankBackgroundStyle =
                new GUIStyle(LabelStyle) {
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Normal
                };

            GUIStyle AlertStyle =
                new GUIStyle(GUI.skin.box) {
                    padding = LabelStyle.padding,
                    margin = LabelStyle.margin,
                    border = LabelStyle.border,
                    overflow = LabelStyle.overflow,
                    contentOffset = LabelStyle.contentOffset,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                };

            Texture2D TexMidGray = TexturesClass.GetTexture(
                EGraynessLevel.Mid);
            Texture2D TexDarkGray = TexturesClass.GetTexture(
                EGraynessLevel.Dark);
            Texture2D TexVeryDarkGray = TexturesClass.GetTexture(
                EGraynessLevel.VeryDark);
            Texture2D TexMidRed = TexturesClass.GetTexture(
                ColorNames.MidRed);
            Texture2D TexDarkRed = TexturesClass.GetTexture(
                ColorNames.DarkRed);

            Color ColorGold = ColorsClass.GetColor(ColorNames.Gold);
            Color fadedWhite = new Color(0.9f, 0.9f, 0.9f, 0.9f);

            TextColorAllStates(fadedWhite, ColorGold,
                WindowStyle,
                ListStyle,
                ButtonStyle,
                ToggleStyle,
                TextFieldStyle,
                TextAreaStyle,
                AlertStyle
                );

            TextColorHover(Color.white,
                WindowStyle,
                ListStyle,
                ButtonStyle,
                ToggleStyle,
                TextFieldStyle,
                TextAreaStyle,
                AlertStyle
                );

            BackgroundAllStates(null, BlankBackgroundStyle);

            TextColorAllStates(
                fadedWhite,
                BlankBackgroundStyle,
                ToolTipStyle,
                BoxStyle,
                LabelStyle
                );

            TextColorHover(
                Color.white,
                BlankBackgroundStyle,
                ToolTipStyle,
                BoxStyle,
                LabelStyle
                );

            BackgroundAllStates(
                TexMidRed,
                AlertStyle
                );

            BackgroundAllStates(
                TexMidGray, TexDarkRed,
                ListStyle
                );

            GUIStyle[] ToggleStyles =
                {
                    ToggleStyle,
                    ButtonStyle,
                };

            BackgroundNormal(
                TexMidGray, TexDarkRed,
                ToggleStyles
                );

            BackgroundActive(
                TexMidRed, TexDarkRed,
                ToggleStyles
                );

            BackgroundHover(
                TexMidRed, TexDarkRed,
                ToggleStyles
                );

            BackgroundFocused(
                TexMidRed, TexDarkRed,
                ToggleStyles
                );

            BackgroundAllStates(
                TexDarkGray, TexMidGray,
                TextFieldStyle,
                TextAreaStyle
                );

            BackgroundAllStates(
                null,
                HorizontalSliderStyle,
                HorizontalSliderThumbStyle,
                ScrollViewStyle
                );

            BackgroundAllStates(
                TexVeryDarkGray,
                WindowStyle
                );

            BackgroundAllStates(
                TexDarkGray,
                VerticalScrollbarStyle,
                VerticalScrollbarUpButtonStyle,
                VerticalScrollbarDownButtonStyle,
                BoxStyle,
                LabelStyle,
                ToolTipStyle
                );

            BackgroundHover(TexturesClass.GetTexture(EGraynessLevel.DarkMid),
                VerticalScrollbarStyle,
                VerticalScrollbarUpButtonStyle,
                VerticalScrollbarDownButtonStyle,
                BoxStyle,
                LabelStyle,
                ToolTipStyle);

            BackgroundAllStates(
                TexDarkRed,
                VerticalScrollbarThumbStyle
                );

            GUIStyle selectGridStyle =
                new GUIStyle(ToggleStyle) {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Normal,
                };

            LabelStyle.margin = BoxStyle.margin;
            LabelStyle.padding = BoxStyle.margin;

            GUIStyle dragBarStyle =
                new GUIStyle(BlankBackgroundStyle) {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(10, 10, 3, 3)
                };

            GUIStyle botTypeGridStyle =
                new GUIStyle(ToggleStyle) {
                    fontStyle = FontStyle.Normal,
                    alignment = TextAnchor.MiddleLeft
                };

            GUIStyle SelectionListStyle =
                new GUIStyle(ToggleStyle) {
                    fontStyle = FontStyle.Normal,
                    alignment = TextAnchor.MiddleLeft
                };

            GUIStyle botTypeSectionStyle = new GUIStyle(ToggleStyle) {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Normal
            };

            Styles.Add(Style.botTypeSection, botTypeSectionStyle);
            Styles.Add(Style.scrollView, ScrollViewStyle);
            Styles.Add(Style.selectionList, SelectionListStyle);
            Styles.Add(Style.alert, AlertStyle);
            Styles.Add(Style.botTypeGrid, botTypeGridStyle);
            Styles.Add(Style.dragBar, dragBarStyle);
            Styles.Add(Style.selectionGrid, selectGridStyle);
            Styles.Add(Style.horizontalSliderThumb, HorizontalSliderThumbStyle);
            Styles.Add(Style.button, ButtonStyle);
            Styles.Add(Style.box, BoxStyle);
            Styles.Add(Style.toggle, ToggleStyle);
            Styles.Add(Style.textField, TextFieldStyle);
            Styles.Add(Style.textArea, TextAreaStyle);
            Styles.Add(Style.window, WindowStyle);
            Styles.Add(Style.verticalScrollbarUpButton, VerticalScrollbarUpButtonStyle);
            Styles.Add(Style.verticalScrollbarThumb, VerticalScrollbarThumbStyle);
            Styles.Add(Style.verticalScrollbar, VerticalScrollbarStyle);
            Styles.Add(Style.verticalScrollbarDownButton, VerticalScrollbarDownButtonStyle);
            Styles.Add(Style.horizontalSlider, HorizontalSliderStyle);
            Styles.Add(Style.label, LabelStyle);
            Styles.Add(Style.list, ListStyle);
            Styles.Add(Style.verticalSlider, VerticalSliderStyle);
            Styles.Add(Style.verticalSliderThumb, VerticalSliderThumbStyle);
            Styles.Add(Style.blankbox, BlankBackgroundStyle);
            Styles.Add(Style.tooltip, ToolTipStyle);

            foreach (var style in EnumValues.GetEnum<Style>()) {
                if (!Styles.ContainsKey(style)) {
                    //Logger.LogWarning(style);
                }
            }
        }

        private sealed class DynamicStyle
        {
            public GUIStyle Normal;
            public GUIStyle Active;
        }
    }
}