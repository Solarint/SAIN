using EFT.InventoryLogic;
using SAIN.Helpers;
using SAIN.Plugin;
using SAIN.Preset;
using System;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Info
{
    public class WeaponInfo
    {
        public WeaponInfo(Weapon weapon)
        {
            Weapon = weapon;
            WeaponClass = TryGetWeaponClass(weapon);
            AmmoCaliber = TryGetAmmoCaliber(weapon);
            updateSettings(SAINPresetClass.Instance);
            PresetHandler.OnPresetUpdated += updateSettings;
        }

        private void updateSettings(SAINPresetClass preset)
        {
            if (preset.GlobalSettings.Shoot.EngagementDistance.TryGetValue(WeaponClass, out float distance)) {
                EngagementDistance = distance;
            }
            if (preset.GlobalSettings.Hearing.HearingDistances.TryGetValue(AmmoCaliber, out float range)) {
                BaseAudibleRange = range;
            }
        }

        public bool Update()
        {
            if (_nextUpdateTime > Time.time) {
                return false;
            }
            _nextUpdateTime = Time.time + 10f;

            if (Weapon != null)
                BulletSpeed = Weapon.CurrentAmmoTemplate.InitialSpeed * SpeedFactor;

            checkAllMods();
            //Log();

            return true;
        }

        public void Dispose()
        {
            PresetHandler.OnPresetUpdated -= updateSettings;
        }

        public float BulletSpeed { get; private set; } = 600f;

        public float EngagementDistance { get; private set; } = 150f;

        public Weapon Weapon { get; private set; }

        public float Durability {
            get
            {
                return Weapon.Repairable.Durability / (float)Weapon.Repairable.TemplateDurability;
            }
        }

        public EWeaponClass WeaponClass { get; private set; }
        public ECaliber AmmoCaliber { get; private set; }
        public float CalculatedAudibleRange { get; private set; }
        public AISoundType AISoundType => HasSuppressor ? AISoundType.silencedGun : AISoundType.gun;
        public SAINSoundType SoundType => HasSuppressor ? SAINSoundType.SuppressedShot : SAINSoundType.Shot;

        public float BaseAudibleRange { get; private set; } = 150f;

        public float MuzzleLoudness { get; private set; }
        public float MuzzleLoudnessRealism { get; private set; }

        public float SuppressorModifier {
            get
            {
                float supmod = 1f;
                bool suppressed = HasSuppressor;

                if (suppressed && Subsonic) {
                    supmod *= SAINPlugin.LoadedPreset.GlobalSettings.Hearing.SubsonicModifier;
                }
                else if (suppressed) {
                    supmod *= SAINPlugin.LoadedPreset.GlobalSettings.Hearing.SuppressorModifier;
                }
                return supmod;
            }
        }

        public float SpeedFactor => 2f - Weapon.SpeedFactor;

        public bool Subsonic {
            get
            {
                if (Weapon == null) {
                    return false;
                }
                return BulletSpeed < SuperSonicSpeed;
            }
        }

        public bool HasRedDot { get; private set; }
        public bool HasOptic { get; private set; }
        public bool HasSuppressor { get; private set; }

        private void checkAllMods()
        {
            var mods = Weapon?.Mods;
            if (mods == null) {
                return;
            }

            float realismLoudness = 0;
            float loudness = 0;

            HasSuppressor = false;
            HasRedDot = false;
            HasOptic = false;

            foreach (var item in mods) {
                checkItemType(item.GetType());
                calcLoudness(item, ref loudness, ref realismLoudness);

                foreach (var slot in item.Slots) {
                    if (slot.ContainedItem != null &&
                        slot.ContainedItem is Mod mod) {
                        checkItemType(mod.GetType());
                        calcLoudness(mod, ref loudness, ref realismLoudness);
                    }
                }
            }

            if (ModDetection.RealismLoaded) {
                MuzzleLoudnessRealism = (realismLoudness / 200) + 1f;
                CalculatedAudibleRange = BaseAudibleRange * SuppressorModifier * MuzzleLoudnessRealism;
            }
            else {
                MuzzleLoudness = loudness / 4f;
                CalculatedAudibleRange = BaseAudibleRange * SuppressorModifier + MuzzleLoudness;
            }
        }

        private static void calcLoudness(Mod mod, ref float loudness, ref float realismLoudness)
        {
            // Calculate loudness
            if (!ModDetection.RealismLoaded) {
                loudness += mod.Template.Loudness;
            }
            else {
                // For RealismMod: if the muzzle device has a silencer attached to it then it shouldn't contribute to the loudness stat.
                Item containedItem = null;
                if (mod.Slots.Length > 0) {
                    containedItem = mod.Slots[0].ContainedItem;
                }
                if (containedItem == null
                    || (containedItem is Mod modItem && IsModSuppressor(modItem, out var suppressor))) {
                    realismLoudness += mod.Template.Loudness;
                }
            }
        }

        private static bool IsModSuppressor(Mod mod, out Item suppressor)
        {
            suppressor = null;
            if (mod.Slots.Length > 0) {
                Item item = mod.Slots[0].ContainedItem;
                if (item != null && mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[SuppressorTypeId]) {
                    suppressor = item;
                }
            }
            return suppressor != null;
        }

        private void CheckModForSuppresorAndSights(Mod mod)
        {
            if (mod != null) {
                Type modType = mod.GetType();
                checkItemType(modType);
            }
        }

        private void checkItemType(Type type)
        {
            if (!HasSuppressor &&
                isSuppressor(type)) {
                HasSuppressor = true;
            }
            else if (!HasOptic &&
                IsOptic(type)) {
                HasOptic = true;
            }
            else if (!HasRedDot &&
                IsRedDot(type)) {
                HasRedDot = true;
            }
        }

        private static bool isSuppressor(Type modType)
        {
            return modType == TemplateIdToObjectMappingsClass.TypeTable[SuppressorTypeId];
        }

        private static bool IsOptic(Type modType)
        {
            return CheckTemplates(modType, AssaultScopeTypeId, OpticScopeTypeId, SpecialScopeTypeId);
        }

        private static bool IsRedDot(Type modType)
        {
            return CheckTemplates(modType, CollimatorTypeId, CompactCollimatorTypeId);
        }

        private static bool CheckTemplates(Type modType, params string[] templateIDs)
        {
            for (int i = 0; i < templateIDs.Length; i++) {
                if (CheckTemplateType(modType, templateIDs[i])) {
                    return true;
                }
            }
            return false;
        }

        private static bool CheckTemplateType(Type modType, string id)
        {
            if (TemplateIdToObjectMappingsClass.TypeTable.TryGetValue(id, out Type result)) {
                if (result == modType) {
                    return true;
                }
            }
            if (TemplateIdToObjectMappingsClass.TemplateTypeTable.TryGetValue(id, out result)) {
                if (result == modType) {
                    return true;
                }
            }
            return false;
        }

        private static EWeaponClass TryGetWeaponClass(Weapon weapon)
        {
            EWeaponClass WeaponClass = EnumValues.TryParse<EWeaponClass>(weapon.Template.weapClass);
            if (WeaponClass == default) {
                WeaponClass = EnumValues.TryParse<EWeaponClass>(weapon.WeapClass);
            }
            return WeaponClass;
        }

        private static ECaliber TryGetAmmoCaliber(Weapon weapon)
        {
            ECaliber caliber = EnumValues.TryParse<ECaliber>(weapon.Template.ammoCaliber);
            if (caliber == default) {
                caliber = EnumValues.TryParse<ECaliber>(weapon.AmmoCaliber);
            }
            return caliber;
        }

        private void Log()
        {
            if (SAINPlugin.DebugMode) {
                Logger.LogWarning(
                    $"Found Weapon Info: " +
                    $"Weapon: [{Weapon.ShortName}] " +
                    $"Weapon Class: [{WeaponClass}] " +
                    $"Ammo Caliber: [{AmmoCaliber}] " +
                    $"Calculated Audible Range: [{CalculatedAudibleRange}] " +
                    $"Base Audible Range: [{BaseAudibleRange}] " +
                    $"Muzzle Loudness: [{MuzzleLoudness}] " +
                    $"Muzzle Loudness Realism: [{MuzzleLoudnessRealism}] " +
                    $"Speed Factor: [{SpeedFactor}] " +
                    $"Subsonic: [{Subsonic}] " +
                    $"Has Red Dot? [{HasRedDot}] " +
                    $"Has Optic? [{HasOptic}] " +
                    $"Has Suppressor? [{HasSuppressor}]");
            }
        }

        private float _nextUpdateTime;
        private const float SuperSonicSpeed = 343.2f;
        private static readonly string SuppressorTypeId = "550aa4cd4bdc2dd8348b456c";
        private static readonly string CollimatorTypeId = "55818ad54bdc2ddc698b4569";
        private static readonly string CompactCollimatorTypeId = "55818acf4bdc2dde698b456b";
        private static readonly string AssaultScopeTypeId = "55818add4bdc2d5b648b456f";
        private static readonly string OpticScopeTypeId = "55818ae44bdc2dde698b456c";
        private static readonly string SpecialScopeTypeId = "55818aeb4bdc2ddc698b456a";
    }

    public class OpticAIConfig
    {
        public string Name;
        public string TypeId;
        public float FarDistanceScaleStart;
        public float FarDistanceScaleEnd;
        public float FarMultiplier;
        public float CloseDistanceScaleStart;
        public float CloseDistanceScaleEnd;
        public float CloseMultiplier;
    }
}