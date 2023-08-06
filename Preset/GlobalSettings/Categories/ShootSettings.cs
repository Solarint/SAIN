using SAIN.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings
{
    public class ShootSettings
    {
        [NameAndDescription(
            "Max Recoil Per Shot",
            "Maximum Impulse force from a single shot for a bot.")]
        [DefaultValue(2f)]
        [MinMaxRound(0.1f, 10f, 100f)]
        [AdvancedOptions(true)]
        public float MaxRecoil = 2f;

        [NameAndDescription(
            "Add or Subtract Recoil",
            "Linearly add or subtract from the final recoil result")]
        [DefaultValue(0f)]
        [MinMaxRound(0.1f, 5f, 100f)]
        [AdvancedOptions(true)]
        public float AddRecoil = 0f;

        [NameAndDescription(
            "Recoil Decay p/frame",
            "How much to decay the recoil impulse per frame. 0.8 means 20% of the recoil will be removed per frame.")]
        [DefaultValue(0.8f)]
        [MinMaxRound(0.1f, 0.99f, 100f)]
        [AdvancedOptions(true)]
        public float RecoilDecay = 0.8f;

        [List]
        [AdvancedOptions(true)]
        public AmmoShootabilityClass AmmoShootability = new AmmoShootabilityClass();

        [List]
        [AdvancedOptions(true)]
        public WeaponShootabilityClass WeaponShootability = new WeaponShootabilityClass();
    }
}