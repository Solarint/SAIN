using SAIN.Plugin;
using UnityEngine;

namespace SAIN.Editor
{
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

        public static Rect MainWindow = new Rect(0, 0, 1920, 1080);

        private const float RectHeight = 30;
        private const float ExitWidth = 35f;
        private const float PauseWidth = 225f;
        private const float SaveAllWidth = 175f;

        private static readonly float ExitStartX = MainWindow.width - ExitWidth;
        private static readonly float PauseStartX = ExitStartX - PauseWidth;
        private static readonly float SaveAllStartX = PauseStartX - SaveAllWidth;
        private static readonly float DragWidth = SaveAllStartX;

        public static Rect ExitRect = new Rect(ExitStartX, 0, ExitWidth, RectHeight);
        public static Rect DragRect = new Rect(0, 0, DragWidth, RectHeight);
        public static Rect PauseRect = new Rect(PauseStartX, 0, PauseWidth, RectHeight);
        public static Rect SaveAllRect = new Rect(SaveAllStartX, 0, SaveAllWidth, RectHeight);
    }
}