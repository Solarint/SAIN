using EFT;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.Models.Enums;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Mover;

public class BlindFireController : BotBase, IBotClass
{
    public BlindFireController(BotComponent sain) : base(sain)
    {

    }

    private bool CheckAllowBlindFire()
    {
        if (!Bot.SAINLayersActive ||
            !BotOwner.WeaponManager.IsReady ||
            !BotOwner.WeaponManager.HaveBullets ||
            Bot.Mover.Moving ||
            Bot.Cover.CoverInUse == null)
        {
            return false;
        }

        Enemy enemy = Bot.GoalEnemy;
        if (enemy == null ||
            !enemy.Seen ||
            enemy.TimeSinceSeen > 30f)
        {
            return false;
        }

        if (enemy.IsVisible && enemy.CanShoot)
        {
            if (_blindFire != 0) return true; // dont stop blindfiring suddenly
            return false;
        }

        if (GlobalSettingsClass.Instance.General.AILimit.LimitAIvsAIGlobal
            && enemy.IsAI
            && Bot.CurrentAILimit != AILimitSetting.None)
        {
            return false;
        }

        if (!Bot.ManualShoot.CanShoot())
        {
            return false;
        }

        return true;
    }

    public override void ManualUpdate()
    {
        base.ManualUpdate();
        if (_nextBlindFireCheck > Time.time)
        {
            if (_blindFire != 0)
            {
                if (!CheckAllowBlindFire())
                {
                    _blindFire = 0;
                }
                else
                {
                    TryShoot(Bot.GoalEnemy);
                }
            }
            return;
        }
        _nextBlindFireCheck = Time.time + 0.2f;

        if (!CheckAllowBlindFire())
        {
            ResetBlindFire();
            return;
        }

        Vector3? lastKnownPos = Bot.GoalEnemy.KnownPlaces.LastKnownPosition;
        if (lastKnownPos == null)
        {
            ResetBlindFire();
            _changeBlindFireTime = Time.time + 0.5f;
            return;
        }

        if (!Bot.Steering.FindLastKnownTarget(Bot.GoalEnemy, out Vector3 targetPos) || (lastKnownPos.Value - targetPos).sqrMagnitude > 10f)
        {
            ResetBlindFire();
            _changeBlindFireTime = Time.time + 0.5f;
            return;
        }

        int lastBlindFire = _blindFire;
        _blindFire = checkBlindFire(targetPos);

        if (_blindFire == 0)
        {
            ResetBlindFire();
            _changeBlindFireTime = Time.time + 0.5f;
            return;
        }

        bool noLastBlindFire = lastBlindFire == 0;
        if (noLastBlindFire || _changeBlindFireTime < Time.time)
        {
            _changeBlindFireTime = Time.time + 1f;
            SetBlindFire(_blindFire);
        }
        if (noLastBlindFire || _nextUpdateAimTargetTime < Time.time || BlindFireTargetPos == Vector3.zero)
        {
            _nextUpdateAimTargetTime = Time.time + 1.5f;
            Vector3 start = Bot.Position;
            Vector3 blindFireDirection = Vector.Rotate(targetPos - start, Vector.RandomRange(3), Vector.RandomRange(3), Vector.RandomRange(3));
            BlindFireTargetPos = blindFireDirection + start;
        }
        TryShoot(Bot.GoalEnemy);
    }

    private void TryShoot(Enemy enemy)
    {
        if (Bot.ManualShoot.TryShoot(enemy, BlindFireTargetPos, false, EShootReason.Blindfire))
        {
            _manualShooting = true;
        }
    }

    private float _nextUpdateAimTargetTime;
    private float _nextBlindFireCheck;
    private int _blindFire;

    private bool _manualShooting;

    public void ResetBlindFire()
    {
        if (ActiveBlindFireSetting != 0)
        {
            Player.MovementContext.SetBlindFire(0);
        }
        if (_manualShooting)
        {
            _manualShooting = false;
            Bot.ManualShoot.Reset();
        }
        _blindFire = 0;
        BlindFireTargetPos = Vector3.zero;
    }

    private Vector3 BlindFireTargetPos;

    public bool BlindFireActive => ActiveBlindFireSetting != 0;

    public int ActiveBlindFireSetting => Player.MovementContext.BlindFire;

    private float _changeBlindFireTime = 0f;


    private int checkBlindFire(Vector3 targetPos)
    {
        LayerMask mask = LayerMaskClass.HighPolyWithTerrainMask;
        Vector3 firePort = Bot.Transform.WeaponData.FirePort;
        Vector3 direction = targetPos - firePort;

        if (Physics.Raycast(firePort, direction, 5f, mask))
        {
            // Overhead blindfire
            firePort = Bot.Transform.HeadData.HeadPosition + Vector3.up * 0.15f;
            if (!Vector.Raycast(firePort, targetPos, mask))
            {
                return 1;
            }

            Quaternion rotation = Quaternion.Euler(0f, 90f, 0f);
            Vector3 SideShoot = rotation * direction.normalized * 0.2f;
            firePort += SideShoot;
            direction = targetPos - firePort;
            if (!Physics.Raycast(firePort, direction, direction.magnitude, mask))
            {
                return -1;
            }
        }
        return 0;
    }

    public void SetBlindFire(int value)
    {
        if (ActiveBlindFireSetting != value)
        {
            Player.MovementContext.SetBlindFire(value);
        }
    }
}