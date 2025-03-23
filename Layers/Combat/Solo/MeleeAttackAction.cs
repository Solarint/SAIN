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
            this.StartProfilingSample("Update");
            BotOwner.WeaponManager.Melee.RunToEnemyUpdate();
            this.EndProfilingSample();
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