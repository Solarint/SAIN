using Aki.Reflection.Patching;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace SAIN_Movement.Patches
{
    public class TryPlayShootSoundPatch : ModulePatch
    {
        private static PropertyInfo _boolean_0;
        protected override MethodBase GetTargetMethod()
        {
            _boolean_0 = AccessTools.Property(typeof(AiDataClass), "Boolean_0");

            return AccessTools.Method(typeof(AiDataClass), "TryPlayShootSound");
        }
        [PatchPrefix]
        public static bool PatchPrefix(AiDataClass __instance, ref float ___float_0)
        {
            bool flag = ___float_0 < Time.time;

            _boolean_0.SetValue(__instance, true);

            if (flag)
            {
                ___float_0 = Time.time + 1f;
            }

            return false;
        }
    }
}
