using EFT.InventoryLogic;
using SAIN.Preset.GearStealthValues;
using SAIN.SAINComponent.Classes.Info;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components.PlayerComponentSpace.Classes.Equipment;

public class AIGearModifierClass(SAINAIData sAINAIData) : AIDataBase(sAINAIData)
{
    public float StealthModifier(float distance)
    {
        return getSightMod(distance);
    }

    private float gearStealthModifier
    {
        get
        {
            if (_calcGearTime < Time.time)
            {
                _calcGearTime = Time.time + 1f;

                float modifier = 1f;
                bool success = false;
                try
                {
                    modifier = calcGearEffects();
                    success = true;
                }
                catch //(Exception e)
                {
                }

                if (success)
                {
#if DEBUG
                    if (_nextLogTime < Time.time && modifier != 1f)
                    {
                        _nextLogTime = Time.time + 60f;
                        //Logger.LogDebug($"Stealth Mod: {modifier}");
                    }
#endif
                    _gearStealthModifier = modifier;
                }
                else
                {
                    float backPackMod = getBackpackMod();
                    float headWearMod = getHeadWearMod();
                    float faceCoverMod = getFaceCoverMod();
                    _gearStealthModifier = backPackMod * headWearMod * faceCoverMod;
                }
            }
            return _gearStealthModifier;
        }
    }

#if DEBUG
    private static float _nextLogTime;
#endif

    private float calcGearEffects()
    {
        var stealthValues = SAINPlugin.LoadedPreset.GearStealthValuesClass.ItemStealthValues;
        float result = 1f;
        Item item;
        foreach (var value in stealthValues)
        {
            if (value.Value.Count == 0)
                continue;

            switch (value.Key)
            {
                case EEquipmentType.Headwear:
                    item = GearInfo.GetItem(EquipmentSlot.Headwear);
                    if (item != null)
                        result *= calcEffect(item.TemplateId, value.Value);
                    break;

                case EEquipmentType.BackPack:
                    item = GearInfo.GetItem(EquipmentSlot.Backpack);
                    if (item != null)
                        result *= calcEffect(item.TemplateId, value.Value);
                    else
                        result *= 1.1f;
                    break;

                case EEquipmentType.FaceCover:
                    item = GearInfo.GetItem(EquipmentSlot.FaceCover);
                    if (item != null)
                    {
                        float faceCoverValue = calcEffect(item.TemplateId, value.Value);
                        if (faceCoverValue == 1f)
                            result *= 1.05f;
                    }
                    break;

                case EEquipmentType.Rig:
                    item = GearInfo.GetItem(EquipmentSlot.TacticalVest);
                    if (item != null)
                        result *= calcEffect(item.TemplateId, value.Value);
                    break;

                case EEquipmentType.ArmorVest:
                    item = GearInfo.GetItem(EquipmentSlot.ArmorVest);
                    if (item != null)
                        result *= calcEffect(item.TemplateId, value.Value);
                    break;

                case EEquipmentType.EyeWear:
                    item = GearInfo.GetItem(EquipmentSlot.Eyewear);
                    if (item != null)
                        result *= calcEffect(item.TemplateId, value.Value);
                    break;

                default:
                    break;
            }
        }
        return result;
    }

    private float calcEffect(string id, List<ItemStealthValue> values)
    {
        foreach (var value in values)
        {
            if (value.ItemID == id)
            {
                return value.StealthValue;
            }
        }
        return 1f;
    }

    private float _gearStealthModifier = 1f;

    private float _calcGearTime;

    private float getSightMod(float distance)
    {
        float min = 30f;
        float max = 60f;
        if (distance <= min)
        {
            return 1f;
        }

        float modifier = gearStealthModifier;

        if (distance >= max)
        {
            return modifier;
        }

        float num = max - min;
        float num2 = distance - min;
        float ratio = num2 / num;
        float result = Mathf.Lerp(1f, modifier, ratio);

        return result;
    }

    private float getBackpackMod()
    {
        Item backpack = GearInfo.GetItem(EquipmentSlot.Backpack);
        if (backpack == null)
        {
            return 1.15f;
        }
        switch (backpack.TemplateId)
        {
            case backpack_pilgrim:
                return 0.875f;

            case backpack_raid:
                return 0.925f;

            default:
                return 1f;
        }
    }

    private float getHeadWearMod()
    {
        Item headwear = GearInfo.GetItem(EquipmentSlot.Headwear);
        if (headwear == null)
        {
            return 1f;
        }
        switch (headwear.TemplateId)
        {
            case boonie_MILTEC:
                return 1.2f;

            case boonie_CHIMERA:
                return 1.2f;

            case boonie_DOORKICKER:
                return 1.2f;

            case boonie_JACK_PYKE:
                return 1.2f;

            case helmet_TAN_ULACH:
                return 0.925f;

            case helmet_UNTAR_BLUE:
                return 0.9f;

            default:
                return 1f;
        }
    }

    private float getFaceCoverMod()
    {
        Item faceCover = GearInfo.GetItem(EquipmentSlot.FaceCover);
        if (faceCover == null)
        {
            return 1f;
        }
        switch (faceCover.TemplateId)
        {
            default:
                return 1.05f;
        }
    }

    private const string backpack_pilgrim = "59e763f286f7742ee57895da";
    private const string backpack_raid = "5df8a4d786f77412672a1e3b";
    private const string boonie_MILTEC = "5b4327aa5acfc400175496e0";
    private const string boonie_CHIMERA = "60b52e5bc7d8103275739d67";
    private const string boonie_DOORKICKER = "5d96141523f0ea1b7f2aacab";
    private const string boonie_JACK_PYKE = "618aef6d0a5a59657e5f55ee";
    private const string helmet_TAN_ULACH = "5b40e2bc5acfc40016388216";
    private const string helmet_UNTAR_BLUE = "5aa7d03ae5b5b00016327db5";
}