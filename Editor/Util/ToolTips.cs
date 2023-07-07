using System.Collections.Generic;
using UnityEngine;
using static SAIN.Editor.Styles;

namespace SAIN.Editor
{
    internal class ToolTips
    {
        public static void CheckMouse(string text)
        {
            if (text == null) return;

            var rect = GUILayoutUtility.GetLastRect();
            Vector2 mousePos = Event.current.mousePosition;
            if (rect.Contains(mousePos) && Event.current.type == EventType.Repaint)
            {
                ToolTipContent = Create(text);
                Vector2 tooltipSize = TooltipStyle.CalcSize(ToolTipContent);
                //ToolTip = new Rect(rect.x + rect.width + 20, rect.y + rect.height + 50, tooltipSize.x, tooltipSize.y);
                ToolTip = new Rect(mousePos.x + 20, mousePos.y + 50, tooltipSize.x, tooltipSize.y);
            }
        }

        public static void CheckMouseTable(List<string> options)
        {
            var rect = GUILayoutUtility.GetLastRect();
            Vector2 mousePos = Event.current.mousePosition;
            if (rect.Contains(mousePos) && Event.current.type == EventType.Repaint)
            {
                OpenToolTip = true;
                ToolTipContent = StringList(options);
            }
            else
            {
                OpenToolTip = false;
            }
        }

        private static bool OpenToolTip = false;

        public static Rect ToolTip;
        public static GUIContent ToolTipContent;

        public static void DrawToolTip()
        {
            if (OpenToolTip)
            {
                Vector2 mousePos = Event.current.mousePosition;
                Vector2 tooltipSize = TooltipStyle.CalcSize(ToolTipContent);
                ToolTip = new Rect(mousePos.x + 20, mousePos.y + 50, tooltipSize.x, tooltipSize.y);
                ToolTip = GUI.Window(1, ToolTip, ToolTipFunc, "");
            }
        }

        public static void ToolTipFunc(int i)
        {
            GUI.DrawTexture(ToolTip, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Color.black, 0, 0);
            GUI.Box(ToolTip, ToolTipContent, TooltipStyle);
        }

        public static GUIContent Create(string text, int lineMaxWidth = 350)
        {
            GUIContent wrappedContent = new GUIContent();

            string[] words = text.Split(' ');

            float lineWidth = 0f;
            string lineText = string.Empty;

            foreach (string word in words)
            {
                GUIContent wordContent = new GUIContent(word + " ");

                float wordWidth = TooltipStyle.CalcSize(wordContent).x;

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

        public static GUIContent StringList(string[] text)
        {
            string result = "";
            foreach (string word in text)
            {
                result += word;
                result += ", ";
            }
            return Create(result);
        }

        public static GUIContent StringList(List<string> text)
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