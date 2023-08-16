using BepInEx.Logging;
using EFT.UI;
using UnityEngine;

namespace SAIN.Editor
{
    public static class SAINLayout
    {
        public static void Box(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.Box(content, style, options);
        }

        public static void Box(string text, string tooltip, params GUILayoutOption[] options)
        {
            Box(new GUIContent(text, tooltip), GetStyle(Style.box), options);
        }

        public static void Box(string text, params GUILayoutOption[] options)
        {
            Box(new GUIContent(text), GetStyle(Style.box), options);
        }

        public static void BlankBox(string text, params GUILayoutOption[] options)
        {
            Box(new GUIContent(text), GetStyle(Style.blankbox), options);
        }

        public static void BlankBox(string text, string tooltip, params GUILayoutOption[] options)
        {
            Box(new GUIContent(text, tooltip), GetStyle(Style.blankbox), options);
        }

        public static void ToolTip(Rect rect, GUIContent text)
        {
            GUI.Box(rect, text, GetStyle(Style.tooltip));
        }

        public static void Label(string text, GUIStyle style, params GUILayoutOption[] options)
        {
            Label(new GUIContent(text), style, options);
        }

        public static void Label(string text, string tooltip, GUIStyle style, params GUILayoutOption[] options)
        {
            Label(new GUIContent(text, tooltip), style, options);
        }

        public static void Label(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.Label(content, style, options);
        }

        public static void Label(Rect rect, GUIContent content, GUIStyle style = null)
        {
            if (style == null)
            {
                style = GetStyle(Style.label);
            }
            GUI.Label(rect, content, style);
        }

        public static void Label(Rect rect, string text, GUIStyle style = null)
        {
            if (style == null)
            {
                style = GetStyle(Style.label);
            }
            GUI.Label(rect, text, style);
        }

        public static void Label(string text, params GUILayoutOption[] options)
        {
            Label(new GUIContent(text), GetStyle(Style.label), options);
        }

        public static void Label(string text, string tooltip, params GUILayoutOption[] options)
        {
            Label(new GUIContent(text, tooltip), GetStyle(Style.label), options);
        }

        public static void Label(GUIContent content, params GUILayoutOption[] options)
        {
            Label(content, GetStyle(Style.label), options);
        }

        public static string TextField(string value, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            string newvalue = GUILayout.TextField(value, GetStyle(Style.textField), options);
            bool soundPlayed = CompareValuePlaySound(value, newvalue, sound);
            if (soundPlayed && SAINPlugin.DebugModeEnabled)
            {
                Logger.LogDebug($"Toggle {sound.Value}");
            }
            return newvalue;
        }

        public static string TextArea(string value, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            string newvalue = GUILayout.TextArea(value, GetStyle(Style.textField), options);
            bool soundPlayed = CompareValuePlaySound(value, newvalue, sound);
            if (soundPlayed && SAINPlugin.DebugModeEnabled)
            {
                Logger.LogDebug($"Toggle {sound.Value}");
            }
            return newvalue;
        }

        public static bool Button(string text, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            return Button(new GUIContent(text), sound, options);
        }

        public static bool Button(string text, string tooltip, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            return Button(new GUIContent(text, tooltip), sound, options);
        }

        public static bool Button(GUIContent content, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(content, GetStyle(Style.button), options))
            {
                CompareValuePlaySound(true, false, sound);
                return true;
            }
            return false;
        }

        public static bool Toggle(bool value, string text, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            return Toggle(value, new GUIContent(text), sound, options);
        }

        public static bool Toggle(bool value, string text, string tooltip, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            return Toggle(value, new GUIContent(text, tooltip), sound, options);
        }

        public static bool Toggle(bool value, GUIContent content, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            return Toggle(value, content, GetStyle(Style.toggle), sound, options);
        }

        public static bool Toggle(bool value, GUIContent content, GUIStyle style, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            bool newvalue = GUILayout.Toggle(value, content, style, options);
            bool soundPlayed = CompareValuePlaySound(value, newvalue, sound);
            if (soundPlayed && SAINPlugin.DebugModeEnabled)
            {
                Logger.LogDebug($"Toggle {sound.Value}");
            }
            return newvalue;
        }

        private static bool CompareValuePlaySound(object oldValue, object newValue, EUISoundType? sound = null)
        {
            if (oldValue.ToString() != newValue.ToString() && sound != null)
            {
                Sounds.PlaySound(sound.Value);
                return true;
            }
            return false;
        }

        public static float HorizontalSlider(float value, float min, float max, EUISoundType? sound = null)
        {
            return HorizontalSlider(value, min, max, sound, StandardHeight);
        }

        public static float HorizontalSlider(float value, float min, float max, float width, EUISoundType? sound = null)
        {
            return HorizontalSlider(value, min, max, sound, StandardHeight, Width(width));
        }

        public static float HorizontalSlider(float value, float min, float max, float width, float height, EUISoundType? sound = null)
        {
            return HorizontalSlider(value, min, max, sound, Height(height), Width(width));
        }

        public static float HorizontalSlider(float value, float min, float max, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            float newvalue = GUILayout.HorizontalSlider(value, min, max, GetStyle(Style.horizontalSlider), GetStyle(Style.horizontalSliderThumb), options);
            sound = sound ?? EUISoundType.MenuEscape;
            bool soundPlayed = CompareValuePlaySound(value, newvalue, sound);
            if (soundPlayed && SAINPlugin.DebugModeEnabled)
            {
                Logger.LogDebug($"Toggle {sound.Value}");
            }
            return newvalue;
        }

        public static float HorizontalSlider(Rect rect, float value, float min, float max, EUISoundType? sound = null)
        {
            return GUI.HorizontalSlider(rect, value, min, max, GetStyle(Style.horizontalSlider), GetStyle(Style.horizontalSliderThumb));
        }

        public static float HorizontalSliderNoStyle(string label, float value, float min, float max, float LabelWidth = 150f, float ValueWidth = 100f, EUISoundType? sound = null)
        {
            GUILayout.BeginHorizontal();

            Label(label, Width(LabelWidth), StandardHeight);

            value = HorizontalSlider(value, min, max, sound, StandardHeight);

            Box(value.ToString(), Width(ValueWidth), StandardHeight);

            GUILayout.EndHorizontal();
            return value;
        }

        public static void BeginHorizontalSpace(float space = 10)
        {
            BeginHorizontal();
            Space(space);
        }

        public static void EndHorizontalSpace(float space = 10)
        {
            Space(space);
            EndHorizontal();
        }

        public static void BeginHorizontal(float indent = 0)
        {
            GUILayout.BeginHorizontal();
            if (indent > 0)
            {
                GUILayout.Space(indent);
            }
        }

        public static void BeginHorizontal(bool flexibleSpace)
        {
            GUILayout.BeginHorizontal();
            if (flexibleSpace)
            {
                GUILayout.FlexibleSpace();
            }
        }

        public static void EndHorizontal(float indent = 0)
        {
            if (indent > 0)
            {
                GUILayout.Space(indent);
            }
            GUILayout.EndHorizontal();
        }

        public static void EndHorizontal(bool flexibleSpace)
        {
            if (flexibleSpace)
            {
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }

        public static void BeginVertical(float indent = 0)
        {
            GUILayout.BeginVertical();
            if (indent > 0)
            {
                GUILayout.Space(indent);
            }
        }

        public static void EndVertical(float indent = 0)
        {
            if (indent > 0)
            {
                GUILayout.Space(indent);
            }
            GUILayout.EndVertical();
        }

        public static void BeginArea(Rect rect)
        {
            GUILayout.BeginArea(rect);
        }

        public static void EndArea()
        {
            GUILayout.EndArea();
        }

        public static void Space(float value, bool enable = true)
        {
            if (enable) GUILayout.Space(value);
        }

        public static void BeginGroup(Rect rect)
        {
            GUI.BeginGroup(rect, GetStyle(Style.blankbox));
        }

        public static void EndGroup()
        {
            GUI.EndGroup();
        }

        public static GUILayoutOption ExpandHeight(bool value)
        {
            return GUILayout.ExpandHeight(value);
        }

        public static GUILayoutOption ExpandWidth(bool value)
        {
            return GUILayout.ExpandWidth(value);
        }

        public static void FlexibleSpace(bool value = true)
        {
            if (value) GUILayout.FlexibleSpace();
        }

        public static Vector2 BeginScrollView(Vector2 scrollPos)
        {
            return GUILayout.BeginScrollView(scrollPos, GetStyle(Style.scrollView));
        }

        public static Vector2 BeginScrollView(Rect rect, Vector2 scrollPos, Rect viewRect)
        {
            return GUI.BeginScrollView(rect, scrollPos, viewRect, GetStyle(Style.horizontalScrollbar), GetStyle(Style.verticalScrollbar));
        }

        public static void EndScrollView()
        {
            GUILayout.EndScrollView();
        }

        public static void EndScrollView(bool handleScrollWheel)
        {
            GUI.EndScrollView(handleScrollWheel);
        }

        public static Vector2 BeginScrollView(Vector2 pos, params GUILayoutOption[] options)
        {
            return GUILayout.BeginScrollView(pos, GetStyle(Style.verticalScrollbar), GetStyle(Style.verticalScrollbar), options);
        }

        public static GUIStyle GetStyle(Style key)
        {
            return StylesClass.GetStyle(key);
        }

        public static GUILayoutOption Height(float height)
        {
            return GUILayout.Height(height);
        }

        public static GUILayoutOption Width(float width)
        {
            return GUILayout.Width(width);
        }

        public static Rect NewWindow(int id, Rect viewRect, GUI.WindowFunction func, string title)
        {
            return GUI.Window(id, viewRect, func, title, GetStyle(Style.window));
        }

        public static GUILayoutOption StandardHeight => Height(BaseHeight);
        public static GUILayoutOption ExtendedHeight => Height(TallHeight);

        private static float BaseHeight = 22.5f;
        private static float TallHeight = 35f;
    }
}