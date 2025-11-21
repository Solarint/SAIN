using Comfort.Common;
using EFT;
using SAIN.Components;
using SAIN.Helpers.Events;
using SAIN.Layers;
using SAIN.Models.Enums;
using UnityEngine;

namespace SAIN.SAINComponent.Classes;

public class SAINActivationClass(BotComponent botComponent) : BotComponentClassBase(botComponent)
{
    private const float _activate_standby_human = 150;
    private const float _activate_standby_ai = 50;
    private const float _activate_standby_check_freq = 3f;

    public ESAINLayer ActiveLayer { get; private set; }
    public IBotAction CurrentAction { get; private set; }

    public void SetCurrentAction(IBotAction action)
    {
        CurrentAction = action;
    }

    public bool BotActive => BotActiveToggle.Value;
    public ToggleEvent BotActiveToggle { get; } = new ToggleEvent();
    public bool BotInStandBy => BotStandByToggle.Value;
    public ToggleEvent BotStandByToggle { get; } = new ToggleEvent();
    public bool GameEnding => GameEndingToggle.Value;
    public ToggleEvent GameEndingToggle { get; } = new ToggleEvent();
    public bool SAINLayersActive => SAINLayersActiveToggle.Value;
    public ToggleEvent SAINLayersActiveToggle { get; } = new ToggleEvent();
    public bool BotInCombat => BotInCombatToggle.Value;
    public ToggleEvent BotInCombatToggle { get; } = new ToggleEvent();

    public void SetActive(bool botActive)
    {
        float time = Time.time;
        BotActiveToggle.CheckToggle(botActive, time);

        if (!botActive)
        {
            BotStandByToggle.CheckToggle(true, time);
            ActiveLayer = ESAINLayer.None;
            SAINLayersActiveToggle.CheckToggle(false, time);
            Bot.Mover.Stop();
            Bot.AimDownSightsController.SetADS(false, true);
            CurrentAction = null;
        }
    }

    public void SetInCombat(bool inCombat)
    {
        BotInCombatToggle.CheckToggle(inCombat, Time.time);
    }

    public void SetActiveLayer(ESAINLayer layer)
    {
        ActiveLayer = layer;
    }

    public override void ManualUpdate()
    {
        CheckGameEnding();
        CheckBotActive();
        CheckStandBy();

        bool wasActive = SAINLayersActiveToggle.Value;
        SAINLayersActiveToggle.CheckToggle(ActiveLayer != ESAINLayer.None, Time.time);
        bool activeNow = SAINLayersActiveToggle.Value;
        if (wasActive && !activeNow)
        {
            Bot.Mover.Stop();
            Bot.AimDownSightsController.SetADS(false, true);
            CurrentAction = null;
        }
    }

    private void CheckBotActive()
    {
        if (GameEnding)
        {
            SetActive(false);
            return;
        }

        if (!Bot.PlayerComponent.IsActive)
        {
            SetActive(false);
            return;
        }

        if (BotOwner != null && BotOwner.BotState == EBotState.Active)
        {
            SetActive(true);
        }
    }

    private void CheckStandBy()
    {
        bool standby = BotOwner?.StandBy?.StandByType != BotStandByType.active;
        if (standby)
        {
            //if (Bot.HasEnemy)
            //{
            //    //Logger.LogWarning($"Had to activate bot manually because they were in stand by.");
            //    BotOwner.StandBy.Activate();
            //    standby = false;
            //}
            //else if (CheckAllEnemies())
            //{
#if DEBUG       //
            //    Logger.LogDebug($"[{BotOwner.name}] disabled standby due to enemies being near.");
#endif          //
            //    BotOwner.StandBy.Activate();
            //    standby = false;
            //}
        }

        if (standby)
        {
            Bot.Mover.Stop();
            CurrentAction = null;
        }

        BotStandByToggle.CheckToggle(standby, Time.time);
    }

    private bool CheckAllEnemies()
    {
        if (_nextCheckEnemiesTime > Time.time)
        {
            return false;
        }
        _nextCheckEnemiesTime = Time.time + _activate_standby_check_freq;

        var enemies = Bot.EnemyController.Enemies.Values;
        foreach (var enemy in enemies)
            if (enemy != null &&
                (enemy.InLineOfSight ||
                (enemy.IsAI && enemy.RealDistance < _activate_standby_ai) ||
                (!enemy.IsAI && enemy.RealDistance < _activate_standby_human)))
                return true;

        return false;
    }

    private void CheckGameEnding()
    {
        var botGame = Singleton<IBotGame>.Instance;
        bool gameEnding = botGame == null || botGame.Status == GameStatus.Stopping;
        GameEndingToggle.CheckToggle(gameEnding, Time.time);
    }

    public override void Init()
    {
        SetActive(true);
        base.Init();
    }

    public override void Dispose()
    {
        SetActive(false);
        base.Dispose();
    }

    private float _nextCheckEnemiesTime;
}