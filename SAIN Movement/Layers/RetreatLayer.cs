using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using static RoadSplineGenerator;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class RetreatLayer : CustomLayer
    {
        public override string GetName()
        {
            return "SAIN Retreat";
        }

        public RetreatLayer(BotOwner bot, int priority) : base(bot, priority)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);

            if (DebugLayers.Value) Logger.LogInfo($"Added [{GetName()}] Layer to [{bot.name}] Bot Type: [{bot.Profile.Info.Settings.Role}]");

            SAIN = bot.GetComponent<SAINComponent>();
        }

        public override bool IsActive()
        {
            // Check that the bot is active and that they have an enemy
            if (BotOwner.BotState != EBotState.Active || BotOwner.Memory.GoalEnemy == null)
            {
                return false;
            }

            // Check that there isn't a grenade to run from
            if (BotOwner.BewareGrenade?.GrenadeDangerPoint?.Grenade != null)
            {
                return false;
            }

            Reload = false;
            Heal = false;
            // Decide if we need to activate this layer
            var decision = SAIN.Decisions;
            if (decision.ShouldBotHeal)
            {
                Heal = true;
                return true;
            }

            if (decision.ShouldBotReload)
            {
                Reload = true;
                return true;
            }

            return false;
        }

        bool Heal = false;
        bool Reload = false;

        public override Action GetNextAction()
        {
            string reason = Reload ? ": Reload" : ": Heal";
            if (DebugLayers.Value) Logger.LogWarning($"Called {GetName()} Action for [{BotOwner.name}] because [{reason}]");
            return new Action(typeof(RetreatAction), GetName() + reason);
        }

        public override bool IsCurrentActionEnding()
        {
            return !Heal && !Reload;
        }

        private readonly SAINComponent SAIN;
        protected ManualLogSource Logger;
    }
}