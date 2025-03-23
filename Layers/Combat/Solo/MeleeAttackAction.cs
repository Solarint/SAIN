using EFT;
using System.Collections;
using DrakiaXYZ.BigBrain.Brains;

namespace SAIN.Layers.Combat.Solo
{
    internal class MeleeAttackAction : CombatAction, ISAINAction
    {
        public MeleeAttackAction(BotOwner bot) : base(bot, "Melee Attack")
        {
        }

        public override void Update(CustomLayer.ActionData actionData)
        {
            BotOwner.WeaponManager.Melee.RunToEnemyUpdate();
        }

        public override void Start()
        {
            Toggle(true);
        }

        public override void Stop()
        {
            Toggle(false);
        }

        public void Toggle(bool value)
        {
            ToggleAction(value);
        }
    }
}