using EFT;
using SAIN.Classes.CombatFunctions;
using UnityEngine;

namespace SAIN.Classes
{
    public class MoveToCoverClass : SAINBot
    {
        public MoveToCoverClass(BotOwner owner) : base(owner)
        {
            Shoot = new ShootClass(owner);
            NavigationPoint = new NavigationPointObject(owner);
        }

        public NavigationPointObject NavigationPoint { get; private set; }
        public CoverPoint TargetType
        {
            get
            {
                CoverPoint result;
                var cover = SAIN.Cover;
                switch (CurrentDecision)
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

        private bool ShallSprint;

        public bool CloseToCover { get; private set; }

        public void Update(bool sprint)
        {
            NavigationPoint.Update();
            ShallSprint = sprint;
            var target = TargetType;
            if (target != null && UpdateCoverTimer < Time.time && (DestinationPosition - target.Position).sqrMagnitude > 1f)
            {
                UpdateCoverTimer = Time.time + 0.5f;
                MoveToCoverPoint(target, ShallSprint);
            }
            if (ShallSprint)
            {
                float distance = (DestinationPosition - BotPosition).magnitude;
                if (distance < 0.5f)
                {
                    CloseToCover = true;
                }
                else if (distance > 1f)
                {
                    CloseToCover = false;
                }
                ToggleSprint(!CloseToCover);
            }
            else
            {
                EngageEnemy();
            }
        }

        private float UpdateCoverTimer = 0f;

        public bool MoveToCoverPoint(CoverPoint point, bool shallSprint, float targetPose = 1f, float targetSpeed = 1f, bool slowAtEnd = false, float reachDist = -1)
        {
            ShallSprint = shallSprint;
            if (point != null)
            {
                System.Console.WriteLine("Moving to cover");
                CoverDestination = point;
                DestinationPosition = point.Position;
                NavigationPoint.GoToPoint(point.Position, -1f);
                SAIN.Mover.SetTargetMoveSpeed(targetSpeed);
                SAIN.Mover.SetTargetPose(targetPose);
                BotOwner.DoorOpener.Update();
            }
            return CoverDestination != null;
        }

        public Vector3 DestinationPosition { get; private set; }

        public void EngageEnemy()
        {
            SAIN.Steering.Steer(false);
            Shoot.Update();
        }

        public void ToggleSprint(bool value)
        {
            if (value)
            {
                BotOwner.Steering.LookToMovingDirection();
                Player.MovementContext.EnableSprint(true);
            }
            else
            {
                Player.MovementContext.EnableSprint(false);
                EngageEnemy();
            }
        }

        public float? Distance => CoverDestination?.Distance;

        public CoverPoint CoverDestination { get; private set; }

        private readonly ShootClass Shoot;
    }
}