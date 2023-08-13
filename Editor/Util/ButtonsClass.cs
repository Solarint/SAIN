using SAIN.Editor.Abstract;
using UnityEngine;

namespace SAIN.Editor
{
    public class ButtonsClass : EditorAbstract
    {
        public ButtonsClass(SAINEditor editor) : base(editor)
        {
        }

        private const float InfoWidth = 25f;

        public void InfoBox(string description, float height)
        {
            InfoBox(description, Width(InfoWidth), Height(height));
        }

        public void InfoBox(string description, float height, float width)
        {
            InfoBox(description, Width(width), Height(height));
        }

        public void InfoBox(string description, params GUILayoutOption[] options)
        {
            var alertStyle = GetStyle(Style.alert);
            Box(new GUIContent("?", description), alertStyle, options);
        }

        public string Toggle(bool value, string on, string off)
        {
            return value ? on : off;
        }

        public void SingleTextBool(string text, bool value, params GUILayoutOption[] options)
        {
            string status = value ? ": Detected" : ": Not Detected";
            Box(text + status, options);
        }
    }
}