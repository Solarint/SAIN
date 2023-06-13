
using EFT;
using SAIN.Helpers;
using UnityEngine.UIElements;

namespace SAIN.Classes
{
    public class MoveToCoverObject : SAINBot
    {
        public MoveToCoverObject(BotOwner owner) : base(owner) 
        {
            Shoot = new GClass105(BotOwner);
        }

        private bool ShallSprint;
        public bool CloseToCover { get; private set; }

        public bool MoveToCoverPoint(CoverPoint point, bool shallSprint, float targetPose = 1f, float targetSpeed = 1f, bool slowAtEnd = false, float reachDist = -1)
        {
            ShallSprint = shallSprint;
            bool close = false;
            if (point != null)
            {
                if (CoverDestination == null || SAIN.BotStuck.BotIsStuck || point != CoverDestination)
                {
                    CoverDestination = point;
                    BotOwner.GoToPoint(point.Position, false, reachDist, false, false);
                    BotOwner.SetTargetMoveSpeed(targetSpeed);
                    BotOwner.SetPose(targetPose);
                    BotOwner.DoorOpener.Update();
                }
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
    }
}
