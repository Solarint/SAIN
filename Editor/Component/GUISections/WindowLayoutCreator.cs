using SAIN.Editor.Abstract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SAIN.Editor.GUISections
{
    public class WindowLayoutCreator : EditorAbstract
    {
        public WindowLayoutCreator(GameObject gameObject) : base(gameObject)
        {
            Windows = new Rect[SAINEditor.Tabs.Length];
            for (int i = 0; i < Windows.Length; i++)
            {
                Windows[i] = new Rect(0, WindowYStart, FinalWindowWidth, 0);
            }
        }

        public Rect CreateWindow(int windowCount)
        {
            for (int i = 0; i < Windows.Length; i++)
            {
                Windows[i] = Animate(Windows[i], i == windowCount);
            }
            return Windows[windowCount];
        }

        private Rect Animate(Rect rect, bool open)
        {
            float height = rect.height;
            float sign = open ? 1f : -1f;
            height += FinalWindowHeight * sign / GraduationSteps;
            height = Clamp(height);
            FinishedAnimation = height == FinalWindowHeight;
            rect.height = height;
            return rect;
        }

        private float Clamp(float height)
        {
            return Mathf.Clamp(height, 0f, FinalWindowHeight);
        }

        public bool FinishedAnimation { get; private set; } = false;
        private float GraduationSteps = 40f;

        readonly Rect[] Windows;

        static float WindowYStart = 100f;
        public float FinalWindowWidth => RectLayout.MainWindow.width;
        public float FinalWindowHeight { get; private set; } = 400f;

        public void ModifySizesMenu()
        {
            GUILayout.BeginVertical();
            FinalWindowHeight = HorizontalSliderNoStyle(nameof(FinalWindowHeight), FinalWindowHeight, 150f, 500f);
            GraduationSteps = HorizontalSliderNoStyle(nameof(GraduationSteps), GraduationSteps, 5f, 50f);
            WindowYStart = HorizontalSliderNoStyle(nameof(WindowYStart), WindowYStart, 25f, 350f);
            GUILayout.EndVertical();
        }
    }
}
