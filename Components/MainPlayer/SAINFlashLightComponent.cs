using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Visual;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SAIN.Components
{
    public class SAINFlashLightComponent : MonoBehaviour
    {
        private Player Player { get; set; }
        protected static ManualLogSource Logger { get; private set; }

        private LightDetectionClass _lightDetection;

        private void Awake()
        {
            Player = GetComponent<Player>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(SAINFlashLightComponent));
            _lightDetection = new LightDetectionClass();
        }

        private void Update()
        {
            // Check if the bot is alive before continuing
            if (Player == null || Player.HealthController.IsAlive == false)
            {
                Dispose();
                return;
            }

            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld == null || gameWorld.RegisteredPlayers == null)
            {
                Dispose();
                return;
            }

            if (FrameCount >= 5)
            {
                FrameCount = 0;
                if (Player.IsAI)
                {
                    _lightDetection.DetectAndInvestigateFlashlight(Player);
                }
                if (Player.IsYourPlayer)
                {
                    bool visibleLight = (WhiteLight || Laser);
                    bool irLight = (IRLight || IRLaser);
                    if (visibleLight || irLight)
                    {
                        _lightDetection.CreateDetectionPoints(Player, visibleLight);
                    }
                }
            }
            FrameCount++;
        }

        private float FrameCount = 0;

        public void Dispose()
        {
            try
            {
                StopAllCoroutines();
                Destroy(this);
            }
            catch { }
        }

        public bool IRLaser { get; private set; }
        public bool IRLight { get; private set; }
        public bool Laser { get; private set; }
        public bool WhiteLight { get; private set; }

        public void CheckDevice(Player player, FieldInfo _tacticalModesField)
        {
            if (Logger == null) Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(SAINFlashLightComponent));

            if (player == null) return;

            if (_tacticalModesField == null)
            {
                Logger.LogError("Could find not find _tacticalModesField");
                return;
            }

            // Get the firearmsController for the player, this will be their IsCurrentEnemy weapon
            Player.FirearmController firearmController = player.HandsController as Player.FirearmController;
            if (firearmController == null)
            {
                Logger.LogError("Could find not find firearmController");
                return;
            }

            // Get the list of tacticalComboVisualControllers for the current weapon (One should exist for every flashlight, laser, or combo device)
            Transform weaponRoot = firearmController.WeaponRoot;
            List<TacticalComboVisualController> tacticalComboVisualControllers = weaponRoot.GetComponentsInChildrenActiveIgnoreFirstLevel<TacticalComboVisualController>();
            if (tacticalComboVisualControllers == null)
            {
                Logger.LogError("Could find not find tacticalComboVisualControllers");
                return;
            }

            WhiteLight = false;
            Laser = false;
            IRLight = false;
            IRLaser = false;
            // Loop through all of the tacticalComboVisualControllers, then its modes, then that modes children, and look for a light
            foreach (TacticalComboVisualController tacticalComboVisualController in tacticalComboVisualControllers)
            {
                List<Transform> tacticalModes = _tacticalModesField.GetValue(tacticalComboVisualController) as List<Transform>;

                if (CheckWhiteLight(tacticalModes))
                {
                    if (DebugFlash) Logger.LogDebug("Found Light!");
                    WhiteLight = true;
                }

                if (CheckVisibleLaser(tacticalModes))
                {
                    if (DebugFlash) Logger.LogDebug("Found Visible Laser!");
                    Laser = true;
                }

                if (CheckIRLight(tacticalModes))
                {
                    if (DebugFlash) Logger.LogDebug("Found IR Light!");
                    IRLight = true;
                }

                if (CheckIRLaser(tacticalModes))
                {
                    if (DebugFlash) Logger.LogDebug("Found IR Laser!");
                    IRLaser = true;
                }
            }
        }
        static bool DebugFlash => SAINPlugin.LoadedPreset.GlobalSettings.Flashlight.DebugFlash;

        private bool CheckVisibleLaser(List<Transform> tacticalModes)
        {
            foreach (Transform tacticalMode in tacticalModes)
            {
                // Skip disabled modes
                if (!tacticalMode.gameObject.activeInHierarchy) continue;

                // Try to find a "light" under the mode, here's hoping BSG stay consistent
                foreach (Transform child in tacticalMode.GetChildren())
                {
                    if (child.name.StartsWith("VIS_"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CheckIRLight(List<Transform> tacticalModes)
        {
            foreach (Transform tacticalMode in tacticalModes)
            {
                // Skip disabled modes
                if (!tacticalMode.gameObject.activeInHierarchy) continue;

                // Try to find a "VolumetricLight", hopefully only visible flashlights have these
                IkLight irLight = tacticalMode.GetComponentInChildren<IkLight>();
                if (irLight != null)
                {
                    return true;
                }
            }
            return false;
        }

        private bool CheckIRLaser(List<Transform> tacticalModes)
        {
            foreach (Transform tacticalMode in tacticalModes)
            {
                // Skip disabled modes
                if (!tacticalMode.gameObject.activeInHierarchy) continue;

                // Try to find a "light" under the mode, here's hoping BSG stay consistent
                foreach (Transform child in tacticalMode.GetChildren())
                {
                    if (child.name.StartsWith("IR_"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CheckWhiteLight(List<Transform> tacticalModes)
        {
            foreach (Transform tacticalMode in tacticalModes)
            {
                // Skip disabled modes
                if (!tacticalMode.gameObject.activeInHierarchy) continue;

                // Try to find a "VolumetricLight", hopefully only visible flashlights have these
                VolumetricLight volumetricLight = tacticalMode.GetComponentInChildren<VolumetricLight>();
                if (volumetricLight != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}