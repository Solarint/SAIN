using SAIN.Helpers;
using System;
using System.Collections.Generic;

namespace SAIN.Preset.GearStealthValues;

public class GearStealthValuesClass
{
    public Dictionary<EEquipmentType, List<ItemStealthValue>> ItemStealthValues = new();
    public readonly List<ItemStealthValue> Defaults = new();

    public GearStealthValuesClass(SAINPresetDefinition preset)
    {
        try
        {
            import(preset);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
        initDefaults();
        Export(this, preset);
    }

    private void import(SAINPresetDefinition preset)
    {
        if (!preset.IsCustom)
        {
            return;
        }
        if (!JsonUtility.DoesFolderExist("Presets", preset.Name, "ItemStealthValues"))
        {
            return;
        }

        var list = new List<ItemStealthValue>();
        JsonUtility.Load.LoadStealthValues(list, "Presets", preset.Name, "ItemStealthValues");
        foreach (var type in EnumValues.GetEnum<EEquipmentType>())
        {
            var itemList = getList(type);
            foreach (var item in list)
            {
                if (item.EquipmentType != type)
                    continue;

                Logger.LogDebug($"Adding {item.Name}");
                addItem(item.Name, item.EquipmentType, item.ItemID, item.StealthValue, itemList);
            }
        }
    }

    public static void Export(GearStealthValuesClass stealthValues, SAINPresetDefinition preset)
    {
        if (!preset.IsCustom)
        {
            return;
        }

        JsonUtility.CreateFolder("Presets", preset.Name, "ItemStealthValues");
        JsonUtility.SaveObjectToJson(EnumValues.GetEnum<EEquipmentType>(), "Possible Item Types For Stealth Modifiers", "Presets", preset.Name);

        foreach (var list in stealthValues.ItemStealthValues.Values)
        {
            foreach (var item in list)
            {
                JsonUtility.SaveObjectToJson(item, item.Name, "Presets", preset.Name, "ItemStealthValues");
            }
        }
    }

    private void initDefaults()
    {
        var headWears = getList(EEquipmentType.Headwear);
        addItem("MILTEC", EEquipmentType.Headwear, boonie_MILTEC, 1.2f, headWears, true);
        addItem("CHIMERA", EEquipmentType.Headwear, boonie_CHIMERA, 1.2f, headWears, true);
        addItem("DOORKICKER", EEquipmentType.Headwear, boonie_DOORKICKER, 1.2f, headWears, true);
        addItem("JACK_PYKE", EEquipmentType.Headwear, boonie_JACK_PYKE, 1.2f, headWears, true);
        addItem("TAN_ULACH", EEquipmentType.Headwear, helmet_TAN_ULACH, 0.9f, headWears, true);
        addItem("UNTAR_BLUE", EEquipmentType.Headwear, helmet_UNTAR_BLUE, 0.85f, headWears, true);

        var backPacks = getList(EEquipmentType.BackPack);
        addItem("Pilgrim", EEquipmentType.BackPack, backpack_pilgrim, 0.85f, backPacks, true);
        addItem("Raid", EEquipmentType.BackPack, backpack_raid, 0.875f, backPacks, true);
    }

    private List<ItemStealthValue> getList(EEquipmentType type)
    {
        if (!ItemStealthValues.TryGetValue(type, out var list))
        {
            list = new List<ItemStealthValue>();
            ItemStealthValues.Add(type, list);
        }
        return list;
    }

    private void addItem(string name, EEquipmentType type, string id, float stealthValue, List<ItemStealthValue> list, bool addAsDefault = false)
    {
        if (!doesItemExist(name, list))
        {
            list.Add(new ItemStealthValue
            {
                Name = name,
                EquipmentType = type,
                ItemID = id,
                StealthValue = stealthValue,
            });
        }
        if (addAsDefault)
        {
            addItem(name, type, id, stealthValue, Defaults, false);
        }
    }

    private bool doesItemExist(string name, List<ItemStealthValue> list)
    {
        foreach (var item in list)
        {
            if (item.Name == name)
            {
                return true;
            }
        }
        return false;
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