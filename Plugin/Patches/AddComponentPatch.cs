using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using System.Reflection;
using System;
using SAIN.SAINComponent;
using SAIN.Components.BotController;

namespace SAIN.Patches.Components
{
    public class AddComponentPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotOwner), "method_10");
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
                Logger.LogError($" SAIN Add Component Error: {ex}");
            }
        }
    }

    public class DisposeComponentPatch : ModulePatch
    {
        private static FieldInfo _ebotState_0;

        protected override MethodBase GetTargetMethod()
        {
            _ebotState_0 = AccessTools.Field(typeof(BotOwner), "ebotState_0");
            return AccessTools.Method(typeof(BotOwner), "Dispose");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref BotOwner __instance)
        {
            EBotState botState = (EBotState)_ebotState_0.GetValue(__instance);

            if (botState == EBotState.PreActive)
            {
                return;
            }

            try
            {
                //SAINPlugin.SAINBotControllerComponent.RemoveBot(__instance.ProfileId);
            }
            catch (Exception ex)
            {
                Logger.LogError($" SAIN Dispose Component Error: {ex}");
            }
        }
    }
}
