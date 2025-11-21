
using SAIN.Components;

namespace SAIN.SAINComponent.Classes.Decision;

public class SAINDecisionClass : BotComponentClassBase
{
    public bool HasDecision => DecisionManager.HasDecision;

    public ECombatDecision CurrentCombatDecision => DecisionManager.CurrentCombatDecision;
    public ECombatDecision PreviousCombatDecision => DecisionManager.PreviousCombatDecision;

    public ESquadDecision CurrentSquadDecision => DecisionManager.CurrentSquadDecision;
    public ESquadDecision PreviousSquadDecision => DecisionManager.PreviousSquadDecision;

    public ESelfActionType CurrentSelfDecision => DecisionManager.CurrentSelfDecision;
    public ESelfActionType PreviousSelfDecision => DecisionManager.PreviousSelfDecision;

    public float ChangeDecisionTime => DecisionManager.ChangeDecisionTime;
    public float TimeSinceChangeDecision => DecisionManager.TimeSinceChangeDecision;

    public bool RunningToCover
    {
        get
        {
            switch (CurrentCombatDecision)
            {
                case ECombatDecision.Retreat:
                case ECombatDecision.RunAway:
                    return true;
                case ECombatDecision.SeekCover:
                    return Bot.Cover.CoverInUse == null && Bot.Cover.SprintingToCover;

                default:
                    return false;
            }
        }
    }

    public bool IsSearching =>
        CurrentCombatDecision == ECombatDecision.Search ||
        CurrentSquadDecision == ESquadDecision.Search ||
        CurrentSquadDecision == ESquadDecision.GroupSearch;

    public BotDecisionManager DecisionManager { get; }
    public DogFightDecisionClass DogFightDecision { get; }
    public SelfActionDecisionClass SelfActionDecisions { get; }
    public EnemyDecisionClass EnemyDecisions { get; }
    public SquadDecisionClass SquadDecisions { get; }

    public SAINDecisionClass(BotComponent bot) : base(bot)
    {
        TickRequirement = ESAINTickState.OnlyBotActive;
        DecisionManager = new BotDecisionManager(this);
        SelfActionDecisions = new SelfActionDecisionClass(bot);
        EnemyDecisions = new EnemyDecisionClass(bot);
        SquadDecisions = new SquadDecisionClass(bot);
        DogFightDecision = new DogFightDecisionClass(bot);
    }

    public override void Init()
    {
        DecisionManager.Init();
        SelfActionDecisions.Init();
        EnemyDecisions.Init();
        SquadDecisions.Init();
        DogFightDecision.Init();
        base.Init();
    }

    public override void ManualUpdate()
    {
        DecisionManager.ManualUpdate();
        SelfActionDecisions.ManualUpdate();
        EnemyDecisions.ManualUpdate();
        SquadDecisions.ManualUpdate();
        DogFightDecision.ManualUpdate();
        base.ManualUpdate();
    }

    public override void Dispose()
    {
        DecisionManager.Dispose();
        SelfActionDecisions.Dispose();
        EnemyDecisions.Dispose();
        SquadDecisions.Dispose();
        DogFightDecision?.Dispose();
        base.Dispose();
    }

    public void ResetDecisions(bool active) => DecisionManager.ResetDecisions(active);
}