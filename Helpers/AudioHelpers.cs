using EFT;
using EFT.InventoryLogic;
using EFT.Weather;
using System.Collections.Generic;
using UnityEngine;
using static EFT.Player;

namespace SAIN.Helpers
{
    public class AudioHelpers
    {
        public static void TryPlayShootSound(Player player, AISoundType soundType)
        {
            float range = 135f;
            if (player?.AIData != null)
            {
                var firearmController = player.HandsController as FirearmController;
                if (firearmController != null)
                {
                    Weapon weapon = firearmController.Item;
                    if (weapon != null)
                    {
                        string playerWeaponId = $"{player.name}_{weapon.TemplateId}";
                        if (!Calculations.ContainsKey(playerWeaponId))
                        {
                            float speedFactor = CalcVelocityFactor(weapon);
                            bool subsonic = IsSubsonic(weapon.CurrentAmmoTemplate.InitialSpeed, speedFactor);
                            float supModifier = GetSuppressorMod(subsonic, soundType);
                            float muzzleLoudness = GetMuzzleLoudness(weapon.Mods);
                            float baseAudibleRange = AudibleRange(weapon.Template.ammoCaliber);

                            float result = (supModifier * muzzleLoudness * baseAudibleRange).Round10();
                            Calculations.Add(playerWeaponId, result);

                            if (SAINPlugin.DebugMode)
                            {
                                Logger.LogDebug(
                                    $"Name: {player.name}  " +
                                    $"TemplateID {weapon.TemplateId}" +
                                    $"Result: {result} " +
                                    $"SpeedFactor: {speedFactor} " +
                                    $"MuzzleLoudness: {muzzleLoudness} " +
                                    $"Base Audible Range: {baseAudibleRange} " +
                                    $"supModifier: {supModifier} " +
                                    $"Subsonic: {subsonic}");
                            }
                        }
                        range = Calculations[playerWeaponId] * RainSoundModifier();
                    }
                }
            }
            HelpersGClass.PlaySound(player, player.WeaponRoot.position, range, soundType);
        }

        public static void ClearCache()
        {
            Calculations.Clear();
        }

        private static readonly Dictionary<string, float> Calculations = new Dictionary<string, float>();

        private static float CalcVelocityFactor(Weapon weapon)
        {
            return 2f - weapon.SpeedFactor;
        }

        private static float GetMuzzleLoudness(Mod[] mods)
        {
            if (!ModDetection.RealismLoaded)
            {
                return 1f;
            }
            float loudness = 0f;
            for (int i = 0; i < mods.Length; i++)
            {
                //if the muzzle device has a silencer attached to it then it shouldn't contribute to the loudness stat.
                if (mods[i].Slots.Length > 0 && mods[i].Slots[0].ContainedItem != null && IsSilencer((Mod)mods[i].Slots[0].ContainedItem))
                {
                    continue;
                }
                else
                {
                    loudness += mods[i].Template.Loudness;
                }
            }
            return (loudness / 200) + 1f;
        }

        private static float GetSuppressorMod(bool subsonic, AISoundType soundtype)
        {
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
            return supmod;
        }

        private static float AudibleRange(string ammocaliber)
        {
            var caliber = EnumValues.ParseCaliber(ammocaliber);
            if (SAINPlugin.LoadedPreset?.GlobalSettings?.Hearing?
                .HearingDistances.TryGetValue(caliber, out var range) == true)
            {
                return range;
            }
            Logger.LogError(caliber);
            return 150f;
        }

        public static bool IsSilencer(Mod mod)
        {
            return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[SuppressorTypeId];
        }

        public static readonly string SuppressorTypeId = "550aa4cd4bdc2dd8348b456c";
        public static readonly string CollimatorTypeId = "55818ad54bdc2ddc698b4569";
        public static readonly string CompactCollimatorTypeId = "55818acf4bdc2dde698b456b";
        public static readonly string AssaultScopeTypeId = "55818add4bdc2d5b648b456f";
        public static readonly string OpticScopeTypeId = "55818ae44bdc2dde698b456c";
        public static readonly string SpecialScopeTypeId = "55818aeb4bdc2ddc698b456a";

        private const float SuperSonic = 343.2f;

        private static bool IsSubsonic(float velocity, float speedFactor)
        {
            return velocity * speedFactor <= SuperSonic;
        }

        public static float RainSoundModifier()
        {
            if (WeatherController.Instance?.WeatherCurve == null)
                return 1f;

            if (RainCheckTimer < Time.time)
            {
                RainCheckTimer = Time.time + 10f;
                // Grabs the current rain Rounding
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