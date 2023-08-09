using SAIN.Editor.GUISections;
using UnityEngine;
using SAIN.Editor.Abstract;

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
         
        static float ExitWidthHeight = 25f;
        static float PauseWidth = 120f;
        static float ExitStartX = MainWindow.width - ExitWidthHeight;
        static float PauseStartX = ExitStartX - PauseWidth;
        static float DragWidth = PauseStartX;

        public static Rect ExitRect = new Rect(ExitStartX, 0, ExitWidthHeight, ExitWidthHeight);
        public static Rect DragRect = new Rect(0, 0, DragWidth, ExitWidthHeight);
        public static Rect PauseRect = new Rect(PauseStartX, 0, PauseWidth, ExitWidthHeight);
    }
}
