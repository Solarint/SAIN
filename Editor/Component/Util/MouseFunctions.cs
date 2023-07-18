using JetBrains.Annotations;
using SAIN.Editor.Abstract;
using System;
using UnityEngine;

namespace SAIN.Editor.Util
{
    public class MouseFunctions : EditorAbstract
    {
        public MouseFunctions(SAINEditor editor) : base(editor)
        {
            MouseDrag = new MouseDragClass(editor);
        }

        public void Update()
        {
            MouseDrag.Update();
        }

        public void OnGUI()
        {
            MouseDrag.OnGUI();
        }

        public MouseDragClass MouseDrag { get; private set; }

        public bool CheckMouseDrag()
        {
            return Event.current.type == EventType.Repaint && CheckMouseDrag(GUILayoutUtility.GetLastRect());
        }

        public bool CheckMouseDrag(Rect rect)
        {
            return MouseDrag.DragRectangle.Overlaps(rect);
        }

        public bool IsMouseInside()
        {
            return Event.current.type == EventType.Repaint && IsMouseInside(GUILayoutUtility.GetLastRect());
        }

        public bool IsMouseInside(Rect rect)
        {
            return rect.Contains(Event.current.mousePosition);
        }

        public bool IsNearMouse(Vector2 point, float distance = 20f)
        {
            return (MousePos - point).magnitude <= distance;
        }

        public bool IsNearMouse(Rect rect, float distance = 20f)
        {
            if (rect.Contains(MousePos))
            {
                return true;
            }
            Rect rect2 = new Rect(0f, 0f, distance * 2, distance * 2)
            {
                center = MousePos
            };
            return rect2.Overlaps(rect);
        }

        [CanBeNull]
        public Event CurrentMouseEvent
        {
            get
            {
                if (Event.current.isMouse)
                {
                    return Event.current;
                }
                return null;
            }
        }

        private Vector2 MousePos => Event.current.mousePosition;
    }

    public class MouseDragClass : EditorAbstract
    {
        public MouseDragClass(SAINEditor editor) : base(editor)
        {
            FullScreen = new Rect(0, 0, Screen.width, Screen.height);
        }

        GUIStyle BlankStyle;

        public Rect DragRectangle = Rect.zero;
        private Rect DrawPosition = new Rect(193, 148, 249 - 193, 148 - 104);
        public Color color = Color.white;
        private readonly Vector3[] mousePositions = new Vector3[2];
        private readonly Vector2[] mousePositions2D = new Vector2[2];
        private bool drawRect = false;

        public void OnGUI()
        {
            if (BlankStyle == null)
            {
                BlankStyle = new GUIStyle(GUI.skin.window);
                ApplyToStyle.BGAllStates(BlankStyle, null);
            }
            if (Editor.MainWindowOpen && drawRect)
            {
                FullScreen = GUI.Window(999, FullScreen, EmptyWindowFunc, "", BlankStyle);
            }
        }

        Rect FullScreen;

        void EmptyWindowFunc(int i)
        {
            DrawRectangle(DrawPosition, 1, color);
        }

        private void DrawRectangle(Rect area, int frameWidth, Color color)
        {
            //Create a one pixel texture with the right color
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();

            Rect lineArea = area;
            lineArea.height = frameWidth; //Top line
            GUI.DrawTexture(lineArea, texture);
            lineArea.y = area.yMax - frameWidth; //Bottom
            GUI.DrawTexture(lineArea, texture);
            lineArea = area;
            lineArea.width = frameWidth; //Left
            GUI.DrawTexture(lineArea, texture);
            lineArea.x = area.xMax - frameWidth;//Right
            GUI.DrawTexture(lineArea, texture);
        }

        private void Reset()
        {
            drawRect = false;
            mousePositions[0] = new Vector3();
            mousePositions[1] = new Vector3();
            DragRectangle = Rect.zero;
        }

        public void Update()
        {
            if (Event.current.isMouse && Event.current.type == EventType.MouseDrag)
            {
                if (!drawRect)
                {
                    mousePositions[0] = Input.mousePosition;
                }

                mousePositions[1] = Input.mousePosition;

                float width = Math.Abs(mousePositions[1].x - mousePositions[0].x);
                float height = Math.Abs(mousePositions[1].y - mousePositions[0].y);
                float x = mousePositions[0].x;
                float y = Screen.height - mousePositions[0].y;

                if (mousePositions[0].x < mousePositions[1].x && mousePositions[0].y < mousePositions[1].y)
                {
                    DrawPosition = new Rect(x, y, width, -height);
                }
                else if (mousePositions[0].x > mousePositions[1].x && mousePositions[0].y < mousePositions[1].y)
                {
                    DrawPosition = new Rect(x, y, -width, -height);
                }
                else if (mousePositions[0].x < mousePositions[1].x && mousePositions[0].y > mousePositions[1].y)
                {
                    DrawPosition = new Rect(x, y, width, height);
                }
                else
                {
                    DrawPosition = new Rect(x, y, -width, height);
                }

                DragRectangle = DrawPosition;
                drawRect = true;
            }
            else if (Event.current.isMouse && Event.current.type != EventType.MouseDrag)
            {
                Reset();
            }
        }
    }
}