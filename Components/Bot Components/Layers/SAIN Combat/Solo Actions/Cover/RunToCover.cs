using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
using UnityEngine;

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
            if (CoverDestination != null && CoverDestination.BotIsHere)
            {
                EngageEnemy();
                return;
            }

            SAIN.Mover.SetTargetMoveSpeed(1f);
            SAIN.Mover.SetTargetPose(1f);

            FindTargetCover();

            if (CoverDestination == null)
            {
                EngageEnemy();
                return;
            }
            if (CoverDestination != null)
            {
                if (SAIN.Mover.Prone.IsProne)
                {
                    SAIN.Mover.Prone.SetProne(false);
                }

                MoveTo(DestinationPosition);

                if (CheckDistanceToCover(DestinationPosition))
                {
                    SAIN.Steering.LookToMovingDirection(true);
                    SAIN.Mover.Sprint(true);
                }
                else
                {
                    SAIN.Steering.LookToMovingDirection(false);
                    SAIN.Mover.Sprint(false);
                    EngageEnemy();
                }
            }
        }

        private bool FindTargetCover()
        {
            var coverPoint = SAIN.Cover.ClosestPoint;
            if (coverPoint != null && !coverPoint.Spotted)
            {
                if (CanMoveTo(coverPoint, out Vector3 pointToGo))
                {
                    CoverDestination = coverPoint;
                    DestinationPosition = pointToGo;
                    return true;
                }
                else
                {
                    coverPoint.Spotted = true;
                }
            }
            if (CoverDestination != null)
            {
                CoverDestination.BotIsUsingThis = false;
                CoverDestination = null;
            }
            return false;
        }

        private void MoveTo(Vector3 position)
        {
            CoverDestination.BotIsUsingThis = true;
            SAIN.Mover.GoToPoint(position);
        }

        private bool CheckDistanceToCover(Vector3 destPosition)
        {
            float sqrMag = (destPosition - SAIN.Position).sqrMagnitude;
            if (sqrMag < 2f)
            {
                FarFromCover = false;
            }
            else if (sqrMag > 4f)
            {
                FarFromCover = true;
            }
            return FarFromCover;
        }

        private bool CanMoveTo(CoverPoint coverPoint, out Vector3 pointToGo)
        {
            if (coverPoint != null && SAIN.Mover.CanGoToPoint(coverPoint.Position, out pointToGo))
            {
                CoverDestination = coverPoint;
                DestinationPosition = pointToGo;
                return true;
            }
            pointToGo = Vector3.zero;
            return false;
        }

        private CoverPoint CoverDestination;
        private Vector3 DestinationPosition;

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
            if (SAIN.Decision.SelfDecision == SAINSelfDecision.RunAwayGrenade)
            {
                SAIN.Talk.Say(EPhraseTrigger.OnEnemyGrenade, ETagStatus.Combat);
            }
        }

        public override void Stop()
        {
            if (Decision != SAINSoloDecision.Retreat && Decision != SAINSoloDecision.RunToCover)
            {
                SAIN.Mover.Sprint(false);
            }
        }

        private readonly SAINComponent SAIN;
    }
}