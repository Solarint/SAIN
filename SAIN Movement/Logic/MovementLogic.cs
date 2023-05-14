using BepInEx.Logging;
using EFT;
using SAIN.Components;

namespace SAIN.Layers.Logic
{
    public class MovementLogic
    {
        public MovementLogic(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = bot;
            SAIN = bot.GetComponent<SAINCore>();
        }

        private readonly SAINCore SAIN;

        private readonly BotOwner BotOwner;
        private readonly ManualLogSource Logger;

        public void DecideMovementSpeed()
        {
            if (BotOwner.Memory.GoalEnemy == null)
            {
                SlowWalk();
                return;
            }

            if (SAIN.Enemy.Path.RangeVeryClose)
            {
                FullSpeed();
            }
            else if (SAIN.Enemy.Path.RangeClose)
            {
                NormalSpeed();
            }
            else if (ShouldBotSneak)
            {
                Sneak();
            }
            else
            {
                SlowWalk();
            }
        }

        public void SetSprint(bool value)
        {
            if (value)
            {
                BotOwner.SetPose(1f);
                BotOwner.SetTargetMoveSpeed(1f);
            }
            BotOwner.GetPlayer.EnableSprint(value);
            BotOwner.Sprint(value);
        }

        private void FullSpeed()
        {
            BotOwner.SetTargetMoveSpeed(1f);
        }

        private void Sneak()
        {
            BotOwner.SetPose(0f);
            BotOwner.SetTargetMoveSpeed(0f);
        }

        private void SlowWalk()
        {
            BotOwner.SetTargetMoveSpeed(0.45f);
        }

        private void NormalSpeed()
        {
            BotOwner.SetTargetMoveSpeed(0.85f);
        }

        private bool ShouldBotSneak
        {
            get
            {
                if (BotOwner.Memory.GoalEnemy == null)
                {
                    return true;
                }
                else
                {
                    return !SAIN.Enemy.CanSee && !SAIN.Enemy.CanShoot && SAIN.Enemy.LastSeen.TimeSinceSeen > 10f;
                }
            }
        }
    }
}