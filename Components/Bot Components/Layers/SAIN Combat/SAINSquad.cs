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
            var info = SAIN.Info;
            stringBuilder.AppendLabeledValue("Decisions", $"Base: {SAIN.CurrentDecision} Self: {SAIN.Decision.CurrentSelfDecision} Squad: {SAIN.Decision.CurrentSquadDecision}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Personality", $"{info.Personality} {info.WildSpawnType}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Power of Equipment", $"{info.PowerLevel}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Start Search + Hold Ground Time", $"{info.TimeBeforeSearch} + {info.HoldGroundDelay}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Difficulty + Modifier", $"{info.BotDifficulty} + {info.DifficultyModifier}", Color.white, Color.yellow, true);

            var weapon = info.WeaponInfo;
            var modif = weapon.Modifiers;
            stringBuilder.AppendLabeledValue("WeaponClass + Modifier", $"{modif.WeaponClass} + {modif.WeaponClassModifier}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("AmmoCaliber + Modifier", $"{modif.AmmoCaliber} + {modif.AmmoTypeModifier}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Final Shoot Modifier", $"{weapon.FinalModifier}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Recoil, Ergo, Role Modifiers", $"{modif.RecoilModifier}, {modif.ErgoModifier}, {modif.BotRoleModifier}", Color.white, Color.yellow, true);

            if (SAIN.Enemy != null)
            {
                stringBuilder.AppendLabeledValue("Enemy Time Since Seen", $"{SAIN.Enemy.TimeSinceSeen}", Color.red, Color.yellow, true);
            }
        }
    }
}