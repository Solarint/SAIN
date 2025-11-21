using EFT;
using HarmonyLib;
using SAIN.Components;
using SAIN.Components.BotController;
using SAIN.Components.PlayerComponentSpace;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using UnityEngine;

namespace SAIN.Patches.Components;

internal class AddBotComponentPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotOwner), nameof(BotOwner.PreActivate));
    }

    [PatchPostfix]
    public static void PatchPostfix(ref BotOwner __instance)
    {
        try
        {
            BotSpawnController.Instance.AddBot(__instance);
        }
        catch (Exception ex)
        {
            Logger.LogError($" SAIN Add Bot Error: {ex}");
        }
    }
}

internal class ActivateBotComponentPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotOwner), nameof(BotOwner.method_10));
    }

    [PatchPostfix]
    public static void PatchPostfix(ref BotOwner __instance)
    {
        __instance.GetComponent<BotComponent>()?.Activate(__instance);
    }
}

internal class AddGameWorldPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GameWorldUnityTickListener), nameof(GameWorldUnityTickListener.Create));
    }

    [PatchPostfix]
    public static void PatchPostfix(GameObject gameObject, GameWorld gameWorld)
    {
        if (gameWorld is HideoutGameWorld)
            return;

        try
        {
            if (GameWorldComponent.Instance != null)
            {
                Logger.LogWarning($"Old SAIN Gameworld is not null! Destroying...");
                GameWorldComponent.Instance.DestroyComponent();
            }
            GameWorldComponent gameWorldComponent = gameWorld.gameObject.AddComponent<GameWorldComponent>();
            BotManagerComponent botController = gameWorld.gameObject.AddComponent<BotManagerComponent>();
            gameWorldComponent.Init(gameWorld, botController);
            botController.Activate(gameWorldComponent);
        }
        catch (Exception ex)
        {
            Logger.LogError($" SAIN Init Gameworld Error: {ex}");
        }
    }
}

internal class WorldTickPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.DoWorldTick));
    }

    [PatchPostfix]
    public static void Patch(GameWorld __instance, float dt)
    {
        GameWorldComponent.Instance?.WorldTick(dt);
    }
}

internal class GetBotController : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotsController), nameof(BotsController.Init));
    }

    [PatchPostfix]
    public static void Patch(BotsController __instance)
    {
        GameWorldComponent gameWorld = GameWorldComponent.Instance;
        if (gameWorld == null)
        {
            Logger.LogError("gameWorld Null");
            return;
        }
        gameWorld.Activate(__instance);
    }
}

/// <summary>
/// Bot update is handled by sain's gameworld component, disable it here if that exists.
/// </summary>
internal class DisableBotUpdateByUnityPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotsController), nameof(BotsController.method_0));
    }

    [PatchPrefix]
    public static bool Patch()
    {
        if (GameWorldComponent.Instance == null)
        {
            return true;
        }
        return false;
    }
}

internal class PlayerLateUpdatePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.LateUpdate));
    }

    [PatchPostfix]
    public static void Patch(Player __instance)
    {
        if (GameWorldComponent.TryGetPlayerComponent(__instance, out PlayerComponent playerComponent))
        {
            playerComponent.ManualLateUpdate();
        }
    }
}