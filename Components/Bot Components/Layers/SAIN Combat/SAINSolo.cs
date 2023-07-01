using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using System.Text;
using UnityEngine;

namespace SAIN.Layers
{
    internal class SAINSolo : CustomLayer
    {
        public override string GetName()
        {
            return Name;
        }

        public static string Name => "SAIN Combat Solo";

        public SAINSolo(BotOwner bot, int priority) : base(bot, priority)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
        }

        public override Action GetNextAction()
        {
            Action nextAction;

            var Decision = CurrentDecision;
            var SelfDecision = SAIN.Decision.CurrentSelfDecision;
            switch (Decision)
            {
                case SAINSoloDecision.MoveToEngage:
                    nextAction = new Action(typeof(MoveToEngage), $"{Decision}");
                    break;

                case SAINSoloDecision.RushEnemy:
                    nextAction = new Action(typeof(RushEnemy), $"{Decision}");
                    break;

                case SAINSoloDecision.ThrowGrenade:
                    nextAction = new Action(typeof(ThrowGrenade), $"{Decision}");
                    break;

                case SAINSoloDecision.ShiftCover:
                    nextAction = new Action(typeof(ShiftCover), $"{Decision}");
                    break;

                case SAINSoloDecision.RunToCover:
                    nextAction = new Action(typeof(RunToCover), $"{Decision}");
                    break;

                case SAINSoloDecision.Retreat:
                    nextAction = new Action(typeof(RunToCover), $"{Decision} + {SelfDecision}");
                    break;

                case SAINSoloDecision.WalkToCover:
                case SAINSoloDecision.UnstuckMoveToCover:
                    nextAction = new Action(typeof(WalkToCover), $"{Decision}");
                    break;

                case SAINSoloDecision.DogFight:
                case SAINSoloDecision.UnstuckDogFight:
                    nextAction = new Action(typeof(DogFight), $"{Decision}");
                    break;

                case SAINSoloDecision.StandAndShoot:
                    nextAction = new Action(typeof(StandAndShoot), $"{Decision}");
                    break;

                case SAINSoloDecision.HoldInCover:
                    nextAction = new Action(typeof(HoldinCover), $"{Decision}");
                    break;

                case SAINSoloDecision.Shoot:
                    nextAction = new Action(typeof(ShootAction), $"{Decision}");
                    break;

                case SAINSoloDecision.Search:
                case SAINSoloDecision.UnstuckSearch:
                    nextAction = new Action(typeof(SearchAction), $"{Decision}");
                    break;

                case SAINSoloDecision.Investigate:
                    nextAction = new Action(typeof(InvestigateAction), $"{Decision}");
                    break;

                default:
                    nextAction = new Action(typeof(WalkToCover), $"DEFAULT!");
                    break;
            }

            if (nextAction == null)
            {
                Logger.LogError("Action Null?");
                nextAction = new Action(typeof(WalkToCover), $"DEFAULT!");
            }

            LastActionDecision = Decision;
            return nextAction;
        }

        public override bool IsActive()
        {
            bool Active = CurrentDecision != SAINSoloDecision.None;
            return Active;
        }

        public override bool IsCurrentActionEnding()
        {
            return CurrentDecision != LastActionDecision;
        }

        private SAINSoloDecision LastActionDecision = SAINSoloDecision.None;
        public SAINSoloDecision CurrentDecision => SAIN.CurrentDecision;

        private readonly SAINComponent SAIN;
        protected ManualLogSource Logger; 

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"SAIN Info:");
            var info = SAIN.Info;
            stringBuilder.AppendLabeledValue("Decisions", $"Base: {SAIN.CurrentDecision} Self: {SAIN.Decision.CurrentSelfDecision} Squad: {SAIN.Decision.CurrentSquadDecision}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Personality", $"{info.Personality} {info.BotType}", Color.white, Color.yellow, true);
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