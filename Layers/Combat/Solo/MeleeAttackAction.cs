using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes.EnemyClasses;

namespace SAIN.Layers.Combat.Solo;

internal class FightZombiesAction(BotOwner bot) : BotAction(bot, "Fight Zombies"), IBotAction
{
    public override void Update(CustomLayer.ActionData data)
    {
        Enemy priorityEnemy = Bot.GoalEnemy;
        Enemy shotEnemy = Shoot.GetEnemyToShoot(priorityEnemy);
        if (shotEnemy != null)
        {
            if (shotEnemy.RealDistance < 10f)
            {
                Bot.Mover.DogFight.BackUpFromEnemy(shotEnemy);
                Bot.Mover.SetTargetMoveSpeed(1f);
                Bot.Mover.SetTargetPose(1f);
            }
            else if (shotEnemy.RealDistance > 20f)
            {
                Bot.Mover.Stop();
            }
            return;
        }
        Bot.Mover.DogFight.DogFightMove(true, priorityEnemy);
    }
}

internal class MeleeAttackAction : BotAction
{
    public MeleeAttackAction(BotOwner bot) : base(bot, "Melee Attack")
    {
    }

    public override void OnSteeringTicked()
    {
        // handled in RunToEnemyUpdate
    }

    public override void Update(CustomLayer.ActionData data)
    {
        
        BotOwner.WeaponManager.Melee.RunToEnemyUpdate();
        
    }
}