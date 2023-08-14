using BepInEx.Logging;
using EFT.UI;
using UnityEngine;

namespace SAIN.Editor.Abstract
{
    public abstract class LayoutAbstract
    {
        public LayoutAbstract(SAINEditor editor)
        {
            Editor = editor;
        }

        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(LayoutAbstract));

        public SAINEditor Editor { get; private set; }

        public void Box(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.Box(content, style, options);
        }

        public void Box(string text, string tooltip, params GUILayoutOption[] options)
        {
            Box(new GUIContent(text, tooltip), GetStyle(Style.box), options);
        }

        public void Box(string text, params GUILayoutOption[] options)
        {
            Box(new GUIContent(text), GetStyle(Style.box), options);
        }

        public void BlankBox(string text, params GUILayoutOption[] options)
        {
            Box(new GUIContent(text), GetStyle(Style.blankbox), options);
        }

        public void BlankBox(string text, string tooltip, params GUILayoutOption[] options)
        {
            Box(new GUIContent(text, tooltip), GetStyle(Style.blankbox), options);
        }

        public void ToolTip(Rect rect, GUIContent text)
        {
            GUI.Box(rect, text, GetStyle(Style.tooltip));
        }

        public void Label(string text, GUIStyle style, params GUILayoutOption[] options)
        {
            Label(new GUIContent(text), style, options);
        }

        public void Label(string text, string tooltip, GUIStyle style, params GUILayoutOption[] options)
        {
            Label(new GUIContent(text, tooltip), style, options);
        }

        public void Label(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.Label(content, style, options);
        }

        public void Label(string text, params GUILayoutOption[] options)
        {
            Label(new GUIContent(text), GetStyle(Style.label), options);
        }

        public void Label(string text, string tooltip, params GUILayoutOption[] options)
        {
            Label(new GUIContent(text, tooltip), GetStyle(Style.label), options);
        }

        public void Label(GUIContent content, params GUILayoutOption[] options)
        {
            Label(content, GetStyle(Style.label), options);
        }

        public string TextField(string value, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            string newvalue = GUILayout.TextField(value, GetStyle(Style.textField), options);
            bool soundPlayed = CompareValuePlaySound(value, newvalue, sound);
            if (soundPlayed && SAINPlugin.DebugModeEnabled)
            {
                Logger.LogDebug($"Toggle {sound.Value}");
            }
            return newvalue;
        }

        public string TextArea(string value, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            string newvalue = GUILayout.TextArea(value, GetStyle(Style.textField), options);
            bool soundPlayed = CompareValuePlaySound(value, newvalue, sound);
            if (soundPlayed && SAINPlugin.DebugModeEnabled)
            {
                Logger.LogDebug($"Toggle {sound.Value}");
            }
            return newvalue;
        }

        public bool Button(string text, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            return Button(new GUIContent(text), sound, options);
        }

        public bool Button(string text, string tooltip, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            return Button(new GUIContent(text, tooltip), sound, options);
        }

        public bool Button(GUIContent content, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(content, GetStyle(Style.button), options))
            {
                CompareValuePlaySound(true, false, sound);
                return true;
            }
            return false;
        }

        public bool Toggle(bool value, string text, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            return Toggle(value, new GUIContent(text), sound, options);
        }

        public bool Toggle(bool value, string text, string tooltip, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            return Toggle(value, new GUIContent(text, tooltip), sound, options);
        }

        public bool Toggle(bool value, GUIContent content, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            return Toggle(value, content, GetStyle(Style.toggle), sound, options);
        }

        public bool Toggle(bool value, GUIContent content, GUIStyle style, EUISoundType? sound = null, params GUILayoutOption[] options)
        {
            bool newvalue = GUILayout.Toggle(value, content, style, options);
            bool soundPlayed = CompareValuePlaySound(value, newvalue, sound);
            if (soundPlayed && SAINPlugin.DebugModeEnabled)
            {
                Logger.LogDebug($"Toggle {sound.Value}");
            }
            return newvalue;
        }

        private bool CompareValuePlaySound(object oldValue, object newValue, EUISoundType? sound = null)
        {
            if (oldValue.ToString() != newValue.ToString() && sound != null)
            {
                Sounds.PlaySound(sound.Value);
                return true;
            }
            return false;
        }

        public float HorizontalSlider(float value, float min, float max, EUISoundType? sound = null)
        {
            return HorizontalSlider(value, min, max, sound, StandardHeight);
        }

        public float HorizontalSlider(float value, float min, float max, float width, EUISoundType? sound = null)
        {
            return HorizontalSlider(value, min, max, sound, StandardHeight, Width(width));
        }

        public float HorizontalSlider(float value, float min, float max, float width, float height, EUISoundType? sound = null)
        {
            return HorizontalSlider(value, min, max, sound, Height(height), Width(width));
        }

        public float HorizontalSlider(float value, float min, float max, EUISoundType? sound = null, params GUILayoutOption[] options)
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

        public float HorizontalSlider(Rect rect, float value, float min, float max, EUISoundType? sound = null)
        {
            return GUI.HorizontalSlider(rect, value, min, max, GetStyle(Style.horizontalSlider), GetStyle(Style.horizontalSliderThumb));
        }

        public float HorizontalSliderNoStyle(string label, float value, float min, float max, float LabelWidth = 150f, float ValueWidth = 100f, EUISoundType? sound = null)
        {
            GUILayout.BeginHorizontal();

            Label(label, Width(LabelWidth), StandardHeight);

            value = HorizontalSlider(value, min, max, sound, StandardHeight);

            Box(value.ToString(), Width(ValueWidth), StandardHeight);

            GUILayout.EndHorizontal();
            return value;
        }

        public void BeginHorizontal(bool value = true)
        {
            if (value) GUILayout.BeginHorizontal();
        }

        public void EndHorizontal(bool value = true)
        {
            if (value) GUILayout.EndHorizontal();
        }

        public void BeginVertical(bool value = true)
        {
            if (value) GUILayout.BeginVertical();
        }

        public void EndVertical(bool value = true)
        {
            if (value) GUILayout.EndVertical();
        }

        public void BeginArea(Rect rect)
        {
            GUILayout.BeginArea(rect);
        }

        public void EndArea()
        {
            GUILayout.EndArea();
        }

        public void Space(float value, bool enable = true)
        {
            if (enable) GUILayout.Space(value);
        }

        public void BeginGroup(Rect rect)
        {
            GUI.BeginGroup(rect, GetStyle(Style.window));
        }

        public void EndGroup()
        {
            GUI.EndGroup();
        }

        public GUILayoutOption ExpandHeight(bool value)
        {
            return GUILayout.ExpandHeight(value);
        }

        public GUILayoutOption ExpandWidth(bool value)
        {
            return GUILayout.ExpandWidth(value);
        }

        public void FlexibleSpace(bool value = true)
        {
            if (value) GUILayout.FlexibleSpace();
        }

        public Vector2 BeginScrollView(Vector2 scrollPos)
        {
            return GUILayout.BeginScrollView(scrollPos, GetStyle(Style.scrollView));
        }

        public Vector2 BeginScrollView(Rect rect, Vector2 scrollPos, Rect viewRect)
        {
            return GUI.BeginScrollView(rect, scrollPos, viewRect, GetStyle(Style.horizontalScrollbar), GetStyle(Style.verticalScrollbar));
        }

        public void EndScrollView()
        {
            GUILayout.EndScrollView();
        }

        public void EndScrollView(bool handleScrollWheel)
        {
            GUI.EndScrollView(handleScrollWheel);
        }

        public Vector2 BeginScrollView(Vector2 pos, params GUILayoutOption[] options)
        {
            return GUILayout.BeginScrollView(pos, GetStyle(Style.verticalScrollbar), GetStyle(Style.verticalScrollbar), options);
        }

        public GUIStyle GetStyle(Style key)
        {
            return Editor.StyleOptions.CustomStyle.GetStyle(key);
        }

        public GUILayoutOption Height(float height)
        {
            return GUILayout.Height(height);
        }

        public GUILayoutOption Width(float width)
        {
            return GUILayout.Width(width);
        }

        public Rect NewWindow(int id, Rect viewRect, GUI.WindowFunction func, string title)
        {
            return GUI.Window(id, viewRect, func, title, GetStyle(Style.window));
        }

        public GUILayoutOption StandardHeight => Height(BaseHeight);
        public GUILayoutOption ExtendedHeight => Height(TallHeight);

        private float BaseHeight = 22.5f;
        private float TallHeight = 35f;
    }
}