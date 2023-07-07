using UnityEngine;

namespace SAIN.Editor.Component
{
    public abstract class EditorAbstract
    {
        public EditorAbstract(GameObject editor)
        {
            Editor = editor.GetComponent<SAINEditor>();
        }

        public SAINEditor Editor { get; private set; }
    }
}
