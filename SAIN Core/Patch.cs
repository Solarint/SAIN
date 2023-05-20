using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN.Components;
using SAIN.Helpers;
using System.Reflection;

namespace SAIN
{
    public class AddComponentPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotOwner), "PreActivate");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref BotOwner __instance)
        {
            __instance.gameObject.AddComponent<SAINCoreComponent>();
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

            if (botState == EBotState.PreActive) return;

            __instance.gameObject.GetComponent<SAINCoreComponent>().Dispose();
        }
    }

    public class InitHelper : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotControllerClass), "Init");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref BotOwner __instance)
        {
            VectorHelpers.Init();
        }
    }
}