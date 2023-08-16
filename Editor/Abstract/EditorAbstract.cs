using UnityEngine;
using SAIN.Editor.Util;

namespace SAIN.Editor.Abstract
{
    public abstract class EditorAbstract
    {
        public EditorAbstract(SAINEditor editor)
        {
            Editor = editor;
        }

        public readonly SAINEditor Editor;

        public BuilderClass Builder => Editor.Builder;
        public ButtonsClass ButtonsClass => Editor.Buttons;
        public MouseFunctions MouseFunctions => Editor.MouseFunctions;

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