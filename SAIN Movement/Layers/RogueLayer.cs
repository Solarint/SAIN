using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class RogueLayer : CustomLayer
    {
        public RogueLayer(BotOwner bot, int priority) : base(bot, priority)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            Logger.LogInfo($"Added SAIN Rogue Layer to {bot.name}");

            BotDecision = bot.GetComponent<Decisions>();
            SAIN = bot.GetComponent<SAINCore>();
        }

        private readonly Decisions BotDecision;
        private readonly SAINCore SAIN;

        public override string GetName()
        {
            return "SAIN Rogue";
        }

        private const float maxStartDistance = 400f;

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
            Action action;

            if (BotDecision.ShouldBotHeal || BotDecision.ShouldBotPopStims)
            {
                Logger.LogDebug($"Called Heal Action for [{BotOwner.name}]");
                action = new Action(typeof(FallBackLogic), "Heal");
            }
            else if (BotDecision.ShouldBotReload)
            {
                Logger.LogDebug($"Called Reload Action for [{BotOwner.name}]");
                action = new Action(typeof(FallBackLogic), "Reload");
            }
            else if (BotDecision.StartDogFight)
            {
                Logger.LogDebug($"Called DogFight Action for [{BotOwner.name}]");
                action = new Action(typeof(DogFightLogic), "DogFight");
            }
            else if (BotDecision.StartFight)
            {
                Logger.LogDebug($"Called Fight Action for [{BotOwner.name}]");
                action = new Action(typeof(FightLogic), "Fight");
            }
            else
            {
                Logger.LogDebug($"Called Skirmish Action for [{BotOwner.name}]");
                action = new Action(typeof(SkirmishLogic), "Skirmish");
            }

            return action;
        }

        public override bool IsCurrentActionEnding()
        {
            return false;
        }

        private bool DebugMode => DebugLayers.Value;
        protected ManualLogSource Logger;
    }
}