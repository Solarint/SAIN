using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Text;

namespace SAIN.Layers.Combat.Solo;

internal class DogFightAction(BotOwner bot) : BotAction(bot, "Dog Fight"), IBotAction
{
    public override void Update(CustomLayer.ActionData data)
    {
        Enemy Enemy = Bot.GoalEnemy;
        Bot.Mover.SetTargetPose(1f);
        Bot.Mover.DogFight.DogFightMove(true, Enemy);
    }

    public override void OnSteeringTicked()
    {
        Enemy enemy = Bot.GoalEnemy;
        TryShootAnyTarget(enemy);
        if (!Bot.Steering.SteerByPriority(enemy, false))
        {
            Bot.Steering.LookToLastKnownEnemyPosition(enemy);
        }
    }

    public override void Stop()
    {
        base.Stop();
        Bot.Mover.DogFight.ResetDogFightStatus();
    }

    public override void BuildDebugText(StringBuilder stringBuilder)
    {
        DebugOverlay.AddBaseInfo(Bot, BotOwner, stringBuilder);
    }
}