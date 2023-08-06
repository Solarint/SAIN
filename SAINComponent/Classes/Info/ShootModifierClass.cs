using SAIN.Preset.GlobalSettings;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Info
{
    public class ShootModifierClass : SAINBase
    {
        public ShootModifierClass(SAINComponentClass bot) : base(bot) { }

        private float WeaponClassScaling => ShootSettings.WeaponClassScaling;
        private float RecoilScaling => ShootSettings.RecoilScaling;
        private float ErgoScaling => ShootSettings.ErgoScaling;
        private float AmmoCaliberScaling => ShootSettings.AmmoCaliberScaling;
        private float WeaponProficiencyScaling => ShootSettings.WeaponProficiencyScaling;
        private float DifficultyScaling => ShootSettings.DifficultyScaling;

        private ShootSettings ShootSettings => SAINPlugin.LoadedPreset.GlobalSettings.Shoot;

        public float GetFinalModifier()
        {
            AmmoCaliberModifier = ShootSettings.AmmoShootability.Get(SAIN.Info.WeaponInfo.AmmoCaliber);
            AmmoCaliberModifier = Scaling(AmmoCaliberModifier, 0f, 1f, 1 - AmmoCaliberScaling, 1 + AmmoCaliberScaling);

            ProficiencyModifier = SAIN.Info.FileSettings.Mind.WeaponProficiency;
            ProficiencyModifier = Scaling(ProficiencyModifier, 0f, 1f, 1 - WeaponProficiencyScaling, 1 + WeaponProficiencyScaling);

            WeaponClassModifier = ShootSettings.WeaponShootability.Get(SAIN.Info.WeaponInfo.WeaponClass);
            WeaponClassModifier = Scaling(WeaponClassModifier, 0f, 1f, 1 - WeaponClassScaling, 1 + WeaponClassScaling);

            ErgoModifier = 1f - SAIN.Info.WeaponInfo.CurrentWeapon.ErgonomicsTotal / 100f;
            ErgoModifier = Mathf.Clamp(ErgoModifier, 0.01f, 1f);
            ErgoModifier = Scaling(ErgoModifier, 0f, 1f, 1 - ErgoScaling, 1 + ErgoScaling);

            float ammoRecoil = SAIN.Info.WeaponInfo.CurrentWeapon.CurrentAmmoTemplate.ammoRec / 200f;
            RecoilModifier = SAIN.Info.WeaponInfo.CurrentWeapon.RecoilTotal / SAIN.Info.WeaponInfo.CurrentWeapon.RecoilBase + ammoRecoil;
            RecoilModifier = Scaling(RecoilModifier, 0f, 1f, 1 - RecoilScaling, 1 + RecoilScaling);

            DifficultyModifier = SAIN.Info.Profile.DifficultyModifier;
            DifficultyModifier = Scaling(DifficultyModifier, 0f, 1f, 1 - DifficultyScaling, 1 + DifficultyScaling);

            //Logger.LogWarning($" AmmoCaliberModifier [{AmmoCaliberModifier}]");
            //Logger.LogWarning($" ProficiencyModifier [{ProficiencyModifier}]");
            //Logger.LogWarning($" WeaponClassModifier [{WeaponClassModifier}]");
            //Logger.LogWarning($" ErgoModifier [{ErgoModifier}]");
            //Logger.LogWarning($" RecoilModifier [{RecoilModifier}]");
            //Logger.LogWarning($" DifficultyModifier [{DifficultyModifier}]");

            float result = WeaponClassModifier * RecoilModifier * ErgoModifier * AmmoCaliberModifier * ProficiencyModifier * DifficultyModifier;
            result = Mathf.Round(result * 100f) / 100f;
            //Logger.LogWarning($" FinalModifier [{result}]");
            return result;
        }

        public float AmmoCaliberModifier { get; private set; }
        public float ProficiencyModifier { get; private set; }
        public float WeaponClassModifier { get; private set; }
        public float ErgoModifier { get; private set; }
        public float RecoilModifier { get; private set; }
        public float DifficultyModifier { get; private set; }

        public static float Scaling(float value, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return outputMin + (outputMax - outputMin) * ((value - inputMin) / (inputMax - inputMin));
        }
    }
}