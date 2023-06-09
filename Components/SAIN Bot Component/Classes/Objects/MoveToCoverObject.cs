
using EFT;

namespace SAIN.Classes
{
    public class MoveToCoverObject
    {
        public MoveToCoverObject(BotOwner owner)
        {
            BotOwner = owner;
        }

        public bool MoveToCoverPoint(CoverPoint point, float pose = 1f, float speed = 1f, bool slowAtEnd = true, float reachDist = -1)
        {
            if (CoverDestination == null)
            {
                if (point != null)
                {
                    CoverDestination = point;
                    point.MoveToCover(pose, speed, slowAtEnd, reachDist);
                    BotOwner.DoorOpener.Update();
                }
            }

            return CoverDestination != null;
        }

        public void ToggleSprint(bool value)
        {
            if (BotOwner.GetPlayer.MovementContext.IsSprintEnabled != value)
            {
                BotOwner.GetPlayer.EnableSprint(value);
                if (value)
                {
                    BotOwner.Steering.LookToMovingDirection();
                }
            }
        }

        public float? Distance => CoverDestination?.Distance;
        public CoverPoint CoverDestination { get; private set; }

        private readonly BotOwner BotOwner;
    }
}
