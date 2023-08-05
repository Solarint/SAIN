using UnityEngine;

namespace SAIN.Editor.Abstract
{
    public abstract class LayoutAbstract
    {
        public LayoutAbstract(SAINEditor editor)
        {
            Editor = editor;
        }

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

        public string TextField(string text, params GUILayoutOption[] options)
        {
            return GUILayout.TextField(text, GetStyle(Style.textField), options);
        }

        public string TextArea(string text, params GUILayoutOption[] options)
        {
            return GUILayout.TextArea(text, GetStyle(Style.textArea), options);
        }

        public bool Button(string text, params GUILayoutOption[] options)
        {
            return Button(new GUIContent(text), options);
        }

        public bool Button(string text, string tooltip, params GUILayoutOption[] options)
        {
            return Button(new GUIContent(text, tooltip), options);
        }

        public bool Button(GUIContent content, params GUILayoutOption[] options)
        {
            return GUILayout.Button(content, GetStyle(Style.button), options);
        }

        public bool Toggle(bool value, string text, params GUILayoutOption[] options)
        {
            return Toggle(value, new GUIContent(text), options);
        }

        public bool Toggle(bool value, string text, string tooltip, params GUILayoutOption[] options)
        {
            return Toggle(value, new GUIContent(text, tooltip), options);
        }

        public bool Toggle(bool value, GUIContent content, params GUILayoutOption[] options)
        {
            return Toggle(value, content, GetStyle(Style.toggle), options);
        }

        public bool Toggle(bool value, GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            return GUILayout.Toggle(value, content, style, options);
        }

        public float HorizontalSlider(float value, float min, float max)
        {
            return HorizontalSlider(value, min, max, StandardHeight);
        }

        public float HorizontalSlider(float value, float min, float max, float width)
        {
            return HorizontalSlider(value, min, max, StandardHeight, Width(width));
        }

        public float HorizontalSlider(float value, float min, float max, float width, float height)
        {
            return HorizontalSlider(value, min, max, Height(height), Width(width));
        }

        public float HorizontalSlider(float value, float min, float max, params GUILayoutOption[] options)
        {
            return GUILayout.HorizontalSlider(value, min, max, GetStyle(Style.horizontalSlider), GetStyle(Style.horizontalSliderThumb), options);
        }

        public float HorizontalSlider(Rect rect, float value, float min, float max)
        {
            return GUI.HorizontalSlider(rect, value, min, max, GetStyle(Style.horizontalSlider), GetStyle(Style.horizontalSliderThumb));
        }

        public float HorizontalSliderNoStyle(string label, float value, float min, float max, float LabelWidth = 150f, float ValueWidth = 100f)
        {
            GUILayout.BeginHorizontal();

            Label(label, Width(LabelWidth), StandardHeight);

            value = HorizontalSlider(value, min, max, StandardHeight);

            Box(value.ToString(), Width(ValueWidth), StandardHeight);

            GUILayout.EndHorizontal();
            return value;
        }

        public void BeginHorizontal()
        {
            GUILayout.BeginHorizontal();
        }

        public void EndHorizontal()
        {
            GUILayout.EndHorizontal();
        }

        public void BeginVertical()
        {
            GUILayout.BeginVertical();
        }

        public void EndVertical()
        {
            GUILayout.EndVertical();
        }

        public void BeginArea(Rect rect)
        {
            GUILayout.BeginArea(rect);
        }

        public void EndArea()
        {
            GUILayout.EndArea();
        }

        public void Space(float value)
        {
            GUILayout.Space(value);
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

        public void FlexibleSpace()
        {
            GUILayout.FlexibleSpace();
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

        public void CreateGUIAdjustmentSliders()
        {
            BaseHeight = HorizontalSliderNoStyle(nameof(BaseHeight), BaseHeight, 5f, 50f);
            BaseHeight = Editor.BotSelectionClass.Round(BaseHeight);

            TallHeight = HorizontalSliderNoStyle(nameof(TallHeight), TallHeight, 5f, 80f);
            TallHeight = Editor.BotSelectionClass.Round(TallHeight);

            BaseLabelWidth = HorizontalSliderNoStyle(nameof(BaseLabelWidth), BaseLabelWidth, 50f, 250f);
            BaseLabelWidth = Editor.BotSelectionClass.Round(BaseLabelWidth);
        }

        public GUILayoutOption StandardHeight => Height(BaseHeight);
        public GUILayoutOption ExtendedHeight => Height(TallHeight);

        private float BaseHeight = 22.5f;
        private float TallHeight = 35f;
        private float BaseLabelWidth = 100f;
    }
}