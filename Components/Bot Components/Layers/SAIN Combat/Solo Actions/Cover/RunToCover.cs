using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
using System.Text;
using UnityEngine;

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

            if (RecalcTimer < Time.time)
            {
                if (FindTargetCover())
                {
                    RecalcTimer = Time.time + 2f;
                    BotOwner.BotRun.Run(CoverDestination.Position, false, 0.6f);
                }
                else
                {
                    RecalcTimer = Time.time + 0.5f;
                }
            }
            if (CoverDestination == null || CoverDestination.BotIsHere)
            {
                EngageEnemy();
            }
        }

        private float RecalcTimer;

        private bool FindTargetCover()
        {
            if (CoverDestination != null)
            {
                CoverDestination.BotIsUsingThis = false;
                CoverDestination = null;
            }

            CoverPoint coverPoint = SelectPoint();
            if (coverPoint != null && !coverPoint.Spotted)
            {
                if (SAIN.Mover.CanGoToPoint(coverPoint.Position, out Vector3 pointToGo))
                {
                    coverPoint.Position = pointToGo;
                    coverPoint.BotIsUsingThis = true;
                    CoverDestination = coverPoint;
                    return true;
                }
            }
            return false;
        }

        private CoverPoint SelectPoint()
        {
            CoverPoint fallback = SAIN.Cover.FallBackPoint;
            if (SAIN.CurrentDecision == SAINSoloDecision.Retreat && fallback != null)
            {
                return fallback;
            }
            else
            {
                return SAIN.Cover.ClosestPoint;
            }
        }

        private CoverPoint CoverDestination;

        private void EngageEnemy()
        {
            SAIN.Steering.SteerByPriority();
            Shoot.Update();
        }

        private readonly ShootClass Shoot;

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
            stringBuilder.AppendLabeledValue("Personality", $"{SAIN.Info.Personality}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("BotType", $"{SAIN.Info.WildSpawnType}", Color.white, Color.yellow, true);
            CoverPoint cover = SAIN.Cover.CoverInUse;
            if (cover != null)
            {
                stringBuilder.AppendLine($"SAIN Cover Info:");
                stringBuilder.AppendLabeledValue("Cover Position", $"{cover.Position}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover Distance", $"{cover.Distance}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover Spotted?", $"{cover.Spotted}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover Path Length", $"{cover.Distance}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover ID", $"{cover.Id}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover Status", $"{cover.CoverStatus}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover HitInCoverCount", $"{cover.HitInCoverCount}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover HitInCoverUnknownCount", $"{cover.HitInCoverUnknownCount}", Color.white, Color.yellow, true);
            }
        }

        private readonly SAINComponent SAIN;
    }
}