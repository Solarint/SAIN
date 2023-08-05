using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using System.Text;
using UnityEngine;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.Decision;
using SAIN.SAINComponent.Classes.Talk;
using SAIN.SAINComponent.Classes.WeaponFunction;
using SAIN.SAINComponent.Classes.Mover;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;

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
            SAIN = bot.GetComponent<SAINComponentClass>();
        }

        public override Action GetNextAction()
        {
            Action nextAction;
            var Decision = SquadDecision;
            switch (Decision)
            {
                case SquadDecision.Regroup:
                    nextAction = new Action(typeof(RegroupAction), $"{Decision}");
                    break;

                case SquadDecision.Suppress:
                    nextAction = new Action(typeof(SuppressAction), $"{Decision}");
                    break;

                case SquadDecision.Search:
                    nextAction = new Action(typeof(SearchAction), $"{Decision}");
                    break;

                case SquadDecision.Help:
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
            bool Active = SAIN.Decision.CurrentSquadDecision != SquadDecision.None;
            return Active;
        }

        public override bool IsCurrentActionEnding()
        {
            return SquadDecision != LastActionDecision;
        }

        private SquadDecision LastActionDecision = SquadDecision.None;
        public SquadDecision SquadDecision => SAIN.Decision.CurrentSquadDecision;
        private readonly SAINComponentClass SAIN;
        protected ManualLogSource Logger;

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"SAIN Info:");
            var info = SAIN.Info;
            stringBuilder.AppendLabeledValue("Decisions", $"Base: {SAIN.CurrentDecision} Self: {SAIN.Decision.CurrentSelfDecision} Squad: {SAIN.Decision.CurrentSquadDecision}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Personality", $"{info.Personality} {info.Profile.WildSpawnType}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Power of Equipment", $"{info.Profile.PowerLevel}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Start Search + Hold Ground Time", $"{info.TimeBeforeSearch} + {info.HoldGroundDelay}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Difficulty + Modifier", $"{info.Profile.BotDifficulty} + {info.Profile.DifficultyModifier}", Color.white, Color.yellow, true);

            var weapon = info.WeaponInfo;
            var modif = weapon.Modifiers;
            stringBuilder.AppendLabeledValue("WeaponClass + Modifier", $"{weapon.WeaponClass} + {modif.WeaponClassModifier}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("AmmoCaliber + Modifier", $"{weapon.AmmoCaliber} + {modif.AmmoTypeModifier}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Final Shoot Modifier", $"{weapon.FinalModifier}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Recoil, Ergo, Role Modifiers", $"{modif.RecoilModifier}, {modif.ErgoModifier}, {modif.BotRoleModifier}", Color.white, Color.yellow, true);

            if (SAIN.Enemy != null)
            {
                stringBuilder.AppendLabeledValue("Enemy Time Since Seen", $"{SAIN.Enemy.TimeSinceSeen}", Color.red, Color.yellow, true);
            }
        }
    }
}