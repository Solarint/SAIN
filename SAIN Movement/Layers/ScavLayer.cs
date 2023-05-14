using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;

namespace SAIN.Layers
{
    internal class ScavLayer : CustomLayer
    {
        public ScavLayer(BotOwner bot, int priority) : base(bot, priority)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            Logger.LogInfo($"Added SAIN Scav Layer to {bot.name}");

            BotDecision = bot.GetComponent<Decisions>();
            SAIN = bot.GetComponent<SAINCore>();
        }

        private readonly Decisions BotDecision;
        private readonly SAINCore SAIN;

        public override string GetName()
        {
            return "SAIN Scav";
        }

        public override bool IsActive()
        {
            if (SAIN == null)
            {
                Logger.LogError("Bot Sense Component Is Null");
                return false;
            }

            if (BotOwner.BotState != EBotState.Active)
            {
                return false;
            }

            if (BotOwner.Memory.GoalEnemy == null)
            {
                return false;
            }

            if (BotOwner.BewareGrenade?.GrenadeDangerPoint?.Grenade != null)
            {
                return false;
            }

            return true;
        }

        public override Action GetNextAction()
        {
            if (BotDecision.ShouldBotHeal)
            {
                Logger.LogInfo($"Called Heal Action for [{BotOwner.name}]");
                return new Action(typeof(FallBackLogic), "SAIN Heal");
            }

            if (BotDecision.ShouldBotReload)
            {
                Logger.LogInfo($"Called Reload Action for [{BotOwner.name}]");
                return new Action(typeof(FallBackLogic), "SAIN Reload");
            }

            if (BotDecision.StartDogFight)
            {
                Logger.LogInfo($"Called DogFight Action for [{BotOwner.name}]");
                return new Action(typeof(DogFightLogic), "SAIN DogFight");
            }

            if (BotDecision.StartFight)
            {
                Logger.LogInfo($"Called Fight Action for [{BotOwner.name}]");
                return new Action(typeof(FightLogic), "SAIN Fight");
            }
            else
            {
                Logger.LogInfo($"Called Skirmish Action for [{BotOwner.name}]");
                return new Action(typeof(SkirmishLogic), "SAIN Skirmish");
            }
        }

        public override bool IsCurrentActionEnding()
        {
            return false;
        }

        protected ManualLogSource Logger;
    }
}