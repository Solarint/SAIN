using System.Collections.Generic;
using UnityEngine;
using static SAIN.Editor.Util.Colors;
using Colors = SAIN.Editor.Util.Colors;

namespace SAIN.Editor
{
    internal class Styles
    {
        static Styles()
        {

        }

        public static Font DefaultFont;

        public static Color Gray = new Color(0.33f, 0.33f, 0.33f);

        public static GUILayoutOption Height(float height)
        {
            return GUILayout.Height(height);
        }

        public static GUILayoutOption Height(float? height)
        {
            return GUILayout.Height(height.Value);
        }

        public static GUILayoutOption Width(float width)
        {
            return GUILayout.Width(width);
        }

        public static GUILayoutOption Width(float? width)
        {
            return GUILayout.Width(width.Value);
        }

        public static void InitStyles()
        {
            DefaultFont = GUI.skin.font;

            GUI.skin.toggle.alignment = TextAnchor.MiddleCenter;
            GUI.skin.toggle.fontStyle = FontStyle.Bold;

            ApplyTextures(GUI.skin.toggle, TexMidGray, TextureDarkRed, TextureLightRed, TextureDarkRed, TextureLightRed, TextureDarkRed, TextureLightRed, TextureDarkRed);

            ApplyTextures(GUI.skin.button, TexMidGray, TextureDarkRed, TextureLightRed, TextureDarkRed, TextureLightRed, TextureDarkRed, TextureLightRed, TextureDarkRed);
            ApplyTextures(GUI.skin.horizontalSlider, null);

            ApplyTextures(GUI.skin.verticalScrollbar, TexVeryDarkGray);

            ApplyTextures(GUI.skin.verticalScrollbarThumb, TextureDarkRed);
            
            ApplyTextures(GUI.skin.verticalScrollbarUpButton, TexDarkGray);

            ApplyTextures(GUI.skin.verticalScrollbarDownButton, TexDarkGray);

            ApplyTextures(GUI.skin.box, TexVeryDarkGray);
            ApplyTextures(GUI.skin.label, TexVeryDarkGray);


            GUI.skin.window.normal.textColor = Color.white;

            GUI.skin.textField.alignment = TextAnchor.MiddleCenter;
            GUI.skin.textField.fontStyle = FontStyle.Bold;

            GUI.skin.window.normal.textColor = Color.white;

            ApplyTextures(GUI.skin.horizontalSliderThumb, null);

            TooltipStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(6, 6, 6, 6),
                border = new RectOffset(2, 2, 2, 2),
                wordWrap = true,
                clipping = TextClipping.Clip,
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
            };
            Texture2D tooltiptex = Texture2D.blackTexture;
            ApplyTextures(TooltipStyle, tooltiptex);
            ApplyTextColor(TooltipStyle, Color.white);

            BlankBoxBG = new GUIStyle(GUI.skin.box)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            ApplyTextures(BlankBoxBG, null);
            BlankBoxBG.normal.textColor = Color.white;
            ApplyTextColor(GUI.skin.box, Color.white);
            ApplyTextColor(GUI.skin.label, Color.white);
        }

        public static void ColorEditorMenu()
        {
            GUILayout.BeginVertical();
            Red = BuilderUtil.HorizSlider("Red", Red, 0f, 1f, 100f);
            Green = BuilderUtil.HorizSlider("Blue", Green, 0f, 1f, 100f);
            Blue = BuilderUtil.HorizSlider("Blue", Blue, 0f, 1f, 100f);
            Alpha = BuilderUtil.HorizSlider("Alpha", Alpha, 0f, 1f, 100f);

            if (GUILayout.Button("Save"))
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

        public static GUIStyle TooltipStyle { get; private set; }
        public static GUIStyle BlankBoxBG { get; private set; }

        public static void ApplyTextures(GUIStyle style, Texture2D normal)
        {
            style.normal.background = normal;
            style.onNormal.background = normal;

            style.active.background = normal;
            style.onActive.background = normal;

            style.hover.background = normal;
            style.onHover.background = normal;

            style.focused.background = normal;
            style.onFocused.background = normal;
        }

        public static void ApplyTextures(GUIStyle style, Texture2D normal, Texture2D onNormal, Texture2D active, Texture2D onActive, Texture2D hover, Texture2D onHover, Texture2D focused, Texture2D onFocused)
        {
            style.normal.background = normal;
            style.onNormal.background = onNormal;

            style.active.background = active;
            style.onActive.background = onActive;

            style.hover.background = hover;
            style.onHover.background = onHover;

            style.focused.background = focused;
            style.onFocused.background = onFocused;
        }

        public static void ApplyTextColor(GUIStyle style, Color color)
        {
            style.normal.textColor = color;
            style.onNormal.textColor = color;

            style.active.textColor = color;
            style.onActive.textColor = color;

            style.hover.textColor = color;
            style.onHover.textColor = color;

            style.focused.textColor = color;
            style.onFocused.textColor = color;
        }
    }
}