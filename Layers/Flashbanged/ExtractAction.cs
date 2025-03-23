using EFT;
using DrakiaXYZ.BigBrain.Brains;

namespace SAIN.Layers
{
    internal class FlashbangedAction : CombatAction, ISAINAction
    {
        public void Toggle(bool value)
        {
            ToggleAction(value);
        }

        public FlashbangedAction(BotOwner bot) : base(bot, "Flashbanged")
        {
        }

        public override void Start()
        {
            Toggle(true);
        }

        public override void Stop()
        {
            Toggle(false);
        }

        public override void Update(CustomLayer.ActionData actionData)
        {
            this.StartProfilingSample("Update");
            this.EndProfilingSample();
        }
    }
}