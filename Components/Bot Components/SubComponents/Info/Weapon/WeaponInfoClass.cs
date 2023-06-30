using BepInEx.Logging;
using EFT;
using EFT.InventoryLogic;
using System.Reflection;
using UnityEngine;
using static SAIN.UserSettings.DifficultyConfig;

namespace SAIN.Classes
{
    public class WeaponInfo : SAINWeapon
    {
        public WeaponInfo(BotOwner bot) : base(bot)
        {
            Modifiers = new ModifierClass(bot);
            DefaultAccuracy = BotOwner.WeaponManager.WeaponAIPreset.XZ_COEF;
            Recoil = new Recoil(bot);
            Firerate = new Firerate(bot);
            Firemode = new Firemode(bot);
        }

        private readonly float DefaultAccuracy;

        public void ManualUpdate()
        {
            BotOwner.WeaponManager.WeaponAIPreset.XZ_COEF = DefaultAccuracy * BaseAccuracy.Value;

            if (BotOwner.WeaponManager?.Selector?.IsWeaponReady == true)
            {
                Firemode.CheckSwap();
                if (BotOwner.WeaponManager.CurrentWeapon?.Template != LastCheckedWeapon)
                {
                    LastCheckedWeapon = CurrentWeapon.Template;
                    FinalModifier = Modifiers.FinalModifier;
                }
            }
        }

        public Recoil Recoil { get; private set; }
        public Firerate Firerate { get; private set; }
        public Firemode Firemode { get; private set; }

        public float FinalModifier { get; private set; }

        private WeaponTemplate LastCheckedWeapon;

        public ModifierClass Modifiers { get; private set; }

        public float EffectiveWeaponDistance
        {
            get
            {
                float PreferedDist;
                switch (WeaponClass)
                {
                    case "assaultCarbine":
                    case "assaultRifle":
                    case "machinegun":
                        PreferedDist = 100f;
                        break;

                    case "smg":
                        PreferedDist = 40f;
                        break;

                    case "pistol":
                        PreferedDist = 30f;
                        break;

                    case "marksmanRifle":
                        PreferedDist = 150f;
                        break;

                    case "sniperRifle":
                        PreferedDist = 200f;
                        break;

                    case "shotgun":
                        PreferedDist = 30f;
                        break;
                    case "grenadeLauncher":
                    case "specialWeapon":
                        PreferedDist = 100f;
                        break;

                    default:
                        PreferedDist = 120f;
                        break;
                }
                if (AmmoCaliber == "9x39")
                {
                    PreferedDist = 75f;
                }
                return PreferedDist;
            }
        }
    }

    public class ModifierClass : SAINWeapon
    {
        public ModifierClass(BotOwner bot) : base(bot) { }

        private const float WeaponClassScaling = 0.3f;
        private const float RecoilScaling = 0.2f;
        private const float ErgoScaling = 0.1f;
        private const float AmmoTypeScaling = 0.2f;
        private const float BotRoleScaling = 0.3f;
        private const float DifficultyScaling = 0.5f;

        public float FinalModifier => WeaponClassModifier * RecoilModifier * ErgoModifier * AmmoTypeModifier * BotRoleModifier * DifficultyModifier;

        public float AmmoTypeModifier
        {
            get
            {
                float modifier = Ammo();
                return Scaling(modifier, 0f, 1f, 1 - AmmoTypeScaling, 1 + AmmoTypeScaling);
            }
        }
        public float BotRoleModifier
        {
            get
            {
                float modifier = BotType();
                return Scaling(modifier, 0f, 1f, 1 - BotRoleScaling, 1 + BotRoleScaling);
            }
        }
        public float WeaponClassModifier
        {
            get
            {
                float modifier = WeaponClassMod();
                return Scaling(modifier, 0f, 1f, 1 - WeaponClassScaling, 1 + WeaponClassScaling);
            }
        }
        public float ErgoModifier
        {
            get
            {
                float ergoModifier = 1f - CurrentWeapon.ErgonomicsTotal / 100f;
                ergoModifier = Mathf.Clamp(ergoModifier, 0.01f, 1f);
                return Scaling(ergoModifier, 0f, 1f, 1 - ErgoScaling, 1 + ErgoScaling);
            }
        }
        public float RecoilModifier
        {
            get
            {
                float ammoRecoil = CurrentWeapon.CurrentAmmoTemplate.ammoRec / 200f;
                float recoilmodifier = (CurrentWeapon.RecoilTotal / CurrentWeapon.RecoilBase) + ammoRecoil;
                return Scaling(recoilmodifier, 0f, 1f, 1 - RecoilScaling, 1 + RecoilScaling);
            }
        }

        public float DifficultyModifier
        {
            get
            {
                return Difficulty();
            }
        }

        public static float Scaling(float value, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return outputMin + (outputMax - outputMin) * ((value - inputMin) / (inputMax - inputMin));
        }

        public float Ammo()
        {
            float ammomodifier;
            // Sets Modifier based on Ammo Type. Scaled between 0 - 1. Lower is better.
            switch (AmmoCaliber)
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

        public float BotType()
        {
            float botTypeModifier;
            if (SAIN.Info.IsPMC)
            {
                botTypeModifier = 0.1f;
            }
            else
            {
                switch (Role)
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

                    default: // PMCRaider
                        botTypeModifier = 0.1f;
                        break;
                }
            }
            return botTypeModifier;
        }

        public float WeaponClassMod()
        {
            // Weapon Class ModifiersClass. Scaled Between 0 and 1. Lower is Better
            float classmodifier;
            switch (WeaponClass)
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
                    if (AmmoCaliber == "9x39") classmodifier = 0.3f;
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

        public float Difficulty()
        {
            return 1f;
            float modifier = 1f;
            if (SAIN.Info.IsPMC)
            {
                modifier /= PMCRecoil.Value;
            }
            else if (SAIN.Info.IsScav)
            {
                modifier /= ScavRecoil.Value;
            }
            else
            {
                modifier /= OtherRecoil.Value;
            }

            modifier /= BotRecoilGlobal.Value;
            return modifier;
        }
    }

    public abstract class SAINWeapon : SAINBot
    {
        public SAINWeapon(BotOwner owner) : base(owner)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        public readonly ManualLogSource Logger;

        public WeaponInfo WeaponInfo => SAIN.Info.WeaponInfo;

        public string WeaponClass => CurrentWeapon.Template.weapClass;

        public string AmmoCaliber => CurrentWeapon.CurrentAmmoTemplate.Caliber;

        public Weapon CurrentWeapon => BotOwner.WeaponManager.CurrentWeapon;
    }
}