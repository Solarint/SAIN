using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using System.Linq;
using System.Reflection;
using static SAIN_Audio.Combat.Configs.FullAutoConfig;

namespace SAIN_Audio.Combat.Patches
{
    public class FiremodePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("ShootData")?.PropertyType?.GetMethod("Shoot");
        }
        [PatchPrefix]
        public static void PatchPrefix(ref BotOwner ___botOwner_0)
        {
            if (!BurstLengthToggle.Value || ___botOwner_0.AimingData == null)
            {
                return;
            }

            // Shortcuts
            float distance = ___botOwner_0.AimingData.LastDist2Target;
            var weapon = ___botOwner_0.WeaponManager.CurrentWeapon;

            var autoMode = Weapon.EFireMode.fullauto;
            var semiMode = Weapon.EFireMode.single;
            var burstMode = Weapon.EFireMode.burst;

            bool isAutoWeapon = weapon.WeapFireType.Contains(autoMode);
            bool isSemiWeapon = weapon.WeapFireType.Contains(semiMode);
            bool isBurstWeapon = weapon.WeapFireType.Contains(burstMode);

            bool isWeaponSetAuto = weapon.SelectedFireMode == autoMode;
            bool isWeaponSetSemi = weapon.SelectedFireMode == semiMode;
            bool isWeaponSetburst = weapon.SelectedFireMode == burstMode;

            // Swap Distances
            float semidist = 80f;
            float autodist = 75f;

            // Skip firemode swap if a bot is using a stationary weapon
            if (___botOwner_0.WeaponManager.Stationary.Taken)
            {
                return;
            }

            // Firemode Swap
            // Semiauto
            if (distance > semidist && isSemiWeapon && !isWeaponSetSemi)
            {
                weapon.FireMode.SetFireMode(semiMode);
                return;
            }

            // Fullauto and Burst
            if (distance <= autodist)
            {
                // Switch to full auto if the weapon has it
                if (isAutoWeapon && !isWeaponSetAuto)
                {
                    weapon.FireMode.SetFireMode(autoMode);
                }
                // Switch to burst fire if the weapon has it
                else if (isBurstWeapon && !isWeaponSetburst)
                {
                    weapon.FireMode.SetFireMode(burstMode);
                }
            }
        }
    }
}