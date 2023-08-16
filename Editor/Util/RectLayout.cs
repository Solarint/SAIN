using UnityEngine;

namespace SAIN.Editor
{
    public static class RectLayout
    {
        private static Vector2 OldScale;

        public static Vector2 ScaledPivot
        {
            get
            {
                float ScalingFactor = GetScaling(Screen.width);
                return new Vector2(ScalingFactor, ScalingFactor);
            }
        }

        public static float ConfigScaling = 1f;

        public static float LastScreenWidth = 0;

        private static float ReferenceResX => 1920 * ConfigScaling;
        private static float ReferenceResY => 1080 * ConfigScaling;

        public static float GetScaling(float screenWidth)
        {
            float ScreenHeight = Screen.height;
            float scalingFactor = Mathf.Min(screenWidth / ReferenceResX, ScreenHeight / ReferenceResY);
            return scalingFactor;
        }

        public static float ScalingFactor { get; private set; }

        public static Rect MainWindow = new Rect(0, 0, 1920, 1080);

        private static float RectHeight = 30;
        private static float ExitWidth = 35f;
        private static float PauseWidth = 225f;
        private static float SaveAllWidth = 175f;
        private static float ExitStartX = MainWindow.width - ExitWidth;
        private static float PauseStartX = ExitStartX - PauseWidth;
        private static float SaveAllStartX = PauseStartX - SaveAllWidth;
        private static float DragWidth = SaveAllStartX;

        public static Rect ExitRect = new Rect(ExitStartX, 0, ExitWidth, RectHeight);
        public static Rect DragRect = new Rect(0, 0, DragWidth, RectHeight);
        public static Rect PauseRect = new Rect(PauseStartX, 0, PauseWidth, RectHeight);
        public static Rect SaveAllRect = new Rect(SaveAllStartX, 0, SaveAllWidth, RectHeight);
    }
}