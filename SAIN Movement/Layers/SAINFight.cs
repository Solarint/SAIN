using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using UnityEngine;
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
        }

        public override Action GetNextAction()
        {
            Action nextAction;

            switch (CurrentDecision)
            {
                case SAINLogicDecision.Stims:
                case SAINLogicDecision.Heal:
                case SAINLogicDecision.Reload:
                case SAINLogicDecision.CombatHeal:
                case SAINLogicDecision.RunAway:
                case SAINLogicDecision.RunForCover:
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
                    nextAction = new Action(typeof(FightAction), "Fight");
                    break;
            }

            LastDecision = CurrentDecision;

            return nextAction;
        }

        public override bool IsActive()
        {
            return CurrentDecision != SAINLogicDecision.None;
        }

        public override bool IsCurrentActionEnding()
        {
            return CurrentDecision != LastDecision;
        }

        public SAINLogicDecision LastDecision = SAINLogicDecision.None;
        public SAINLogicDecision CurrentDecision => SAIN.CurrentDecision;

        private readonly SAINComponent SAIN;
        protected ManualLogSource Logger;
    }
}