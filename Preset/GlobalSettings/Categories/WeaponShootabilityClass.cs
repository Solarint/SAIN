using Newtonsoft.Json;
using SAIN.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings
{
    public class WeaponShootabilityClass : Logging
    {
        public WeaponShootabilityClass() : base(nameof(AmmoShootabilityClass))
        {
        }

        public void UpdateValues()
        {
            Values = GetValuesFromClass.UpdateValues(WeaponClass.Default, Default, this, Values);
        }

        public float Get(WeaponClass key)
        {
            if (Values.Count == 0)
            {
                UpdateValues();
            }
            if (Values.ContainsKey(key))
            {
                return (float)Values[key];
            }
            Logger.LogWarning($"{key} does not exist in {GetType()} Dictionary");
            return Default;
        }

        [JsonIgnore]
        public Dictionary<object, object> Values = new Dictionary<object, object>();
        private const string Description = "Lower is BETTER. How Shootable this weapon type is, affects semi auto firerate and full auto burst length";

        [DefaultValue(0.25f)]
        [WeaponClass(WeaponClass.AssaultRifle)]
        [NameAndDescription(nameof(AssaultRifle), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float AssaultRifle = 0.25f;

        [DefaultValue(0.3f)]
        [WeaponClass(WeaponClass.AssaultCarbine)]
        [NameAndDescription(nameof(AssaultCarbine), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float AssaultCarbine = 0.3f;

        [DefaultValue(0.25f)]
        [WeaponClass(WeaponClass.Machinegun)]
        [NameAndDescription(nameof(Machinegun), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Machinegun = 0.25f;

        [DefaultValue(0.2f)]
        [WeaponClass(WeaponClass.SMG)]
        [NameAndDescription(nameof(SMG), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float SMG = 0.2f;

        [DefaultValue(0.4f)]
        [WeaponClass(WeaponClass.Pistol)]
        [NameAndDescription(nameof(Pistol), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Pistol = 0.4f;

        [DefaultValue(0.5f)]
        [WeaponClass(WeaponClass.MarksmanRifle)]
        [NameAndDescription(nameof(MarksmanRifle), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float MarksmanRifle = 0.5f;

        [DefaultValue(0.75f)]
        [WeaponClass(WeaponClass.SniperRifle)]
        [NameAndDescription(nameof(SniperRifle), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float SniperRifle = 0.75f;

        [DefaultValue(0.5f)]
        [WeaponClass(WeaponClass.Shotgun)]
        [NameAndDescription(nameof(Shotgun), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float Shotgun = 0.5f;

        [DefaultValue(1f)]
        [WeaponClass(WeaponClass.GrenadeLauncher)]
        [NameAndDescription(nameof(GrenadeLauncher), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float GrenadeLauncher = 1f;

        [DefaultValue(1f)]
        [WeaponClass(WeaponClass.SpecialWeapon)]
        [NameAndDescription(nameof(SpecialWeapon), Description)]
        [MinMaxRound(0.01f, 1f, 100f)]
        public float SpecialWeapon = 1;

        public static readonly float Default = 150f;
    }
}