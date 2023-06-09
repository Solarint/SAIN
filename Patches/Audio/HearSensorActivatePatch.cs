using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN.Components;
using System.Reflection;
using UnityEngine;

namespace SAIN.Patches
{
    public class HearingSensorPatch : ModulePatch
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
            var component = ___botOwner_0.GetComponent<HearingSensorClass>();

            if (component != null)
            {
                component.HearSound(player, position, power, type);
            }

            return false;
        }
    }
}
