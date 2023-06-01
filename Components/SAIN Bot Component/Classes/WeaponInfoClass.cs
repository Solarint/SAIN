using BepInEx.Logging;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;
using UnityEngine.UIElements;

namespace SAIN.Classes
{
    public class WeaponInfo : SAINBot
    {
        public WeaponInfo(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            Modifiers = new EquipmentModifiers(bot);
        }

        public void ManualUpdate()
        {
            if (!SAIN.BotActive || SAIN.GameIsEnding)
            {
                return;
            }

            if (Modifiers == null)
            {
                Modifiers = new EquipmentModifiers(BotOwner);
            }

            if (ShouldIRecalc)
            {
                LastCheckedWeapon = CurrentWeapon.Template;
                FinalModifier = Modifiers.FinalModifier;

                //Logger.LogInfo($"Final Modifier: [{FinalModifier}] Class: [{WeaponClass}] Caliber: [{AmmoCaliber}] Role: [{Modifiers.Role}]");
            }
        }

        public float FinalModifier { get; private set; }

        public string WeaponClass => CurrentWeapon.Template.weapClass;

        public string AmmoCaliber => CurrentWeapon.CurrentAmmoTemplate.Caliber;

        public Weapon CurrentWeapon => BotOwner.WeaponManager.CurrentWeapon;

        private readonly ManualLogSource Logger;

        private WeaponTemplate LastCheckedWeapon;

        private bool ShouldIRecalc => BotOwner?.WeaponManager?.IsWeaponReady == true && BotOwner?.WeaponManager?.CurrentWeapon?.Template != LastCheckedWeapon;

        public EquipmentModifiers Modifiers { get; private set; }

        public float PerMeter
        {
            get
            {
                float perMeter;
                // Higher is faster firerate // Selects a the time for 1 second of wait time for every x meters
                switch (WeaponClass)
                {
                    case "assaultCarbine":
                    case "assaultRifle":
                    case "machinegun":
                        perMeter = 120f;
                        break;

                    case "smg":
                        perMeter = 130f;
                        break;

                    case "pistol":
                        perMeter = 90f;
                        break;

                    case "marksmanRifle":
                        perMeter = 90f;
                        break;

                    case "sniperRifle":
                        perMeter = 70f;
                        // VSS and VAL Exception
                        if (AmmoCaliber == "9x39")
                        {
                            perMeter = 120f;
                        }
                        break;

                    case "shotgun":
                    case "grenadeLauncher":
                    case "specialWeapon":
                        perMeter = 70f;
                        break;

                    default:
                        perMeter = 120f;
                        break;
                }
                return perMeter;
            }
        }
    }

    public class EquipmentModifiers : SAINBot
    {
        public EquipmentModifiers(BotOwner bot) : base(bot) { }

        private const float WeaponClassScaling = 0.3f;
        private const float RecoilScaling = 0.2f;
        private const float ErgoScaling = 0.1f;
        private const float AmmoTypeScaling = 0.2f;
        private const float BotRoleScaling = 0.3f;

        public float FinalModifier => WeaponClassModifier * RecoilModifier * ErgoModifier * AmmoTypeModifier * BotRoleModifier;
        public float AmmoTypeModifier
        {
            get
            {
                float modifier = Modifiers.Ammo(AmmoCaliber);
                return Scaling(modifier, 0f, 1f, 1 - AmmoTypeScaling, 1 + AmmoTypeScaling);
            }
        }
        public float BotRoleModifier
        {
            get
            {
                float modifier = Modifiers.BotType(Role);
                return Scaling(modifier, 0f, 1f, 1 - BotRoleScaling, 1 + BotRoleScaling);
            }
        }
        public float WeaponClassModifier
        {
            get
            {
                float modifier = Modifiers.WeaponClass(WeaponClass, AmmoCaliber);
                return Scaling(modifier, 0f, 1f, 1 - WeaponClassScaling, 1 + WeaponClassScaling);
            }
        }
        public float ErgoModifier
        {
            get
            {
                float modifier = Modifiers.Ergo(CurrentWeapon.ErgonomicsTotal);
                return Scaling(modifier, 0f, 1f, 1 - ErgoScaling, 1 + ErgoScaling);
            }
        }
        public float RecoilModifier
        {
            get
            {
                float modifier = Modifiers.Recoil(CurrentWeapon.RecoilBase, CurrentWeapon.RecoilTotal, CurrentWeapon.CurrentAmmoTemplate.ammoRec);
                return Scaling(modifier, 0f, 1f, 1 - RecoilScaling, 1 + RecoilScaling);
            }
        }

        public Weapon CurrentWeapon => BotOwner.WeaponManager.CurrentWeapon;
        public string WeaponClass => CurrentWeapon.Template.weapClass;
        public string AmmoCaliber => CurrentWeapon.CurrentAmmoTemplate.Caliber;
        public WildSpawnType Role => BotOwner.Profile.Info.Settings.Role;

        public static float Scaling(float value, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return outputMin + (outputMax - outputMin) * ((value - inputMin) / (inputMax - inputMin));
        }

        private class Modifiers
        {
            public static float Ammo(string ammocaliber)
            {
                float ammomodifier;
                // Sets Modifier based on Ammo Type. Scaled between 0 - 1. Lower is better.
                switch (ammocaliber)
                {
                    // Pistol Rounds
                    case "9x18PM":
                        ammomodifier = 0.2f;
                        break;

                    case "9x19PARA":
                        ammomodifier = 0.25f;
                        break;

                    case "46x30":
                        ammomodifier = 0.3f;
                        break;

                    case "9x21":
                        ammomodifier = 0.3f;
                        break;

                    case "57x28":
                        ammomodifier = 0.35f;
                        break;

                    case "762x25TT":
                        ammomodifier = 0.4f;
                        break;

                    case "1143x23ACP":
                        ammomodifier = 0.4f;
                        break;

                    case "9x33R": // .357
                        ammomodifier = 0.65f;
                        break;

                    // Int rifle
                    case "545x39":
                        ammomodifier = 0.5f;
                        break;

                    case "556x45NATO":
                        ammomodifier = 0.5f;
                        break;

                    case "9x39":
                        ammomodifier = 0.55f;
                        break;

                    case "762x35": // 300 blk
                        ammomodifier = 0.55f;
                        break;

                    // big rifle
                    case "762x39":
                        ammomodifier = 0.65f;
                        break;

                    case "366TKM":
                        ammomodifier = 0.65f;
                        break;

                    case "762x51":
                        ammomodifier = 0.7f;
                        break;

                    case "127x55":
                        ammomodifier = 0.75f;
                        break;

                    case "762x54R":
                        ammomodifier = 0.8f;
                        break;

                    case "86x70": // 338
                        ammomodifier = 1.0f;
                        break;
                    // shotgun
                    case "20g":
                        ammomodifier = 0.65f;
                        break;

                    case "12g":
                        ammomodifier = 0.7f;
                        break;

                    case "23x75":
                        ammomodifier = 0.75f;
                        break;

                    // other
                    case "26x75":
                        ammomodifier = 1f;
                        break;

                    case "30x29":
                        ammomodifier = 1f;
                        break;

                    case "40x46":
                        ammomodifier = 1f;
                        break;

                    case "40mmRU":
                        ammomodifier = 1f;
                        break;

                    default:
                        ammomodifier = 0.5f;
                        break;
                }
                return ammomodifier;
            }

            public static float BotType(WildSpawnType bottype)
            {
                float botTypeModifier;
                switch (bottype)
                {
                    case WildSpawnType.assault:
                        botTypeModifier = 1.0f;
                        break;

                    case WildSpawnType.cursedAssault:
                    case WildSpawnType.assaultGroup:
                    case WildSpawnType.sectantWarrior:
                    case WildSpawnType.pmcBot:
                        botTypeModifier = 0.6f;
                        break;

                    case WildSpawnType.exUsec:
                        botTypeModifier = 0.4f;
                        break;

                    case WildSpawnType.bossBully:
                    case WildSpawnType.bossGluhar:
                    case WildSpawnType.bossKilla:
                    case WildSpawnType.bossSanitar:
                    case WildSpawnType.bossKojaniy:
                    case WildSpawnType.bossZryachiy:
                    case WildSpawnType.sectantPriest:
                        botTypeModifier = 0.5f;
                        break;

                    case WildSpawnType.followerBully:
                    case WildSpawnType.followerGluharAssault:
                    case WildSpawnType.followerGluharScout:
                    case WildSpawnType.followerGluharSecurity:
                    case WildSpawnType.followerGluharSnipe:
                    case WildSpawnType.followerKojaniy:
                    case WildSpawnType.followerSanitar:
                    case WildSpawnType.followerTagilla:
                    case WildSpawnType.followerZryachiy:
                        botTypeModifier = 0.4f;
                        break;

                    case WildSpawnType.followerBigPipe:
                    case WildSpawnType.followerBirdEye:
                    case WildSpawnType.bossKnight:
                        botTypeModifier = 0.15f;
                        break;

                    case WildSpawnType.marksman:
                        botTypeModifier = 0.8f;
                        break;

                    default: // PMC
                        botTypeModifier = 0.1f;
                        break;
                }
                return botTypeModifier;
            }

            public static float Ergo(float ergoTotal)
            {
                // Low Ergo results in a higher modifier, with 1 modifier being worst
                float ergoModifier = 1f - ergoTotal / 100f;

                // Makes sure the modifier doesn't come out to 0
                ergoModifier = Mathf.Clamp(ergoModifier, 0.01f, 1f);

                // Final Output
                return ergoModifier;
            }

            public static float Recoil(float recoilBase, float recoilTotal, float ammoRec)
            {
                // Adds Recoil Stat from ammo type currently used.
                float ammoRecoil = ammoRec / 200f;

                // Raw Recoil Modifier
                float recoilmodifier = (recoilTotal / recoilBase) + ammoRecoil;

                return recoilmodifier;
            }

            public static float WeaponClass(string weaponclass, string ammocaliber)
            {
                // Weapon Class Modifiers. Scaled Between 0 and 1. Lower is Better
                float classmodifier;
                switch (weaponclass)
                {
                    case "assaultRifle":
                        classmodifier = 0.25f;
                        break;

                    case "assaultCarbine":
                        classmodifier = 0.3f;
                        break;

                    case "machinegun":
                        classmodifier = 0.25f;
                        break;

                    case "smg":
                        classmodifier = 0.2f;
                        break;

                    case "pistol":
                        classmodifier = 0.4f;
                        break;

                    case "marksmanRifle":
                        classmodifier = 0.5f;
                        break;

                    case "sniperRifle":
                        classmodifier = 0.75f;
                        // VSS and VAL Exception
                        if (ammocaliber == "9x39") classmodifier = 0.3f;
                        break;

                    case "shotgun":
                        classmodifier = 0.5f;
                        break;

                    case "grenadeLauncher":
                        classmodifier = 1f;
                        break;

                    case "specialWeapon":
                        classmodifier = 1f;
                        break;

                    default:
                        classmodifier = 0.3f;
                        break;
                }
                return classmodifier;
            }
        }
    }
}