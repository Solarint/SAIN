using EFT;
using System.Collections;
using DrakiaXYZ.BigBrain.Brains;

namespace SAIN.Layers.Combat.Solo
{
    internal class FreezeAction : CombatAction
    {
        public FreezeAction(BotOwner bot) : base(bot, nameof(FreezeAction))
        {
        }

        public void Toggle(bool value)
        {
            ToggleAction(value);
        }

        public override void Update(CustomLayer.ActionData actionData)
        {
            Bot.Mover.SetTargetPose(0f);
            if (!Bot.Steering.SteerByPriority(null, false)) {
                Bot.Steering.LookToLastKnownEnemyPosition(Bot.Enemy);
            }
            Shoot.CheckAimAndFire();
        }

        public override void Start()
        {
            Toggle(true);
            Bot.Mover.StopMove();
        }

        public override void Stop()
        {
            Toggle(false);
        }
    }
}