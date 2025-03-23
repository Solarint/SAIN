using EFT.InventoryLogic;
using SAIN.Components.BotComponentSpace.Classes;
using SAIN.Helpers;
using SAIN.Preset;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.WeaponFunction;
using System.Linq;
using UnityEngine;
using static EFT.InventoryLogic.Weapon;

namespace SAIN.SAINComponent.Classes.Info
{
    public class WeaponInfoClass : BotBase, IBotClass
    {
        private const float MACHINEGUN_SWAPDIST_MULTI = 1.5f;
        public float FinalModifier { get; private set; }
        public EWeaponClass EWeaponClass { get; private set; }
        public ECaliber ECaliber { get; private set; }
        public float SwapToSemiDist { get; private set; } = 50f;
        public float SwapToAutoDist { get; private set; } = 45f;

        public Recoil Recoil { get; private set; }
        public Firerate Firerate { get; private set; }
        public Firemode Firemode { get; private set; }
        public ReloadClass Reload { get; private set; }

        public WeaponInfoClass(BotComponent bot) : base(bot)
        {
            Recoil = new Recoil(bot);
            Firerate = new Firerate(bot);
            Firemode = new Firemode(bot);
            Reload = new ReloadClass(bot);
        }

        private void forceRecheckWeapon(SAINPresetClass preset)
        {
            _forceNewCheck = true;
        }

        public void Init()
        {
            base.SubscribeToPreset(UpdatePresetSettings);
            Recoil.Init();
            Firerate.Init();
            Firemode.Init();
            Reload.Init();
        }

        protected void UpdatePresetSettings(SAINPresetClass preset)
        {
            _forceNewCheck = true;
        }

        public void Update()
        {
            checkCalcWeaponInfo();
            Recoil.Update();
            Firerate.Update();
            Firemode.Update();
            Reload.Update();
        }

        public void checkCalcWeaponInfo()
        {
            if (_nextCheckWeapTime < Time.time || _forceNewCheck) {
                Weapon currentWeapon = CurrentWeapon;
                if (currentWeapon != null) {
                    _nextCheckWeapTime = Time.time + _checkWeapFreq;
                    if (_forceNewCheck || _nextRecalcTime < Time.time || _lastCheckedWeapon == null || _lastCheckedWeapon != currentWeapon) {
                        if (_forceNewCheck)
                            _forceNewCheck = false;

                        _nextRecalcTime = Time.time + _recalcFreq;
                        _lastCheckedWeapon = currentWeapon;
                        calculateCurrentWeapon(currentWeapon);
                    }
                }
            }
        }

        private void calculateCurrentWeapon(Weapon weapon)
        {
            EWeaponClass = EnumValues.ParseWeaponClass(weapon.Template.weapClass);
            ECaliber = EnumValues.ParseCaliber(weapon.CurrentAmmoTemplate.Caliber);
            calculateShootModifier();
            SwapToSemiDist = getWeaponSwapToSemiDist(ECaliber, EWeaponClass);
            SwapToAutoDist = getWeaponSwapToFullAutoDist(ECaliber, EWeaponClass);
        }

        private static float getAmmoShootability(ECaliber caliber)
        {
            if (_shootSettings.AmmoCaliberShootability.TryGetValue(caliber, out var ammo)) {
                return ammo;
            }
            return 0.5f;
        }

        private static float getWeaponShootability(EWeaponClass weaponClass)
        {
            if (_shootSettings.WeaponClassShootability.TryGetValue(weaponClass, out var weap)) {
                return weap;
            }
            return 0.5f;
        }

        private static float getWeaponSwapToSemiDist(ECaliber caliber, EWeaponClass weaponClass)
        {
            if (_shootSettings.AmmoCaliberFullAutoMaxDistances.TryGetValue(caliber, out var caliberDist)) {
                if (weaponClass == EWeaponClass.machinegun) {
                    return caliberDist * MACHINEGUN_SWAPDIST_MULTI;
                }
                return caliberDist;
            }
            return 55f;
        }

        private static float getWeaponSwapToFullAutoDist(ECaliber caliber, EWeaponClass weaponClass)
        {
            return getWeaponSwapToSemiDist(caliber, weaponClass) * 0.85f;
        }

        private void calculateShootModifier()
        {
            var weapInfo = Bot.Info.WeaponInfo;

            float AmmoCaliberModifier =
                getAmmoShootability(ECaliber)
                .Scale0to1(_shootSettings.AmmoCaliberScaling)
                .Round100();

            float WeaponClassModifier =
                getWeaponShootability(EWeaponClass)
                .Scale0to1(_shootSettings.WeaponClassScaling)
                .Round100();

            float ProficiencyModifier =
                Bot.Info.FileSettings.Mind.WeaponProficiency
                .Scale0to1(_shootSettings.WeaponProficiencyScaling)
                .Round100();

            var weapon = weapInfo.CurrentWeapon;
            float ErgoModifier =
                Mathf.Clamp(1f - weapon.ErgonomicsTotal / 100f, 0.01f, 1f)
                .Scale0to1(_shootSettings.ErgoScaling)
                .Round100();

            float RecoilModifier = ((weapon.RecoilTotal / weapon.RecoilBase) + (weapon.CurrentAmmoTemplate.ammoRec / 200f))
                .Scale0to1(_shootSettings.RecoilScaling)
                .Round100();

            float DifficultyModifier =
                Bot.Info.Profile.DifficultyModifier
                .Scale0to1(_shootSettings.DifficultyScaling)
                .Round100();

            FinalModifier = (WeaponClassModifier * RecoilModifier * ErgoModifier * AmmoCaliberModifier * ProficiencyModifier * DifficultyModifier)
                .Round100();
        }

        public void Dispose()
        {
            Recoil.Dispose();
            Firerate.Dispose();
            Firemode.Dispose();
            Reload.Dispose();
        }

        public float EffectiveWeaponDistance {
            get
            {
                if (ECaliber == ECaliber.Caliber9x39) {
                    return 125f;
                }
                if (GlobalSettings.Shoot.EngagementDistance.TryGetValue(EWeaponClass, out float engagementDist)) {
                    return engagementDist;
                }
                return 125f;
            }
        }

        public float PreferedShootDistance {
            get
            {
                return EffectiveWeaponDistance * 0.66f;
            }
        }

        public bool IsFireModeSet(EFireMode mode)
        {
            return SelectedFireMode == mode;
        }

        public bool HasFullAuto => HasFireMode(EFireMode.fullauto);

        public bool HasBurst => HasFireMode(EFireMode.burst);

        public bool HasSemi => HasFireMode(EFireMode.single);

        public bool HasDoubleAction => HasFireMode(EFireMode.doubleaction);

        public bool HasFireMode(EFireMode fireMode)
        {
            var modes = CurrentWeapon?.WeapFireType;
            if (modes == null) return false;
            return modes.Contains(fireMode);
        }

        public EFireMode SelectedFireMode {
            get
            {
                if (CurrentWeapon != null) {
                    return CurrentWeapon.SelectedFireMode;
                }
                return EFireMode.fullauto;
            }
        }

        public Weapon CurrentWeapon {
            get
            {
                return BotOwner?.WeaponManager?.CurrentWeapon;
            }
        }

        private Weapon _lastCheckedWeapon;
        private float _nextRecalcTime;
        private const float _recalcFreq = 60f;
        private float _nextCheckWeapTime;
        private const float _checkWeapFreq = 1f;
        private bool _forceNewCheck = false;
        private static ShootSettings _shootSettings => SAINPlugin.LoadedPreset.GlobalSettings.Shoot;
    }
}