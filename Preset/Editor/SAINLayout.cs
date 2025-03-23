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

        public static void Box(string text, params GUILayoutOption[] options)
        {
            GUILayout.Box(new GUIContent(text), GetStyle(Style.box), options);
        }

        public static void Box(string text, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.Box(new GUIContent(text), style, options);
        }

        public static void Box(string text, string tooltip, params GUILayoutOption[] options)
        {
            GUILayout.Box(new GUIContent(text, tooltip), GetStyle(Style.box), options);
        }

        public static void Box(string text, string tooltip, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.Box(new GUIContent(text, tooltip), style, options);
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
            if (style == null) {
                style = GetStyle(Style.label);
            }
            GUI.Label(rect, content, style);
        }

        public static void Label(Rect rect, string text, GUIStyle style = null)
        {
            if (style == null) {
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
            if (soundPlayed && SAINPlugin.DebugMode) {
                Logger.LogDebug($"Toggle {sound.Value}");
            }
            return newvalue;
        }

        public static string TextArea(string value, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            string newvalue = GUILayout.TextArea(value, GetStyle(Style.textField), options);
            bool soundPlayed = CompareValuePlaySound(value, newvalue, sound);
            if (soundPlayed && SAINPlugin.DebugMode) {
                Logger.LogDebug($"Toggle {sound.Value}");
            }
            return newvalue;
        }

        public static bool Button(string text, params GUILayoutOption[] options)
        {
            return Button(new GUIContent(text), null, options);
        }

        public static bool Button(string text, EUISoundType? sound, params GUILayoutOption[] options)
        {
            return Button(new GUIContent(text), sound, options);
        }

        public static bool Button(string text, string tooltip, EUISoundType? sound, params GUILayoutOption[] options)
        {
            return Button(new GUIContent(text, tooltip), sound, options);
        }

        public static bool Button(string text, string tooltip, EUISoundType? sound, GUIStyle style, params GUILayoutOption[] options)
        {
            return Button(new GUIContent(text, tooltip), sound, options);
        }

        public static bool Button(GUIContent content, EUISoundType? sound, params GUILayoutOption[] options)
        {
            return Button(content, GetStyle(Style.button), sound, options);
        }

        public static bool Button(GUIContent content, GUIStyle style, EUISoundType? sound, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(content, style, options)) {
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

        public static bool Toggle(bool value, string text, string tooltip, GUIStyle style, EUISoundType? sound = null, params GUILayoutOption[] options)
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
            if (soundPlayed && SAINPlugin.DebugMode) {
                Logger.LogDebug($"Toggle {sound.Value}");
            }
            return newvalue;
        }

        private static bool CompareValuePlaySound(object oldValue, object newValue, EUISoundType? sound = null, float volume = 1f)
        {
            if (oldValue.ToString() != newValue.ToString() && sound != null) {
                Sounds.PlaySound(sound.Value, volume);
                return true;
            }
            return false;
        }

        public static float HorizontalSlider(float value, float min, float max, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            float newvalue = GUILayout.HorizontalSlider(value, min, max, GetStyle(Style.horizontalSlider), GetStyle(Style.horizontalSliderThumb), options);
            float progress = (newvalue - min) / (max - min);
            sound = sound ?? EUISoundType.ButtonOver;
            progress = Mathf.Clamp(progress, 0.33f, 1f);
            bool soundPlayed = CompareValuePlaySound(value, newvalue, sound, progress);
            if (soundPlayed && SAINPlugin.DebugMode) {
                //Logger.LogDebug($"Toggle {sound.Value}");
            }
            return newvalue;
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
            if (indent > 0) {
                GUILayout.Space(indent);
            }
        }

        public static void BeginHorizontal(bool flexibleSpace)
        {
            GUILayout.BeginHorizontal();
            if (flexibleSpace) {
                GUILayout.FlexibleSpace();
            }
        }

        public static void EndHorizontal(float indent = 0)
        {
            if (indent > 0) {
                GUILayout.Space(indent);
            }
            GUILayout.EndHorizontal();
        }

        public static void EndHorizontal(bool flexibleSpace)
        {
            if (flexibleSpace) {
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
        }

        public static void BeginVertical(float indent = 0)
        {
            GUILayout.BeginVertical();
            if (indent > 0) {
                GUILayout.Space(indent);
            }
        }

        public static void EndVertical(float indent = 0)
        {
            if (indent > 0) {
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
            if (enable && value > 0) GUILayout.Space(value);
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

        public static Vector2 BeginScrollView(Vector2 scrollPos, float width)
        {
            return GUILayout.BeginScrollView(scrollPos, GetStyle(Style.scrollView), GetStyle(Style.verticalScrollbar), Width(width));
        }

        public static Vector2 BeginScrollView(Vector2 scrollPos, params GUILayoutOption[] options)
        {
            return GUILayout.BeginScrollView(scrollPos, GetStyle(Style.scrollView), GetStyle(Style.verticalScrollbar), options);
        }

        public static Vector2 BeginScrollView(Rect rect, Vector2 scrollPos, Rect viewRect)
        {
            return GUI.BeginScrollView(rect, scrollPos, viewRect, GetStyle(Style.scrollView), GetStyle(Style.verticalScrollbar));
        }

        public static void EndScrollView()
        {
            GUILayout.EndScrollView();
        }

        public static void EndScrollView(bool handleScrollWheel)
        {
            GUI.EndScrollView(handleScrollWheel);
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
    }
}