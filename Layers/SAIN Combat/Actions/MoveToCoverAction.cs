using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.Classes;
using UnityEngine;

namespace SAIN.Layers
{
    internal class MoveToCoverAction : CustomLogic
    {
        public MoveToCoverAction(BotOwner bot) : base(bot)
        {
            SAIN = bot.GetComponent<SAINComponent>();
            MoveToCover = new MoveToCoverObject(BotOwner);
            Shoot = new GClass105(BotOwner);
        }

        private SAINSoloDecision Decision => SAIN.CurrentDecision;
        private bool Sprint => Decision == SAINSoloDecision.RunForCover || Decision == SAINSoloDecision.Retreat;

        public override void Update()
        {
            if (SAIN.Cover.DuckInCover())
            {
                MoveToCover.ToggleSprint(false);
                return;
            }
            BotOwner.SetTargetMoveSpeed(1f);
            BotOwner.SetPose(1f);
            MoveToCover.MoveToCoverPoint(SAIN.Cover.ClosestPoint, Sprint);
        }

        private bool ShallSprint;
        public bool CloseToCover { get; private set; }

        public bool MoveToCoverPoint(CoverPoint point, float targetPose = 1f, float targetSpeed = 1f)
        {
            float reachDist = Sprint ? 0.75f : -1f;
            bool close = false;
            if (point != null)
            {
                CoverDestination = point;
                BotOwner.GoToPoint(point.Position, false, reachDist, false, false);
                BotOwner.SetTargetMoveSpeed(targetSpeed);
                BotOwner.SetPose(targetPose);
                BotOwner.DoorOpener.Update();
            }

            if (CoverDestination != null)
            {
                float distance = CoverDestination.Distance;
                if (distance < 0.5f)
                {
                    close = true;
                    ToggleSprint(false);
                }
                else if (distance > 1f)
                {
                    close = false;
                    ToggleSprint(true);
                }
            }
            else
            {
                EngageEnemy();
            }
            CloseToCover = close;
            return CoverDestination != null;
        }

        private void AdjustMoveSpeed()
        {
            if (CoverDestination != null)
            {
                float distance = CoverDestination.Distance;
                if (distance < 0.5f)
                {
                    CloseToCover = true;
                    ToggleSprint(false);
                }
                else if (distance > 1f)
                {
                    CloseToCover = false;
                    ToggleSprint(true);
                }
            }
            else
            {
                EngageEnemy();
            }
        }

        public static float SqrDist(Vector3 direction)
        {
            if (direction == Vector3.zero)
            {
                return 0f;
            }
            return direction.sqrMagnitude;
        }

        public static Vector3 CoverDirection(CoverPoint point, Vector3 origin)
        {
            if (point == null)
            {
                return Vector3.zero;
            }
            return point.Position - origin;
        }

        public static float CoverSqrDistance(CoverPoint point, Vector3 origin)
        {
            if (point == null)
            {
                return Mathf.Infinity;
            }
            return SqrDist(CoverDirection(point, origin));
        }

        public void EngageEnemy()
        {
            SAIN.Steering.ManualUpdate();
            if (SAIN.Enemy?.IsVisible == true)
            {
                Shoot.Update();
            }
        }

        public void ToggleSprint(bool value)
        {
            if (BotOwner.GetPlayer.MovementContext.IsSprintEnabled != value)
            {
                BotOwner.GetPlayer.EnableSprint(value);
            }
            if (value && SAIN.BotHasStamina)
            {
                BotOwner.SetTargetMoveSpeed(1f);
                BotOwner.SetPose(1f);
                BotOwner.Steering.LookToMovingDirection();
            }
            else
            {
                EngageEnemy();
            }
        }

        public float? Distance => CoverDestination?.Distance;
        public CoverPoint CoverDestination { get; private set; }

        private GClass105 Shoot;

        public override void Start()
        {
            BotOwner.SetTargetMoveSpeed(1f);
            BotOwner.SetPose(1f);
            MoveToCover.MoveToCoverPoint(SAIN.Cover.ClosestPoint, Sprint);
        }

        public override void Stop()
        {
        }

        private MoveToCoverObject MoveToCover;
        private readonly SAINComponent SAIN;
    }
}