using EFT;
using SAIN.Components;
using SAIN.Models.Enums;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.WeaponFunction;

public class ManualShootClass : BotComponentClassBase
{
    public ManualShootClass(BotComponent bot) : base(bot)
    {
        TickRequirement = ESAINTickState.OnlyNoSleep;
    }

    public override void Init()
    {
        Bot.EnemyController.Events.OnEnemyRemoved += CheckClearEnemy;
        base.Init();
    }

    public override void ManualUpdate()
    {
        CheckReset();
        base.ManualUpdate();
    }

    public override void Dispose()
    {
        Bot.EnemyController.Events.OnEnemyRemoved -= CheckClearEnemy;
        base.Dispose();
    }

    private Enemy ManualShootEnemy;

    private void CheckClearEnemy(string ID, Enemy Enemy)
    {
        if (Enemy == ManualShootEnemy)
        {
            Reset();
        }
    }

    public void Reset()
    {
        BotOwner.ShootData.EndShoot();
        Reason = EShootReason.None;
        ShootPosition = Vector3.zero;
        ManualShootEnemy = null;
    }

    private void CheckReset()
    {
        if (Reason != EShootReason.None && (ManualShootEnemy?.EnemyPlayer?.HealthController?.IsAlive != true || !BotOwner.WeaponManager.HaveBullets || _timeStartManualShoot + 2f < Time.time))
        {
            Reset();
        }
    }

    public bool TryShoot(Enemy Enemy, Vector3 targetPos, bool checkFF = true, EShootReason reason = EShootReason.None)
    {
        if (Enemy != null &&
            CanShoot(checkFF) &&
            Bot.Steering.AngleToPointFromLookDir(targetPos) <= 10 &&
            Bot.FriendlyFire.UpdateFriendlyFireStatus(targetPos, Bot.Transform.WeaponData.FirePort, Bot.Transform.WeaponData.PointDirection, Bot))
        {
            if (!Shooting)
            {
                if (BotOwner.ShootData.Shoot())
                {
                    _timeStartManualShoot = Time.time;
                }
                else
                {
                    return false;
                }
            }

            ManualShootEnemy = Enemy;
            ShootPosition = targetPos;
            Reason = reason;
            return true;
        }
        Reset();
        return false;
    }

    public bool Shooting => BotOwner.ShootData.Shooting;

    public bool CanShoot(bool checkFF = true)
    {
        if (checkFF && !Bot.FriendlyFire.ClearShot)
        {
            //BotOwner.ShootData.EndShoot();
            //return false;
        }
        BotWeaponManager weaponManager = BotOwner.WeaponManager;
        if (weaponManager.IsMelee)
        {
            return false;
        }
        if (!weaponManager.IsWeaponReady)
        {
            return false;
        }
        if (weaponManager.Reload.Reloading)
        {
            return false;
        }
        if (!BotOwner.ShootData.CanShootByState)
        {
            return false;
        }
        if (!weaponManager.HaveBullets)
        {
            return false;
        }
        return true;
    }

    private float _timeStartManualShoot;

    public Vector3 ShootPosition { get; set; }

    public EShootReason Reason { get; private set; }
}