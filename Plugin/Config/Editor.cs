using Aki.Reflection.Utils;
using BepInEx.Configuration;
using Comfort.Common;
using EFT.Console.Core;
using EFT.UI;
using EFT.Weather;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SAIN
{
    public class Difficulty
    {
        public static float BotRecoilGlobal { get; private set; } = 1f;
        public static float PMCRecoil { get; private set; } = 1f;
        public static float ScavRecoil { get; private set; } = 1f;
        public static float OtherRecoil { get; private set; } = 1f;

        public static float VisionSpeed { get; private set; } = 1f;
        public static float CloseVisionSpeed { get; private set; } = 1f;
        public static float FarVisionSpeed { get; private set; } = 1f;
        public static float CloseFarThresh { get; private set; } = 50f;
        public static float SpeedResult { get; private set; } = 100f;

        public static bool FasterCQBReactions { get; private set; } = false;
        public static bool AllChads { get; private set; } = false;
        public static bool AllGigaChads { get; private set; } = false;
        public static bool AllRats { get; private set; } = false;

        public class Editor
        {
            internal static ConfigEntry<KeyboardShortcut> TogglePanel;

            private static GameObject input;

            private static Rect windowRect = new Rect(50, 50, 500, 500);
            private static bool guiStatus = false;

            public static float cloudDensity;
            public static float fog;
            public static float rain;
            public static float lightningThunderProb;
            public static float temperature;
            public static float windMagnitude;
            public static int windDir = 2;
            public static WeatherDebug.Direction windDirection;
            public static int topWindDir = 2;
            public static Vector2 topWindDirection;

            private static int targetTimeHours;
            private static int targetTimeMinutes;

            [ConsoleCommand("Open SAIN Difficulty Editor")]
            public static void OpenPanel()
            {
                if (input == null)
                {
                    input = GameObject.Find("___Input");
                }

                guiStatus = !guiStatus;
                Cursor.visible = guiStatus;
                if (guiStatus)
                {
                    CursorSettings.SetCursor(ECursorType.Idle);
                    Cursor.lockState = CursorLockMode.None;
                    Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuContextMenu);
                }
                else
                {
                    CursorSettings.SetCursor(ECursorType.Invisible);
                    Cursor.lockState = CursorLockMode.Locked;
                    Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuDropdown);
                }
                input.SetActive(!guiStatus);
                ConsoleScreen.Log("[SAIN]: Opening Editor...");
            }

            public void Update()
            {
                if (Input.GetKeyDown(TogglePanel.Value.MainKey))
                {
                    //Caching input manager GameObject which script is responsible for reading the player inputs
                    if (input == null)
                    {
                        input = GameObject.Find("___Input");
                    }

                    guiStatus = !guiStatus;
                    Cursor.visible = guiStatus;
                    if (guiStatus)
                    {
                        //Changing the default windows cursor to an EFT-style one and playing a sound when the menu appears
                        CursorSettings.SetCursor(ECursorType.Idle);
                        Cursor.lockState = CursorLockMode.None;
                        Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuContextMenu);
                    }
                    else
                    {
                        //Hiding cursor and playing a sound when the menu disappears
                        CursorSettings.SetCursor(ECursorType.Invisible);
                        Cursor.lockState = CursorLockMode.Locked;
                        Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuDropdown);
                    }
                    //Disabling the input manager so the player won't move
                    input.SetActive(!guiStatus);
                }
            }

            public void OnGUI()
            {
                if (guiStatus)
                    windowRect = GUI.Window(0, windowRect, WindowFunction, "SAIN Bot Difficulty Editor by Solarint. GUI by SamSWAT");
            }

            private void WindowFunction(int TWCWindowID)
            {
                GUI.DragWindow(new Rect(0, 0, 10000, 20));

                //---------------------------------------------\\

                if (GUI.Button(new Rect(390, 40, 50, 30), "Set"))
                {
                    Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuInspectorWindowClose);
                }

                //---------------------------------------------\\
                //---------------------------------------------\\

                GUILayout.BeginArea(new Rect(15, 100, 250, 500));
                GUILayout.BeginVertical();

                GUILayout.Box("Vision Speed Multipliers");
                GUILayout.Box("Multiplied by the existing vision speed");
                GUILayout.Box("Result may vary depending on the bot type.");

                GUILayout.Box("Base: " + Mathf.Round(VisionSpeed * 10) / 10);
                VisionSpeed = GUILayout.HorizontalSlider(VisionSpeed, 0.25f, 3f);

                GUILayout.Box("Close: " + Mathf.Round(CloseVisionSpeed * 10) / 10);
                CloseVisionSpeed = GUILayout.HorizontalSlider(CloseVisionSpeed, 0.25f, 3f);

                GUILayout.Box("Far: " + Mathf.Round(FarVisionSpeed * 10) / 10);
                FarVisionSpeed = GUILayout.HorizontalSlider(FarVisionSpeed, 0.25f, 3f);

                GUILayout.Box("Close/Far Threshold: " + Mathf.Round(CloseFarThresh));
                CloseFarThresh = GUILayout.HorizontalSlider(CloseFarThresh, 10f, 150f);

                GUILayout.Box("Test Distance: " + Mathf.Round(SpeedResult));
                SpeedResult = GUILayout.HorizontalSlider(SpeedResult, 1f, 300f);
                GUILayout.Box("Test Result = " + Patches.Math.VisionSpeed(SpeedResult));

                if (GUILayout.Button("Reset to Default"))
                {
                    VisionSpeed = 1f;
                    CloseVisionSpeed = 1f;
                    FarVisionSpeed = 1f;
                    CloseFarThresh = 50f;
                }

                GUILayout.EndVertical();
                GUILayout.EndArea();

                //---------------------------------------------\\
                //---------------------------------------------\\

                GUILayout.BeginArea(new Rect(160, 100, 140, 160));
                GUILayout.BeginVertical();

                GUILayout.EndVertical();
                GUILayout.EndArea();

                //---------------------------------------------\\
                //---------------------------------------------\\

                GUILayout.BeginArea(new Rect(160, 229, 140, 160));
                GUILayout.BeginVertical();

                GUILayout.EndVertical();
                GUILayout.EndArea();

                //---------------------------------------------\\
                //---------------------------------------------\\

                GUILayout.BeginArea(new Rect(305, 100, 140, 250));
                GUILayout.BeginVertical();

                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
        }

        internal static class CursorSettings
        {
            private static readonly MethodInfo setCursorMethod;

            static CursorSettings()
            {
                var cursorType = PatchConstants.EftTypes.Single(x => x.GetMethod("SetCursor") != null);
                setCursorMethod = cursorType.GetMethod("SetCursor");
            }

            public static void SetCursor(ECursorType type)
            {
                setCursorMethod.Invoke(null, new object[] { type });
            }
        }
    }
}