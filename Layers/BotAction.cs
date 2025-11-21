using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.Classes.Mover;
using System.Text;

namespace SAIN.Layers;

public interface IBotAction
{
    public string Name { get; }

    public void OnPathSteeringTicked(BotPathCorner cornerDestination, int currentCornerIndex, int totalCorners);

    public void OnSteeringTicked();

    public void UpdateMovement();
}

public abstract class BotAction(BotOwner botOwner, string name) : CustomLogic(botOwner), IBotAction
{
    protected SAINShootData Shoot => Bot.Shoot;

    public string Name { get; private set; } = name;

    public override void BuildDebugText(StringBuilder stringBuilder)
    {
        DebugOverlay.AddBaseInfo(Bot, BotOwner, stringBuilder);
    }

    public BotComponent Bot {
        get
        {
            if (_bot == null &&
                BotManagerComponent.Instance.GetSAIN(BotOwner, out var bot))
            {
                _bot = bot;
            }
            if (_bot == null)
            {
                _bot = BotOwner.GetComponent<BotComponent>();
            }
            if (_bot != null)
            {
                KnownEnemies = _bot.EnemyController.KnownEnemies;
            }
            return _bot;
        }
    }

    protected EnemyList KnownEnemies { get; private set; }

    public virtual void UpdateMovement()
    {
    }

    public virtual void OnPathSteeringTicked(BotPathCorner cornerDestination, int currentCornerIndex, int totalCorners)
    {
        OnSteeringTicked();
    }

    public virtual void OnSteeringTicked()
    {
        Enemy enemy = Bot.GoalEnemy;
        TryShootAnyTarget(enemy);
        Bot.Steering.SteerByPriority(enemy);
    }

    protected bool TryShootAnyTarget(Enemy priorityEnemy)
    {
        return Shoot.ShootAnyVisibleEnemies(priorityEnemy) || Bot.Suppression.TrySuppressAnyEnemy(priorityEnemy, Bot.EnemyController.KnownEnemies);
    }

    public override void Start()
    {
        BotOwner.PatrollingData.Pause();
        Bot.BotActivation.SetCurrentAction(this);
    }

    public override void Stop()
    {
    }

    private BotComponent _bot;
}