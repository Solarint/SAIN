using EFT;
using EFT.InventoryLogic;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.Info;
using System;
using System.Collections.Generic;

namespace SAIN.Components.PlayerComponentSpace.Classes.Equipment;

public class SAINEquipmentClass : PlayerComponentBase
{
    public SAINEquipmentClass(PlayerComponent playerComponent) : base(playerComponent)
    {
        EquipmentClass = playerComponent.Player.Equipment;
        GearInfo = new GearInfo(this);
    }

    public InventoryEquipment EquipmentClass { get; private set; }
    public Action<float> OnPowerRecalced { get; set; }

    public void Init()
    {
        getAllWeapons();
        PlayerComponent.OnWeaponEquipped += OnWeaponEquiped;
    }

    public override void Dispose()
    {
        foreach (var WeaponInfo in WeaponInfos)
        {
            WeaponInfo.Value?.Dispose();
        }
        WeaponInfos.Clear();
        PlayerComponent.OnWeaponEquipped -= OnWeaponEquiped;
    }

    public void WeaponModified(Weapon Weapon)
    {
        foreach (var WeaponInfo in WeaponInfos)
        {
            if (WeaponInfo.Value.Weapon == Weapon)
            {
                WeaponInfo.Value.WeaponModified(Player);
                return;
            }
        }
        getAllWeapons();
        foreach (var WeaponInfo in WeaponInfos)
        {
            if (WeaponInfo.Value.Weapon == Weapon)
            {
                WeaponInfo.Value.WeaponModified(Player);
                return;
            }
        }
    }

    protected void OnWeaponEquiped(Weapon weapon, Weapon lastWeapon)
    {
        if (weapon == null)
        {
            return;
        }
        GetCurrentWeaponInfo(weapon);
        if (CurrentWeaponInfo == null)
        {
            getAllWeapons();
            GetCurrentWeaponInfo(weapon);
        }
    }

    private void ReCalcPowerOfEquipment()
    {
        if (SAINPlugin.LoadedPreset.GlobalSettings.PowerCalc.CalcPower(PlayerComponent, out float power))
        {
            OnPowerRecalced?.Invoke(power);
        }
    }

    public void Update()
    {
        //updateAllWeapons();
        if (CurrentWeaponInfo == null && Player.HandsController?.Item is Weapon weapon)
        {
            foreach (var WeaponInfo in WeaponInfos.Values)
            {
                if (WeaponInfo.Weapon == weapon)
                {
                    SetCurrentWeaponInfo(WeaponInfo);
                }
            }
        }
    }

    private void SetCurrentWeaponInfo(WeaponInfo WeaponInfo)
    {
        WeaponInfo.WeaponEquiped(Player);
        CurrentWeaponInfo = WeaponInfo;
        ReCalcPowerOfEquipment();
    }

    private void getAllWeapons()
    {
        foreach (EquipmentSlot slot in _weaponSlots)
        {
            Item item = EquipmentClass.GetSlot(slot).ContainedItem;
            if (item is Weapon weapon)
            {
                if (!WeaponInfos.ContainsKey(slot))
                {
                    WeaponInfos.Add(slot, new WeaponInfo(weapon));
                }
                else if (WeaponInfos.TryGetValue(slot, out WeaponInfo info) &&
                    info.Weapon != weapon)
                {
                    info.Dispose();
                    WeaponInfos[slot] = new WeaponInfo(weapon);
                }
            }
        }
    }

    private static readonly EquipmentSlot[] _weaponSlots =
    [
        EquipmentSlot.FirstPrimaryWeapon,
        EquipmentSlot.SecondPrimaryWeapon,
        EquipmentSlot.Holster,
    ];

    public GearInfo GearInfo { get; private set; }

    public WeaponInfo CurrentWeaponInfo { get; private set; }

    public WeaponInfo WeaponInInventory => PrimaryWeapon ?? SecondaryWeapon ?? HolsterWeapon;

    private void GetCurrentWeaponInfo(Weapon weapon)
    {
        foreach (WeaponInfo Info in WeaponInfos.Values)
        {
            if (weapon == Info.Weapon)
            {
                SetCurrentWeaponInfo(Info);
                return;
            }
        }
        CurrentWeaponInfo = null;
    }

    public WeaponInfo GetWeaponInfo(EquipmentSlot slot)
    {
        if (WeaponInfos.TryGetValue(slot, out WeaponInfo weaponInfo))
            return weaponInfo;
        return null;
    }

    public WeaponInfo PrimaryWeapon => GetWeaponInfo(EquipmentSlot.FirstPrimaryWeapon);
    public WeaponInfo SecondaryWeapon => GetWeaponInfo(EquipmentSlot.SecondPrimaryWeapon);
    public WeaponInfo HolsterWeapon => GetWeaponInfo(EquipmentSlot.Holster);

    public Dictionary<EquipmentSlot, WeaponInfo> WeaponInfos { get; } = new Dictionary<EquipmentSlot, WeaponInfo>();
}