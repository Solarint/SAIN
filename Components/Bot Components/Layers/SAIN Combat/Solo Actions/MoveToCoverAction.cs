using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.Classes;
using SAIN.Classes.CombatFunctions;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Layers
{
    internal class MoveToCoverAction : CustomLogic
    {
        public MoveToCoverAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            Shoot = new ShootClass(bot);
            NavigationPoint = new NavigationPointObject(bot);
        }

        private readonly ManualLogSource Logger;
        private readonly NavigationPointObject NavigationPoint;

        public override void Update()
        {
            if (SAIN.Cover.DuckInCover())
            {
                SAIN.Mover.StopMove();
                SAIN.Mover.Sprint(false);
                return;
            }

            NavigationPoint.Update(1f, 1f);
            bool isProne = SAIN.Player.MovementContext.IsInPronePose;

            var target = TargetType;
            if (target != null)
            {
                if (Decision != SAINSoloDecision.Retreat)
                {
                    if (SAIN.Mover.ShallProne(target, true) || isProne)
                    {
                        SAIN.Mover.SetBotProne(true);
                        EngageEnemy();
                        return;
                    }
                    Move(target);
                    CheckSprint();
                    return;
                }

                if (UpdateCoverTimer < Time.time && (DestinationPosition - target.Position).sqrMagnitude > 1f)
                {
                    if (MoveToCoverPoint(target))
                    {
                        UpdateCoverTimer = Time.time + 0.33f;
                    }
                }
                Move(target);
            }

            CheckSprint();
        }

        private void Move(CoverPoint target)
        {
            if (UpdateCoverTimer < Time.time)
            {
                if (MoveToCoverPoint(target))
                {
                    UpdateCoverTimer = Time.time + 0.33f;
                }
            }
        }

        private void CheckSprint()
        {
            if (Sprint && SAIN.Mover.CanSprint)
            {
                float distance = (DestinationPosition - SAIN.Position).magnitude;
                if (distance < 0.5f)
                {
                    FarFromCover = false;
                }
                else if (distance > 1f)
                {
                    FarFromCover = true;
                }
                SAIN.Mover.Sprint(FarFromCover);
                if (!FarFromCover)
                {
                    EngageEnemy();
                }
            }
            else
            {
                SAIN.Mover.Sprint(false);
                EngageEnemy();
            }
        }

        private CoverPoint TargetType
        {
            get
            {
                CoverPoint result;
                var cover = SAIN.Cover;
                switch (Decision)
                {
                    case SAINSoloDecision.RunForCover:
                        result = cover.ClosestPoint;
                        break;

                    case SAINSoloDecision.MoveToCover:
                        result = cover.ClosestPoint;
                        break;

                    case SAINSoloDecision.Retreat:
                        result = cover.CurrentFallBackPoint ?? cover.ClosestPoint;
                        break;

                    default:
                        result = cover.ClosestPoint;
                        break;
                }
                return result;
            }
        }

        private float UpdateCoverTimer = 0f;

        private bool MoveToCoverPoint(CoverPoint point)
        {
            if (point != null && GoToPoint(point.Position, -1f))
            {
                DestinationPosition = point.Position;
                SAIN.Mover.SetTargetMoveSpeed(1f);
                SAIN.Mover.SetTargetPose(1f);
                BotOwner.DoorOpener.Update();
                return true;
            }
            return false;
        }

        private Vector3 DestinationPosition;

        private void EngageEnemy()
        {
            SAIN.Steering.SteerByPriority(false);
            Shoot.Update();
        }

        private readonly ShootClass Shoot;

        private SAINSoloDecision Decision => SAIN.CurrentDecision;
        private bool Sprint => Decision == SAINSoloDecision.RunForCover || NormalRetreat || GrenadeClose;
        private bool GrenadeClose => GrenadeDanger != null && (GrenadeDanger.Value - BotOwner.Position).sqrMagnitude < 100f && SAIN.Decision.SelfDecision == SAINSelfDecision.RunAwayGrenade;
        private Vector3? GrenadeDanger => SAIN.Grenade.GrenadeDangerPoint;
        private bool NormalRetreat => Decision == SAINSoloDecision.Retreat && SAIN.Decision.SelfDecision != SAINSelfDecision.RunAwayGrenade;

        public bool GoToPoint(Vector3 point, float reachDist = -1f)
        {
            if (NavMesh.SamplePosition(point, out var navHit, 0.1f, -1))
            {
                NavMeshPath Path = new NavMeshPath();
                if (NavMesh.CalculatePath(SAIN.Position, navHit.position, -1, Path) && Path.corners.Length > 1)
                {
                    if (reachDist < 0f)
                    {
                        reachDist = BotOwner.Settings.FileSettings.Move.REACH_DIST;
                    }
                    //BotOwner.Mover.GoToByWay(Path.corners, reachDist, Vector3.zero);
                    BotOwner.Mover.GoToPoint(navHit.position, false, reachDist, false, false, false);
                    return true;
                }
            }

            Logger.LogWarning($"{NavMeshPathStatus.PathInvalid}");
            return false;
        }

        private bool FarFromCover;

        public override void Start()
        {
        }

        public override void Stop()
        {
            SAIN.Mover.Sprint(false);
        }

        private readonly SAINComponent SAIN;
    }
}