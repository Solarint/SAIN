using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.Weather;
using UnityEngine;

namespace SAIN.Helpers
{
    public class GunshotRange
    {
        public static void OnMakingShot(IWeapon weapon, Player player, BulletClass ammo)
        {
            if (player?.AIData != null)
            {
                Player.FirearmController firearmController = player.HandsController as Player.FirearmController;
                string caliber = weapon.WeaponTemplate.ammoCaliber;

                float range = AudibleRange(caliber);

                bool subsonic = IsSubsonic(ammo.InitialSpeed);

                AISoundType soundType;

                if (firearmController != null && firearmController.IsSilenced) soundType = AISoundType.silencedGun;
                else soundType = AISoundType.gun;

                PlayShootSound(player, range, subsonic, soundType);
            }
        }

        /// <summary>
        /// Plays a shoot sound for the given player, range, subsonic and sound type, applying modifiers if necessary.
        /// </summary>
        private static void PlayShootSound(Player player, float range, bool subsonic, AISoundType soundtype)
        {
            // Decides if Suppressor modifier should be applied + subsonic
            float supmod = 1f;
            bool suppressed = soundtype == AISoundType.silencedGun;

            if (suppressed && subsonic)
            {
                supmod *= SAINPlugin.LoadedPreset.GlobalSettings.Hearing.SubsonicModifier;
            }
            else if (suppressed)
            {
                supmod *= SAINPlugin.LoadedPreset.GlobalSettings.Hearing.SuppressorModifier;
            }

            range *= supmod;
            range *= RainSoundModifier();

            HelpersGClass.PlaySound(player, player.WeaponRoot.position, range, soundtype);
        }

        /// <summary>
        /// Calculates the audible range of a given ammunition caliber.
        /// </summary>
        /// <param name="ammocaliber">The ammunition caliber.</param>
        /// <returns>The audible range of the given ammunition caliber.</returns>
        private static float AudibleRange(string ammocaliber)
        {
            Caliber caliber = EnumValues.Parse<Caliber>(ammocaliber);
            var Ranges = SAINPlugin.LoadedPreset?.GlobalSettings?.Hearing?.AudibleRanges;
            if (Ranges != null)
            {
                return Ranges.Get(caliber);
            }
            else
            {
                return 150f;
            }
        }

        /// <summary>
        /// Checks if the given velocity is subsonic.
        /// </summary>
        /// <param name="velocity">The velocity to check.</param>
        /// <returns>True if the velocity is subsonic, false otherwise.</returns>
        private static bool IsSubsonic(float velocity)
        {
            if (velocity < 343.2f) return true;
            else return false;
        }

        /// <summary>
        /// Calculates the rain sound modifier based on the current rain rounding.
        /// </summary>
        /// <returns>
        /// The rain sound modifier.
        /// </returns>
        public static float RainSoundModifier()
        {
            if (WeatherController.Instance?.WeatherCurve == null)
                return 1f;

            if (RainCheckTimer < Time.time)
            {
                RainCheckTimer = Time.time + 10f;
                // Grabs the current rain rounding
                float Rain = WeatherController.Instance.WeatherCurve.Rain;
                RainModifier = 1f;
                float max = 1f;
                float rainMin = 0.65f;

                Rain = InverseScaling(Rain, rainMin, max);

                // Combines ModifiersClass and returns
                RainModifier *= Rain;
            }
            return RainModifier;
        }

        /// <summary>
        /// Calculates the inverse scaling of a given rounding between a minimum and maximum rounding.
        /// </summary>
        /// <param name="value">The rounding to be scaled.</param>
        /// <param name="min">The minimum rounding.</param>
        /// <param name="max">The maximum rounding.</param>
        /// <returns>The scaled rounding.</returns>
        public static float InverseScaling(float value, float min, float max)
        {
            // Inverse
            float InverseValue = 1f - value;

            // Scaling
            float ScaledValue = (InverseValue * (max - min)) + min;

            value = ScaledValue;

            return value;
        }

        private static float RainCheckTimer = 0f;
        private static float RainModifier = 1f;
    }
}