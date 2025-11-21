using SAIN.Components;
using SAIN.Preset.GlobalSettings;
using System;
using UnityEngine;

namespace SAIN.SAINComponent.Classes;

public class SAINAILimit : BotComponentClassBase
{
    public event Action<AILimitSetting> OnAILimitChanged;
    public AILimitSetting CurrentAILimit { get; private set; }
    public float ClosestPlayerDistanceSqr { get; private set; }

    public SAINAILimit(BotComponent sain) : base(sain)
    {
        TickRequirement = ESAINTickState.OnlyBotActive;
    }

    public override void ManualUpdate()
    {
        CheckAILimit();
        base.ManualUpdate();
    }

    private void CheckAILimit()
    {
        AILimitSetting lastLimit = CurrentAILimit;
        if (Bot.EnemyController.ActiveHumanEnemy)
        {
            CurrentAILimit = AILimitSetting.None;
            ClosestPlayerDistanceSqr = -1f;
        }
        else if (_checkDistanceTime < Time.time)
        {
            _checkDistanceTime = Time.time + GlobalSettings.General.AILimit.AILimitUpdateFrequency * UnityEngine.Random.Range(0.9f, 1.1f);
            var gameWorld = GameWorldComponent.Instance;
            if (gameWorld != null &&
                 GameWorldComponent.Instance.PlayerTracker.FindClosestHumanPlayer(out float closestPlayerDistance, PlayerComponent, out _) != null)
            {
                CurrentAILimit = CheckDistances(closestPlayerDistance);
                ClosestPlayerDistanceSqr = closestPlayerDistance;
            }
        }
        if (lastLimit != CurrentAILimit)
        {
            OnAILimitChanged?.Invoke(CurrentAILimit);
        }
    }

    private AILimitSetting CheckDistances(float closestPlayerDist)
    {
        var aiLimit = GlobalSettingsClass.Instance.General.AILimit;
        if (closestPlayerDist < aiLimit.AILimitRanges[AILimitSetting.Far])
        {
            return AILimitSetting.None;
        }
        if (closestPlayerDist < aiLimit.AILimitRanges[AILimitSetting.VeryFar])
        {
            return AILimitSetting.Far;
        }
        if (closestPlayerDist < aiLimit.AILimitRanges[AILimitSetting.Narnia])
        {
            return AILimitSetting.VeryFar;
        }
        return AILimitSetting.Narnia;
    }

    private float _checkDistanceTime;
}
