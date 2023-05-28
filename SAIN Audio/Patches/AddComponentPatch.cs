using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN_Movement.Components;
using System.Reflection;
using UnityEngine;

namespace SAIN_Movement.Patches
{
    public class ComponentAddPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotOwner), "PreActivate");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref BotOwner __instance)
        {
            __instance.gameObject.AddComponent<AudioComponent>();
        }
    }

    public class HearingSensorDisablePatch : ModulePatch
    {
        private static PropertyInfo HearingSensor;

        protected override MethodBase GetTargetMethod()
        {
            HearingSensor = AccessTools.Property(typeof(BotOwner), "HearingSensor");
            return AccessTools.Method(HearingSensor.PropertyType, "method_0");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref IAIDetails player, ref Vector3 position, ref float power, ref AISoundType type)
        {
            var component = ___botOwner_0.GetComponent<AudioComponent>();

            if (component != null)
            {
                component.HearSound(player, position, power, type);
            }

            return false;
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

            __instance.GetComponent<AudioComponent>().Dispose();
        }
    }
}
