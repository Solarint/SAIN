using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.Weather;
using UnityEngine;
using static SAIN.Editor.EditorSettings;

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

            if (soundtype == AISoundType.silencedGun)
            {
                if (subsonic)
                {
                    supmod *= SubsonicModifier.Value;
                }
                else
                {
                    supmod *= SuppressorModifier.Value;
                }
            }

            range *= supmod;

            // Applies Rain Modifier
            if (WeatherController.Instance?.WeatherCurve != null)
            {
                range *= RainSoundModifier();
            }

            // Plays the sound
            if (Singleton<GClass629>.Instantiated)
            {
                Singleton<GClass629>.Instance.PlaySound(player, player.WeaponRoot.position, range, soundtype);
            }
        }

        /// <summary>
        /// Calculates the audible range of a given ammunition caliber.
        /// </summary>
        /// <param name="ammocaliber">The ammunition caliber.</param>
        /// <returns>The audible range of the given ammunition caliber.</returns>
        private static float AudibleRange(string ammocaliber)
        {
            float range;
            switch (ammocaliber)
            {
                case "Caliber9x18PM":
                    range = 125f;
                    break;
                case "Caliber9x19PARA":
                    range = 125f;
                    break;
                case "Caliber46x30":
                    range = 135;
                    break;
                case "Caliber9x21":
                    range = 130;
                    break;
                case "Caliber57x28":
                    range = 140;
                    break;
                case "Caliber762x25TT":
                    range = 140;
                    break;
                case "Caliber1143x23ACP":
                    range = 140;
                    break;
                case "Caliber9x33R":
                    range = 130;
                    break;

                case "Caliber545x39":
                    range = 180;
                    break;
                case "Caliber556x45NATO":
                    range = 180;
                    break;
                case "Caliber9x39":
                    range = 180;
                    break;
                case "Caliber762x35":
                    range = 180;
                    break;

                case "Caliber762x39":
                    range = 200;
                    break;
                case "Caliber366TKM":
                    range = 200;
                    break;
                case "Caliber762x51":
                    range = 225;
                    break;
                case "Caliber127x55":
                    range = 225;
                    break;
                case "Caliber762x54R":
                    range = 300;
                    break;
                case "Caliber86x70":
                    range = 400;
                    break;

                case "Caliber20g":
                    range = 180;
                    break;
                case "Caliber12g":
                    range = 200;
                    break;
                case "Caliber23x75":
                    range = 210;
                    break;

                case "Caliber26x75":
                    range = 50;
                    break;
                case "Caliber30x29":
                    range = 50;
                    break;
                case "Caliber40x46":
                    range = 50;
                    break;
                case "Caliber40mmRU":
                    range = 50;
                    break;

                default:
                    range = 180;
                    break;
            }
            return range;
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
        /// Calculates the rain sound modifier based on the current rain value.
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
                // Grabs the current rain value
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
        /// Calculates the inverse scaling of a given value between a minimum and maximum value.
        /// </summary>
        /// <param name="value">The value to be scaled.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>The scaled value.</returns>
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