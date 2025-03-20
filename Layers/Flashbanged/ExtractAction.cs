using DrakiaXYZ.BigBrain.Brains;
using EFT;

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

        public override void Update(CustomLayer.ActionData data)
        {

        }
    }
}