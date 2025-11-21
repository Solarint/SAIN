using SAIN.Preset.GlobalSettings;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public class EnemyVisionClass(EnemyData enemyData) : EnemyBase(enemyData, enemyData.Enemy.Bot)
{
    private const float _repeatContactMinSeenTime = 12f;
    private const float _lostContactMinSeenTime = 12f;

    public float EnemyVelocity => EnemyTransform.VelocityData.VelocityMagnitudeNormal;
    public bool FirstContactOccured { get; private set; }
    public bool ShallReportRepeatContact { get; set; }
    public bool ShallReportLostVisual { get; set; }
    public bool IsVisible { get; private set; }
    public bool InLineOfSight => EnemyParts.LineOfSight;
    public bool CanShoot => EnemyParts.CanShoot;
    public float VisibleStartTime { get; private set; }
    public float TimeSinceSeen => Seen ? Time.time - TimeLastSeen : -1;
    public bool Seen { get; private set; }
    public float TimeFirstSeen { get; private set; }
    public float TimeLastSeen { get; private set; }
    public float LastChangeVisionTime { get; private set; }
    public float LastGainSightResult { get; set; } = 1f;

    public EnemyAnglesClass Angles { get; } = new();
    public EnemyPartsClass EnemyParts { get; } = new EnemyPartsClass(enemyData.EnemyPlayerComponent);

    public static float AIVisionRangeLimit(Enemy enemy)
    {
        return CheckMaxVisionRangeAI(enemy);
    }

    private static float CheckMaxVisionRangeAI(Enemy enemy)
    {
        if (!enemy.IsAI)
        {
            return float.MaxValue;
        }
        var aiLimit = GlobalSettingsClass.Instance.General.AILimit;
        if (!aiLimit.LimitAIvsAIGlobal)
        {
            return float.MaxValue;
        }
        if (!aiLimit.LimitAIvsAIVision)
        {
            return float.MaxValue;
        }
        if (enemy.IsCurrentEnemy)
        {
            return float.MaxValue;
        }
        var myBot = enemy.Bot;
        return GetMaxVisionRange(myBot.CurrentAILimit);
    }

    private static float GetMaxVisionRange(AILimitSetting aiLimit)
    {
        return aiLimit switch {
            AILimitSetting.Far => GlobalSettingsClass.Instance.General.AILimit.MaxVisionRanges[AILimitSetting.Far],
            AILimitSetting.VeryFar => GlobalSettingsClass.Instance.General.AILimit.MaxVisionRanges[AILimitSetting.VeryFar],
            AILimitSetting.Narnia => GlobalSettingsClass.Instance.General.AILimit.MaxVisionRanges[AILimitSetting.Narnia],
            _ => float.MaxValue,
        };
    }

    public void TickEnemy(float currentTime)
    {
        Angles.CalcAngles(Enemy, currentTime);
        EnemyParts.Update(currentTime);
        Enemy.Events.OnEnemyLineOfSightChanged.CheckToggle(InLineOfSight, currentTime);
        UpdateVisibleState(currentTime);
    }

    protected override void OnEnemyKnownChanged(bool known, Enemy enemy)
    {
        if (!known)
        {
            UpdateVisibleState(Time.time, true);
        }
    }

    public void UpdateVisibleState(float currentTime, bool forceOff = false)
    {
        bool wasVisible = IsVisible;
        if (forceOff)
        {
            IsVisible = false;
        }
        else if (!EnemyParts.CanBeSeen)
        {
            if (EnemyInfo.IsVisible) try { EnemyInfo.SetVisible(false); } catch { /* eft code */ }
            IsVisible = false;
        }
        else
        {
            IsVisible = EnemyInfo.IsVisible;
        }

        if (IsVisible)
        {
            BotOwner.Memory.SetLastTimeSeeEnemy();
            if (!wasVisible)
            {
                VisibleStartTime = currentTime;
                if (Seen && TimeSinceSeen >= _repeatContactMinSeenTime)
                {
                    ShallReportRepeatContact = true;
                }
            }
            if (!Seen)
            {
                FirstContactOccured = true;
                TimeFirstSeen = currentTime;
                Seen = true;
                Enemy.Events.EnemyFirstSeen();
            }

            TimeLastSeen = currentTime;
            Enemy.UpdateCurrentEnemyPos(EnemyTransform.Position, currentTime);
        }

        if (!IsVisible)
        {
            if (wasVisible)
                Enemy.UpdateLastSeenPosition(EnemyTransform.Position, currentTime);

            if (Seen &&
                TimeSinceSeen > _lostContactMinSeenTime &&
                _nextReportLostVisualTime < currentTime)
            {
                _nextReportLostVisualTime = currentTime + 20f;
                ShallReportLostVisual = true;
            }
            VisibleStartTime = -1f;
        }

        Enemy.Events.OnVisionChange.CheckToggle(IsVisible, currentTime);
        if (IsVisible != wasVisible)
        {
            LastChangeVisionTime = currentTime;
        }
    }

    public readonly EnemyVisionDistanceClass _visionDistance = new(enemyData);
    private float _nextReportLostVisualTime;
}