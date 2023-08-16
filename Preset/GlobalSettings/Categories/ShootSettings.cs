using SAIN.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings
{
    public class ShootSettings
    {
        [NameAndDescription(
            "Global EFT Scatter Multiplier",
            "Higher = more scattering. Modifies EFT's default scatter feature. 1.5 = 1.5x more scatter")]
        [DefaultValue(1f)]
        [MinMax(0.1f, 10f, 100f)]
        public float GlobalScatterMultiplier = 1f;

        [NameAndDescription(
            "Global SAIN Recoil Multiplier",
            "Higher = more recoil. Modifies SAIN's new recoil scatter feature. 1.5 = 1.5x more scatter from recoil")]
        [DefaultValue(1f)]
        [MinMax(0.1f, 5f, 100f)]
        public float GlobalRecoilMultiplier = 1f;

        [NameAndDescription(
            "Max Recoil Per Shot",
            "Maximum Impulse force from a single shot for a bot.")]
        [DefaultValue(2f)]
        [MinMax(0.1f, 10f, 100f)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public float MaxRecoil = 2f;

        [NameAndDescription(
            "Add or Subtract Recoil",
            "Linearly add or subtract from the final recoil result")]
        [DefaultValue(0f)]
        [MinMax(-5f, 5f, 100f)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public float AddRecoil = 0f;

        [NameAndDescription(
            "Recoil Decay p/frame",
            "How much to decay the recoil impulse per frame. 0.8 means 20% of the recoil will be removed per frame.")]
        [DefaultValue(0.8f)]
        [MinMax(0.1f, 0.99f, 100f)]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public float RecoilDecay = 0.8f;

        [Dictionary(typeof(Caliber), typeof(float))]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public AmmoShootabilityClass AmmoShootability = new AmmoShootabilityClass();

        [Dictionary(typeof(WeaponClass), typeof(float))]
        [Advanced(AdvancedEnum.IsAdvanced)]
        public WeaponShootabilityClass WeaponShootability = new WeaponShootabilityClass();

        const string DONOTEDIT = "Do not Edit These";
        [Advanced(AdvancedEnum.Hidden)] public float WeaponClassScaling = 0.3f;
        [Advanced(AdvancedEnum.Hidden)] public float RecoilScaling = 0.2f;
        [Advanced(AdvancedEnum.Hidden)] public float ErgoScaling = 0.1f;
        [Advanced(AdvancedEnum.Hidden)] public float AmmoCaliberScaling = 0.2f;
        [Advanced(AdvancedEnum.Hidden)] public float WeaponProficiencyScaling = 0.3f;
        [Advanced(AdvancedEnum.Hidden)] public float DifficultyScaling = 0.5f;
    }
}