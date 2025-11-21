using EFT;
using EFT.Visual;
using HarmonyLib;
using SAIN.Components.PlayerComponentSpace;
using SAIN.SAINComponent;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SAIN.Components;

public class FlashLightClass(PlayerComponent component) : PlayerComponentBase(component)
{
    public event Action<bool> OnLightToggle;

    public event Action<bool> OnLaserToggle;

    public List<TacticalComboVisualController> TacticalDevices { get; private set; }

    public bool UsingLight { get; private set; }
    public bool UsingLaser { get; private set; }

    public bool LaserOnly => !WhiteLight && !IRLight && (Laser || IRLaser);
    public bool DeviceActive => ActiveModes.Count > 0;

    public bool IRLaser => ActiveModes.Contains(DeviceMode.IRLaser);
    public bool IRLight => ActiveModes.Contains(DeviceMode.IRLight);
    public bool Laser => ActiveModes.Contains(DeviceMode.VisibleLaser);
    public bool WhiteLight => ActiveModes.Contains(DeviceMode.WhiteLight);
    public LightDetectionClass LightDetection { get; } = new LightDetectionClass(component);

    private readonly List<DeviceMode> activeModes = [];

    public void Update()
    {
    }

    public void CheckDevice()
    {
        CheckUsingLightModes();

        bool wasUsingLight = UsingLight;
        UsingLight = ActiveModes.Contains(DeviceMode.WhiteLight) || ActiveModes.Contains(DeviceMode.IRLight);
        if (wasUsingLight != UsingLight)
        {
            OnLightToggle?.Invoke(UsingLight);
        }

        bool wasUsingLaser = UsingLaser;
        UsingLaser = ActiveModes.Contains(DeviceMode.VisibleLaser) || ActiveModes.Contains(DeviceMode.IRLaser);
        if (wasUsingLaser != UsingLaser)
        {
            OnLaserToggle?.Invoke(UsingLaser);
        }
    }

    private void CheckUsingLightModes()
    {
        ActiveModes.Clear();
        Player player = Player;
        if (player == null) return;

        if (_tacticalModesField == null)
        {
#if DEBUG
            Logger.LogError("Could find not find _tacticalModesField");
#endif
            return;
        }

        // Get the firearmsController for the player, this will be their IsCurrentEnemy weapon
        Player.FirearmController firearmController = player.HandsController as Player.FirearmController;
        if (firearmController == null)
        {
#if DEBUG
            Logger.LogError("Could find not find firearmController");
#endif
            return;
        }

        // Get the list of tacticalComboVisualControllers for the current weapon (One should exist for every flashlight, laser, or combo device)
        Transform weaponRoot = firearmController.WeaponRoot;
        TacticalDevices = weaponRoot.GetComponentsInChildrenActiveIgnoreFirstLevel<TacticalComboVisualController>();
        if (TacticalDevices == null)
        {
#if DEBUG
            Logger.LogError("Could find not find tacticalComboVisualControllers");
#endif
            return;
        }

        bool foundWhiteLight = false;
        bool foundVisibleLaser = false;
        bool foundIRLight = false;
        bool FoundIRLaser = false;

        // Loop through all of the tacticalComboVisualControllers, then its modes, then that modes children, and look for a light
        foreach (TacticalComboVisualController tacticalComboVisualController in TacticalDevices)
        {
            List<Transform> tacticalModes = _tacticalModesField.GetValue(tacticalComboVisualController) as List<Transform>;
            foreach (var mode in tacticalModes)
            {
                // Skip disabled modes
                if (!mode.gameObject.activeInHierarchy)
                    continue;

                foreach (var child in mode.GetChildren())
                {
                    string name = child.name.ToLower();
                    if (!foundWhiteLight && name.StartsWith("light_0"))
                    {
#if DEBUG
                        if (_debugMode)
                            Logger.LogDebug($"[{player.name}] Found WhiteLight : Name:{name}");
#endif
                        foundWhiteLight = true;
                        ActiveModes.Add(DeviceMode.WhiteLight);
                    }
                    if (!foundVisibleLaser && name.StartsWith("vis_0"))
                    {
#if DEBUG
                        if (_debugMode)
                            Logger.LogDebug($"[{player.name}] Found VisibleLaser : Name:{name}");
#endif
                        foundVisibleLaser = true;
                        ActiveModes.Add(DeviceMode.VisibleLaser);
                    }
                    if (!foundIRLight && name.StartsWith("il_0"))
                    {
#if DEBUG
                        if (_debugMode)
                            Logger.LogDebug($"[{player.name}] Found IRLight : Name:{name}");
#endif
                        foundIRLight = true;
                        ActiveModes.Add(DeviceMode.IRLight);
                    }
                    if (!FoundIRLaser && name.StartsWith("ir_0"))
                    {
#if DEBUG
                        if (_debugMode)
                            Logger.LogDebug($"[{player.name}] Found IRLaser : Name:{name}");
#endif
                        FoundIRLaser = true;
                        ActiveModes.Add(DeviceMode.IRLaser);
                    }
                }
            }
        }
    }

    private static bool _debugMode => SAINPlugin.LoadedPreset.GlobalSettings.General.Flashlight.DebugFlash;

    public List<DeviceMode> ActiveModes => activeModes;

    private static readonly FieldInfo _tacticalModesField = AccessTools.Field(typeof(TacticalComboVisualController), "list_0");
}