using EFT;
using SAIN.Components;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Decision;

public class SAINSelfActionClass : BotComponentClassBase
{
    public SAINSelfActionClass(BotComponent sain) : base(sain)
    {
        TickRequirement = ESAINTickState.OnlyNoSleep;
    }

    private float _handsBusyTimer;
    private float _nextCheckTime;

    // TODO: this should be getting called from the main layers and not in a random manual update. Similar to steer by priority
    public override void ManualUpdate()
    {
        base.ManualUpdate();
        if (!Bot.SAINLayersActive)
        {
            return;
        }
        if (_nextCheckTime > Time.time)
        {
            return;
        }
        if (_nextHealTime > Time.time)
        {
            return;
        }
        var decision = Bot.Decision.CurrentSelfDecision;
        if (decision == ESelfActionType.None)
        {
            _nextCheckTime = Time.time + 1f;
            return;
        }
        if (decision == ESelfActionType.Reload)
        {
            _nextCheckTime = Time.time + 1f;
            return;
        }
        if (UsingMeds)
        {
            _nextCheckTime = Time.time + 1f;
            return;
        }
        if (BotOwner.WeaponManager.Reload.Reloading)
        {
            _nextCheckTime = Time.time + 1f;
            return;
        }
        if (Bot.Medical.TimeSinceShot < 0.5f)
        {
            return;
        }
        _nextCheckTime = Time.time + 0.2f;


        if (_handsBusyTimer > Time.time)
        {
            return;
        }
        if (Player.HandsController.IsInInteractionStrictCheck())
        {
            _handsBusyTimer = Time.time + 0.5f;
            return;
        }


        bool didAction = false;
        switch (decision)
        {
            case ESelfActionType.FirstAid:
                didAction = DoFirstAid();
                break;
            case ESelfActionType.Surgery:
                didAction = true; // surgery calls are handled by the brain layer action
                break;
            case ESelfActionType.Stims:
                didAction = DoStims();
                break;

            default:
                break;
        }

        if (didAction)
        {
            _nextHealTime = Time.time + 5f;
        }
    }

    private bool UsingMeds => BotOwner.Medecine?.Using == true;

    public bool DoFirstAid()
    {
        var heal = BotOwner.Medecine?.FirstAid;
        if (heal == null || BotOwner.IsDead)
        {
            return false;
        }
        if (_firstAidTimer < Time.time &&
            heal.ShallStartUse())
        {
            _firstAidTimer = Time.time + 5f;
            heal.TryApplyToCurrentPart();
            return true;
        }
        return false;
    }

    private float _firstAidTimer;

    public bool DoSurgery()
    {
        var surgery = BotOwner.Medecine?.SurgicalKit;
        if (surgery == null || BotOwner.IsDead)
        {
            return false;
        }
        if (_trySurgeryTime < Time.time &&
            surgery.ShallStartUse())
        {
            _trySurgeryTime = Time.time + 5f;
            surgery.ApplyToCurrentPart();
            return true;
        }
        return false;
    }

    private float _trySurgeryTime;

    public bool DoStims()
    {
        var stims = BotOwner.Medecine?.Stimulators;
        if (stims == null || BotOwner.IsDead)
        {
            return false;
        }
        if (_stimTimer < Time.time &&
            stims.CanUseNow())
        {
            _stimTimer = Time.time + 3f;
            try { stims.TryApply(); }
            catch { }
            return true;
        }
        return false;
    }

    private float _stimTimer;

    private bool HaveStimsToHelp()
    {
        return false;
    }

    public void BotCancelReload()
    {
        if (BotOwner.WeaponManager.Reload.Reloading)
        {
            //BotOwner.WeaponManager.Reload.TryStopReload();
        }
    }

    private float _nextHealTime = 0f;
}