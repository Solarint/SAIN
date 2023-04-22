using BepInEx.Logging;
using EFT;
using EFT.InventoryLogic;
using System.Collections;
using UnityEngine;
using static SAIN.Combat.Configs.DebugConfig;

namespace SAIN.Combat.Components
{
    public class WeaponInfo : MonoBehaviour
    {
        public WeaponTemplate SavedTemplate;
        private BotOwner bot;

        private bool isReady = false;
        public float FinalModifier { get; private set; }
        public float PerMeter { get; private set; }

        public float AmmoModifier { get; private set; }
        public float BotTypeModifier { get; private set; }
        public float ClassModifier { get; private set; }
        public float ErgoModifier { get; private set; }
        public float RecoilModifier { get; private set; }

        protected static ManualLogSource Logger { get; private set; }

        private bool ShouldIRecalc => bot?.WeaponManager?.IsWeaponReady == true && bot?.WeaponManager?.CurrentWeapon?.Template != SavedTemplate;

        public void Calculate()
        {
            try
            {
                // Stores the weapon template to check later if we need to recalculate
                SavedTemplate = bot.WeaponManager.CurrentWeapon.Template;

                // Generates a modifier for use in firerate and burst length
                EquipmentModifiers modifiers = new EquipmentModifiers(0.3f, 0.2f, 0.1f, 0.2f, 0.3f);
                modifiers.Calculate(bot.WeaponManager.CurrentWeapon, bot.Profile.Info.Settings.Role);

                // Calculates what a bot's firerate should be based on current weapon and the modifier above
                PerMeter = Firerate.GetPerMeter(bot.WeaponManager.CurrentWeapon.WeapClass, bot.WeaponManager.CurrentWeapon.AmmoCaliber);

                isReady = true;

                if (DebugShootHelpers.Value) Logger.LogInfo($"New Recalculate: Saved weapon id for {bot.name} with {bot.WeaponManager.CurrentWeapon.Name}]: Modifier: [{FinalModifier}], PerMeter: [{PerMeter}]");
            }
            catch
            {
                // Empty
            }
        }

        private void Awake()
        {
            bot = GetComponent<BotOwner>();

            if (bot != null && bot?.GetPlayer != null)
            {
                StartCoroutine(WeaponUpdateCheck());
            }

            if (Logger == null)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(WeaponInfo));
            }
        }
        private IEnumerator WeaponUpdateCheck()
        {
            while (true)
            {
                // Check if the bot is alive before continuing
                if (bot?.GetPlayer?.HealthController?.IsAlive == false || bot.IsDead)
                {
                    StopAllCoroutines();
                    yield break;
                }

                try
                {
                    // If the weapon is ready and we have not done the initial calculation yet, do it now
                    if (!isReady && ShouldIRecalc)
                    {
                        if (DebugShootHelpers.Value) Logger.LogWarning($"Bot is doing initial weapon calculation");

                        Calculate();
                    }

                    // If we've already done the initial calculation and the weapon has changed, recalculate
                    if (isReady && ShouldIRecalc)
                    {
                        if (DebugShootHelpers.Value) Logger.LogWarning($"Bot Needs to recalculate weapon information");

                        Calculate();
                    }
                }
                catch
                {
                    // Empty
                }

                // Overall Check Frequency
                yield return new WaitForSeconds(0.25f);
            }
        }

        public class EquipmentModifiers : WeaponInfo
        {
            private readonly float ammoScaling;

            private readonly float botTypeScaling;

            private readonly float classScaling;

            private readonly float ergoScaling;

            private readonly float recoilScaling;

            public EquipmentModifiers(float weaponclassscale, float recoilscale, float ergoscale, float ammoscale, float bottypescale)
            {
                // Define scaling factors // 0.3 means +- 30% influence
                classScaling = weaponclassscale;
                recoilScaling = recoilscale;
                ergoScaling = ergoscale;
                ammoScaling = ammoscale;
                botTypeScaling = bottypescale;
            }

            public void Calculate(Weapon weapon, WildSpawnType bottype)
            {
                Modifiers equipmod = new Modifiers();

                ClassModifier = equipmod.WeaponClass(weapon.Template.weapClass, weapon.AmmoCaliber);
                ClassModifier = Scaling(ClassModifier, 0f, 1f, 1 - classScaling, 1 + classScaling);

                RecoilModifier = equipmod.Recoil(weapon.RecoilBase, weapon.RecoilTotal, weapon.CurrentAmmoTemplate.ammoRec);
                RecoilModifier = Scaling(RecoilModifier, 0f, 1f, 1 - recoilScaling, 1 + recoilScaling);

                ErgoModifier = equipmod.Ergo(weapon.ErgonomicsTotal);
                ErgoModifier = Scaling(ErgoModifier, 0f, 1f, 1 - ergoScaling, 1 + ergoScaling);

                AmmoModifier = equipmod.Ammo(weapon.CurrentAmmoTemplate.Caliber);
                AmmoModifier = Scaling(AmmoModifier, 0f, 1f, 1 - ammoScaling, 1 + ammoScaling);

                BotTypeModifier = equipmod.BotType(bottype);
                BotTypeModifier = Scaling(BotTypeModifier, 0f, 1f, 1 - botTypeScaling, 1 + botTypeScaling);

                // Final Modifier
                FinalModifier = ClassModifier * RecoilModifier * ErgoModifier * AmmoModifier * BotTypeModifier;

                if (DebugShootHelpers.Value)
                {
                    Logger.LogInfo($"Shoot Modifiers: Final Output:[{FinalModifier}], " +
                        $"Weapon: [[{weapon.Template.weapClass},{weapon.AmmoCaliber}]: " +
                        $"Class: [{ClassModifier}], " +
                        $"Recoil:[{RecoilModifier}], Ergo: [{ErgoModifier}], " +
                        $"Ammo:[{AmmoModifier}], Bot Type: [{BotTypeModifier}]]");
                }
            }

            public float Scaling(float value, float inputMin, float inputMax, float outputMin, float outputMax)
            {
                return outputMin + (outputMax - outputMin) * ((value - inputMin) / (inputMax - inputMin));
            }

            public class Modifiers
            {
                public float Ammo(string ammocaliber)
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

                public float BotType(WildSpawnType bottype)
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

                public float Ergo(float ergoTotal)
                {
                    // Low Ergo results in a higher modifier, with 1 modifier being worst
                    float ergoModifier = 1f - ergoTotal / 100f;

                    // Makes sure the modifier doesn't come out to 0
                    ergoModifier = Mathf.Clamp(ergoModifier, 0.01f, 1f);

                    // Final Output
                    return ergoModifier;
                }

                public float Recoil(float recoilBase, float recoilTotal, float ammoRec)
                {
                    // Adds Recoil Stat from ammo type currently used.
                    float ammoRecoil = ammoRec / 200f;

                    // Raw Recoil Modifier
                    float recoilmodifier = (recoilTotal / recoilBase) + ammoRecoil;

                    return recoilmodifier;
                }

                public float WeaponClass(string weaponclass, string ammocaliber)
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

        private static class Firerate
        {
            public static float GetPerMeter(string weapclass, string ammocaliber)
            {
                float perMeter;
                // Higher is faster firerate // Selects a the time for 1 second of wait time for every x meters
                switch (weapclass)
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
                        if (ammocaliber == "Caliber9x39")
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
}