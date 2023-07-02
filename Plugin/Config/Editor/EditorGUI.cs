using Aki.Reflection.Utils;
using BepInEx.Configuration;
using Comfort.Common;
using EFT.Console.Core;
using EFT.UI;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static SAIN.Editor.EditorSettings;

namespace SAIN.Editor
{
        public class EditorGUI
        {
            internal static ConfigEntry<KeyboardShortcut> TogglePanel;

            private static GameObject input;

            private static bool guiStatus = false;

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

            private bool CloseEditor;

            public void Update()
            {
                if (Input.GetKeyDown(TogglePanel.Value.MainKey) || CloseEditor)
                {
                    CloseEditor = false;
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

            private void ApplyStyle()
            {
                GUI.skin.window.normal.background = Texture2D.blackTexture;
                GUI.skin.window.normal.textColor = Color.white;
                var slider = GUI.skin.horizontalSlider;
                var thumb = GUI.skin.horizontalSliderThumb;
                slider.normal.background = null;
                slider.fixedHeight = 20f;
                slider.alignment = TextAnchor.MiddleCenter;
                thumb.fixedHeight = 20f;
                thumb.alignment = TextAnchor.MiddleCenter;
                GUI.color = Color.white;
            }

            public void OnGUI()
            {
                if (guiStatus)
                {
                    ApplyStyle();
                    MainWindowRect = GUI.Window(0, MainWindowRect, MainWindow, "SAIN AI Settings Editor"); 
                    BuilderUtil.OnGUI();
                }
            }

            public static Color DarkGray = new Color(0.2f, 0.2f, 0.2f, 1f);

            private int SelectedTab = 0;
            private const int GeneralTab = 0;
            private const int VisionTab = 1;
            private const int ShootTab = 2;
            private const int PersonalityTab = 3;
            private const int AdvancedTab = 4;

            private static Rect MainWindowRect = new Rect(50, 50, 600, 650);

            private void MainWindow(int TWCWindowID)
            {
                //GUI.DrawTexture(new Rect(0, 20, MainWindowRect.width, MainWindowRect.height - 20), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, DarkGray, 0, 0);

                GUI.DragWindow(new Rect(0, 0, 600, 20));
                GUILayout.BeginArea(new Rect(0, 20, 580, 30));
                GUILayout.BeginVertical();

                SelectedTab = GUILayout.Toolbar(SelectedTab, new string[] { "General", "Vision", "Shoot", "Personalties", "Advanced" });

                GUILayout.EndVertical();
                GUILayout.EndArea();

                if (SelectedTab == GeneralTab)
                {
                    GUILayout.BeginArea(new Rect(25, 55, 550, 500));
                    GUILayout.BeginVertical();

                    GeneralMenu();

                    GUILayout.EndVertical();
                    GUILayout.EndArea();
                }
                else if (SelectedTab == VisionTab)
                {
                    GUILayout.BeginArea(new Rect(25, 55, 550, 500));
                    GUILayout.BeginVertical();

                    GUILayout.Box("Vision Speed Multipliers");
                    GUILayout.Box("1.5 = 1.5x faster vision speed. Result will vary between bot types.");

                    BuilderUtil.DrawSlider("Base Speed", VisionSpeed, 0.25f, 3f, 100f, "The Base vision speed multiplier. Bots will see this much faster, or slower, at any range.");
                    BuilderUtil.DrawSlider("Close Speed", CloseVisionSpeed, 0.25f, 3f, 100f, "Vision speed multiplier at close range. Bots will see this much faster, or slower, at close range.");
                    BuilderUtil.DrawSlider("Far Speed", FarVisionSpeed, 0.25f, 3f, 100f, "Vision speed multiplier at Far range. Bots will see this much faster, or slower, at Far range.");
                    BuilderUtil.DrawSlider("Close/Far Threshold", CloseFarThresh, 10f, 150f, 1f, "The Distance that defines what is Close Or Far for the above options.");

                    GUILayout.Box("Vision Speed Test");
                    BuilderUtil.DrawSlider("Test Distance", TestDistance, 0f, 500f);
                    float result = Patches.Math.VisionSpeed(TestDistance.Value);
                    GUILayout.Box($"How much faster or slower bot vision will be. In Meters.");
                    GUILayout.Box("Test Result Final Vision Speed Multiplier = " + result + " at " + TestDistance + " meters away from the player");

                    if (GUILayout.Button("Reset Vision to Default"))
                    {
                        MenuClickSound();
                        ResetVision();
                    }

                    GUILayout.EndVertical();
                    GUILayout.EndArea();
                }
                else if (SelectedTab == ShootTab)
                {
                    GUILayout.BeginArea(new Rect(25, 55, 550, 500));
                    GUILayout.BeginVertical();

                    ShootMenu();

                    GUILayout.EndVertical();
                    GUILayout.EndArea();
                }
                else if (SelectedTab == PersonalityTab)
                {
                    GUILayout.BeginArea(new Rect(25, 55, 550, 500));
                    GUILayout.BeginVertical();

                    PersonalityMenu();

                    GUILayout.EndVertical();
                    GUILayout.EndArea();
                }
                else if (SelectedTab == AdvancedTab)
                {
                    GUILayout.BeginArea(new Rect(25, 55, 550, 500));
                    GUILayout.BeginVertical();

                    AdvancedMenu();

                    GUILayout.EndVertical();
                    GUILayout.EndArea();
                }

                GUILayout.BeginArea(new Rect(10, 590, 580, 50));
                GUILayout.BeginVertical();

                if (GUILayout.Button("Close Editor"))
                {
                    ResetClickSound();
                    CloseEditor = true;
                }
                GUILayout.Box("GUI Style Referenced from SamSWAT's Weather Editor");

                GUILayout.EndVertical();
                GUILayout.EndArea();
            }


            private void GeneralMenu()
            {
                GUILayout.Box("General Settings");

                GUILayout.BeginHorizontal();
                NoBushESPToggle.Value = Button("No Bush ESP", NoBushESPToggle.Value);
                HeadShotProtection.Value = Button("HeadShot Protection", HeadShotProtection.Value);
                FasterCQBReactions.Value = Button("Faster CQB Reactions", FasterCQBReactions.Value);
                GUILayout.EndHorizontal();

                GUILayout.Box("Mod Detection");
                GUILayout.BeginHorizontal();
                SingleTextBool("Looting Bots", SAINPlugin.LootingBotsLoaded);
                SingleTextBool("Realism Mod", SAINPlugin.RealismLoaded);
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Reset General to Default"))
                {
                    MenuClickSound();
                    ResetGeneral();
                }
            }

            private void SingleTextBool(string text, bool value)
            {
                Color old = GUI.backgroundColor;
                if (value)
                {
                    GUI.backgroundColor = Color.green;
                }
                else
                {
                    GUI.backgroundColor = Color.red;
                }
                string status = value ? ": Detected" : ": Not Detected";
                GUILayout.Box(text + status);
                GUI.backgroundColor = old;
            }

            private bool Button(string name, bool value)
            {
                Color old = GUI.backgroundColor;
                if (value)
                {
                    GUI.backgroundColor = Color.green;
                }
                else
                {
                    GUI.backgroundColor = Color.red;
                }
                if (GUILayout.Button($"{name} : {Toggle(value)}"))
                {
                    MenuClickSound();
                    value = !value;
                }
                GUI.backgroundColor = old;
                return value;
            }

            private string Toggle(bool value)
            {
                return value ? "On" : "Off";
            }

            private void ResetGeneral()
            {
                NoBushESPToggle.Value = true;
                HeadShotProtection.Value = true;
                FasterCQBReactions.Value = true;
            }

            public static GUIStyle TextStyle;
            public static GUIStyle TextRightAlign;
            public static GUIStyle TextLeftAlign;

            private void ResetVision()
            {
                VisionSpeed.Value = 1f;
                CloseVisionSpeed.Value = 1f;
                FarVisionSpeed.Value = 1f;
                CloseFarThresh.Value = 50f;
                TestDistance.Value = 50f;
            }

            private void ShootMenu()
            {
                GUILayout.Box("Bot Shoot Settings");
                BuilderUtil.DrawSlider("Semiauto Firerate Multiplier", FireratMulti, 0.25f, 3f, 10f);
                BuilderUtil.DrawSlider("Fullauto Burst Multiplier", BurstMulti, 0.25f, 3f, 10f);
                BuilderUtil.DrawSlider("Bot Accuracy Multiplier", AccuracyMulti, 0.25f, 3f, 10f);
                BuilderUtil.DrawSlider("Semiauto Firerate Multiplier", FireratMulti, 0.25f, 3f, 100f);

                GUILayout.Box("Bot Recoil Settings. 1.5 = 1.5x more recoil and scatter per shot");
                BuilderUtil.DrawSlider("All Bots", BotRecoilGlobal, 0.25f, 3f, 10f);
                BuilderUtil.DrawSlider("PMCs", PMCRecoil, 0.25f, 3f, 10f);
                BuilderUtil.DrawSlider("Scavs", ScavRecoil, 0.25f, 3f, 10f);
                BuilderUtil.DrawSlider("Other Bots", OtherRecoil, 0.25f, 3f, 10f);

                if (GUILayout.Button("Reset Shoot to Default"))
                {
                    MenuClickSound(); 
                    ResetShoot();
                }
            }

            private void ResetShoot()
            {
                FireratMulti.Value = 1f;
                BurstMulti.Value = 1f;
                AccuracyMulti.Value = 1f;
                BotRecoilGlobal.Value = 1f;
                PMCRecoil.Value = 1f;
                ScavRecoil.Value = 1f;
                OtherRecoil.Value = 1f;
            }

            private void PersonalityMenu()
            {
                GUILayout.Box("Personality Settings");
                GUILayout.Box("For The Memes. Recommended not to use these during normal gameplay!");
                GUILayout.Box("Bots will be more predictable and exploitable.");

                GUILayout.Box("All Bots are Gigachads = " + AllGigaChads.Value);

                if (GUILayout.Button("Gigachads Only"))
                {
                    MenuClickSound();
                    AllGigaChads.Value = !AllGigaChads.Value;
                    AllChads.Value = false;
                    AllRats.Value = false;
                }

                GUILayout.Box("All Bots are Chads = " + AllChads.Value);

                if (GUILayout.Button("Chads Only"))
                {
                    MenuClickSound();
                    AllGigaChads.Value = false;
                    AllChads.Value = !AllChads.Value;
                    AllRats.Value = false;
                }

                GUILayout.Box("All Bots are Rats = " + AllRats.Value);

                if (GUILayout.Button("Rats, rats, were the rats"))
                {
                    MenuClickSound();
                    AllGigaChads.Value = false;
                    AllChads.Value = false;
                    AllRats.Value = !AllRats.Value;
                }

                BuilderUtil.DrawSlider("Random Any Bot GigaChad Chance", RandomGigaChadChance, 0.25f, 3f, 10f);
                float chance = RandomGigaChadChance.Value;
                GUILayout.Box("Random GigaChad Chance: " + chance);
                GUILayout.Box("Does Not Affect Gear or Level Based Personality Selection");
                chance = GUILayout.HorizontalSlider(chance, 0f, 100f);
                RandomGigaChadChance.Value = Mathf.RoundToInt(chance);

                chance = RandomChadChance.Value;
                GUILayout.Box("Random Chad Chance: " + chance);
                GUILayout.Box("Does Not Affect Gear or Level Based Personality Selection");
                chance = GUILayout.HorizontalSlider(chance, 0f, 100f);
                RandomChadChance.Value = Mathf.RoundToInt(chance);

                chance = RandomRatChance.Value;
                GUILayout.Box("Random Rat Chance: " + chance);
                chance = GUILayout.HorizontalSlider(chance, 0f, 100f);
                RandomRatChance.Value = Mathf.RoundToInt(chance);

                chance = RandomCowardChance.Value;
                GUILayout.Box("Random Coward Chance: " + chance);
                chance = GUILayout.HorizontalSlider(chance, 0f, 100f);
                RandomCowardChance.Value = Mathf.RoundToInt(chance);

                if (GUILayout.Button("Reset Personalities to Default"))
                {
                    MenuClickSound();
                    ResetPersonality();
                }
            }

            private void ResetPersonality()
            {
                RandomGigaChadChance.Value = 3;
                RandomChadChance.Value = 3;
                RandomRatChance.Value = 30;
                RandomCowardChance.Value = 30;
                AllGigaChads.Value = false;
                AllChads.Value = false;
                AllRats.Value = false;
            }

            private void AdvancedMenu()
            {
                GUILayout.Box("Advanced Settings. Edit at your own risk.");

                GUILayout.Box("MaxScatter: " + MaxScatter.Value);
                MaxScatter.Value = GUILayout.HorizontalSlider(MaxScatter.Value, 0.5f, 10f);
                MaxScatter.Value = Mathf.Round(MaxScatter.Value * 100f) / 100f;

                GUILayout.Box("AddRecoil: " + AddRecoil.Value);
                AddRecoil.Value = GUILayout.HorizontalSlider(AddRecoil.Value, -1f, 1f);
                AddRecoil.Value = Mathf.Round(AddRecoil.Value * 100f) / 100f;

                GUILayout.Box("RecoilDecay: " + RecoilDecay.Value);
                RecoilDecay.Value = GUILayout.HorizontalSlider(RecoilDecay.Value, 0.8f, 0.99f);
                RecoilDecay.Value = Mathf.Round(RecoilDecay.Value * 1000f) / 1000f;

                if (GUILayout.Button("Reset Advanced to Default"))
                {
                    MenuClickSound();
                    ResetAdvanced();
                }
                if (GUILayout.Button("Reset ALL Settings"))
                {
                    ResetClickSound();
                    ResetAll();
                }
                if (GUILayout.Button("Mysterious Button"))
                {
                    Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.PlayerIsDead);
                    Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.QuestFailed);
                }
                if (GUILayout.Button("Edit Editor GUI"))
                {
                    BuilderUtil.OpenEditWindow = true;
                }
            }

            private void ResetAdvanced()
            {
                MaxScatter.Value = 2f;
                AddRecoil.Value = 0f;
                RecoilDecay.Value = 0.85f;
            }

            private void ResetAll()
            {
                ResetGeneral();
                ResetVision();
                ResetShoot();
                ResetPersonality();
                ResetAdvanced();
            }

            private void MenuClickSound()
            {
                Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ButtonClick);
            }
            private void ResetClickSound()
            {
                Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.BackpackClose);
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