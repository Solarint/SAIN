using EFT;
using HarmonyLib;
using SAIN.Components;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SAIN.Patches.Talk;

public class PlayerHurtPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.ApplyHitDebuff));
    }

    [PatchPrefix]
    public static void PatchPrefix(Player __instance, float damage)
    {
        if (__instance?.HealthController?.IsAlive == true &&
            __instance.IsAI &&
            (!__instance.MovementContext.PhysicalConditionIs(EPhysicalCondition.OnPainkillers) || damage > 4f))
        {
            __instance.Speaker?.Play(EPhraseTrigger.OnBeingHurt, __instance.HealthStatus, true, null);
        }
    }
}

public class PlayerTalkPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Player), nameof(Player.Say));
    }

    [PatchPrefix]
    public static bool PatchPrefix(Player __instance, EPhraseTrigger phrase, ETagStatus mask, bool aggressive)
    {
        switch (phrase)
        {
            case EPhraseTrigger.OnDeath:
            case EPhraseTrigger.OnBeingHurt:
            case EPhraseTrigger.OnAgony:
            case EPhraseTrigger.OnBreath:
                BotManagerComponent.Instance?.BotHearing.PlayerTalked(phrase, mask, __instance);
                return true;

            default:
                break;
        }

        if (__instance.IsAI)
        {
            if (SAINPlugin.LoadedPreset.GlobalSettings.Talk.DisableBotTalkPatching || !SAINEnableClass.GetSAIN(__instance.ProfileId, out _))
            {
                BotManagerComponent.Instance?.BotHearing.PlayerTalked(phrase, mask, __instance);
                return true;
            }
            return false;
        }

        BotManagerComponent.Instance?.BotHearing.PlayerTalked(phrase, mask, __instance);
        return true;
    }
}

public class BotTalkPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotTalk), nameof(BotTalk.Say));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotTalk __instance, EPhraseTrigger type)
    {
        if (SAINPlugin.LoadedPreset.GlobalSettings.Talk.DisableBotTalkPatching)
        {
            return true;
        }
        if (__instance.BotOwner_0?.HealthController?.IsAlive == false)
        {
            return true;
        }
        switch (type)
        {
            case EPhraseTrigger.OnDeath:
            case EPhraseTrigger.OnBeingHurt:
            case EPhraseTrigger.OnAgony:
            case EPhraseTrigger.OnBreath:
                return true;

            default:
                break;
        }
        if (!SAINEnableClass.GetSAIN(__instance.BotOwner_0.ProfileId, out BotComponent bot))
        {
            return true;
        }
        switch (type)
        {
            case EPhraseTrigger.HandBroken:
            case EPhraseTrigger.LegBroken:
                bot.Talk.GroupSay(type, null, false, 60);
                break;

            default:
                break;
        }
        return false;
    }
}

public class BotTalkManualUpdatePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotTalk), nameof(BotTalk.ManualUpdate));
    }

    [PatchPrefix]
    public static bool PatchPrefix(BotTalk __instance)
    {
        // If handling of bots talking is disabled, let the original method run
        return SAINPlugin.LoadedPreset.GlobalSettings.Talk.DisableBotTalkPatching ||
            !SAINEnableClass.GetSAIN(__instance.BotOwner_0.ProfileId, out _);
    }
}