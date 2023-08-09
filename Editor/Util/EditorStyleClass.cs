using SAIN.Editor.Abstract;
using SAIN.Editor.Util;
using SAIN.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static SAIN.Editor.Util.ApplyToStyle;
using Color = UnityEngine.Color;

namespace SAIN.Editor
{
    public class EditorStyleClass : EditorAbstract, IEditorCache
    {
        public EditorStyleClass(SAINEditor editor) : base(editor)
        {
            CustomStyle = new CustomStyleClass(editor);
        }

        public void ClearCache()
        {
            CustomStyle.ClearCache();
        }

        public void CreateCache()
        {
            CustomStyle.CreateCache();
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

    public class CustomStyleClass : IEditorCache
    {
        public CustomStyleClass(SAINEditor editor)
        {
            Editor = editor;
        }

        public void ClearCache()
        {
            if (Styles.Count > 0)
            {
                Styles.Clear();
                DynamicStyles.Clear();
            }
        }

        public void CreateCache()
        {
            if (Styles.Count == 0)
            {
                CreateStyles();
                SetTextSettings();
            }
        }

        private readonly Dictionary<Style, GUIStyle> Styles = new Dictionary<Style, GUIStyle>();
        private readonly SAINEditor Editor;

        public GUIStyle GetStyle(Style key)
        {
            if (!Styles.ContainsKey(key))
            {
                Styles.Add(key, new GUIStyle(GUI.skin.box));
            }
            return Styles[key];
        }

        public GUIStyle GetFontStyleDynamic(Style key, bool active)
        {
            if (!DynamicStyles.ContainsKey(key))
            {
                var originalStyle = GetStyle(key);
                var normalStyle = new GUIStyle(originalStyle)
                {
                    fontStyle = FontStyle.Normal,
                    alignment = TextAnchor.LowerLeft
                };
                var activeStyle = new GUIStyle(originalStyle)
                {
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.LowerLeft
                };

                DynamicStyles.Add(key,
                    new DynamicStyle
                    {
                        Normal = normalStyle,
                        Active = activeStyle
                    }
                );
            }

            DynamicStyle dynamicStyle = DynamicStyles[key];
            return active ? dynamicStyle.Active : dynamicStyle.Normal;
        }

        private readonly Dictionary<Style, DynamicStyle> DynamicStyles = new Dictionary<Style, DynamicStyle>();

        public sealed class DynamicStyle
        {
            public GUIStyle Normal;
            public GUIStyle Active;
        }

        public sealed class EditorStyleClass
        {
            static EditorStyleClass()
            {
                // Get all style states in the GUIStyle class and their property info
                foreach (var (property, state) in from property in (from property in typeof(GUIStyle).GetProperties() where property.PropertyType == typeof(GUIStyleState) select property).ToList()
                                                  from StyleStates state in EnumValues.GetEnum<StyleStates>()
                                                  where property.Name == state.ToString()
                                                  select (property, state))
                {
                    StateProperties.Add(state, property);
                }
            }

            public EditorStyleClass(Dictionary<StyleStates, StyleStateColors> colorNames)
            {
                ColorNames = colorNames;
            }

            public GUIStyle Create(GUIStyle original)
            {
                GUIStyle result = new GUIStyle(original)
                {
                    border = border ?? original.border,
                    padding = padding ?? original.padding,

                    fontStyle = fontStyle != null ? fontStyle.Value : original.fontStyle,
                    alignment = alignment != null ? alignment.Value : original.alignment,
                    clipping = clipping != null ? clipping.Value : original.clipping,
                    wordWrap = wordWrap != null ? wordWrap.Value : original.wordWrap,
                };

                ApplyColors(result);
                return result;
            }

            private void ApplyColors(GUIStyle style)
            {
                foreach (var color in ColorNames)
                {
                    if (StateProperties.TryGetValue(color.Key, out var stateProperty))
                    {
                        var styleState = (GUIStyleState)stateProperty.GetValue(style);

                        styleState.background = Textures.GetColor(color.Value.Background);
                        styleState.textColor = Colors.GetColor(color.Value.Text);

                        stateProperty.SetValue(style, styleState);
                    }
                }
            }

            private static readonly Dictionary<StyleStates, PropertyInfo> StateProperties = new Dictionary<StyleStates, PropertyInfo>();

            public FontStyle? fontStyle;
            public TextAnchor? alignment;
            public TextClipping? clipping;
            public bool? wordWrap;

            public RectOffset padding;
            public RectOffset border;

            public readonly Dictionary<StyleStates, StyleStateColors> ColorNames;

            private static TexturesClass Textures => SAINPlugin.Editor.TexturesClass;
            private static ColorsClass Colors => SAINPlugin.Editor.Colors;
        }

        public sealed class StyleStateColors
        {
            public ColorNames Background = ColorNames.VeryDarkGray;
            public ColorNames Text = ColorNames.White;
        }

        private void CreateStyles()
        {
            GUIStyle ButtonStyle =
                new GUIStyle(GUI.skin.button);
            GUIStyle BoxStyle =
                new GUIStyle(GUI.skin.box);
            GUIStyle ToggleStyle =
                new GUIStyle(GUI.skin.toggle);
            GUIStyle TextAreaStyle =
                new GUIStyle(GUI.skin.textArea);
            GUIStyle TextFieldStyle =
                new GUIStyle(GUI.skin.textField);
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
            GUIStyle LabelStyle =
                new GUIStyle(GUI.skin.label);
            GUIStyle HorizontalSliderStyle =
                new GUIStyle(GUI.skin.horizontalSlider);
            GUIStyle HorizontalSliderThumbStyle =
                new GUIStyle(GUI.skin.horizontalSliderThumb);
            GUIStyle VerticalSliderStyle =
                new GUIStyle(GUI.skin.verticalSlider);
            GUIStyle VerticalSliderThumbStyle =
                new GUIStyle(GUI.skin.verticalSliderThumb);
            GUIStyle ListStyle =
                new GUIStyle(GUI.skin.toggle);
            GUIStyle ToolTipStyle =
                new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset(8, 8, 8, 8),
                    border = new RectOffset(5, 5, 5, 5),
                    wordWrap = true,
                    clipping = TextClipping.Clip,
                    alignment = TextAnchor.MiddleLeft,
                    fontStyle = FontStyle.Normal
                };
            GUIStyle BlankBackgroundStyle =
                new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };

            BGAllStates(BlankBackgroundStyle,
                null);
            ApplyTextColorAllStates(BlankBackgroundStyle,
                Color.white, Color.white);

            Texture2D TexMidGray = GetTexture(
                ColorNames.MidGray);
            Texture2D TexDarkGray = GetTexture(
                ColorNames.DarkGray);
            Texture2D TexVeryDarkGray = GetTexture(
                ColorNames.VeryDarkGray);
            Texture2D TexMidRed = GetTexture(
                ColorNames.MidRed);
            Texture2D TexDarkRed = GetTexture(
                ColorNames.DarkRed);

            ApplyTextColorAllStates(ToolTipStyle,
                Color.white, Color.white);

            BGAllStates(TexMidGray, TexDarkRed,
                ListStyle);

            BGNormal(ToggleStyle, ButtonStyle,
                TexMidGray, TexDarkRed);
            BGActive(ToggleStyle, ButtonStyle,
                TexMidRed, TexDarkRed);
            BGHover(ToggleStyle, ButtonStyle,
                TexMidRed, TexDarkRed);
            BGFocused(ToggleStyle, ButtonStyle,
                TexMidRed, TexDarkRed);

            BGAllStates(TexDarkGray, TexDarkRed,
                TextFieldStyle,
                TextAreaStyle);

            BGAllStates(null,
                HorizontalSliderStyle,
                HorizontalSliderThumbStyle);

            BGAllStates(WindowStyle,
                TexVeryDarkGray);

            BGAllStates(TexDarkGray,
                VerticalScrollbarStyle,
                VerticalScrollbarUpButtonStyle,
                VerticalScrollbarDownButtonStyle,
                BoxStyle,
                LabelStyle,
                ToolTipStyle);

            BGAllStates(VerticalScrollbarThumbStyle,
                TexDarkRed);

            RectOffset offset =
                new RectOffset(10, 10, 3, 3);
            GUIStyle selectGridStyle =
                new GUIStyle(ToggleStyle)
                {
                    padding = offset,
                    border = offset
                };

            LabelStyle.margin = BoxStyle.margin;
            LabelStyle.padding = BoxStyle.margin;

            StyleText(BoxStyle,
                Color.white, Color.white,
                TextAnchor.MiddleCenter, FontStyle.Bold);

            StyleText(LabelStyle,
                Color.white, Color.white,
                TextAnchor.MiddleLeft, FontStyle.Normal);

            Color ColorGold = Editor.Colors.GetColor(ColorNames.Gold);

            StyleText(ListStyle,
                Color.white, ColorGold,
                TextAnchor.MiddleLeft, FontStyle.Normal);

            StyleText(ButtonStyle,
                Color.white, ColorGold,
                TextAnchor.MiddleCenter, FontStyle.Bold);

            StyleText(ToggleStyle,
                Color.white, ColorGold,
                TextAnchor.MiddleCenter, FontStyle.Bold);

            StyleText(TextFieldStyle,
                Color.white, ColorGold,
                TextAnchor.MiddleLeft, FontStyle.Normal);

            StyleText(TextAreaStyle,
                Color.white, ColorGold,
                TextAnchor.MiddleLeft, FontStyle.Normal);

            StyleText(selectGridStyle,
                Color.white, ColorGold,
                TextAnchor.MiddleLeft, FontStyle.Bold);

            StyleText(WindowStyle,
                Color.white);

            GUIStyle dragBarStyle =
                new GUIStyle(BlankBackgroundStyle)
                {
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(10, 10, 3, 3)
                };

            GUIStyle botTypeGridStyle =
                new GUIStyle(ToggleStyle)
                {
                    fontStyle = FontStyle.Normal,
                    alignment = TextAnchor.LowerCenter
                };

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
        }

        private Texture2D GetTexture(ColorNames key)
        {
            return Editor.TexturesClass.GetColor(key);
        }

        private void StyleText(Style key, Color color, Color? active = null, TextAnchor? anchor = null, FontStyle? fontStyle = null)
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

        private void StyleText(GUIStyle style, Color color, Color? active = null, TextAnchor? anchor = null, FontStyle? fontStyle = null)
        {
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
            StyleText(Style.box, white, white, TextAnchor.MiddleCenter, FontStyle.Bold);
            StyleText(Style.label, white, white, TextAnchor.MiddleLeft, FontStyle.Normal);

            Color ColorGold = Editor.Colors.GetColor(ColorNames.Gold);
            StyleText(Style.list, white, ColorGold, TextAnchor.MiddleLeft, FontStyle.Normal);
            StyleText(Style.button, white, ColorGold, TextAnchor.MiddleCenter, FontStyle.Bold);
            StyleText(Style.toggle, white, ColorGold, TextAnchor.MiddleCenter, FontStyle.Bold);
            StyleText(Style.textField, white, ColorGold, TextAnchor.MiddleLeft, FontStyle.Normal);
            StyleText(Style.textArea, white, ColorGold, TextAnchor.MiddleLeft, FontStyle.Normal);
            StyleText(Style.selectionGrid, white, ColorGold, TextAnchor.MiddleLeft, FontStyle.Bold);

            StyleText(Style.window, white);
        }
    }
}