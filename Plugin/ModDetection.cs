using BepInEx.Bootstrap;
using SAIN.Helpers;
using UnityEngine;
using static SAIN.AssemblyInfoClass;
using static SAIN.Editor.SAINLayout;

namespace SAIN;

public static class ModDetection
{
    static ModDetection()
    {
        ModsCheckTimer = Time.time + 5f;
    }

    public static void ManualUpdate()
    {
        if (!ModsChecked && ModsCheckTimer < Time.time && ModsCheckTimer > 0)
        {
            ModsChecked = true;
            CheckPlugins();
        }
    }

    public static bool RealismLoaded { get; private set; }
    public static bool QuestingBotsLoaded { get; private set; }
    public static bool ProjectFikaLoaded { get; private set; }
    public static bool ProjectFikaHeadlessLoaded { get; private set;  }

    public static void CheckPlugins()
    {
        if (Chainloader.PluginInfos.ContainsKey(FikaGUID))
        {
            ProjectFikaLoaded = true;
            Logger.LogInfo($"SAIN: Project Fika Detected.");
        }

        if (Chainloader.PluginInfos.ContainsKey(FikaHeadlessGUID))
        {
            ProjectFikaHeadlessLoaded = true;
            Logger.LogInfo($"SAIN: Project Fika Headless Detected.");
            new Patches.Hearing.FikaHeadlessTempFixPatch().Enable();
        }

        if (Chainloader.PluginInfos.ContainsKey(QuestingBotsGUID))
        {
            QuestingBotsLoaded = true;
            Logger.LogInfo($"SAIN: Questing Bots Detected.");
        }
        if (Chainloader.PluginInfos.ContainsKey(RealismModKey))
        {
            RealismLoaded = true;
            Logger.LogInfo($"SAIN: Realism Detected.");
        }
    }

    public static void UpdateArmorClassCoef()
    {
        if (RealismLoaded)
        {
            EFTCoreSettings.UpdateArmorClassCoef(3.5f);
            Logger.LogInfo($"Realism Detected, updating armor class number to reflect new armor classes...");
        }
        else
        {
            EFTCoreSettings.UpdateArmorClassCoef(6f);
        }
    }

    public static void ModDetectionGUI()
    {
        BeginVertical();

        BeginHorizontal();
        IsDetected(QuestingBotsLoaded, "Questing Bots");
        IsDetected(RealismLoaded, "Realism Mod");
        EndHorizontal();

        EndVertical();
    }

    private static void IsDetected(bool value, string name)
    {
        Label(name);
        Box(value ? "Detected" : "Not Detected");
    }

    private static readonly float ModsCheckTimer = -1f;
    private static bool ModsChecked = false;
}