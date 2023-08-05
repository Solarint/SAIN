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
            SAIN = bot.GetComponent<SAINComponentClass>();
        }

        public override Action GetNextAction()
        {
            Action nextAction;

            var Decision = CurrentDecision;
            var SelfDecision = SAIN.Decision.CurrentSelfDecision;
            switch (Decision)
            {
                case SoloDecision.MoveToEngage:
                    nextAction = new Action(typeof(MoveToEngage), $"{Decision}");
                    break;

                case SoloDecision.RushEnemy:
                    nextAction = new Action(typeof(RushEnemy), $"{Decision}");
                    break;

                case SoloDecision.ThrowGrenade:
                    nextAction = new Action(typeof(ThrowGrenade), $"{Decision}");
                    break;

                case SoloDecision.ShiftCover:
                    nextAction = new Action(typeof(ShiftCover), $"{Decision}");
                    break;

                case SoloDecision.RunToCover:
                    nextAction = new Action(typeof(RunToCover), $"{Decision}");
                    break;

                case SoloDecision.Retreat:
                    nextAction = new Action(typeof(RunToCover), $"{Decision} + {SelfDecision}");
                    break;

                case SoloDecision.WalkToCover:
                case SoloDecision.UnstuckMoveToCover:
                    nextAction = new Action(typeof(WalkToCover), $"{Decision}");
                    break;

                case SoloDecision.DogFight:
                case SoloDecision.UnstuckDogFight:
                    nextAction = new Action(typeof(DogFight), $"{Decision}");
                    break;

                case SoloDecision.StandAndShoot:
                    nextAction = new Action(typeof(StandAndShoot), $"{Decision}");
                    break;

                case SoloDecision.HoldInCover:
                    nextAction = new Action(typeof(HoldinCover), $"{Decision}");
                    break;

                case SoloDecision.Shoot:
                    nextAction = new Action(typeof(ShootAction), $"{Decision}");
                    break;

                case SoloDecision.Search:
                case SoloDecision.UnstuckSearch:
                    nextAction = new Action(typeof(SearchAction), $"{Decision}");
                    break;

                case SoloDecision.Investigate:
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
            bool Active = CurrentDecision != SoloDecision.None;
            return Active;
        }

        public override bool IsCurrentActionEnding()
        {
            return CurrentDecision != LastActionDecision;
        }

        private SoloDecision LastActionDecision = SoloDecision.None;
        public SoloDecision CurrentDecision => SAIN.Memory.Decisions.Main.Current;

        private readonly SAINComponentClass SAIN;
        protected ManualLogSource Logger; 

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"SAIN Info:");
            var info = SAIN.Info;
            stringBuilder.AppendLabeledValue("Decisions", $"Base: {SAIN.Memory.Decisions.Main.Current} Self: {SAIN.Decision.CurrentSelfDecision} Squad: {SAIN.Decision.CurrentSquadDecision}", Color.white, Color.yellow, true);
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