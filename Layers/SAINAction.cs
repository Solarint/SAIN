using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.Classes.Decision;
using System.Text;

namespace SAIN.Layers
{
    public abstract class SAINAction : CustomLogic
    {
        public SAINAction(BotOwner botOwner, string name) : base(botOwner)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(name);
            SAIN = botOwner.GetComponent<SAINComponentClass>();
            Shoot = new ShootClass(botOwner);
        }

        public SAINBotControllerComponent BotController => SAINPlugin.BotController;
        public DecisionWrapper Decisions => SAIN.Memory.Decisions;

        public readonly SAINComponentClass SAIN;

        public readonly ManualLogSource Logger;

        public readonly ShootClass Shoot;

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            DebugOverlay.AddBaseInfo(SAIN, BotOwner, stringBuilder);
        }
    }
}
