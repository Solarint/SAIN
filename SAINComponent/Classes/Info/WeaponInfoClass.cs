using EFT;
using EFT.InventoryLogic;
using Mono.WebBrowser;
using SAIN.Helpers;
using SAIN.Plugin;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.WeaponFunction;
using System.Linq;
using UnityEngine;
using static EFT.InventoryLogic.Weapon;

namespace SAIN.SAINComponent.Classes.Info
{
    public class WeaponInfoClass : SAINBase, ISAINClass
    {
        public WeaponInfoClass(SAINComponentClass sain) : base(sain)
        {
            Recoil = new Recoil(sain);
            Firerate = new Firerate(sain);
            Firemode = new Firemode(sain);
            PresetHandler.PresetsUpdated += Calculate;
        }

        public void Init()
        {
            Recoil.Init();
            Firerate.Init();
            Firemode.Init();
        }

        public void Update()
        {
            Recoil.Update();
            Firerate.Update();
            Firemode.Update();

            var manager = BotOwner?.WeaponManager;
            if (manager.Selector?.IsWeaponReady == true)
            {
                Firemode.CheckSwap();
                Weapon weapon = manager.CurrentWeapon;
                if (weapon != null && weapon.Template != LastCheckedWeapon)
                {
                    Calculate();
                }
            }
        }

        public void Calculate()
        {
            var manager = BotOwner?.WeaponManager;
            if (manager.Selector?.IsWeaponReady == true)
            {
                Weapon weapon = manager.CurrentWeapon;
                if (weapon != null)
                {
                    IWeaponClass = EnumValues.ParseWeaponClass(weapon.Template.weapClass);
                    ICaliber = EnumValues.ParseCaliber(weapon.CurrentAmmoTemplate.Caliber);

                    LastCheckedWeapon = weapon.Template;
                    CalculateShootModifier();
                }
            }
        }

        public IWeaponClass IWeaponClass { get; private set; }
        public ICaliber ICaliber { get; private set; }

        private ShootSettings ShootSettings => SAINPlugin.LoadedPreset.GlobalSettings.Shoot;

        private float GetAmmoShootability()
        {
            if (ShootSettings.AmmoCaliberShootability.TryGetValue(ICaliber, out var ammo))
            {
                return ammo;
            }
            return 0.5f;
        }

        private float GetWeaponShootability()
        {
            if (ShootSettings.WeaponClassShootability.TryGetValue(IWeaponClass, out var weap))
            {
                return weap;
            }
            return 0.5f;
        }

        private void CalculateShootModifier()
        {
            var weapInfo = SAIN.Info.WeaponInfo;

            float AmmoCaliberModifier =
                GetAmmoShootability()
                .Scale0to1(ShootSettings.AmmoCaliberScaling)
                .Round100();

            float WeaponClassModifier =
                GetWeaponShootability()
                .Scale0to1(ShootSettings.WeaponClassScaling)
                .Round100();

            float ProficiencyModifier =
                SAIN.Info.FileSettings.Mind.WeaponProficiency
                .Scale0to1(ShootSettings.WeaponProficiencyScaling)
                .Round100();

            var weapon = weapInfo.CurrentWeapon;
            float ErgoModifier =
                Mathf.Clamp
                    (
                    1f - weapon.ErgonomicsTotal / 100f,
                    0.01f,
                    1f
                    )
                .Scale0to1(ShootSettings.ErgoScaling)
                .Round100();

            float RecoilModifier =
                (
                (weapon.RecoilTotal / weapon.RecoilBase)
                +
                (weapon.CurrentAmmoTemplate.ammoRec / 200f)
                )
                .Scale0to1(ShootSettings.RecoilScaling)
                .Round100();

            float DifficultyModifier =
                SAIN.Info.Profile.DifficultyModifier
                .Scale0to1(ShootSettings.DifficultyScaling)
                .Round100();

            FinalModifier =
                (
                WeaponClassModifier
                * RecoilModifier
                * ErgoModifier
                * AmmoCaliberModifier
                * ProficiencyModifier
                * DifficultyModifier
                )
                .Round100();
        }

        public void Dispose()
        {
            Recoil.Dispose();
            Firerate.Dispose();
            Firemode.Dispose();
            PresetHandler.PresetsUpdated -= Calculate;
        }

        public Recoil Recoil { get; private set; }
        public Firerate Firerate { get; private set; }
        public Firemode Firemode { get; private set; }
        public float FinalModifier { get; private set; }

        private WeaponTemplate LastCheckedWeapon;

        public float EffectiveWeaponDistance
        {
            get
            {
                if (ICaliber == ICaliber.Caliber9x39)
                {
                    return 75f;
                }
                if (GlobalSettings.Shoot.EngagementDistance.TryGetValue(IWeaponClass, out float PreferedDist))
                {
                    return PreferedDist;
                }
                return 75f;
            }
        }

        public bool IsFireModeSet(EFireMode mode)
        {
            return SelectedFireMode == mode;
        }

        public bool IsSetFullAuto()
        {
            return IsFireModeSet(EFireMode.fullauto);
        }

        public bool IsSetBurst()
        {
            return IsFireModeSet(EFireMode.burst);
        }

        public bool IsSetSemiAuto()
        {
            return IsFireModeSet(EFireMode.single);
        }

        public bool HasFullAuto()
        {
            return HasFireMode(EFireMode.fullauto);
        }

        public bool HasBurst()
        {
            return HasFireMode(EFireMode.burst);
        }

        public bool HasSemi()
        {
            return HasFireMode(EFireMode.single);
        }

        public bool HasDoubleAction()
        {
            return HasFireMode(EFireMode.doubleaction);
        }

        public bool HasFireMode(EFireMode fireMode)
        {
            var modes = CurrentWeapon?.WeapFireType;
            if (modes == null) return false;
            return modes.Contains(fireMode);
        }

        public EFireMode SelectedFireMode
        {
            get
            {
                if (CurrentWeapon != null)
                {
                    return CurrentWeapon.SelectedFireMode;
                }
                return EFireMode.fullauto;
            }
        }

        public float RecoilForceUp
        {
            get
            {
                var template = CurrentWeapon?.Template;
                if (template != null)
                {
                    return template.RecoilForceUp;
                }
                else
                {
                    return 150f;
                }
            }
        }

        public float RecoilForceBack
        {
            get
            {
                var template = CurrentWeapon?.Template;
                if (template != null)
                {
                    return template.RecoilForceBack;
                }
                else
                {
                    return 150f;
                }
            }
        }

        public WeaponInfoClass WeaponInfo => SAIN.Info?.WeaponInfo;

        public string WeaponClass => CurrentWeapon.Template.weapClass;

        public string AmmoCaliber => CurrentWeapon.CurrentAmmoTemplate.Caliber;

        public Weapon CurrentWeapon => BotOwner.WeaponManager.CurrentWeapon;
    }
}