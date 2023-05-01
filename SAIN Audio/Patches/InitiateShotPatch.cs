using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN_Movement.Helpers;
using System.Reflection;

namespace SAIN_Movement.Patches
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
}
