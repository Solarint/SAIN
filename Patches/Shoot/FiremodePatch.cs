using Aki.Reflection.Patching;
using EFT;
using System.Linq;
using System.Reflection;
using static EFT.InventoryLogic.Weapon;

namespace SAIN.Patches
{
    public class FiremodePatch : ModulePatch
    {
        private const float SemiAutoSwapDist = 40f;
        private const float FullAutoSwapDist = 30f;

        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("ShootData")?.PropertyType?.GetMethod("Shoot");
        }
        [PatchPrefix]
        public static void PatchPrefix(ref BotOwner ___botOwner_0)
        {
            if (___botOwner_0.AimingData == null)
            {
                return;
            }

            float distance = ___botOwner_0.AimingData.LastDist2Target;
            var weapon = ___botOwner_0.WeaponManager.CurrentWeapon;

            if (!___botOwner_0.WeaponManager.Stationary.Taken)
            {
                if (distance >= SemiAutoSwapDist && weapon.WeapFireType.Contains(EFireMode.single) && weapon.SelectedFireMode != EFireMode.single)
                {
                    weapon.FireMode.SetFireMode(EFireMode.single);
                    return;
                }

                if (distance <= FullAutoSwapDist)
                {
                    if (weapon.WeapFireType.Contains(EFireMode.fullauto) && weapon.SelectedFireMode != EFireMode.fullauto)
                    {
                        weapon.FireMode.SetFireMode(EFireMode.fullauto);
                    }
                    else if (weapon.WeapFireType.Contains(EFireMode.burst) && weapon.SelectedFireMode != EFireMode.burst)
                    {
                        weapon.FireMode.SetFireMode(EFireMode.burst);
                    }
                }
            }
        }
    }
}