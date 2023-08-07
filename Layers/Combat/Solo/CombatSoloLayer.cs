using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using System.Text;
using SAIN.SAINComponent;

namespace SAIN.Layers.Combat.Solo
{
    internal class CombatSoloLayer : SAINLayer
    {
        public CombatSoloLayer(BotOwner bot, int priority) : base(bot, priority, Name)
        {
        }

        public static readonly string Name = BuildLayerName<CombatSoloLayer>();

        public override Action GetNextAction()
        {
            SoloDecision Decision = CurrentDecision;
            var SelfDecision = SAIN.Decision.CurrentSelfDecision;
            LastActionDecision = Decision;
            switch (Decision)
            {
                case SoloDecision.MoveToEngage:
                    return new Action(typeof(MoveToEngageAction), $"{Decision}");

                case SoloDecision.RushEnemy:
                    return new Action(typeof(RushEnemyAction), $"{Decision}");

                case SoloDecision.ThrowGrenade:
                    return new Action(typeof(ThrowGrenadeAction), $"{Decision}");

                case SoloDecision.ShiftCover:
                    return new Action(typeof(ShiftCover), $"{Decision}");

                case SoloDecision.RunToCover:
                    return new Action(typeof(RunToCover), $"{Decision}");

                case SoloDecision.Retreat:
                    return new Action(typeof(RunToCover), $"{Decision} + {SelfDecision}");

                case SoloDecision.WalkToCover:
                case SoloDecision.UnstuckMoveToCover:
                    return new Action(typeof(WalkToCover), $"{Decision}");

                case SoloDecision.DogFight:
                case SoloDecision.UnstuckDogFight:
                    return new Action(typeof(DogFightAction), $"{Decision}");

                case SoloDecision.StandAndShoot:
                    return new Action(typeof(StandAndShootAction), $"{Decision}");

                case SoloDecision.HoldInCover:
                    return new Action(typeof(HoldinCover), $"{Decision}");

                case SoloDecision.Shoot:
                    return new Action(typeof(ShootAction), $"{Decision}");

                case SoloDecision.Search:
                case SoloDecision.UnstuckSearch:
                    return new Action(typeof(SearchAction), $"{Decision}");

                case SoloDecision.Investigate:
                    return new Action(typeof(InvestigateAction), $"{Decision}");

                default:
                    return new Action(typeof(WalkToCover), $"DEFAULT!");
            }
        }

        public override bool IsActive()
        {
            return CurrentDecision != SoloDecision.None;
        }

        public override bool IsCurrentActionEnding()
        {
            return CurrentDecision != LastActionDecision;
        }

        private SoloDecision LastActionDecision = SoloDecision.None;
        public SoloDecision CurrentDecision => SAIN.Memory.Decisions.Main.Current;
    }
}