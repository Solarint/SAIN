using SAIN.Plugin;
using UnityEngine;

namespace SAIN.Editor;

public static class RectLayout
{
    public static Vector2 ScaledPivot => GetScaling();

    private static float ReferenceResX => 1920 * PresetHandler.EditorDefaults.ConfigScaling;
    private static float ReferenceResY => 1080 * PresetHandler.EditorDefaults.ConfigScaling;

    private static Vector2 GetScaling()
    {
        float scaling = Mathf.Min(Screen.width / ReferenceResX, Screen.height / ReferenceResY);
        return new Vector2(scaling, scaling);
    }

    public static Rect MainWindow = new(0, 0, 1920, 1080);

    private const float RectHeight = 30f;
    private const float ExitWidth = 30f;
    private const float SaveAllWidth = 175f;
    private const float AdvWidth = 225f;

    private static readonly float ExitStartX = MainWindow.width - ExitWidth;
    private static readonly float SaveAllStartX = ExitStartX - SaveAllWidth - 5;
    private static readonly float AdvRectStartX = SaveAllStartX - AdvWidth - 5;
    private static readonly float DragWidth = AdvRectStartX - 5;

    public static Rect ExitRect = new(ExitStartX, 0, ExitWidth, RectHeight);
    public static Rect DragRect = new(0, 0, DragWidth, RectHeight);
    public static Rect SaveAllRect = new(SaveAllStartX, 0, SaveAllWidth, RectHeight);
    public static Rect AdvRect = new(AdvRectStartX, 0, AdvWidth, RectHeight);
}