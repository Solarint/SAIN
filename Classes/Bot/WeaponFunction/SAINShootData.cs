using EFT;
using EFT.InventoryLogic;
using SAIN.Components;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.Info;
using UnityEngine;

namespace SAIN.SAINComponent.Classes;

public class SAINShootData : BotComponentClassBase
{
    public Enemy LastShotEnemy { get; private set; }

    public SAINShootData(BotComponent bot) : base(bot)
    {
        TickRequirement = ESAINTickState.OnlyBotInCombat;
    }

    public override void Init()
    {
        Bot.EnemyController.Events.OnEnemyRemoved += CheckClearEnemy;
        base.Init();
    }

    public override void ManualUpdate()
    {
        CheckEndShoot();
        base.ManualUpdate();
    }

    public override void Dispose()
    {
        Bot.EnemyController.Events.OnEnemyRemoved -= CheckClearEnemy;
        base.Dispose();
    }

    private void CheckEndShoot()
    {
        if (!_shooting) return;
        BotWeaponManager weaponManager = BotOwner.WeaponManager;
        if (weaponManager == null || !weaponManager.HaveBullets || weaponManager.Reload.Reloading)
        {
            EndShoot();
            return;
        }
        if (LastShotEnemy?.EnemyPlayer?.HealthController?.IsAlive != true)
        {
            EndShoot();
            LastShotEnemy = null;
            return;
        }
        if (!BotOwner.ShootData.Shooting)
        {
            EndShoot();
        }
    }

    private void CheckClearEnemy(string profileId, Enemy enemy)
    {
        if (LastShotEnemy == enemy)
        {
            LastShotEnemy = null;
            if (_shooting)
            {
                EndShoot();
            }
        }
    }

    public void EndShoot()
    {
        _shooting = false;
        BotOwner.ShootData?.EndShoot();
    }

    public Enemy GetEnemyToShoot(Enemy priorityEnemy = null)
    {
        if (AimAndShootAtEnemy(priorityEnemy, Bot))
        {
            UpdateADS(priorityEnemy);
            return priorityEnemy;
        }
        Enemy targetEnemy = CheckEnemiesForShootableTargets(Bot.EnemyController.VisibleEnemies);
        if (targetEnemy != null)
        {
            UpdateADS(targetEnemy);
            return targetEnemy;
        }
        UpdateADS(priorityEnemy);
        Bot.Aim.LoseAimTarget();
        return null;
    }

    public bool ShootAnyVisibleEnemies(Enemy priorityEnemy = null)
    {
        if (Bot.Decision.CurrentSelfDecision == ESelfActionType.Reload)
        {
            return false;
        }
        if (Bot.Mover.Running && 
            (Bot.Mover.ActivePath.CurrentSprintStatus == Mover.EBotSprintStatus.Running || 
            Bot.Mover.ActivePath.CurrentSprintStatus == Mover.EBotSprintStatus.Turning))
        {
            return false;
        }
        return GetEnemyToShoot(priorityEnemy) != null;
    }

    private void UpdateADS(Enemy enemy)
    {
        Bot.AimDownSightsController.UpdateADSstatus(enemy);
    }

    public Enemy CheckEnemiesForShootableTargets(EnemyList VisibleEnemies)
    {
        foreach (Enemy Enemy in VisibleEnemies)
            if (Enemy.IsVisible && Time.time - Enemy.Vision.LastChangeVisionTime > 0.33f && AimAndShootAtEnemy(Enemy, Bot))
                return Enemy;
        return null;
    }

    private bool AimAndShootAtEnemy(Enemy Enemy, BotComponent bot)
    {
        if (Enemy == null)
            return false;

        if (Enemy.Player?.HealthController?.IsAlive == false)
            return false;

        var weaponManager = bot.BotOwner.WeaponManager;
        if (weaponManager == null)
            return false;

        bool reloading = weaponManager.Reload.Reloading;
        if (reloading || !weaponManager.HaveBullets)
        {
            if (!reloading && weaponManager.Selector.EquipmentSlot == EquipmentSlot.Holster && !weaponManager.Selector.TryChangeToMain())
                SelectWeapon(Enemy);

            return false;
        }

        if (!bot.Aim.CanAim)
            return false;

        Vector3? target = GetAimTarget(Enemy, bot);
        if (target != null &&
            Enemy != null)
        {
            bot.BotLight.HandleLightForEnemy(Enemy);

            if (bot.Aim.AimAtTarget(target.Value, Enemy, out bool AimComplete, bot.BotOwner.AimingManager.CurrentAiming, bot))
            {
                ShootWhenAimComplete(Enemy, bot, AimComplete);
                return true;
            }
        }
        return false;
    }

    private void ShootWhenAimComplete(Enemy Enemy, BotComponent bot, bool AimComplete)
    {
        if (AimComplete)
        {
            var shootData = bot.BotOwner.ShootData;
            if (!shootData.Shooting)
            {
                LastShotEnemy = Enemy;
                _shooting = true;
                bot.BotOwner.ShootData.Shoot();
                Enemy.EnemyInfo?.SetLastShootTime();
            }
        }
    }

    private void SelectWeapon(Enemy Enemy)
    {
        FindOptimalWeaponForDistance(Enemy.RealDistance);
        if (CurrentSlot != optimalSlot)
        {
            TryChangeWeapon(optimalSlot);
        }
    }

    private EquipmentSlot CurrentSlot => BotOwner.WeaponManager.Selector.EquipmentSlot;

    private void TryChangeWeapon(EquipmentSlot slot)
    {
        if (_nextChangeWeaponTime < Time.time)
        {
            var selector = BotOwner?.WeaponManager?.Selector;
            if (selector != null)
            {
                _nextChangeWeaponTime = Time.time + 1f;
                switch (slot)
                {
                    case EquipmentSlot.FirstPrimaryWeapon:
                        selector.TryChangeToMain();
                        break;

                    case EquipmentSlot.SecondPrimaryWeapon:
                        selector.ChangeToSecond();
                        break;

                    case EquipmentSlot.Holster:
                        selector.TryChangeWeapon(true);
                        break;

                    default:
                        break;
                }
            }
        }
    }

    private void FindOptimalWeaponForDistance(float distance)
    {
        if (_nextCheckOptimalTime < Time.time)
        {
            _nextCheckOptimalTime = Time.time + 0.5f;

            var equipment = Bot.PlayerComponent.Equipment;

            float? primaryEngageDist = null;
            var primary = equipment.PrimaryWeapon;
            if (IsWeaponDurableEnough(primary))
            {
                primaryEngageDist = primary.EngagementDistance;
            }

            float? secondaryEngageDist = null;
            var secondary = equipment.SecondaryWeapon;
            if (IsWeaponDurableEnough(secondary))
            {
                secondaryEngageDist = secondary.EngagementDistance;
            }

            float? holsterEngageDist = null;
            var holster = equipment.HolsterWeapon;
            if (IsWeaponDurableEnough(holster))
            {
                holsterEngageDist = holster.EngagementDistance;
            }

            float minDifference = Mathf.Abs(distance - primaryEngageDist ?? 0);
            optimalSlot = EquipmentSlot.FirstPrimaryWeapon;

            float difference = Mathf.Abs(distance - secondaryEngageDist ?? 0);
            if (difference < minDifference)
            {
                minDifference = difference;
                optimalSlot = EquipmentSlot.SecondPrimaryWeapon;
            }

            if (!BotOwner.WeaponManager.HaveBullets)
            {
                difference = Mathf.Abs(distance - holsterEngageDist ?? 0);
                if (difference < minDifference)
                {
                    minDifference = difference;
                    optimalSlot = EquipmentSlot.Holster;
                }
            }
        }
    }

    private static bool IsWeaponDurableEnough(WeaponInfo info, float min = 0.5f) => info != null && info.Durability > min && info.Weapon.ChamberAmmoCount > 0;

    private static Vector3? GetAimTarget(Enemy enemy, BotComponent bot)
    {
        if (enemy != null &&
            enemy.IsVisible &&
            enemy.CanShoot)
        {
            //Vector3? test = enemy.Shoot.Targets.GetPointToShoot();
            //if (test == null) {
            //    Logger.LogWarning($"cant get point to shoot with new system! oh no!");
            //}

            Vector3? centerMass = FindCenterMassPoint(enemy, bot);
            Vector3? partToShoot = GetEnemyPartToShoot(enemy.EnemyInfo);
            Vector3? modifiedTarget = CheckYValue(centerMass, partToShoot);
            Vector3? finalTarget = modifiedTarget ?? partToShoot ?? centerMass;

            return finalTarget;
        }
        return null;
    }

    private static Vector3? CheckYValue(Vector3? centerMass, Vector3? partTarget)
    {
        if (centerMass != null &&
            partTarget != null &&
            centerMass.Value.y < partTarget.Value.y)
        {
            Vector3 newTarget = partTarget.Value;
            newTarget.y = centerMass.Value.y;
            return new Vector3?(newTarget);
        }
        return null;
    }

    private static Vector3? FindCenterMassPoint(Enemy enemy, BotComponent bot)
    {
        if (enemy.IsAI)
        {
            return null;
        }
        if (SAINPlugin.LoadedPreset.GlobalSettings.Aiming.AimCenterMassGlobal)
        {
            return enemy.CenterMass;
        }
        if (bot.Info.FileSettings.Aiming.AimForHead || !bot.Info.FileSettings.Aiming.AimCenterMass)
        {
            return null;
        }
        return enemy.CenterMass;
    }

    private static Vector3? GetEnemyPartToShoot(EnemyInfo enemy)
    {
        if (enemy != null)
        {
            Vector3 value;
            if (enemy.Distance < 6f)
            {
                value = enemy.GetBodyPartPosition();
            }
            else
            {
                value = enemy.GetPartToShoot();
            }
            return new Vector3?(value);
        }
        return null;
    }

    private bool _shooting;
    private EquipmentSlot optimalSlot;
    private float _nextCheckOptimalTime;
    private float _nextChangeWeaponTime;
}