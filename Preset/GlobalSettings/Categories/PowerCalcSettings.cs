using EFT;
using EFT.InventoryLogic;
using Newtonsoft.Json;
using SAIN.Attributes;
using SAIN.Components.PlayerComponentSpace;
using SAIN.SAINComponent.Classes.Info;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings;

public class PowerCalcSettings : SAINSettingsBase<PowerCalcSettings>, ISAINSettings
{
    [Name("PMC Power")]
    [Description("Add X points to a bot's power level if they are a PMC")]
    [Category("Bot Type Power Value")]
    [MinMax(-100, 100, 10)]
    public float PMC_POWER = 20f;

    [Name("Scav Power")]
    [Description("Add X points to a bot's power level if they are a Scav")]
    [Category("Bot Type Power Value")]
    [MinMax(-100, 100, 10)]
    public float SCAV_POWER = -20f;

    [Name("Shotgun Power")]
    [Description("Add X points to a bot's power level if they are using this type of weapon as their primary.")]
    [Category("Weapon Class Power Value")]
    [MinMax(-100, 100, 10)]
    public float SHOTGUN_POWER = 40f;

    [Name("Smg Power")]
    [Description("Add X points to a bot's power level if they are using this type of weapon as their primary.")]
    [Category("Weapon Class Power Value")]
    [MinMax(-100, 100, 10)]
    public float SMG_POWER = 75f;

    [Name("Assault Carbine Power")]
    [Description("Add X points to a bot's power level if they are using this type of weapon as their primary.")]
    [Category("Weapon Class Power Value")]
    [MinMax(-100, 100, 10)]
    public float ASSAULT_CARBINE_POWER = 60f;

    [Name("Assault Rifle Power")]
    [Description("Add X points to a bot's power level if they are using this type of weapon as their primary.")]
    [Category("Weapon Class Power Value")]
    [MinMax(-100, 100, 10)]
    public float ASSAULT_RIFLE_POWER = 45f;

    [Name("Machinegun Power")]
    [Description("Add X points to a bot's power level if they are using this type of weapon as their primary.")]
    [Category("Weapon Class Power Value")]
    [MinMax(-100, 100, 10)]
    public float MG_POWER = 55f;

    [Name("Sniper Rifle Power")]
    [Description("Add X points to a bot's power level if they are using this type of weapon as their primary.")]
    [Category("Weapon Class Power Value")]
    [MinMax(-100, 100, 10)]
    public float SNIPE_POWER = -30f;

    [Name("Marksman Rifle Power")]
    [Description("Add X points to a bot's power level if they are using this type of weapon as their primary.")]
    [Category("Weapon Class Power Value")]
    [MinMax(-100, 100, 10)]
    public float MARKSMAN_RIFLE_POWER = 10f;

    [Name("Pistol Power")]
    [Description("Add X points to a bot's power level if they are using this type of weapon as their primary.")]
    [Category("Weapon Class Power Value")]
    [MinMax(-100, 100, 10)]
    public float PISTOL_POWER = -10f;

    [Name("Red Dot / 1x Holo Sight Power")]
    [Description("Add X points to a bot's power level if they are using this type of attachment on their primary.")]
    [Category("Attachment Power Value")]
    [MinMax(-100, 100, 10)]
    public float RED_DOT_POWER = 30f;

    [Name("Magnified Optic Power")]
    [Description("Add X points to a bot's power level if they are using this type of attachment on their primary.")]
    [Category("Attachment Power Value")]
    [MinMax(-100, 100, 10)]
    public float OPTIC_POWER = -20f;

    [Name("Suppressor Power")]
    [Description("Add X points to a bot's power level if they are using this type of attachment on their primary.")]
    [Category("Attachment Power Value")]
    [MinMax(-100, 100, 10)]
    public float SUPPRESSOR_POWER = 20f;

    [Name("Body Armor Class Power")]
    [Description("For each AC level, add X to a bot's power level. So if they have level 4 armor, add this value 4 times.")]
    [Category("Armor Power Value")]
    [MinMax(-100, 100, 10)]
    public float ARMOR_CLASS_COEF = 30f;

    [Name("Body Armor Class Power - Realism Mod")]
    [Description("If Realism Mod is loaded, use this AC Power value. " +
        "For each AC level, add X to a bot's power level. So if they have level 4 armor, add this value 4 times.")]
    [Category("Armor Power Value")]
    [MinMax(-100, 100, 10)]
    public float ARMOR_CLASS_COEF_REALISM = 20f;

    [Name("Helmet Class Power")]
    [Description("If a bot has an armored helmet above class 1, but lower than 5, add X to thier power level.")]
    [Category("Armor Power Value")]
    [MinMax(-100, 100, 10)]
    public float HELMET_POWER = 30f;

    [Name("Heavy Helmet Class Power")]
    [Description("If a bot has an armored helmet above class 4, add X to thier power level.")]
    [Category("Armor Power Value")]
    [MinMax(-100, 100, 10)]
    public float HELMET_HEAVY_POWER = 60f;

    [Name("Faceshield Power")]
    [Description("If a bot has an armored face shield, add X to thier Power Level.")]
    [Category("Armor Power Value")]
    [MinMax(-100, 100, 10)]
    public float FACESHIELD_POWER = 20f;

    [Name("Headphones Power")]
    [Description("If a bot has headphones, add X to thier Power Level.")]
    [Category("Armor Power Value")]
    [MinMax(-100, 100, 10)]
    public float EARPRO_POWER = 20f;

    public override void Init(List<ISAINSettings> list)
    {
        list.Add(this);
    }

    public bool CalcPower(PlayerComponent playerComponent, out float power)
    {
        power = 0f;
        if (playerComponent == null)
        {
            return false;
        }

        power += WeaponPower(playerComponent);
        if (power == 0f)
        {
            return false;
        }

        power += RolePower(playerComponent.Player.Profile.Info.Settings.Role);
        power += ArmorPower(playerComponent.Player);

        if (playerComponent.Player.AIData is PlayerAIDataClass aiData)
        {
            aiData.PowerOfEquipment = power;
        }

        return true;
    }

    private float RolePower(WildSpawnType type)
    {
        if (_PMCS.Contains(type))
        {
            return PMC_POWER;
        }
        else if (_SCAVS.Contains(type))
        {
            return SCAV_POWER;
        }
        return 0f;
    }

    private float WeaponPower(PlayerComponent player)
    {
        float result = 0f;

        WeaponInfo weaponInfo = player.Equipment.CurrentWeaponInfo ?? player.Equipment.WeaponInInventory;
        if (weaponInfo == null)
        {
            //Logger.LogError("weaponInfo Null");
            return 1f;
        }

        if (weaponInfo.HasSuppressor)
        {
            result += SUPPRESSOR_POWER;
        }
        if (weaponInfo.HasRedDot)
        {
            result += RED_DOT_POWER;
        }
        if (weaponInfo.HasOptic)
        {
            result += OPTIC_POWER;
        }

        switch (weaponInfo.WeaponClass)
        {
            case EWeaponClass.pistol:
                result += PISTOL_POWER;
                break;

            case EWeaponClass.smg:
                result += SMG_POWER;
                break;

            case EWeaponClass.assaultCarbine:
                result += ASSAULT_CARBINE_POWER;
                break;

            case EWeaponClass.assaultRifle:
                result += ASSAULT_RIFLE_POWER;
                break;

            case EWeaponClass.machinegun:
                result += MG_POWER;
                break;

            case EWeaponClass.marksmanRifle:
                result += MARKSMAN_RIFLE_POWER;
                break;

            case EWeaponClass.sniperRifle:
                result += SNIPE_POWER;
                break;

            case EWeaponClass.shotgun:
                result += SHOTGUN_POWER;
                break;

            default:
                break;
        }
        return result;
    }

    private float ArmorPower(Player player)
    {
        armorComponents.Clear();
        float result = 0;
        var equipment = player.Inventory?.Equipment;
        if (equipment != null)
        {
            var armorVest = equipment.GetSlot(EFT.InventoryLogic.EquipmentSlot.ArmorVest)?.ContainedItem;
            if (armorVest != null)
            {
                armorVest.GetItemComponentsInChildrenNonAlloc(armorComponents, true);
                float highestArmorClass = FindHighestArmorClass(armorComponents);
                //Logger.LogInfo($"Armor Components in Vest: [{armorComponents.Count}] Highest Armor Class: [{highestArmorClass}] Class Coef: [{ArmorClassCoef}] Combined: [{highestArmorClass * ArmorClassCoef}]");
                result += highestArmorClass * ArmorClassCoef;
                armorComponents.Clear();
            }
            else
            {
                var rig = equipment.GetSlot(EFT.InventoryLogic.EquipmentSlot.TacticalVest)?.ContainedItem;
                if (rig != null)
                {
                    rig.GetItemComponentsInChildrenNonAlloc(armorComponents, true);
                    if (armorComponents.Count > 0)
                    {
                        float highestArmorClass = FindHighestArmorClass(armorComponents);
                        //Logger.LogInfo($"Armor Components in Rig: [{armorComponents.Count}] Highest Armor Class: [{highestArmorClass}] Class Coef: [{ArmorClassCoef}] Combined: [{highestArmorClass * ArmorClassCoef}]");
                        result += highestArmorClass * ArmorClassCoef;
                        armorComponents.Clear();
                    }
                }
            }

            var helmet = equipment.GetSlot(EFT.InventoryLogic.EquipmentSlot.Headwear)?.ContainedItem;
            if (helmet != null)
            {
                helmet.GetItemComponentsInChildrenNonAlloc(armorComponents, true);
                if (armorComponents.Count > 0)
                {
                    float highestArmorClass = FindHighestArmorClass(armorComponents);
                    if (highestArmorClass > 4)
                    {
                        result += HELMET_HEAVY_POWER;
                    }
                    else if (highestArmorClass > 1)
                    {
                        result += HELMET_POWER;
                    }
                    //Logger.LogInfo($"Armor Components in Helmet: [{armorComponents.Count}] Highest Armor Class: [{highestArmorClass}]");
                    armorComponents.Clear();
                }
            }

            var faceProtection = equipment.GetSlot(EFT.InventoryLogic.EquipmentSlot.FaceCover)?.ContainedItem;
            if (faceProtection != null)
            {
                faceProtection.GetItemComponentsInChildrenNonAlloc(armorComponents, true);
                if (armorComponents.Count > 0)
                {
                    result += FACESHIELD_POWER;
                }
            }
            var earPro = equipment.GetSlot(EFT.InventoryLogic.EquipmentSlot.Earpiece)?.ContainedItem;
            if (earPro != null)
            {
                result += EARPRO_POWER;
            }
        }
        armorComponents.Clear();
        //Logger.LogInfo($"Armor Power Result: [{result}]");
        return result;
    }

    private float FindHighestArmorClass(List<ArmorComponent> armorComponents)
    {
        float result = 0f;
        foreach (var armorComponent in armorComponents)
        {
            float armorClass = armorComponent.ArmorClass;
            if (armorClass > result)
            {
                result = armorClass;
            }
        }
        return result;
    }

    [JsonIgnore]
    private float ArmorClassCoef
    {
        get
        {
            if (ModDetection.RealismLoaded)
            {
                return ARMOR_CLASS_COEF_REALISM;
            }
            return ARMOR_CLASS_COEF;
        }
    }

    [JsonIgnore]
    private static readonly List<ArmorComponent> armorComponents = new();

    [JsonIgnore]
    private static readonly List<WildSpawnType> _PMCS = new()
    {
        WildSpawnType.pmcUSEC,
        WildSpawnType.pmcBEAR
    };

    [JsonIgnore]
    private static readonly List<WildSpawnType> _SCAVS = new()
    {
        WildSpawnType.assault,
        WildSpawnType.cursedAssault,
        WildSpawnType.assaultGroup,
        WildSpawnType.crazyAssaultEvent,
        WildSpawnType.marksman
    };
}