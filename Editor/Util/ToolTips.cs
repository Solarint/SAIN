using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static SAIN.Editor.StyleOptions;
using static SAIN.Editor.Names.StyleNames;
using SAIN.Editor.Abstract;

namespace SAIN.Editor
{
    public class ToolTips : EditorAbstract
    {
        public ToolTips(SAINEditor editor) : base(editor) { }

        private GUIStyle ToolTipStyle => Editor.StyleOptions.GetStyle(tooltip);

        public void CheckMouseTT(string text)
        {
            if (text == null) return;

            if (MouseFunctions.IsMouseInside())
            {
                OpenToolTip = true;
                Vector2 mousePos = Event.current.mousePosition;
                ToolTipContent = Create(text);
                Vector2 tooltipSize = ToolTipStyle.CalcSize(ToolTipContent);
                ToolTipRect = new Rect(mousePos.x + 20, mousePos.y + 50, tooltipSize.x, tooltipSize.y);
            }
        }

        public void CheckMouseTable(List<string> options)
        {
            if (options == null) return;

            var rect = GUILayoutUtility.GetLastRect();
            Vector2 mousePos = Event.current.mousePosition;
            if (rect.Contains(mousePos) && Event.current.type == EventType.Repaint)
            {
                OpenToolTip = true;
                ToolTipContent = StringList(options);
            }
        }

        private static bool OpenToolTip = false;

        public static GUIContent ToolTipContent;

        public void DrawToolTip()
        {
            if (OpenToolTip)
            {
                Vector2 mousePos = Event.current.mousePosition;
                Vector2 tooltipSize = ToolTipStyle.CalcSize(ToolTipContent);
                ToolTipRect = new Rect(mousePos.x + 20, mousePos.y + 50, tooltipSize.x, tooltipSize.y);
                ToolTipRect = GUI.Window(1, ToolTipRect, ToolTipFunc, "");
                OpenToolTip = false;
            }
        }

        private Rect ToolTipRect;

        public void ToolTipFunc(int i)
        {
            ToolTip(ToolTipRect, ToolTipContent);
        }

        public GUIContent Create(string text, int lineMaxWidth = 350)
        {
            GUIContent wrappedContent = new GUIContent();

            string[] words = text.Split(' ');

            float lineWidth = 0f;
            string lineText = string.Empty;

            foreach (string word in words)
            {
                GUIContent wordContent = new GUIContent(word + " ");

                float wordWidth = ToolTipStyle.CalcSize(wordContent).x;

                if (lineWidth + wordWidth > lineMaxWidth)
                {
                    wrappedContent.text += lineText.TrimEnd() + "\n";
                    lineText = string.Empty;
                    lineWidth = 0f;
                }

                lineText += word + " ";
                lineWidth += wordWidth;
            }

            wrappedContent.text += lineText.TrimEnd();
            return wrappedContent;
        }

        public GUIContent StringList(string[] text)
        {
            string result = "";
            foreach (string word in text)
            {
                result += word;
                result += ", ";
            }
            return Create(result);
        }

        public GUIContent StringList(List<string> text)
        {
            string result = "";
            foreach (string word in text)
            {
                result += word;
                result += ", ";
            }
            return Create(result);
        }
    }
}