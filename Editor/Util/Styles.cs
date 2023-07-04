using UnityEngine;

namespace SAIN.Editor
{
    internal class Styles
    {
        public static void InitStyles()
        {
            if (TextStyle == null)
            {
                ApplyMainStyle();

                TextStyle = new GUIStyle(GUI.skin.textField)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };

                WindowStyle = new GUIStyle(GUI.skin.window);
                ApplyTextures(WindowStyle, null);
                WindowStyle.normal.textColor = Color.white;

                SliderStyle = new GUIStyle(GUI.skin.horizontalSlider);
                SliderStyle.normal.background = null;
                SliderStyle.normal.textColor = Color.clear;

                ThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
                ApplyTextures(ThumbStyle, null);

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
            }
        }

        public static GUIStyle TextStyle { get; private set; }
        public static GUIStyle WindowStyle { get; private set; }
        public static GUIStyle SliderStyle { get; private set; }
        public static GUIStyle ThumbStyle { get; private set; }
        public static GUIStyle TooltipStyle { get; private set; }
        public static GUIStyle BlankBoxBG { get; private set; }

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

        public static void ApplyTextColor(GUIStyle style, Color color)
        {
            style.normal.textColor = color;
            style.active.textColor = color;
            style.hover.textColor = color;
            style.onHover.textColor = color;
            style.onActive.textColor = color;
            style.focused.textColor = color;
            style.onFocused.textColor = color;
        }

        public static void ApplyMainStyle()
        {
            GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
            ApplyTextures(windowStyle, null);
            GUI.skin.window = windowStyle;
            GUI.skin.window.normal.textColor = Color.white;
            var slider = GUI.skin.horizontalSlider;
            var thumb = GUI.skin.horizontalSliderThumb;
            slider.normal.background = null;
            slider.fixedHeight = 20f;
            slider.alignment = TextAnchor.MiddleCenter;
            thumb.fixedHeight = 20f;
            thumb.alignment = TextAnchor.MiddleCenter;
            GUI.color = Color.white;
        }
    }
}
