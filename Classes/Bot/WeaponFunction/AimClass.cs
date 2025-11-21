using EFT;
using SAIN.Classes;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.WeaponFunction;

public class AimClass : BotComponentClassBase, IBotClass
{
    public AimClass(BotComponent sain) : base(sain)
    {
        TickRequirement = ESAINTickState.OnlyNoSleep;
    }

    public event Action<bool> OnAimAllowedOrBlocked;

    public bool CanAim { get; private set; }

    public float LastAimTime { get; set; }

    public AimStatus AimStatus {
        get
        {
            IBotAiming aim = BotOwner.AimingManager.CurrentAiming;
            if (aim is BotAimingClass aimClass)
            {
                return aimClass.AimStatus_0;
            }
            if (aim != null && aim.IsReady)
            {
                return AimStatus.AimComplete;
            }
            return AimStatus.NoTarget;
        }
        set
        {
            if (BotOwner?.AimingManager?.CurrentAiming is BotAimingClass aimClass)
            {
                aimClass.AimStatus_0 = value;
            }
        }
    }

    public override void ManualUpdate()
    {
        checkCanAim();
        CheckLoseTarget();
        base.ManualUpdate();
    }

    public bool AimAtTarget(Vector3 shootPoint, Enemy enemy, out bool AimComplete, IBotAiming currentAiming, BotComponent bot)
    {
        BotOwner botOwner = bot.BotOwner;
        BotWeaponManager weaponManager = botOwner.WeaponManager;
        if (!weaponManager.HaveBullets || weaponManager.Reload.Reloading)
        {
            botOwner.ShootData.EndShoot();
            AimComplete = false;
            Bot.Aim.LoseAimTarget();
            return false;
        }

        Vector3 firePort = bot.Transform.WeaponData.FirePort;
        Vector3 ballisticOffset = PlayerMovementController.Util.CalculateBallisticOffset(firePort, shootPoint, enemy.EnemyPlayer.Velocity, bot.PlayerComponent.Equipment.CurrentWeaponInfo.BulletSpeed);
        Vector3 aimPoint = shootPoint + ballisticOffset;

        var smoother = enemy.PositionSmoother;

        // If we weren't previously aiming, reset smoothing.
        if (AimStatus == AimStatus.NoTarget || enemy != _lastAimEnemy)
        {
            smoother.Snap(aimPoint);
            _lastAimEnemy = enemy;
        }

        // Feed desired aim point to the smoother to account for enemy movement.
        smoother.Update(aimPoint, enemy.EnemyPlayer.Velocity, Time.fixedDeltaTime);

        // Input the final aim point to EFT's bot aim system.
        currentAiming.SetTarget(smoother.Position);

        if (!bot.FriendlyFire.UpdateFriendlyFireStatus(currentAiming.LastDist2Target, bot.Transform.WeaponData.FirePort, bot.Transform.WeaponData.PointDirection, bot))
        {
            botOwner.ShootData.EndShoot();
            AimComplete = false;
            Bot.Aim.LoseAimTarget();
            return false;
        }
        currentAiming.NodeUpdate();
        Bot.Steering.LookToPoint(currentAiming.EndTargetPoint);
        AimComplete = currentAiming.IsReady && (botOwner.ShootData.Shooting || Bot.Steering.IsLookingAtPoint(currentAiming.EndTargetPoint, out float dot, 0.85f));
        return true;
    }

    private Enemy _lastAimEnemy;

    private void checkCanAim()
    {
        bool couldAim = CanAim;
        CanAim = canAim();
        if (couldAim != CanAim)
        {
            OnAimAllowedOrBlocked?.Invoke(CanAim);
        }
    }

    private bool canAim()
    {
        if (Player.IsSprintEnabled)
        {
            return false;
        }
        if (BotOwner.WeaponManager.Reload.Reloading)
        {
            //return false;
        }
        if (!Bot.HasEnemy)
        {
            //return false;
        }
        return true;
    }

    public void LoseAimTarget()
    {
        if (BotOwner.AimingManager.CurrentAiming is BotAimingClass aimClass &&
            aimClass.AimStatus_0 != AimStatus.NoTarget)
        {
            aimClass.AimStatus_0 = AimStatus.NoTarget;
        }
    }

    private void CheckLoseTarget()
    {
        var weaponManager = BotOwner.WeaponManager;
        if (!CanAim || !weaponManager.HaveBullets || weaponManager.Reload.Reloading)
        {
            LoseAimTarget();
            BotOwner.ShootData?.EndShoot();
        }
    }
}