using EFT;
using EFT.InventoryLogic;
using SAIN.Helpers;
using SAIN.Plugin;
using SAIN.Preset;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Info;

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

    public EWeaponClass WeaponClass { get; private set; }
    public ECaliber AmmoCaliber { get; private set; }
    public float CalculatedAudibleRange { get; private set; }
    public AISoundType AISoundType => HasSuppressor ? AISoundType.silencedGun : AISoundType.gun;
    public SAINSoundType SoundType => HasSuppressor ? SAINSoundType.SuppressedShot : SAINSoundType.Shot;
    public bool HasRedDot => RedDot != null;
    public bool HasOptic => Optic != null;
    public bool HasSuppressor { get; private set; }
    public float BaseAudibleRange { get; private set; } = 150f;
    public float MuzzleLoudness { get; private set; }
    public bool Subsonic => Weapon != null && BulletSpeed < SuperSonicSpeed;
    public Mod Suppressor { get; private set; }
    public Mod RedDot { get; private set; }
    public Mod Optic { get; private set; }
    public float BulletSpeed { get; private set; } = 600f;
    public float EngagementDistance { get; private set; } = 150f;
    public Weapon Weapon { get; private set; }
    public float Durability => Weapon.Repairable.Durability / (float)Weapon.Repairable.TemplateDurability;

    private void updateSettings(SAINPresetClass preset)
    {
        if (preset.GlobalSettings.Shoot.EngagementDistance.TryGetValue(WeaponClass, out float distance))
        {
            EngagementDistance = distance;
        }
        if (preset.GlobalSettings.Hearing.HearingDistances.TryGetValue(AmmoCaliber, out float range))
        {
            BaseAudibleRange = range;
        }
    }

    public void Update(Player Player)
    {
        if (Weapon != null)
        {
            UpdateWeaponData(Player, Weapon.Mods);
        }
    }

    private void UpdateWeaponData(Player Player, IEnumerable<Mod> mods)
    {
        Suppressor = FindModType(mods, EModType.Suppressor);
        RedDot = FindModType(mods, EModType.RedDot);
        Optic = FindModType(mods, EModType.Optic);
        MuzzleLoudness = Suppressor != null ? Suppressor.Template.Loudness : 0f;
        BulletSpeed = Weapon.CurrentAmmoTemplate.InitialSpeed * Weapon.SpeedFactor;
        CalculatedAudibleRange = BaseAudibleRange + MuzzleLoudness;
        if (Suppressor != null || (Player.HandsController is Player.FirearmController firearmController && firearmController.IsSilenced))
        {
            CalculatedAudibleRange *= SuppressorModifier(BulletSpeed);
            HasSuppressor = true;
        }
        else
        {
            HasSuppressor = false;
        }
        //Log();
    }
    
    public void WeaponEquiped(Player player)
    {
        Update(player);
    }

    public void WeaponModified(Player player)
    {
        Update(player);
    }

    public void Dispose()
    {
        PresetHandler.OnPresetUpdated -= updateSettings;
    }

    private static Mod FindModType(IEnumerable<Mod> mods, EModType ModType)
    {
        if (mods != null)
        {
            foreach (Mod mod in mods)
            {
                if (CheckItemType(mod.GetType()) == ModType)
                    return mod;
                Slot[] Slots = mod.Slots;
                foreach (Slot slot in Slots)
                    if (slot.ContainedItem is Mod ContainedMod && CheckItemType(ContainedMod.GetType()) == ModType)
                        return ContainedMod;
            }
        }
        return null;
    }

    private static float SuppressorModifier(float bulletspeed)
    {
        if (bulletspeed < SuperSonicSpeed)
        {
            return SAINPlugin.LoadedPreset.GlobalSettings.Hearing.SubsonicModifier;
        }
        return SAINPlugin.LoadedPreset.GlobalSettings.Hearing.SuppressorModifier;
    }

    private static EModType CheckItemType(Type type)
    {
        if (CheckTemplateType(type, SuppressorTypeId))
        {
            return EModType.Suppressor;
        }
        for (int i = 0; i < RedDotTypes.Length; i++)
        {
            if (CheckTemplateType(type, RedDotTypes[i]))
            {
                return EModType.RedDot;
            }
        }
        for (int i = 0; i < OpticTypes.Length; i++)
        {
            if (CheckTemplateType(type, OpticTypes[i]))
            {
                return EModType.Optic;
            }
        }
        return EModType.None;
    }

    private static bool CheckTemplateType(Type modType, string id)
    {
        if (TemplateIdToObjectMappingsClass.TypeTable.TryGetValue(id, out Type result) && result == modType)
        {
            return true;
        }
        if (TemplateIdToObjectMappingsClass.TemplateTypeTable.TryGetValue(id, out result) && result == modType)
        {
            return true;
        }
        return false;
    }

    private static EWeaponClass TryGetWeaponClass(Weapon weapon)
    {
        EWeaponClass WeaponClass = EnumValues.TryParse<EWeaponClass>(weapon.Template.weapClass);
        if (WeaponClass == default)
        {
            WeaponClass = EnumValues.TryParse<EWeaponClass>(weapon.WeapClass);
        }
        return WeaponClass;
    }

    private static ECaliber TryGetAmmoCaliber(Weapon weapon)
    {
        ECaliber caliber = EnumValues.TryParse<ECaliber>(weapon.Template.ammoCaliber);
        if (caliber == default)
        {
            caliber = EnumValues.TryParse<ECaliber>(weapon.AmmoCaliber);
        }
        return caliber;
    }

    private void Log()
    {
        Logger.LogDebug(
            $"Found Weapon Info: " +
            $"Weapon: [{Weapon.ShortName}] " +
            $"Weapon Class: [{WeaponClass}] " +
            $"Ammo Caliber: [{AmmoCaliber}] " +
            $"Calculated Audible Range: [{CalculatedAudibleRange}] " +
            $"Base Audible Range: [{BaseAudibleRange}] " +
            $"Muzzle Loudness: [{MuzzleLoudness}] " +
            $"Speed Factor: [{Weapon.SpeedFactor}] " +
            $"Subsonic: [{Subsonic}] " +
            $"Has Red Dot? [{HasRedDot}] " +
            $"Has Optic? [{HasOptic}] " +
            $"Has Suppressor? [{HasSuppressor}]");
    }

    private const float SuperSonicSpeed = 343.2f;

    //private static readonly string FlashHiderTypeId = "550aa4bf4bdc2dd6348b456b";
    //private static readonly string MuzzleTypeId = "5448fe394bdc2d0d028b456c";
    private static readonly string SuppressorTypeId = "550aa4cd4bdc2dd8348b456c";

    private static readonly string CollimatorTypeId = "55818ad54bdc2ddc698b4569";
    private static readonly string CompactCollimatorTypeId = "55818acf4bdc2dde698b456b";
    private static readonly string AssaultScopeTypeId = "55818add4bdc2d5b648b456f";
    private static readonly string OpticScopeTypeId = "55818ae44bdc2dde698b456c";
    private static readonly string SpecialScopeTypeId = "55818aeb4bdc2ddc698b456a";

    private static readonly string[] OpticTypes = { AssaultScopeTypeId, OpticScopeTypeId, SpecialScopeTypeId };
    private static readonly string[] RedDotTypes = { CollimatorTypeId, CompactCollimatorTypeId };
}

public enum EModType
{
    None,
    Suppressor,
    RedDot,
    Optic,
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