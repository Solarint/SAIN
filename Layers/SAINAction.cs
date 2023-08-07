using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.SAINComponent;
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
        }

        public SAINBotController BotController => SAINPlugin.BotController;
        public DecisionWrapper Decisions => SAIN.Memory.Decisions;

        public readonly SAINComponentClass SAIN;

        public readonly ManualLogSource Logger;

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            AppendStringBuilder.AddBaseInfo(SAIN, BotOwner, stringBuilder);
        }
    }
}
