using JetBrains.Annotations;
using System;
using UnityEngine;

namespace SAIN.Editor.Util
{
    public static class MouseFunctions
    {
        public static void Update()
        {
        }

        public static void OnGUI()
        {
        }

        public static bool CheckMouseDrag()
        {
            return Event.current.type == EventType.Repaint && CheckMouseDrag(GUILayoutUtility.GetLastRect());
        }

        public static bool CheckMouseDrag(Rect rect)
        {
            return MouseDragClass.DragRectangle.Overlaps(rect);
        }

        public static bool IsMouseInside()
        {
            return Event.current.type == EventType.Repaint && IsMouseInside(GUILayoutUtility.GetLastRect());
        }

        public static bool IsMouseInside(Rect rect)
        {
            return rect.Contains(Event.current.mousePosition);
        }

        public static bool IsNearMouse(Vector2 point, float distance = 20f)
        {
            return (MousePos - point).magnitude <= distance;
        }

        public static bool IsNearMouse(Rect rect, float distance = 20f)
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
        public static Event CurrentMouseEvent
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

        private static Vector2 MousePos => Event.current.mousePosition;
    }

    public static class MouseDragClass 
    {
        private static Rect FullScreen = new Rect(0, 0, Screen.width, Screen.height);
        private static GUIStyle BlankStyle;

        public static Rect DragRectangle = Rect.zero;
        private static Rect DrawPosition = new Rect(193, 148, 249 - 193, 148 - 104);
        public static Color color = Color.white;
        private readonly static Vector3[] mousePositions = new Vector3[2];
        private readonly static Vector2[] mousePositions2D = new Vector2[2];
        private static bool drawRect = false;

        public static void OnGUI()
        {
            if (BlankStyle == null)
            {
                BlankStyle = new GUIStyle(GUI.skin.window);
            }
            if (SAINEditor.DisplayingWindow && drawRect)
            {
                FullScreen = GUI.Window(999, FullScreen, EmptyWindowFunc, "", BlankStyle);
            }
        }


        private static void EmptyWindowFunc(int i)
        {
            DrawRectangle(DrawPosition, 1, color);
        }

        private static void DrawRectangle(Rect area, int frameWidth, Color color)
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

        private static void Reset()
        {
            drawRect = false;
            mousePositions[0] = new Vector3();
            mousePositions[1] = new Vector3();
            DragRectangle = Rect.zero;
        }

        public static void Update()
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