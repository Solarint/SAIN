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
        public StyleOptions(GameObject obj) : base(obj)
        {
            CustomStyle = new CustomStyleClass(obj);
        }

        public Font CustomFont { get; private set; }
        public static EditorCustomization Settings { get; private set; }
        public CustomStyleClass CustomStyle { get; private set; }

        public void Init()
        {
            Settings = JsonUtility.LoadFromJson.EditorSettings();
            if (Settings != null && Settings.FontName != null)
            {
                CustomFont = Font.CreateDynamicFontFromOSFont(Settings.FontName, Settings.FontSize);
            }
            CustomStyle.Init();
        }

        public void ColorEditorMenu()
        {
            GUILayout.BeginVertical();
            Red = BuilderClass.HorizSlider("Red", Red, 0f, 1f, 100f);
            Green = BuilderClass.HorizSlider("Blue", Green, 0f, 1f, 100f);
            Blue = BuilderClass.HorizSlider("Blue", Blue, 0f, 1f, 100f);
            Alpha = BuilderClass.HorizSlider("Alpha", Alpha, 0f, 1f, 100f);

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
                    var texture = UITextures.CreateTexture(2, 2, 0, color);
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
        public CustomStyleClass(GameObject editorObject)
        {
            Editor = editorObject.GetComponent<SAINEditor>();
        }

        public void Init()
        {
            CreateStyles();
            SetTextSettings();
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
            var ButtonStyle = new GUIStyle(GUI.skin.button);
            var BoxStyle = new GUIStyle(GUI.skin.box);
            var ToggleStyle = new GUIStyle(GUI.skin.toggle);
            var TextAreaStyle = new GUIStyle(GUI.skin.textArea);
            var TextFieldStyle = new GUIStyle(GUI.skin.textField);
            var WindowStyle = new GUIStyle(GUI.skin.window);
            var VerticalScrollbarDownButtonStyle = new GUIStyle(GUI.skin.verticalScrollbarDownButton);
            var VerticalScrollbarStyle = new GUIStyle(GUI.skin.verticalScrollbar);
            var VerticalScrollbarThumbStyle = new GUIStyle(GUI.skin.verticalScrollbarThumb);
            var VerticalScrollbarUpButtonStyle = new GUIStyle(GUI.skin.verticalScrollbarUpButton);
            var LabelStyle = new GUIStyle(GUI.skin.label);
            var HorizontalSliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
            var HorizontalSliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
            var VerticalSliderStyle = new GUIStyle(GUI.skin.verticalSlider);
            var VerticalSliderThumbStyle = new GUIStyle(GUI.skin.verticalSliderThumb);
            var ListStyle = new GUIStyle(GUI.skin.toggle);            

            var ToolTipStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(6, 6, 6, 6),
                border = new RectOffset(2, 2, 2, 2),
                wordWrap = true,
                clipping = TextClipping.Clip,
                alignment = TextAnchor.UpperLeft,
                fontStyle = FontStyle.Bold
            };

            Texture2D tooltiptex = Texture2D.blackTexture;
            BGAllStates(ToolTipStyle, tooltiptex);
            ApplyTextColorAllStates(ToolTipStyle, Color.white, Color.white);

            var BlankBackgroundStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            BGAllStates(BlankBackgroundStyle, null);
            ApplyTextColorAllStates(BlankBackgroundStyle, Color.white, Color.white);

            //var A = GetUI("A");
            //var Button_Large_A = Editor.TexturesClass.GetRecoloredUI("Button_Large_A", DarkRed);

            var TexMidGray = GetColor(MidGray);
            var TexDarkGray = GetColor(DarkGray);
            var TexVeryDarkGray = GetColor(VeryDarkGray);
            //var TexLightRed = GetColor(LightRed);
            var TexMidRed = GetColor(MidRed);
            var TexDarkRed = GetColor(DarkRed);

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
            BGFocused(TextFieldStyle, TextAreaStyle, TexVeryDarkGray, TexMidRed);

            BGAllStates(HorizontalSliderStyle, null);
            BGAllStates(HorizontalSliderThumbStyle, null);
            BGAllStates(WindowStyle, Editor.TexturesClass.Backgrounds[0]);
            BGAllStates(VerticalScrollbarStyle, TexVeryDarkGray);
            BGAllStates(VerticalScrollbarThumbStyle, TexDarkRed);
            BGAllStates(VerticalScrollbarUpButtonStyle, TexDarkGray);
            BGAllStates(VerticalScrollbarDownButtonStyle, TexDarkGray);
            BGAllStates(BoxStyle, TexVeryDarkGray);
            BGAllStates(LabelStyle, TexVeryDarkGray);

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
        private Texture2D GetUI(string key)
        {
            return Editor.TexturesClass.GetUI(key);
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

        private readonly FontStyle BoldFont = FontStyle.Bold;
        private readonly FontStyle NormalFont = FontStyle.Normal;
        private readonly TextAnchor MidCenterAlign = TextAnchor.MiddleCenter;
        private readonly TextAnchor MidLeftAlign = TextAnchor.MiddleLeft;

        private void SetTextSettings()
        {
            var white = Color.white;
            StyleTextAndSave(box, white, white, MidCenterAlign, BoldFont);
            StyleTextAndSave(label, white, white, TextAnchor.MiddleLeft, FontStyle.Normal);

            Color ColorGold = Editor.Colors.GetColor(Gold);
            StyleTextAndSave(list, white, ColorGold, TextAnchor.MiddleLeft, FontStyle.Normal);
            StyleTextAndSave(button, white, ColorGold, MidCenterAlign, BoldFont);
            StyleTextAndSave(toggle, white, ColorGold, MidCenterAlign, BoldFont);
            StyleTextAndSave(textField, white, ColorGold, MidLeftAlign, NormalFont);
            StyleTextAndSave(textArea, white, ColorGold, MidLeftAlign, NormalFont);

            StyleTextAndSave(window, white);
        }
    }

    public class ReferenceDictionaryClass
    {
        public readonly Dictionary<string, VariableReference> Dictionary = new Dictionary<string, VariableReference>();

        public void SaveVar(string key, Func<object> getter, Action<object> setter)
        {
            Dictionary.Add(key, new VariableReference(getter, setter));
        }

        public void ChangeVar(string key, GUIStyle style) // changing any of them
        {
            if (Dictionary[key].Get() is GUIStyle)
            {
                Dictionary[key].Set(style);
            }
        }
    }

    public sealed class VariableReference
    {
        public Func<object> Get { get; private set; }
        public Action<object> Set { get; private set; }
        public VariableReference(Func<object> getter, Action<object> setter)
        {
            Get = getter;
            Set = setter;
        }
    }
}