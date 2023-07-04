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
            if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.Repaint)
            {
                ToolTipContent = Create(text);
                Vector2 tooltipSize = TooltipStyle.CalcSize(ToolTipContent);
                ToolTip = new Rect(rect.x + rect.width + 20, rect.y + rect.height + 50, tooltipSize.x, tooltipSize.y);
            }
        }

        public static Rect? ToolTip;
        public static Vector2? NearestRect;
        public static GUIContent ToolTipContent;

        public static void DrawToolTip()
        {
            if (ToolTipContent != null)
            {
                GUI.DrawTexture(ToolTip.Value, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Color.black, 0, 0);
                GUI.Box(ToolTip.Value, ToolTipContent, TooltipStyle);
            }
        }

        public static GUIContent Create(string text)
        {
            const int lineMaxWidth = 350; // Maximum width for each line

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
    }
}