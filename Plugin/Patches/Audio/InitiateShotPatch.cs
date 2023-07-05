using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Helpers;
using System.Reflection;

namespace SAIN.Patches
{
    public class InitiateShotPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.FirearmController), "InitiateShot");
        }
        [PatchPostfix]
        public static void PatchPostfix(Player.FirearmController __instance, IWeapon weapon, BulletClass ammo)
        {
            Player playerInstance = AccessTools.FieldRefAccess<Player.FirearmController, Player>(__instance, "_player");
            GunshotRange.OnMakingShot(weapon, playerInstance, ammo);
        }
    }
}
