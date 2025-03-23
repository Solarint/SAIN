using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.Info;
using SPT.Reflection.Patching;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SAIN.Components.PlayerComponentSpace.Classes.Equipment
{
    public class SAINEquipmentClass : PlayerComponentBase
    {
        public SAINEquipmentClass(PlayerComponent playerComponent) : base(playerComponent)
        {
            EquipmentClass = playerComponent.Player.Equipment;
            GearInfo = new GearInfo(this);
        }

        public void Init()
        {
            getAllWeapons();
            updateAllWeapons();
            ReCalcPowerOfEquipment();
        }

        public void Dispose()
        {
            foreach (var weapon in WeaponInfos.Values) {
                weapon?.Dispose();
            }
            WeaponInfos.Clear();
        }

        public InventoryEquipment EquipmentClass { get; private set; }

        private void ReCalcPowerOfEquipment()
        {
            float oldPower = Player.AIData.PowerOfEquipment;
            if (SAINPlugin.LoadedPreset.GlobalSettings.PowerCalc.CalcPower(PlayerComponent, out float power) &&
                oldPower != power) {
                OnPowerRecalced?.Invoke(power);
            }
        }

        public Action<float> OnPowerRecalced { get; set; }

        public bool PlayAIShootSound()
        {
            var weapon = CurrentWeapon;
            if (weapon == null) {
                //Logger.LogWarning("CurrentWeapon Null");
                return false;
            }

            if (_nextPlaySoundTime < Time.time) {
                _nextPlaySoundTime = Time.time + (PlayerComponent.IsAI ? 0.5f : 0.1f);
                SAINBotController.Instance?.BotHearing.PlayAISound(PlayerComponent, weapon.SoundType, PlayerComponent.Transform.WeaponFirePort, weapon.CalculatedAudibleRange, 1f, false);
            }
            return true;
        }

        public bool BulletFired(EftBulletClass bulletClass)
        {
            if (bulletClass == null) {
                return false;
            }
            if (_nextPlaySoundTime < Time.time) {
                _nextPlaySoundTime = Time.time + (PlayerComponent.IsAI ? 0.5f : 0.1f);
                var weapon = CurrentWeapon;
                if (weapon == null) {
                    return false;
                }
                Logger.LogInfo("Bullet Fired");
                FiredBulletContainer bulletContainer = new FiredBulletContainer(bulletClass, weapon);
                if (PlayerComponent.IsAI) {
                    SAINBotController.Instance.BotHearing.PlayAISound(PlayerComponent, weapon.SoundType, PlayerComponent.Transform.WeaponFirePort, weapon.CalculatedAudibleRange, 1f, false);
                }
                else {
                    PlayerComponent.StartCoroutine(bulletFiredCoroutine(bulletContainer));
                }
            }
            return false;
        }

        private IEnumerator bulletFiredCoroutine(FiredBulletContainer bulletContainer)
        {
            float expireTime = Time.time + bulletFiredExpireTime;
            while (bulletContainer.Bullet.BulletState == EftBulletClass.EBulletState.Flying) {
                if (expireTime < Time.time) {
                    yield break;
                }
                yield return null;
            }
            yield return null;
        }

        private void findRelevantBots(List<BotComponent> botList, FiredBulletContainer bulletContainer)
        {
            var botcontroller = SAINBotController.Instance;
            if (botcontroller == null) {
                return;
            }
            var bots = botcontroller.Bots;
            if (bots == null) {
                return;
            }
            Vector3 bulletTravelDir = bulletContainer.Bullet.CurrentDirection;
            foreach (var bot in bots.Values) {
            }
        }

        public class FiredBulletContainer
        {
            public FiredBulletContainer(EftBulletClass bulletClass, WeaponInfo weaponInfo)
            {
                Bullet = bulletClass;
                WeaponInfo = weaponInfo;
            }

            public readonly EftBulletClass Bullet;
            public readonly WeaponInfo WeaponInfo;
            public readonly List<BotBulletData> ActiveBots = new List<BotBulletData>();
        }

        public class BotBulletData
        {
            public BotComponent Bot;
            public bool Active = true;
            public bool Reacted = false;
            public float DotProduct = -1;
        }

        private float bulletFiredExpireTime = 10f;
        private float _nextPlaySoundTime;

        public void Update()
        {
            CurrentWeapon = getCurrentWeapon();
            GearInfo.Update();
            updateAllWeapons();
        }

        private void getAllWeapons()
        {
            foreach (EquipmentSlot slot in _weaponSlots) {
                addWeaponFromSlot(slot);
            }
        }

        private void addWeaponFromSlot(EquipmentSlot slot)
        {
            Item item = EquipmentClass.GetSlot(slot).ContainedItem;
            if (item != null && item is Weapon weapon) {
                if (!WeaponInfos.ContainsKey(slot)) {
                    WeaponInfos.Add(slot, new WeaponInfo(weapon));
                }
                else if (WeaponInfos.TryGetValue(slot, out WeaponInfo info) &&
                    info.Weapon != weapon) {
                    info.Dispose();
                    WeaponInfos[slot] = new WeaponInfo(weapon);
                }
            }
        }

        private void updateAllWeapons()
        {
            if (_nextUpdateWeapTime < Time.time) {
                _nextUpdateWeapTime = Time.time + 1f;

                foreach (var info in WeaponInfos.Values) {
                    if (info?.Update() == true)
                        return;
                }
            }
        }

        private float _nextUpdateWeapTime;

        private static readonly EquipmentSlot[] _weaponSlots = new EquipmentSlot[]
        {
            EquipmentSlot.FirstPrimaryWeapon,
            EquipmentSlot.SecondPrimaryWeapon,
            EquipmentSlot.Holster,
        };

        public GearInfo GearInfo { get; private set; }

        public WeaponInfo CurrentWeapon { get; private set; }

        public WeaponInfo WeaponInInventory => PrimaryWeapon ?? SecondaryWeapon ?? HolsterWeapon;

        private WeaponInfo getCurrentWeapon()
        {
            Item item = Player.HandsController.Item;
            if (item != null) {
                _currentWeapon = getInfoFromItem(item);
            }
            return _currentWeapon;
        }

        private WeaponInfo getInfoFromItem(Item item)
        {
            if (item is Weapon weapon) {
                if (_currentWeapon?.Weapon == weapon) {
                    return _currentWeapon;
                }
                var weaponInfo = getInfoFromWeapon(weapon);
                if (weaponInfo == null) {
                    getAllWeapons();
                    weaponInfo = getInfoFromWeapon(weapon);
                }
                return weaponInfo;
            }
            return null;
        }

        private WeaponInfo getInfoFromWeapon(Weapon weapon)
        {
            foreach (var weaponInfo in WeaponInfos.Values) {
                if (weapon == weaponInfo.Weapon) {
                    _currentWeapon = weaponInfo;
                    ReCalcPowerOfEquipment();
                    return weaponInfo;
                }
            }
            return null;
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

        private WeaponInfo _currentWeapon;

        public Dictionary<EquipmentSlot, WeaponInfo> WeaponInfos { get; } = new Dictionary<EquipmentSlot, WeaponInfo>();

        public class BulletCreatedPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(ClientFirearmController), "RegisterShot");
            }

            [PatchPostfix]
            public static void Patch(EftBulletClass shot)
            {
                if (shot == null) {
                    return;
                }
                var gameWorld = GameWorldComponent.Instance;
                if (gameWorld == null) {
                    return;
                }
                PlayerComponent bulletOwner = gameWorld.PlayerTracker.GetPlayerComponent(shot.PlayerProfileID);
                if (bulletOwner == null) {
                    return;
                }
                bulletOwner.Equipment.BulletFired(shot);
            }
        }
    }
}