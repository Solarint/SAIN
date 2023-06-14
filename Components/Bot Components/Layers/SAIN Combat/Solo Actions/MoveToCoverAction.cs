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
                BotOwner.Mover.Stop();
                ToggleSprint(false);
                return;
            }

            NavigationPoint.Update(1f, 1f);

            var target = TargetType;
            if (target != null && UpdateCoverTimer < Time.time && (DestinationPosition - target.Position).sqrMagnitude > 1f)
            {
                if (MoveToCoverPoint(target))
                {
                    UpdateCoverTimer = Time.time + 1f;
                }
            }

            if (Sprint)
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
                ToggleSprint(FarFromCover);
            }
            else
            {
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
            if (point != null)
            {
                if (GoToPoint(point.Position, -1f))
                {
                    System.Console.WriteLine("Moving to cover");
                    DestinationPosition = point.Position;
                    SAIN.Mover.SetTargetMoveSpeed(1f);
                    SAIN.Mover.SetTargetPose(1f);
                    //BotOwner.DoorOpener.Update();
                    return true;
                }
            }
            return false;
        }

        private Vector3 DestinationPosition;

        private void EngageEnemy()
        {
            SAIN.Steering.Steer(false);
            Shoot.Update();
        }

        private void ToggleSprint(bool value)
        {
            if (value)
            {
                BotOwner.Steering.LookToMovingDirection();
                SAIN.Player.MovementContext.EnableSprint(true);
            }
            else
            {
                SAIN.Player.EnableSprint(false);
                EngageEnemy();
            }
        }

        private readonly ShootClass Shoot;

        private SAINSoloDecision Decision => SAIN.CurrentDecision;
        private bool Sprint => Decision == SAINSoloDecision.RunForCover || Decision == SAINSoloDecision.Retreat;

        public bool GoToPoint(Vector3 point, float reachDist = -1f)
        {
            if (NavMesh.SamplePosition(point, out var navHit, 10f, -1))
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
            ToggleSprint(false);
        }

        private readonly SAINComponent SAIN;
    }
}