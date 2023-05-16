using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class DogFightLayer : CustomLayer
    {
        public override string GetName()
        {
            return "SAIN DogFight";
        }

        public DogFightLayer(BotOwner bot, int priority) : base(bot, priority)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);

            Logger.LogInfo($"Added {GetName()} Layer to {bot.name}. Bot Type: [{bot.Profile.Info.Settings.Role}]");

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

            // Decide if we need to activate this layer
            if (SAIN.Decisions.StartDogFight)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override Action GetNextAction()
        {
            if (DebugLayers.Value) Logger.LogWarning($"Called {GetName()} Action for [{BotOwner.name}]");
            return new Action(typeof(DogfightAction), GetName());
        }

        public override bool IsCurrentActionEnding()
        {
            return false;
        }

        private readonly SAINComponent SAIN;
        protected ManualLogSource Logger;
    }
}