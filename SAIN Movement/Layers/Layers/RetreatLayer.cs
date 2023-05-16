using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
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

            //if (DebugLayers.Value) Logger.LogInfo($"Added [{GetName()}] Layer to [{bot.name}] Bot Type: [{bot.Profile.Info.Settings.Role}]");

            SAIN = bot.GetComponent<SAINComponent>();
        }

        public override bool IsActive()
        {
            bool active = false;

            if (SAIN.ActivateLayers)
            {
                // Decide if we need to activate this layer
                if (BotOwner.Medecine.Using || SAIN.Decisions.ShouldBotHeal)
                {
                    Reason = " Heal";
                    active = true;
                }
                else if (BotOwner.WeaponManager.Reload.Reloading || SAIN.Decisions.ShouldBotReload)
                {
                    Reason = " Reload";
                    active = true;
                }
            }

            SAIN.FallingBack = active;
            return active;
        }

        public override Action GetNextAction()
        {
            if (DebugLayers.Value) Logger.LogWarning($"Called {GetName()} Action for [{BotOwner.name}]");
            return new Action(typeof(RetreatAction), GetName() + Reason);
        }

        public override bool IsCurrentActionEnding()
        {
            return false;
        }

        private readonly SAINComponent SAIN;
        protected ManualLogSource Logger;
        private string Reason = "None";
    }
}