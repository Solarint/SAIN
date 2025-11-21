using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.Components.BotController;
using SAIN.Models.Enums;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Text;

namespace SAIN.Layers;

public abstract class SAINLayer(BotOwner botOwner, int priority, string layerName, ESAINLayer eSAINLayer) : CustomLayer(botOwner, priority)
{
    public static string BuildLayerName(string name)
    {
        return $"SAIN : {name}";
    }

    protected virtual bool GetBotComponent()
    {
        if (Bot == null)
        {
            Bot = BotSpawnController.Instance.GetSAIN(BotOwner);
            if (Bot != null)
            {
                Bot.Decision.DecisionManager.OnDecisionMade += BotDecisionMade;
            }
        }
        return Bot != null;
    }

    public override bool IsCurrentActionEnding()
    {
        if (_actionReset)
        {
            _actionReset = false;
            return true;
        }
        return false;
    }

    protected void CheckActiveChanged(bool isActiveNow)
    {
        if (isActiveNow)
        {
            BotOwner.PatrollingData.Pause();
            SetLayer(true);
        }
        else
        {
            SetLayer(false);
        }
        _wasActive = isActiveNow;
    }

    private bool _wasActive = false;

    private void BotDecisionMade(ECombatDecision combatDecision, ESquadDecision squadDecision, ESelfActionType selfDecision, Enemy targetEnemy, BotComponent bot)
    {
        if (_wasActive)
        {
            _actionReset = true;
        }
    }

    private bool _actionReset = false;

    private readonly string LayerName = layerName;
    private readonly ESAINLayer ELayer = eSAINLayer;

    private void SetLayer(bool active)
    {
        if (Bot != null)
        {
            if (active)
            {
                Bot.ActiveLayer = ELayer;
            }
            else if (Bot.ActiveLayer == ELayer)
            {
                Bot.ActiveLayer = ESAINLayer.None;
            }
        }
    }

    public override string GetName() => LayerName;

    public static BotManagerComponent BotController => BotManagerComponent.Instance;

    public BotComponent Bot { get; private set; }

    public override void BuildDebugText(StringBuilder stringBuilder)
    {
        if (Bot != null)
        {
            DebugOverlay.AddBaseInfo(Bot, BotOwner, stringBuilder);
        }
    }
}