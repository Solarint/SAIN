using SAIN.Editor.GUISections;
using UnityEngine;

namespace SAIN.Editor
{
    internal class RectLayout
    {
        public static void Init()
        {
            Scaling();
        }

        public static int BaseFontSize { get; private set; }
        public static float ScalingFactor { get; private set; }

        private static void Scaling()
        {
            BaseFontSize = GUI.skin.font.fontSize;
            // Calculate the scaled font size based on the screen resolution
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            ScalingFactor = Mathf.Min(screenWidth / 1920f, screenHeight / 1080f); // Adjust these values based on your desired reference resolution
           
            GUI.skin.font = Font.CreateDynamicFontFromOSFont(GUI.skin.font.name, BaseFontSize);
            MainWindow = new Rect(50, 50, 800, 800);
            DragRectangle = new Rect(0, 0, MainWindow.width * 0.95f, MainWindow.height * 0.05f);
            ExitButton = new Rect(MainWindow.width * 0.95f, 0, MainWindow.width * 0.05f, MainWindow.height * 0.05f);
        }

        public static int SelectedTab { get; set; } = 0;
        public static int TabCount { get; } = Toolbar.Tabs.Length;
        public static Rect MainWindow { get; set; }
        public static Rect ExitButton { get; set; }
        public static Rect DragRectangle { get; set; }
    }
}
