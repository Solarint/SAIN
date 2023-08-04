using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Editor.Util
{
    internal class ApplyToStyle
    {
        public static void BGAllStates(GUIStyle style, Texture2D normal)
        {
            BGNormal(style, normal, normal);
            BGActive(style, normal, normal);
            BGHover(style, normal, normal);
            BGFocused(style, normal, normal);
        }

        public static void BGAllStates(GUIStyle style, GUIStyle style2, Texture2D normal)
        {
            BGAllStates(style, normal);
            BGAllStates(style2, normal);
        }

        public static void BGAllStates(GUIStyle[] styles, Texture2D normal)
        {
            foreach (var style in styles)
            {
                BGAllStates(style, normal);
            }
        }

        public static void BGAllStates(List<GUIStyle> styles, Texture2D normal)
        {
            foreach (var style in styles)
            {
                BGAllStates(style, normal);
            }
        }

        public static void ApplyTextColorAllStates(GUIStyle style, Color normal, Color? active = null)
        {
            active = active ?? normal;
            var activeVal = active.Value;
            SetColorNormal(style, normal, activeVal);
            SetColorActive(style, normal, activeVal);
            SetColorHover(style, normal, activeVal);
            SetColorFocused(style, normal, activeVal);
        }

        public static void ApplyTextColor(GUIStyleState state, Color color)
        {
            state.textColor = color;
        }

        public static void SetColorNormal(GUIStyle style, Color normal, Color active)
        {
            ApplyTextColor(style.normal, normal);
            ApplyTextColor(style.onNormal, active);
        }

        public static void SetColorActive(GUIStyle style, Color normal, Color active)
        {
            ApplyTextColor(style.active, normal);
            ApplyTextColor(style.onActive, active);
        }

        public static void SetColorHover(GUIStyle style, Color normal, Color active)
        {
            ApplyTextColor(style.hover, normal);
            ApplyTextColor(style.onHover, active);
        }

        public static void SetColorFocused(GUIStyle style, Color normal, Color active)
        {
            ApplyTextColor(style.focused, normal);
            ApplyTextColor(style.onFocused, active);
        }

        public static void BGNormal(GUIStyle style, Texture2D normal, Texture2D on)
        {
            style.normal.background = normal;
            style.onNormal.background = on;
        }

        public static void BGActive(GUIStyle style, Texture2D normal, Texture2D on)
        {
            style.active.background = normal;
            style.onActive.background = on;
        }

        public static void BGHover(GUIStyle style, Texture2D normal, Texture2D on)
        {
            style.hover.background = normal;
            style.onHover.background = on;
        }

        public static void BGFocused(GUIStyle style, Texture2D normal, Texture2D on)
        {
            style.focused.background = normal;
            style.onFocused.background = on;
        }

        public static void BGNormal(GUIStyle style, GUIStyle style2, Texture2D normal, Texture2D on)
        {
            BGNormal(style, normal, on);
            BGNormal(style2, normal, on);
        }

        public static void BGActive(GUIStyle style, GUIStyle style2, Texture2D normal, Texture2D on)
        {
            BGActive(style, normal, on);
            BGActive(style2, normal, on);
        }

        public static void BGHover(GUIStyle style, GUIStyle style2, Texture2D normal, Texture2D on)
        {
            BGHover(style, normal, on);
            BGHover(style2, normal, on);
        }

        public static void BGFocused(GUIStyle style, GUIStyle style2, Texture2D normal, Texture2D on)
        {
            BGFocused(style, normal, on);
            BGFocused(style2, normal, on);
        }

        public static void BGNormal(GUIStyle[] styles, Texture2D normal, Texture2D on)
        {
            foreach (var style in styles)
            {
                BGNormal(style, normal, on);
            }
        }

        public static void BGActive(GUIStyle[] styles, Texture2D normal, Texture2D on)
        {
            foreach (var style in styles)
            {
                BGActive(style, normal, on);
            }
        }

        public static void BGHover(GUIStyle[] styles, Texture2D normal, Texture2D on)
        {
            foreach (var style in styles)
            {
                BGHover(style, normal, on);
            }
        }

        public static void BGFocused(GUIStyle[] styles, Texture2D normal, Texture2D on)
        {
            foreach (var style in styles)
            {
                BGFocused(style, normal, on);
            }
        }

        public static void BGNormal(List<GUIStyle> styles, Texture2D normal, Texture2D on)
        {
            foreach (var style in styles)
            {
                BGNormal(style, normal, on);
            }
        }

        public static void BGActive(List<GUIStyle> styles, Texture2D normal, Texture2D on)
        {
            foreach (var style in styles)
            {
                BGActive(style, normal, on);
            }
        }

        public static void BGHover(List<GUIStyle> styles, Texture2D normal, Texture2D on)
        {
            foreach (var style in styles)
            {
                BGHover(style, normal, on);
            }
        }

        public static void BGFocused(List<GUIStyle> styles, Texture2D normal, Texture2D on)
        {
            foreach (var style in styles)
            {
                BGFocused(style, normal, on);
            }
        }
    }
}