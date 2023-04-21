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
        protected static ManualLogSource Logger { get; private set; }
        public float PerMeter { get; private set; }
        public float ShootModifier { get; private set; }
        public WeaponTemplate SavedTemplate;
        private BotOwner bot;
        private bool isReady = false;
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
                    if (!isReady && ShouldIRecalc())
                    {
                        if (DebugShootHelpers.Value) Logger.LogWarning($"Bot is doing initial weapon calculation");

                        Calculate();
                    }

                    // If we've already done the initial calculation and the weapon has changed, recalculate
                    if (isReady && ShouldIRecalc())
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
        private bool ShouldIRecalc()
        {
            return bot?.WeaponManager?.IsWeaponReady == true && bot?.WeaponManager?.CurrentWeapon?.Template != SavedTemplate;
        }
        public void Calculate()
        {
            try
            {
                // Stores the weapon template to check later if we need to recalculate
                SavedTemplate = bot.WeaponManager.CurrentWeapon.Template;

                // Generates a modifier for use in firerate and burst length
                EquipmentModifiers calculation = new EquipmentModifiers();
                ShootModifier = calculation.CalculateShoot(bot.WeaponManager.CurrentWeapon, bot.Profile.Info.Settings.Role);

                // Calculates what a bot's firerate should be based on current weapon and the modifier above
                PerMeter = Firerate.Calculate(bot.WeaponManager.CurrentWeapon.WeapClass, bot.WeaponManager.CurrentWeapon.AmmoCaliber);

                isReady = true;

                if (DebugShootHelpers.Value) Logger.LogInfo($"New Recalculate: Saved weapon id for {bot.name} with {bot.WeaponManager.CurrentWeapon.Name}]: Modifier: [{ShootModifier}], PerMeter: [{PerMeter}]");
            }
            catch
            {
                // Empty
            }
        }
        public class EquipmentModifiers : WeaponInfo
        {
            private float classScaling;
            private float recoilScaling;
            private float ergoScaling;
            private float ammoScaling;
            private float botTypeScaling;
            public EquipmentModifiers()
            {
                classScaling = 0.3f;
                recoilScaling = 0.2f;
                ergoScaling = 0.1f;
                ammoScaling = 0.2f;
                botTypeScaling = 0.3f;
            }
            public float CalculateShoot(Weapon weapon, WildSpawnType bottype)
            {
                // Define scaling factors // 0.3 means +- 30% influence
                float classScaling = 0.3f;
                float recoilScaling = 0.2f;
                float ergoScaling = 0.1f;
                float ammoScaling = 0.2f;
                float botTypeScaling = 0.3f;

                // Calculates and Applies Scaling
                WeaponClass(weapon.Template.weapClass, weapon.AmmoCaliber, out float classModifier);
                classModifier = Scale(classModifier, 0f, 1f, 1 - classScaling, 1 + classScaling);

                // Calculates and Applies Scaling
                Recoil(weapon, out float recoilModifier);
                recoilModifier = Scale(recoilModifier, 0f, 1f, 1 - recoilScaling, 1 + recoilScaling);

                // Calculates and Applies Scaling
                Ergo(weapon, out float ergoModifier);
                ergoModifier = Scale(ergoModifier, 0f, 1f, 1 - ergoScaling, 1 + ergoScaling);

                // Calculates and Applies Scaling
                Ammo(weapon.CurrentAmmoTemplate.Caliber, out float ammoModifier);
                ammoModifier = Scale(ammoModifier, 0f, 1f, 1 - ammoScaling, 1 + ammoScaling);

                // Calculates and Applies Scaling
                BotType(bottype, out float botTypeModifier);
                botTypeModifier = Scale(botTypeModifier, 0f, 1f, 1 - botTypeScaling, 1 + botTypeScaling);

                // Final Modifier
                float finalmod = classModifier * recoilModifier * ergoModifier * ammoModifier * botTypeModifier;

                if (DebugShootHelpers.Value)
                {
                    Logger.LogInfo($"Shoot Modifiers: Final Output:[{finalmod}], " +
                        $"Weapon: [[{weapon.Template.weapClass},{weapon.AmmoCaliber}]: " +
                        $"Class: [{classModifier}], " +
                        $"Recoil:[{recoilModifier}], Ergo: [{ergoModifier}], " +
                        $"Ammo:[{ammoModifier}], Bot Type: [{botTypeModifier}]]");
                }

                return finalmod;
            }
            public float MoveModifier(Weapon weapon, WildSpawnType bottype, out float movemodifier)
            {
                if (DebugShootHelpers.Value)
                {
                    System.Console.WriteLine($"Move Modifier: Final Output:[{null}], " +
                        $"Weapon: [[{weapon.WeapClass},{weapon.AmmoCaliber}]: " +
                        $"Class: [{null}], " +
                        $"Recoil:[{null}], Ergo: [{null}], " +
                        $"Ammo:[{null}], Bot Type: [{null}]]");
                }
                movemodifier = 1f;
                return movemodifier;
            }
            public float AggressionModifier(Weapon weapon, WildSpawnType bottype, out float aggressionmodifier)
            {
                if (DebugShootHelpers.Value)
                {
                    System.Console.WriteLine($"Aggression Modifier: Final Output:[{null}], " +
                        $"Weapon: [[{weapon.WeapClass}]: " +
                        $"Class: [{null}], " +
                        $"Recoil:[{null}], Ergo: [{null}], " +
                        $"Ammo:[{null}], Bot Type: [{null}]]");
                }
                aggressionmodifier = 1f;
                return aggressionmodifier;
            }
            public float WeaponClass(string weaponclass, string ammocaliber, out float classmodifier)
            {
                // Weapon Class Modifiers. Scaled Between 0 and 1. Lower is Better
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
            public float Ammo(string ammocaliber, out float ammomodifier)
            {
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

                // Scales result to between 0.75 and 1. Maximum 25% better from ammo type
                ammomodifier = (ammomodifier * (1f - 0.75f)) + 0.75f;

                return ammomodifier;
            }
            public float BotType(WildSpawnType bottype, out float bottypemodifier)
            {
                switch (bottype)
                {
                    case WildSpawnType.assault:
                        bottypemodifier = 1.0f;
                        break;

                    case WildSpawnType.cursedAssault:
                    case WildSpawnType.assaultGroup:
                    case WildSpawnType.sectantWarrior:
                    case WildSpawnType.pmcBot:
                        bottypemodifier = 0.6f;
                        break;

                    case WildSpawnType.exUsec:
                        bottypemodifier = 0.4f;
                        break;

                    case WildSpawnType.bossBully:
                    case WildSpawnType.bossGluhar:
                    case WildSpawnType.bossKilla:
                    case WildSpawnType.bossSanitar:
                    case WildSpawnType.bossKojaniy:
                    case WildSpawnType.bossZryachiy:
                    case WildSpawnType.sectantPriest:
                        bottypemodifier = 0.5f;
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
                        bottypemodifier = 0.4f;
                        break;

                    case WildSpawnType.followerBigPipe:
                    case WildSpawnType.followerBirdEye:
                    case WildSpawnType.bossKnight:
                        bottypemodifier = 0.15f;
                        break;

                    case WildSpawnType.marksman:
                        bottypemodifier = 0.8f;
                        break;

                    default: // PMC
                        bottypemodifier = 0.1f;
                        break;
                }
                return bottypemodifier;
            }
            public float Recoil(Weapon weapon, out float recoilmodifier)
            {
                // Shortcuts
                float recoilBase = weapon.RecoilBase;

                float recoilTotal = weapon.RecoilTotal; // this.RecoilBase + this.RecoilBase * this.RecoilDelta

                // Adds Recoil Stat from ammo type currently used.
                float ammoRecoil = weapon.CurrentAmmoTemplate.ammoRec / 200f;

                // Raw Recoil Modifier
                float recoilRaw = (recoilTotal / recoilBase) + ammoRecoil;

                return recoilmodifier = recoilRaw;
            }
            public float Ergo(Weapon weapon, out float ergomodifier)
            {
                float ergoTotal = weapon.ErgonomicsTotal;

                // Low Ergo results in a higher modifier, with 1 modifier being worst
                float ergoModifier = 1f - ergoTotal / 100f;

                // Makes sure the modifier doesn't come out to 0
                ergoModifier = Mathf.Clamp(ergoModifier, 0.01f, 1f);

                // Final Output
                return ergomodifier = ergoModifier;
            }
            public float Scale(float value, float inputMin, float inputMax, float outputMin, float outputMax)
            {
                return outputMin + (outputMax - outputMin) * ((value - inputMin) / (inputMax - inputMin));
            }
        }
        private static class Firerate
        {
            public static float Calculate(string weapclass, string ammocaliber)
            {
                float firerate = WeaponClassSwitch(weapclass, ammocaliber);

                return firerate;
            }
            private static float WeaponClassSwitch(string weaponclass, string ammotype)
            {
                float firerate;
                // Higher is faster firerate // Selects a the time for 1 second of wait time for every x meters
                switch (weaponclass)
                {
                    case "assaultCarbine":
                    case "assaultRifle":
                    case "machinegun":
                        firerate = 120f;
                        break;

                    case "smg":
                        firerate = 130f;
                        break;

                    case "pistol":
                        firerate = 90f;
                        break;

                    case "marksmanRifle":
                        firerate = 90f;
                        break;

                    case "sniperRifle":
                        firerate = 70f;
                        // VSS and VAL Exception
                        if (ammotype == "Caliber9x39")
                        {
                            firerate = 120f;
                        }
                        break;

                    case "shotgun":
                    case "grenadeLauncher":
                    case "specialWeapon":
                        firerate = 70f;
                        break;

                    default:
                        firerate = 120f;
                        break;
                }
                return firerate;
            }
        }
    }
}