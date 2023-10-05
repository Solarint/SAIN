using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using HarmonyLib;
using Interpolation;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings.Categories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using static EFT.Player;

namespace SAIN.SAINComponent.Classes.Info
{
    public class SAINBotEquipmentClass : SAINBase, ISAINClass
    {
        static SAINBotEquipmentClass()
        {
            InventoryControllerProp = AccessTools.Field(typeof(Player), "_inventoryController");
        }

        public static readonly FieldInfo InventoryControllerProp;

        public SAINBotEquipmentClass(SAINComponentClass sain) : base(sain)
        {
        }

        public void Init()
        {
            InventoryController = Reflection.GetValue<InventoryControllerClass>(Player, InventoryControllerProp);
            GearInfo = new GearInfo(Player, InventoryController);
        }

        public void Update()
        {
            if (UpdateEquipmentTimer < Time.time)
            {
                UpdateEquipmentTimer = Time.time + 60f;
                GearInfo.Update();
                UpdateWeapons(Player);
            }
        }

        public void Dispose()
        {
        }

        public InventoryControllerClass InventoryController { get; private set; }

        // delete later
        public bool HasEarPiece => GearInfo.HasEarPiece;
        public bool HasHeavyHelmet => GearInfo.HasHeavyHelmet;


        private float UpdateEquipmentTimer = 0f;

        public void UpdateWeapons(Player player)
        {
            if (player == null)
            {
                return;
            }
            Item primaryItem = GearInfo.GetItem(EquipmentSlot.FirstPrimaryWeapon);
            if (primaryItem != null && primaryItem is Weapon primaryWeapon)
            {
                // if (SAINPlugin.DebugMode) Logger.LogWarning("Found FirstPrimary Weapon");
                PrimaryWeapon.Update(primaryWeapon);
                PrimaryWeapon.Log();
            }
            Item secondaryItem = GearInfo.GetItem(EquipmentSlot.SecondPrimaryWeapon);
            if (secondaryItem != null && secondaryItem is Weapon secondaryWeapon)
            {
                // if (SAINPlugin.DebugMode) Logger.LogWarning("Found SecondPrimary Weapon");
                SecondaryWeapon.Update(secondaryWeapon);
                SecondaryWeapon.Log();
            }
            Item holsterItem = GearInfo.GetItem(EquipmentSlot.Holster);
            if (holsterItem != null && holsterItem is Weapon holsterWeapon)
            {
                // if (SAINPlugin.DebugMode) Logger.LogWarning("Found Holster Weapon");
                HolsterWeapon.Update(holsterWeapon);
                HolsterWeapon.Log();
            }
        }

        public static float CalcEquipmentPower(Player player, Inventory inventory)
        {
            return 1f;
        }

        public GearInfo GearInfo { get; private set; }
        public WeaponInfo PrimaryWeapon { get; private set; } = new WeaponInfo();
        public WeaponInfo SecondaryWeapon { get; private set; } = new WeaponInfo();
        public WeaponInfo HolsterWeapon { get; private set; } = new WeaponInfo();
    }

    public class WeaponInfo 
    {
        public void Update(Weapon weapon)
        {
            HasSuppressor = false;
            HasRedDot = false;
            HasOptic = false;

            WeaponClass = EnumValues.ParseWeaponClass(weapon.Template.weapClass);

            var mods = weapon.Mods;
            for (int i = 0; i < mods.Length; i++)
            {
                CheckMod(mods[i]);
                if (mods[i].Slots.Length > 0)
                {
                    for (int j = 0; j < mods[i].Slots.Length; j++)
                    {
                        Item containedItem = mods[i].Slots[j].ContainedItem;
                        if (containedItem != null && containedItem is Mod mod)
                        {
                            Type modType = mod.GetType();
                            if (IsSilencer(modType))
                            {
                                HasSuppressor = true;
                            }
                            else if (IsOptic(modType))
                            {
                                HasOptic = true;
                            }
                            else if (IsRedDot(modType))
                            {
                                HasRedDot = true;
                            }
                        }
                    }
                }
            }
        }

        private void CheckMod(Mod mod)
        {
            if (mod != null)
            {
                Type modType = mod.GetType();
                if (IsSilencer(modType))
                {
                    HasSuppressor = true;
                }
                else if (IsOptic(modType))
                {
                    HasOptic = true;
                }
                else if (IsRedDot(modType))
                {
                    HasRedDot = true;
                }
            }
        }

        public void Log()
        {
            if (SAINPlugin.DebugMode)
            {
                Logger.LogWarning(
                    $"Found Weapon Info: " +
                    $"Weapon Class: [{WeaponClass}] " +
                    $"Has Red Dot? [{HasRedDot}] " +
                    $"Has Optic? [{HasOptic}] " +
                    $"Has Suppressor? [{HasSuppressor}]");
            }
        }

        public IWeaponClass WeaponClass;
        public bool HasRedDot;
        public bool HasOptic;
        public bool HasSuppressor;

        private static bool IsSilencer(Type modType)
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
            for (int i = 0; i < templateIDs.Length; i++)
            {
                if (CheckTemplateType(modType, templateIDs[i]))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool CheckTemplateType(Type modType, string id)
        {
            if (TemplateIdToObjectMappingsClass.TypeTable.TryGetValue(id, out Type result))
            {
                if (result == modType)
                {
                    return true;
                }
            }
            if (TemplateIdToObjectMappingsClass.TemplateTypeTable.TryGetValue(id, out result))
            {
                if (result == modType)
                {
                    return true;
                }
            }
            return false;
        }

        private static readonly string SuppressorTypeId = "550aa4cd4bdc2dd8348b456c";
        private static readonly string CollimatorTypeId = "55818ad54bdc2ddc698b4569";
        private static readonly string CompactCollimatorTypeId = "55818acf4bdc2dde698b456b";
        private static readonly string AssaultScopeTypeId = "55818add4bdc2d5b648b456f";
        private static readonly string OpticScopeTypeId = "55818ae44bdc2dde698b456c";
        private static readonly string SpecialScopeTypeId = "55818aeb4bdc2ddc698b456a";
    }

    public enum EScopeType
    {
        RedDot,
        Optic,
        Assault,
        Special
    }

    public class GearInfo
    {
        public GearInfo(Player player, InventoryControllerClass inventoryController)
        {
            Player = player;
            InventoryController = inventoryController;
        }

        public readonly Player Player;

        public readonly InventoryControllerClass InventoryController;

        public void Update()
        {
            HasEarPiece = GetItem(EquipmentSlot.Earpiece) != null;

            // Reset previous results if any
            HasFaceShield = false;

            // Get the headwear item on this player
            Item helmetItem = GetItem(EquipmentSlot.Headwear);

            if (helmetItem != null)
            {
                // Get a list of faceshield components attached to the headwear item, see if any have AC.
                helmetItem.GetItemComponentsInChildrenNonAlloc(FaceShieldComponents);
                foreach (var faceComponent in FaceShieldComponents)
                {
                    if (faceComponent.Item.IsArmorMod())
                    {
                        HasFaceShield = true;
                        break;
                    }
                }
                FaceShieldComponents.Clear();
            }

            // Reset previous results if any
            HasHeavyHelmet = false;

            // Get a list of armor components attached to the headwear item, check to see which has the highest AC, and check if any make the user deaf.
            HelmetArmorClass = FindMaxAC(helmetItem, HelmetArmorComponents);

            if (HelmetArmorComponents.Count > 0)
            {
                foreach (ArmorComponent armor in HelmetArmorComponents)
                {
                    if (armor.Deaf == EDeafStrength.High)
                    {
                        HasHeavyHelmet = true;
                        break;
                    }
                }
                HelmetArmorComponents.Clear();
            }

            int vestAC = FindMaxAC(EquipmentSlot.ArmorVest);
            int bodyAC = FindMaxAC(EquipmentSlot.TacticalVest);
            BodyArmorClass = Mathf.Max(vestAC, bodyAC);

            if (SAINPlugin.DebugMode)
            {
                Logger.LogInfo(
                    $" Found GearInfo for [{Player.Profile.Nickname}]:" +
                    $" Body Armor Class: [{BodyArmorClass}]" + 
                    $" Helmet Armor Class [{HelmetArmorClass}]" + 
                    $" Has Heavy Helmet? [{HasHeavyHelmet}]" +
                    $" Has EarPiece? [{HasEarPiece}]" +
                    $" Has Face Shield? [{HasFaceShield}]");
            }
        }

        public bool HasEarPiece { get; private set; }

        public bool HasHelmet => HelmetArmorClass > 0;

        public bool HasHeavyHelmet { get; private set; }

        public int HelmetArmorClass { get; private set; }

        private readonly List<ArmorComponent> HelmetArmorComponents = new List<ArmorComponent>();

        public bool HasFaceShield { get; private set; }

        private readonly List<FaceShieldComponent> FaceShieldComponents = new List<FaceShieldComponent>();

        public bool HasArmor => BodyArmorClass != 0;

        public int BodyArmorClass { get; private set; }

        public Item GetItem(EquipmentSlot slot)
        {
            return InventoryController.Inventory.Equipment.GetSlot(slot).ContainedItem;
        }

        private static int FindMaxAC(Item item, List<ArmorComponent> armorComponents)
        {
            if (item == null) return 0;

            armorComponents.Clear();
            item.GetItemComponentsInChildrenNonAlloc(armorComponents, true);
            return FindMaxAC(armorComponents);
        }

        private static int FindMaxAC(List<ArmorComponent> armorComponents)
        {
            int result = 0;
            for (int i = 0; i < armorComponents.Count; i++)
            {
                ArmorComponent armor = armorComponents[i];
                if (armor.ArmorClass > result)
                {
                    result = armor.ArmorClass;
                }
            }
            return result;
        }

        private int FindMaxAC(EquipmentSlot slot)
        {
            Item item = InventoryController.Inventory.Equipment.GetSlot(slot).ContainedItem;
            return FindMaxAC(item, StaticArmorList);
        }


        private static readonly List<ArmorComponent> StaticArmorList = new List<ArmorComponent>();
    }
}