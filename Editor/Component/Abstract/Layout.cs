using UnityEngine;
using SAIN.Editor.Util;
using static SAIN.Editor.Names.StyleNames;

namespace SAIN.Editor.Abstract
{
    public abstract class LayoutAbstract
    {
        public LayoutAbstract(GameObject editor)
        {
            Editor = editor.GetComponent<SAINEditor>();
        }

        public SAINEditor Editor { get; private set; }

        public void Box(string text, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.Box(text, style, options);
        }
        public void Box(string text, params GUILayoutOption[] options)
        {
            Box(text, GetStyle(box), options);
        }
        public void Box(string text, float width, bool extended = false)
        {
            Box(text, GetHeight(extended), Width(width));
        }
        public void Box(string text, bool extended = false)
        {
            Box(text, GetHeight(extended));
        }

        public void BlankBox(string text, params GUILayoutOption[] options)
        {
            Box(text, GetStyle(blankbox), options);
        }
        public void BlankBox(string text, float width, bool extended = false)
        {
            BlankBox(text, GetHeight(extended), Width(width));
        }
        public void BlankBox(string text, bool extended = false)
        {
            BlankBox(text, GetHeight(extended));
        }

        public void ToolTip(Rect rect, GUIContent text)
        {
            GUI.Box(rect, text, GetStyle(tooltip));
        }

        public void Label(string text, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.Label(text, style, options);
        }
        public void Label(string text, params GUILayoutOption[] options)
        {
            Label(text, GetStyle(label), options);
        }
        public void Label(string text, float width, bool extended = false)
        {
            Label(text, GetHeight(extended), Width(width));
        }
        public void Label(string text, bool extended = false)
        {
            Label(text, GetHeight(extended), Width(BaseLabelWidth));
        }

        private GUILayoutOption GetHeight(bool extended = false)
        {
            float height = extended ? TallHeight : BaseHeight;
            return Height(height);
        }

        public string TextField(string text, params GUILayoutOption[] options)
        {
            return GUILayout.TextField(text, GetStyle(textField), options);
        }
        public string TextField(string text, float width, bool extended = false)
        {
            return TextField(text, GetHeight(extended), Width(width));
        }
        public string TextField(string text, bool extended = false)
        {
            return TextField(text, GetHeight(extended));
        }

        public string TextArea(string text, params GUILayoutOption[] options)
        {
            return GUILayout.TextArea(text, GetStyle(textArea), options);
        }
        public string TextArea(string text, float width, bool extended = false)
        {
            return TextArea(text, GetHeight(extended), Width(width));
        }
        public string TextArea(string text, bool extended = false)
        {
            return TextArea(text, GetHeight(extended));
        }

        public bool Button(string text, GUIStyle style, params GUILayoutOption[] options)
        {
            return GUILayout.Button(text, style, options);
        }
        public bool Button(string text, params GUILayoutOption[] options)
        {
            return Button(text, GetStyle(button), options);
        }
        public bool Button(string text, float width, bool extended = false)
        {
            return Button(text, GetHeight(extended), Width(width));
        }
        public bool Button(string text, bool extended = false)
        {
            return Button(text, GetHeight(extended));
        }

        public bool Toggle(bool value, string text, GUIStyle style, params GUILayoutOption[] options)
        {
            return GUILayout.Toggle(value, text, style, options);
        }
        public bool Toggle(bool value, string text, params GUILayoutOption[] options)
        {
            return Toggle(value, text, GetStyle(toggle), options);
        }
        public bool Toggle(bool value, string text, float width, bool extended = false)
        {
            return Toggle(value, text, GetHeight(extended), Width(width));
        }
        public bool Toggle(bool value, string text, bool extended = false)
        {
            return Toggle(value, text, GetHeight(extended));
        }
        public float HorizontalSlider(float value, float min, float max)
        {
            return HorizontalSlider(value, min, max, StandardHeight);
        }

        public float HorizontalSlider(float value, float min, float max, float width)
        {
            return HorizontalSlider(value, min, max, StandardHeight, Width(width));
        }

        public float HorizontalSlider(float value, float min, float max, params GUILayoutOption[] options)
        {
            value = GUILayout.HorizontalSlider(value, min, max, GetStyle(horizontalSlider), GetStyle(horizontalSliderThumb), options);
            GUI.tooltip = value.ToString();
            return value;
        }

        public float HorizontalSliderFlexible(string label, float value, float min, float max, float LabelWidth = 150f, float SliderWidth = 300f, float ValueWidth = 100f)
        {
            GUILayout.BeginHorizontal();

            Label(label, Width(LabelWidth), StandardHeight);

            GUILayout.FlexibleSpace();
            value = HorizontalSlider(value, min, max, StandardHeight, Width(SliderWidth));
            GUILayout.FlexibleSpace();

            Box(value.ToString(), Width(ValueWidth), StandardHeight);

            GUILayout.EndHorizontal();
            return value;
        }

        public float HorizontalSliderFilled(string label, float value, float min, float max, float LabelWidth = 150f, float ValueWidth = 100f)
        {
            GUILayout.BeginHorizontal();

            Label(label, Width(LabelWidth), StandardHeight);

            value = HorizontalSlider(value, min, max, StandardHeight);

            Box(value.ToString(), Width(ValueWidth), StandardHeight);

            GUILayout.EndHorizontal();
            return value;
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

        public float HorizontalSliderFixed(string label, float value, float min, float max, float LabelWidth = 150f, float ValueWidth = 100f, float sliderWidth = 300f, float gap = 15f, float indent = 0f)
        {
            GUILayout.BeginHorizontal();
            if (indent > 0)
            {
                Space(indent);
            }

            Space(gap);

            Label(label, Width(LabelWidth), StandardHeight);

            Space(gap);

            value = HorizontalSlider(value, min, max, Width(sliderWidth), StandardHeight);

            Space(gap);

            Box(value.ToString(), Width(ValueWidth), StandardHeight);

            GUILayout.EndHorizontal();
            return value;
        }

        public float HorizontalSliderFixedInfoBox(string description, string label, float value, float min, float max, float LabelWidth = 150f, float ValueWidth = 100f, float InfoWidth = 25f, float sliderWidth = 300f, float gap = 15f, float indent = 0f)
        {
            GUILayout.BeginHorizontal();
            if (indent > 0)
            {
                Space(indent);
            }
            Box(description, Width(InfoWidth), StandardHeight);
            //CheckMouse(description);

            Space(gap);

            Label(label, Width(LabelWidth), StandardHeight);

            Space(gap);

            value = HorizontalSlider(value, min, max, Width(sliderWidth), StandardHeight);

            Space(gap);

            Box(value.ToString(), Width(ValueWidth), StandardHeight);

            GUILayout.EndHorizontal();
            return value;
        }

        public Vector2 BeginScrollView(Vector2 pos, params GUILayoutOption[] options)
        {
            return GUILayout.BeginScrollView(pos, GetStyle(verticalScrollbar), GetStyle(verticalScrollbar), options);
        }

        public GUIStyle GetStyle(string key)
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

        public void Space(float width)
        {
            GUILayout.Space(width);
        }

        public Rect NewWindow(int id, Rect viewRect, GUI.WindowFunction func, string title)
        {
            return GUI.Window(id, viewRect, func, title, GetStyle(window));
        }

        public void CreateGUIAdjustmentSliders()
        {
            BaseHeight = HorizontalSliderNoStyle(nameof(BaseHeight), BaseHeight, 5f, 50f);
            BaseHeight = Editor.PresetEditor.Round(BaseHeight);

            TallHeight = HorizontalSliderNoStyle(nameof(TallHeight), TallHeight, 5f, 80f);
            TallHeight = Editor.PresetEditor.Round(TallHeight);

            BaseLabelWidth = HorizontalSliderNoStyle(nameof(BaseLabelWidth), BaseLabelWidth, 50f, 250f);
            BaseLabelWidth = Editor.PresetEditor.Round(BaseLabelWidth);
        }

        public GUILayoutOption StandardHeight => Height(BaseHeight);
        public GUILayoutOption ExtendedHeight => Height(TallHeight);

        private float BaseHeight = 22.5f;
        private float TallHeight = 35f;
        private float BaseLabelWidth = 100f;
    }
}
