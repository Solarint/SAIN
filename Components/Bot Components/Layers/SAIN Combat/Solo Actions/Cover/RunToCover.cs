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
            Shoot = new ShootClass(bot);
        }

        private readonly ManualLogSource Logger;

        public override void Update()
        {
            if (CoverDestination != null)
            {
                if (!SAIN.Cover.CoverPoints.Contains(CoverDestination) || CoverDestination.Spotted)
                {
                    CoverDestination = null;
                }
            }

            SAIN.Mover.SetTargetMoveSpeed(1f);
            SAIN.Mover.SetTargetPose(1f);

            if (CoverDestination == null)
            {
                if (FindTargetCover())
                {
                    if (SAIN.Mover.Prone.IsProne)
                    {
                        SAIN.Mover.Prone.SetProne(false);
                    }
                    SAIN.Mover.GoToPoint(CoverDestination.Position, 0.6f);
                }
            }
            if (CoverDestination == null)
            {
                EngageEnemy();
                SAIN.Mover.Sprint(false);
            }
        }

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
                    coverPoint.Spotted = true;
                }
            }
            return false;
        }

        private void MoveTo(Vector3 position)
        {
            SAIN.Mover.GoToPoint(position, 0.6f);
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
            if (!SAIN.Steering.SteerByPriority(false))
            {
                if (SAIN.Enemy != null)
                {
                    SAIN.Steering.LookToEnemy(SAIN.Enemy);
                }
            }
            Shoot.Update();
        }

        private readonly ShootClass Shoot;

        private SAINSoloDecision Decision => SAIN.CurrentDecision;

        private bool FarFromCover;

        public override void Start()
        {
            SAIN.Mover.Sprint(true);
            if (SAIN.Decision.SelfDecision == SAINSelfDecision.RunAwayGrenade)
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