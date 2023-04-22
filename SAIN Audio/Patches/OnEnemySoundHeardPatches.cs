using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Audio.Helpers;
using System.Reflection;
using UnityEngine;

namespace SAIN.Audio.Patches
{
    public class sound : ModulePatch
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
}
