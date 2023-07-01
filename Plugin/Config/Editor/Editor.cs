using Aki.Reflection.Utils;
using BepInEx.Configuration;
using Comfort.Common;
using EFT.Console.Core;
using EFT.UI;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static SAIN.UserSettings.EditorSettings;

namespace SAIN
{
    public class Difficulty
    {
        public static float SpeedResult { get; private set; } = 100f;

        public class Editor
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

            public void OnGUI()
            {
                if (guiStatus)
                {
                    windowRect = GUI.Window(0, windowRect, WindowFunction, "SAIN AI Settings Editor");
                }
            }

            private int SelectedTab = 0;
            private const int GeneralTab = 0;
            private const int VisionTab = 1;
            private const int ShootTab = 2;
            private const int PersonalityTab = 3;
            private const int AdvancedTab = 4;

            private static Rect windowRect = new Rect(50, 50, 600, 650);
            private void WindowFunction(int TWCWindowID)
            {
                GUI.DragWindow(new Rect(0, 0, 600, 20));

                GUILayout.BeginArea(new Rect(0, 20, 600, 30));
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

                    VisionMenu();

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

                GUILayout.Box("No Bush ESP Enabled = " + NoBushESPToggle.Value);

                if (GUILayout.Button("No Bush ESP"))
                {
                    MenuClickSound();
                    NoBushESPToggle.Value = !NoBushESPToggle.Value;
                }

                GUILayout.Box("HeadShot Protection Enabled = " + HeadShotProtection.Value);

                if (GUILayout.Button("HeadShot Protection"))
                {
                    MenuClickSound();
                    HeadShotProtection.Value = !HeadShotProtection.Value;
                }

                GUILayout.Box("Faster CQB Reactions Enabled = " + FasterCQBReactions.Value);

                if (GUILayout.Button("FasterCQBReactions"))
                {
                    MenuClickSound();
                    FasterCQBReactions.Value = !FasterCQBReactions.Value;
                }

                GUILayout.Box("Looting Bots Mod Detected = " + SAINPlugin.LootingBotsLoaded);
                GUILayout.Box("Realism Mod Detected = " + SAINPlugin.RealismLoaded);

                if (GUILayout.Button("Reset General to Default"))
                {
                    MenuClickSound();
                    ResetGeneral();
                }
            }

            private void ResetGeneral()
            {
                NoBushESPToggle.Value = true;
                HeadShotProtection.Value = true;
                FasterCQBReactions.Value = true;
            }

            private void VisionMenu()
            {
                GUILayout.Box("Vision Speed Multipliers");
                GUILayout.Box("1.5 = 1.5x faster vision speed. Result will vary between bot types.");

                GUILayout.Box("Base: " + VisionSpeed.Value);
                VisionSpeed.Value = GUILayout.HorizontalSlider(VisionSpeed.Value, 0.25f, 3f);
                VisionSpeed.Value = Mathf.Round(VisionSpeed.Value * 10f) / 10f;

                GUILayout.Box("Close: " + CloseVisionSpeed.Value);
                CloseVisionSpeed.Value = GUILayout.HorizontalSlider(CloseVisionSpeed.Value, 0.25f, 3f);
                CloseVisionSpeed.Value = Mathf.Round(CloseVisionSpeed.Value * 10f) / 10f;

                GUILayout.Box("Far: " + FarVisionSpeed.Value);
                FarVisionSpeed.Value = GUILayout.HorizontalSlider(FarVisionSpeed.Value, 0.25f, 3f);
                FarVisionSpeed.Value = Mathf.Round(FarVisionSpeed.Value * 10f) / 10f;

                GUILayout.Box("Close/Far Threshold: " + CloseFarThresh.Value);
                CloseFarThresh.Value = GUILayout.HorizontalSlider(CloseFarThresh.Value, 10f, 150f);
                CloseFarThresh.Value = Mathf.Round(CloseFarThresh.Value);

                GUILayout.Box("Vision Speed Test");
                SpeedResult = GUILayout.HorizontalSlider(SpeedResult, 0f, 300f);
                SpeedResult = Mathf.Round(SpeedResult);
                float result = Patches.Math.VisionSpeed(SpeedResult);
                GUILayout.Box($"How much faster or slower bot vision will be. In Meters.");
                GUILayout.Box("Test Result Final Vision Speed Multiplier = " + result + " at " + SpeedResult + " meters away from the player");

                if (GUILayout.Button("Reset Vision to Default"))
                {
                    MenuClickSound();
                    ResetVision();
                }
            }

            private void ResetVision()
            {
                VisionSpeed.Value = 1f;
                CloseVisionSpeed.Value = 1f;
                FarVisionSpeed.Value = 1f;
                CloseFarThresh.Value = 50f;
                SpeedResult = 100f;
            }

            private void ShootMenu()
            {
                GUILayout.Box("Bot Shoot Settings");

                GUILayout.Box("Semiauto Firerate Multiplier: " + FireratMulti.Value);
                FireratMulti.Value = GUILayout.HorizontalSlider(FireratMulti.Value, 0.25f, 3f);
                FireratMulti.Value = Mathf.Round(FireratMulti.Value * 10f) / 10f;

                GUILayout.Box("Fullauto Burst Multiplier: " + BurstMulti.Value);
                BurstMulti.Value = GUILayout.HorizontalSlider(BurstMulti.Value, 0.25f, 3f);
                BurstMulti.Value = Mathf.Round(BurstMulti.Value * 10f) / 10f;

                GUILayout.Box("Bot Accuracy Multiplier: " + AccuracyMulti.Value);
                AccuracyMulti.Value = GUILayout.HorizontalSlider(AccuracyMulti.Value, 0.25f, 3f);
                AccuracyMulti.Value = Mathf.Round(AccuracyMulti.Value * 100f) / 100f;

                GUILayout.Box("Bot Recoil Settings. 1.5 = 1.5x more recoil and scatter per shot");

                GUILayout.Box("All Bots: " + BotRecoilGlobal.Value);
                BotRecoilGlobal.Value = GUILayout.HorizontalSlider(BotRecoilGlobal.Value, 0.25f, 3f);
                BotRecoilGlobal.Value = Mathf.Round(BotRecoilGlobal.Value * 10f) / 10f;

                GUILayout.Box("PMCs: " + PMCRecoil.Value);
                PMCRecoil.Value = GUILayout.HorizontalSlider(PMCRecoil.Value, 0.25f, 3f);
                PMCRecoil.Value = Mathf.Round(PMCRecoil.Value * 10f) / 10f;

                GUILayout.Box("Scav: " + ScavRecoil.Value);
                ScavRecoil.Value = GUILayout.HorizontalSlider(ScavRecoil.Value, 0.25f, 3f);
                ScavRecoil.Value = Mathf.Round(ScavRecoil.Value * 10f) / 10f;

                GUILayout.Box("Other Bots: " + OtherRecoil.Value);
                OtherRecoil.Value = GUILayout.HorizontalSlider(OtherRecoil.Value, 0.25f, 3f);
                OtherRecoil.Value = Mathf.Round(OtherRecoil.Value);

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
}