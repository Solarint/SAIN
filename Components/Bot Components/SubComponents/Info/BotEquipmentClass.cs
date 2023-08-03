using BepInEx.Logging;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Components;
using SAIN.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAIN.Classes
{
    public class BotEquipmentClass : SAINBot
    {
        public BotEquipmentClass(SAINComponent sain) : base(sain)
        {
            if (Logger == null)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            }
            Inventory = (InventoryClass)AccessTools.Property(typeof(Player), "Inventory").GetValue(BotOwner.GetPlayer);
            InventoryController = HelpersGClass.GetInventoryController(BotOwner.GetPlayer);
        }

        public InventoryControllerClass InventoryController { get; private set; }

        private static ManualLogSource Logger;

        public InventoryClass Inventory { get; private set; }
        public Weapon CurrentWeapon => BotOwner.WeaponManager.CurrentWeapon;
        public Weapon SecondaryWeapon => BotOwner.WeaponManager.SecondWeaponInfo?.weapon;

        private readonly List<ArmorComponent> HelmetArmorComponents = new List<ArmorComponent>();

        public float InventorySpaceFilled { get; private set; }

        public void ManualUpdate()
        {
            if (UpdateEquipmentTimer < Time.time)
            {
                UpdateEquipmentTimer = Time.time + 30f;

                HasEarPiece = Equipment.GetSlot(EquipmentSlot.Earpiece).ContainedItem != null;
                CheckHelmet();
            }
        }

        private void CheckHelmet()
        {
            HasHelmet = false;
            HasHeavyHelmet = false;
            HelmetArmorComponents.Clear();

            var headWear = Equipment.GetSlot(EquipmentSlot.Headwear).ContainedItem;
            if (headWear != null)
            {
                headWear.GetItemComponentsInChildrenNonAlloc<ArmorComponent>(HelmetArmorComponents, true);
                if (HelmetArmorComponents.Count > 0)
                {
                    HasHelmet = true;
                    foreach (ArmorComponent armor in HelmetArmorComponents)
                    {
                        if (armor.Deaf == EDeafStrength.High)
                        {
                            HasHeavyHelmet = true;
                            break;
                        }
                    }
                }
            }
        }

        public bool HasEarPiece { get; private set; }
        public bool HasOptic { get; private set; }
        public bool HasHelmet { get; private set; }
        public bool HasHeavyHelmet { get; private set; }
        public bool HasArmor { get; private set; }
        public bool HasPrimary => PrimaryWeaponClass != WeaponClass.None;
        public bool HasSecondPrimary => SecondPrimaryWeaponClass != WeaponClass.None;
        public bool HasSideArm { get; private set; }
        public bool HasSuppressor { get; private set; }
        public WeaponClass PrimaryWeaponClass { get; private set; }
        public WeaponClass SecondPrimaryWeaponClass { get; private set; }
        public EquipmentClass Equipment => Inventory.Equipment;

        private float UpdateEquipmentTimer = 0f;
    }

    public enum WeaponClass
    {
        None,
        AssaultRifle,
        BattleRifle,
        SMG,
        PumpShotgun,
        AutoShotgun,
        MarksmanRifle,
        SniperRifle,
        Pistol,
    }
}
