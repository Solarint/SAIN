using BepInEx.Logging;
using Comfort.Common;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.UserSettings;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class SAINFight : CustomLayer
    {
        public override string GetName()
        {
            return "SAIN Fight";
        }

        public SAINFight(BotOwner bot, int priority) : base(bot, priority)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);

            //Logger.LogInfo($"Added {GetName()} Layer to {bot.name}. Bot Type: [{bot.Profile.Info.Settings.Role}]");

            SAIN = bot.GetComponent<SAINComponent>();

            LastDecision = CurrentDecision;
        }

        public override Action GetNextAction()
        {
            if (CoverConfig.AllBotsMoveToPlayer.Value)
            {
                return new Action(typeof(DebugMoveToPlayerAction), $"CHARGE!");
            }

            Action nextAction;

            switch (CurrentDecision)
            {
                case SAINLogicDecision.Stims:
                case SAINLogicDecision.Heal:
                case SAINLogicDecision.Reload:
                case SAINLogicDecision.CombatHeal:
                case SAINLogicDecision.RunAway:
                case SAINLogicDecision.RunForCover:
                case SAINLogicDecision.RunAwayGrenade:
                case SAINLogicDecision.WalkToCover:
                    nextAction = new Action(typeof(RetreatAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.Fight:
                    nextAction = new Action(typeof(FightAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.Suppress:
                    nextAction = new Action(typeof(SuppressAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.DogFight:
                    nextAction = new Action(typeof(DogfightAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.HoldInCover:
                    nextAction = new Action(typeof(HoldInCoverAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.Skirmish:
                    nextAction = new Action(typeof(SkirmishAction), $"{CurrentDecision}");
                    break;

                case SAINLogicDecision.Search:
                    nextAction = new Action(typeof(SearchAction), $"{CurrentDecision}");
                    break;

                default:
                    nextAction = new Action(typeof(RetreatAction), $"DEFAULT!");
                    break;
            }

            LastDecision = CurrentDecision;

            Logger.LogInfo($"New Action for {BotOwner.name}. {nextAction.GetType().Name}");

            return nextAction;
        }

        public override bool IsActive()
        {
            if (CoverConfig.AllBotsMoveToPlayer.Value)
            {
                return true;
            }

            return CurrentDecision != SAINLogicDecision.None;
        }

        public override bool IsCurrentActionEnding()
        {
            if (CoverConfig.AllBotsMoveToPlayer.Value)
            {
                return false;
            }

            return CurrentDecision != LastDecision;
        }

        public SAINLogicDecision LastDecision;
        public SAINLogicDecision CurrentDecision => SAIN.CurrentDecision;

        private readonly SAINComponent SAIN;
        protected ManualLogSource Logger;
    }
}