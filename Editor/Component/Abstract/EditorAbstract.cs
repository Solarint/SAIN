using UnityEngine;
using SAIN.Editor.Util;

namespace SAIN.Editor.Abstract
{
    public abstract class EditorAbstract : LayoutAbstract
    {
        public EditorAbstract(SAINEditor editor) : base(editor) { }

        public BuilderClass Builder => Editor.Builder;
        public TexturesClass TexturesClass => Editor.TexturesClass;
        public ButtonsClass ButtonsClass => Editor.Buttons;
        public ColorsClass ColorsClass => Editor.Colors;
        public MouseFunctions MouseFunctions => Editor.MouseFunctions;

        public void CheckMouse(string text)
        {
            //Editor.ToolTips.CheckMouseTT(text);
        }
        public void CheckMouse()
        {
            //Editor.ToolTips.CheckMouseTT(text);
        }
        public bool CheckDragLayout()
        {
            return MouseFunctions.CheckMouseDrag();
        }
        public bool CheckDrag(Rect rect)
        {
            return MouseFunctions.CheckMouseDrag(rect);
        }
    }
}