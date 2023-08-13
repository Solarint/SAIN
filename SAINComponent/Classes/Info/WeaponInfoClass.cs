using EFT;
using EFT.InventoryLogic;
using SAIN.SAINComponent.Classes.WeaponFunction;
using System.Linq;
using static EFT.InventoryLogic.Weapon;

namespace SAIN.SAINComponent.Classes.Info
{
    public class WeaponInfoClass : SAINBase, ISAINClass
    {
        public WeaponInfoClass(SAINComponentClass sain) : base(sain)
        {
            Modifiers = new ShootModifierClass(sain);
            Recoil = new Recoil(sain);
            Firerate = new Firerate(sain);
            Firemode = new Firemode(sain);
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
                    LastCheckedWeapon = weapon.Template;
                    FinalModifier = Modifiers.GetFinalModifier();
                }
            }
        }

        public void Dispose()
        {
            Recoil.Dispose();
            Firerate.Dispose();
            Firemode.Dispose();
        }

        public Recoil Recoil { get; private set; }
        public Firerate Firerate { get; private set; }
        public Firemode Firemode { get; private set; }
        public float FinalModifier { get; private set; }

        private WeaponTemplate LastCheckedWeapon;
        public ShootModifierClass Modifiers { get; private set; }

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