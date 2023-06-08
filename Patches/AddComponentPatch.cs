using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN.Components;
using System.Reflection;
using SAIN.Helpers;

namespace SAIN.Patches
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
            if (__instance.IsRole(WildSpawnType.marksman))
            {
                return;
            }

            __instance.GetOrAddComponent<SAINComponent>();
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

            var component = __instance.GetComponent<SAINComponent>();
            component?.Dispose();
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
