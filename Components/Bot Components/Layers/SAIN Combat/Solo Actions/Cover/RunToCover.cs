using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace SAIN.Layers
{
    internal class RunToCover : CustomLogic
    {
        public RunToCover(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            Shoot = new ShootClass(bot, SAIN);
        }

        private readonly ManualLogSource Logger;

        public override void Update()
        {
            SAIN.Mover.SetTargetMoveSpeed(1f);
            SAIN.Mover.SetTargetPose(1f);

            if (CoverDestination == null || RecalcTimer < Time.time)
            {
                RecalcTimer = Time.time + 2f;
                if (Decision == SAINSoloDecision.Retreat && SAIN.Cover.FallBackPoint != null)
                {
                    CoverDestination = SAIN.Cover.FallBackPoint;
                    BotOwner.BotRun.Run(CoverDestination.Position, false);
                }
                else if (FindTargetCover())
                {
                    BotOwner.BotRun.Run(CoverDestination.Position, false);
                }
            }
            if (CoverDestination == null)
            {
                EngageEnemy();
            }
        }

        private float RecalcTimer;

        private bool FindTargetCover()
        {
            var coverPoint = SAIN.Cover.ClosestPoint;
            if (coverPoint != null && !coverPoint.Spotted)
            {
                if (CanMoveTo(coverPoint, out Vector3 pointToGo))
                {
                    coverPoint.BotIsUsingThis = true;
                    CoverDestination = coverPoint;
                    return true;
                }
                else
                {
                    coverPoint.BotIsUsingThis = false;
                }
            }
            return false;
        }

        private bool CanMoveTo(CoverPoint coverPoint, out Vector3 pointToGo)
        {
            if (coverPoint != null && SAIN.Mover.CanGoToPoint(coverPoint.Position, out pointToGo))
            {
                return true;
            }
            pointToGo = Vector3.zero;
            return false;
        }

        private CoverPoint CoverDestination;

        private void EngageEnemy()
        {
            SAIN.Steering.SteerByPriority();
            Shoot.Update();
        }

        private readonly ShootClass Shoot;

        private SAINSoloDecision Decision => SAIN.CurrentDecision;

        private bool FarFromCover;

        public override void Start()
        {
            if (SAIN.Decision.CurrentSelfDecision == SAINSelfDecision.RunAwayGrenade)
            {
                SAIN.Talk.Say(EPhraseTrigger.OnEnemyGrenade, ETagStatus.Combat);
            }
        }

        public override void Stop()
        {
            CoverDestination = null;
        }

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"SAIN Info:");
            stringBuilder.AppendLabeledValue("Personality", $"{SAIN.Info.BotPersonality}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("BotType", $"{SAIN.Info.BotType}", Color.white, Color.yellow, true);
            CoverPoint cover = SAIN.Cover.CoverInUse;
            if (cover != null)
            {
                stringBuilder.AppendLine($"SAIN Cover Info:");
                stringBuilder.AppendLabeledValue("Cover Position", $"{cover.Position}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover Distance", $"{cover.Distance}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover Spotted?", $"{cover.Spotted}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover Path Length", $"{cover.PathDistance}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover ID", $"{cover.Id}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover Status", $"{cover.CoverStatus}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover HitInCoverCount", $"{cover.HitInCoverCount}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover HitInCoverUnknownCount", $"{cover.HitInCoverUnknownCount}", Color.white, Color.yellow, true);
            }
        }

        private readonly SAINComponent SAIN;
    }
}