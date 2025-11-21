using SAIN.Helpers;
using SAIN.Models.Enums;
using SAIN.Models.Structs;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.Classes.EnemyClasses;
using UnityEngine;

namespace SAIN.Components.BotComponentSpace.Classes.EnemyClasses;

public class EnemyHearing(EnemyData enemyData) : EnemyBase(enemyData, enemyData.Enemy.Bot)
{
    public bool Heard { get; private set; }
    public bool EnemyHeardFromPeace { get; set; }
    public float TimeSinceHeard => Heard ? Time.time - _timeLastHeard : float.MaxValue;
    public BotSound LastSoundHeard { get; set; }
    public Vector3? LastHeardPosition { get; private set; }

    private const float REPORT_HEARD_FREQUENCY = 1f;

    public override void Init()
    {
        Enemy.Events.OnFirstSeen += resetHeardFromPeace;
        base.Init();
    }

    public void TickEnemy(float currentTime)
    {
        if (Enemy.Seen && EnemyHeardFromPeace)
        {
            EnemyHeardFromPeace = false;
        }
    }

    private void resetHeardFromPeace(Enemy enemy)
    {
        EnemyHeardFromPeace = false;
    }

    public override void Dispose()
    {
        Enemy.Events.OnFirstSeen -= resetHeardFromPeace;
        base.Dispose();
    }

    protected override void OnEnemyKnownChanged(bool known, Enemy enemy)
    {
        if (!known)
        {
            Heard = false;
            LastSoundHeard = null;
            LastHeardPosition = null;
            _timeLastHeard = 0f;
            EnemyHeardFromPeace = false;
        }
    }

    public EnemyPlace SetHeard(SAINHearingReport report, float currentTime)
    {
        if (Enemy.IsVisible)
        {
            report.position = Enemy.EnemyPosition;
        }

        Heard = true;
        bool wasGunfire = report.soundType.IsGunShot();
        Enemy.Status.HeardRecently = true;
        _timeLastHeard = Time.time;

        EnemyPlace place = UpdateHeardPosition(report, currentTime);

        if (!Bot.HasEnemy)
            EnemyHeardFromPeace = true;

        if (place != null)
            Enemy.Events.EnemyHeard(report.soundType, report.isDanger, place);

        if (wasGunfire || !report.shallReportToSquad)
        {
            return place;
        }
        updateEnemyAction(report.soundType, report.position, currentTime);
        return place;
    }

    private void updateEnemyAction(SAINSoundType soundType, Vector3 soundPosition, float currentTime)
    {
        EEnemyAction action;
        switch (soundType)
        {
            case SAINSoundType.GrenadeDraw:
            case SAINSoundType.GrenadePin:
                action = EEnemyAction.HasGrenade;
                break;

            case SAINSoundType.Reload:
            case SAINSoundType.DryFire:
                action = EEnemyAction.Reloading;
                break;

            case SAINSoundType.Looting:
                action = EEnemyAction.Looting;
                break;

            case SAINSoundType.Heal:
                action = EEnemyAction.Healing;
                break;

            case SAINSoundType.Surgery:
                action = EEnemyAction.UsingSurgery;
                break;

            default:
                action = EEnemyAction.None;
                break;
        }

        if (action != EEnemyAction.None)
        {
            Enemy.Status.SetVulnerableAction(action);
            Bot.Squad.SquadInfo.UpdateSharedEnemyStatus(EnemyPlayer, action, Bot, soundType, soundPosition, currentTime);
        }
    }

    public EnemyPlace UpdateHeardPosition(SAINHearingReport report, float currentTime)
    {
        EnemyPlace place = Enemy.KnownPlaces.UpdatePersonalHeardPosition(report, currentTime);
        if (report.shallReportToSquad &&
            place != null &&
            _nextReportHeardTime < currentTime)
        {
            _nextReportHeardTime = currentTime + REPORT_HEARD_FREQUENCY;
            Bot.Squad?.SquadInfo?.ReportEnemyPosition(Enemy, place, false, currentTime);
        }
        return place;
    }

    private float _nextReportHeardTime;
    private float _timeLastHeard;
}