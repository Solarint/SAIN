using SAIN.Editor.Abstract;
using UnityEngine;

namespace SAIN.Editor
{
    public class RectLayout : EditorAbstract
    {
        public RectLayout(SAINEditor editor) : base(editor)
        {
        }

        private static Vector2 OldScale;

        public static Vector2 ScaledPivot
        {
            get
            {
                float screenWidth = Screen.width;
                if (LastScreenWidth != screenWidth)
                {
                    LastScreenWidth = screenWidth;
                    ScalingFactor = GetScaling(screenWidth);
                    OldScale = new Vector2(ScalingFactor, ScalingFactor);
                }
                return OldScale;
            }
        }

        public static float LastScreenWidth = 0;

        private const float ReferenceResX = 1920;
        private const float ReferenceResY = 1080;

        public static float GetScaling(float screenWidth)
        {
            float ScreenHeight = Screen.height;
            float scalingFactor = Mathf.Min(screenWidth / ReferenceResX, ScreenHeight / ReferenceResY);
            return scalingFactor;
        }

        public static float ScalingFactor { get; private set; }

        public static Rect MainWindow = new Rect(0, 0, 1920, 1080);

        private static float Height = 25;
        private static float ExitWidth = 25f;
        private static float PauseWidth = 120f;
        private static float SaveAllWidth = 180f;
        private static float ExitStartX = MainWindow.width - ExitWidth;
        private static float PauseStartX = ExitStartX - PauseWidth;
        private static float SaveAllStartX = PauseStartX - SaveAllWidth;
        private static float DragWidth = SaveAllStartX;

        public static Rect ExitRect = new Rect(ExitStartX, 0, ExitWidth, Height);
        public static Rect DragRect = new Rect(0, 0, DragWidth, Height);
        public static Rect PauseRect = new Rect(PauseStartX, 0, PauseWidth, Height);
        public static Rect SaveAllRect = new Rect(SaveAllStartX, 0, SaveAllWidth, Height);
    }
}