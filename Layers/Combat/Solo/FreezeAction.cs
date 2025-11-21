using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes.EnemyClasses;

namespace SAIN.Layers.Combat.Solo;

internal class FreezeAction(BotOwner bot) : BotAction(bot, nameof(FreezeAction)), IBotAction
{
    public override void Update(CustomLayer.ActionData data)
    {
        Enemy Enemy = Bot.GoalEnemy;
        Bot.Mover.Pose.SetPoseToCover(Enemy);
    }

    public override void Start()
    {
        base.Start();
        Bot.Mover.Stop();
    }
}