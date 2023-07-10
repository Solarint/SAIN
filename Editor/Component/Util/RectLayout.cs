using SAIN.Editor.GUISections;
using UnityEngine;
using SAIN.Editor.Abstract;

namespace SAIN.Editor
{
    public class RectLayout : EditorAbstract
    {
        public RectLayout(GameObject go) : base(go)
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

        public static int SelectedTab { get; set; } = 0;
        public static int TabCount { get; } = SAINEditor.Tabs.Length;

        public static Rect MainWindow = new Rect(50, 50, 800, 800);
        public static Rect ExitButton = new Rect(MainWindow.width* 0.95f, 0, MainWindow.width * 0.05f, MainWindow.height * 0.05f);
        public static Rect DragBarMainWindow = new Rect(0, 0, MainWindow.width * 0.95f, MainWindow.height * 0.05f);
        public static Rect PresetMenuRect => new Rect(MainWindow.x + MainWindow.width, MainWindow.y + MainWindow.height, 800f, 800f);
        public static Rect PauseGameOptionRect = new Rect(MainWindow.width * 0.8f, 0, MainWindow.width * 0.15f, MainWindow.height * 0.05f);


        public float MainMenuWidth = 800f;
        public float MainMenuHeight = 800f;
        public float PresetMenuWidth = 800f;
        public float PresetMenuHeight = 600f;
    }
}
