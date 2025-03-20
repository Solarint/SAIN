using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Preset.GlobalSettings;
using System;
using System.Collections.Generic;
using FloatFunc = GClass828<float>;

namespace SAIN.SAINComponent.Classes
{
    public class BotWeightManagement : BotBase, IBotClass
    {
        public BotWeightManagement(BotComponent sain) : base(sain)
        {
        }

        public void Init()
        {
            if (GlobalSettingsClass.Instance.General.BotWeightEffects)
            {
                getSlots();
				Traverse.Create(Person.Player.InventoryController.Inventory).Field<FloatFunc>("TotalWeight").Value = new FloatFunc(getBotTotalWeight);
				Person.Player.Physical.EncumberDisabled = false;
            }
        }

        private void getSlots()
        {
            _slots.Clear();
            foreach (var slot in _botEquipmentSlots)
            {
                _slots.Add(Player.Equipment.GetSlot(slot));
            }
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        private float getBotTotalWeight()
        {
            float result = InventoryEquipment.smethod_1(_slots);
			_slots.Clear();
            // Logger.LogWarning(result);
            return result;
        }

        private readonly List<Slot> _slots = new List<Slot>();

        public static readonly EquipmentSlot[] _botEquipmentSlots = new EquipmentSlot[]
        {
            EquipmentSlot.Backpack,
            EquipmentSlot.TacticalVest,
            EquipmentSlot.ArmorVest,
            EquipmentSlot.Eyewear,
            EquipmentSlot.FaceCover,
            EquipmentSlot.Headwear,
            EquipmentSlot.Earpiece,
            EquipmentSlot.FirstPrimaryWeapon,
            EquipmentSlot.SecondPrimaryWeapon,
            EquipmentSlot.Holster,
            EquipmentSlot.Pockets,
        };
    }
}
