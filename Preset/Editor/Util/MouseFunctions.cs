using UnityEngine;

namespace SAIN.Editor.Util;

public static class MouseFunctions
{
    public static void Update()
    {
        checkMouseEvents();
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

    private const float MOUSE_FUNC_TIME = 0.2f;

    private static Vector2 _lastMousePos;

    private static void checkMouseEvents()
    {
        Vector2 mousePos = Event.current.mousePosition;
        if ((mousePos - _lastMousePos).sqrMagnitude > 0.001f)
        {
            _mouseMoveTime = Time.time + MOUSE_FUNC_TIME;
        }
        _lastMousePos = mousePos;
        if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            Sounds.PlaySound(EFT.UI.EUISoundType.ButtonBottomBarClick);
        }
    }

    public static bool MouseIsMoving => _mouseMoveTime > Time.time;

    private static float _mouseMoveTime;

    public static bool IsMouseInside(Rect rect)
    {
        return rect.Contains(Event.current.mousePosition);
    }

    public static bool IsNearMouse(Vector2 point, float distance = 20f)
    {
        return (MousePos - point).magnitude <= distance;
    }

    public static bool IsNearMouse(Rect rect, float widthDistance = 10f, float heightDistance = 10f)
    {
        if (rect.Contains(MousePos))
        {
            return true;
        }
        Rect rect2 = new(0f, 0f, widthDistance * 2, heightDistance * 2)
        {
            center = MousePos
        };
        return rect2.Overlaps(rect);
    }

    private static Vector2 MousePos => Event.current.mousePosition;
}

public static class MouseDragClass
{
    private static Rect FullScreen = new(0, 0, Screen.width, Screen.height);
    private static GUIStyle BlankStyle;

    public static Rect DragRectangle = Rect.zero;
    private static Rect DrawPosition = new(193, 148, 249 - 193, 148 - 104);
    public static Color color = Color.white;
    private static readonly Vector3[] mousePositions = new Vector3[2];
    private static readonly Vector2[] mousePositions2D = new Vector2[2];
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
}