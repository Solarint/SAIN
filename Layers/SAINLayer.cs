using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.Decision;
using System.Text;

namespace SAIN.Layers
{
    public abstract class SAINLayer : CustomLayer
    {
        public static string BuildLayerName<T>()
        {
            return $"{nameof(SAIN)} {typeof(T).Name}";
        }

        public SAINLayer(BotOwner botOwner, int priority, string layerName) : base(botOwner, priority)
        {
            LayerName = layerName;
            Logger = BepInEx.Logging.Logger.CreateLogSource(layerName);
            SAIN = botOwner.GetComponent<SAINComponentClass>();
        }

        private readonly string LayerName;

        public override string GetName() => LayerName;

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