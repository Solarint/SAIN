using EFT;
using SAIN.Components;
using SAIN.SAINComponent.Classes.EnemyClasses;

namespace SAIN.BotController.Classes;

public class MemberInfo
{
    private readonly Squad _squad;
    public MemberInfo(BotComponent sain, Squad squad)
    {
        _squad = squad;
        Bot = sain;
        Player = sain.Player;
        ProfileId = sain.ProfileId;
        Nickname = sain.Player?.Profile?.Nickname;

        HealthStatus = sain.Memory.Health.HealthStatus;

        sain.Decision.DecisionManager.OnDecisionMade += UpdateDecisions;
        sain.Memory.Health.HealthStatusChanged += UpdateHealth;
        sain.OnDispose += removeMe;

        UpdatePowerLevel();
    }

    private void removeMe()
    {
        _squad?.RemoveMember(ProfileId);
    }

    private void UpdateDecisions(ECombatDecision solo, ESquadDecision squad, ESelfActionType self, Enemy enemy, BotComponent member)
    {
        SoloDecision = solo;
        SquadDecision = squad;
        SelfDecision = self;

        // Update power level here just to see if equipment changed.
        UpdatePowerLevel();
    }

    public void UpdatePowerLevel()
    {
        var aiData = Bot?.Player?.AIData;
        if (aiData != null)
        {
            PowerLevel = aiData.PowerOfEquipment;
        }
    }

    private void UpdateHealth(ETagStatus healthStatus)
    {
        HealthStatus = healthStatus;
    }

    public readonly BotComponent Bot;
    public readonly Player Player;
    public readonly string ProfileId;
    public readonly string Nickname;

    public bool HasEnemy => Bot?.HasEnemy == true;

    public ETagStatus HealthStatus;

    public ECombatDecision SoloDecision { get; private set; }
    public ESquadDecision SquadDecision { get; private set; }
    public ESelfActionType SelfDecision { get; private set; }
    public float PowerLevel { get; private set; }

    public void Dispose()
    {
        if (Bot != null)
        {
            Bot.OnDispose -= removeMe;
            Bot.Decision.DecisionManager.OnDecisionMade -= UpdateDecisions;
            Bot.Memory.Health.HealthStatusChanged -= UpdateHealth;
        }
    }
}
