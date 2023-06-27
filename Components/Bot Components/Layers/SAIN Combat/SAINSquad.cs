using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using System.Text;
using UnityEngine;

namespace SAIN.Layers
{
    internal class SAINSquad : CustomLayer
    {
        public override string GetName()
        {
            return Name;
        }

        public static string Name => "SAIN Combat Squad";

        public SAINSquad(BotOwner bot, int priority) : base(bot, priority)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
        }

        public override Action GetNextAction()
        {
            Action nextAction;
            var Decision = SquadDecision;
            switch (Decision)
            {
                case SAINSquadDecision.Regroup:
                    nextAction = new Action(typeof(RegroupAction), $"{Decision}");
                    break;

                case SAINSquadDecision.Suppress:
                    nextAction = new Action(typeof(SuppressAction), $"{Decision}");
                    break;

                case SAINSquadDecision.Search:
                    nextAction = new Action(typeof(SearchAction), $"{Decision}");
                    break;

                case SAINSquadDecision.Help:
                    nextAction = new Action(typeof(SearchAction), $"{Decision}");
                    break;

                default:
                    nextAction = new Action(typeof(RegroupAction), $"DEFAULT!");
                    break;
            }
            if (nextAction == null)
            {
                Logger.LogError("Action Null?");
                nextAction = new Action(typeof(RegroupAction), $"DEFAULT!");
            }
            LastActionDecision = Decision;
            return nextAction;
        }

        public override bool IsActive()
        {
            bool Active = SAIN.Decision.CurrentSquadDecision != SAINSquadDecision.None;
            return Active;
        }

        public override bool IsCurrentActionEnding()
        {
            return SquadDecision != LastActionDecision;
        }

        private SAINSquadDecision LastActionDecision = SAINSquadDecision.None;
        public SAINSquadDecision SquadDecision => SAIN.Decision.CurrentSquadDecision;
        private readonly SAINComponent SAIN;
        protected ManualLogSource Logger;

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"SAIN Info:");
            stringBuilder.AppendLabeledValue("Personality", $"{SAIN.Info.BotPersonality}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("BotType", $"{SAIN.Info.BotType}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Power of Equipment", $"{SAIN.Info.PowerLevel}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Time Before Start Search", $"{SAIN.Info.TimeBeforeSearch}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Hold Ground Time", $"{SAIN.Info.HoldGroundDelay}", Color.white, Color.yellow, true);
            if (SAIN.Enemy != null)
            {
                stringBuilder.AppendLine($"SAIN Enemy Info:");
                stringBuilder.AppendLabeledValue("Enemy", $"{SAIN.Enemy.EnemyPlayer.name}", Color.red, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Seen?", $"{SAIN.Enemy.Seen}", Color.red, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Time Since Seen", $"{SAIN.Enemy.TimeSinceSeen}", Color.red, Color.yellow, true);
            }
        }
    }
}