using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Audio.Helpers;
using System.Reflection;
using UnityEngine;

namespace SAIN.Audio.Patches
{
    public class InitiateShotPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.FirearmController), "InitiateShot");
        }
        [PatchPrefix]
        public static void PatchPrefix(Player.FirearmController __instance, IWeapon weapon, BulletClass ammo)
        {
            Player playerInstance = AccessTools.FieldRefAccess<Player.FirearmController, Player>(__instance, "_player");

            GunshotRange playsound = new GunshotRange();

            playerInstance.StartCoroutine(playsound.OnMakingShotCoroutine(weapon, playerInstance, ammo));
        }
    }
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
    public class HearingSensorDisablePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("HearingSensor")?.PropertyType?.GetMethod("Init");
        }
        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return false;
        }
    }
}
