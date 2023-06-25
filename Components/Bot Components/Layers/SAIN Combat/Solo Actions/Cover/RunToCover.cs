using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
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
                RecalcTimer = Time.time + 4f;
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

        private readonly SAINComponent SAIN;
    }
}