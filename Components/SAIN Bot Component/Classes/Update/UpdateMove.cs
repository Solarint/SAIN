using BepInEx.Logging;
using EFT;
using SAIN.Components;
using UnityEngine;
using SAIN.Helpers;

namespace SAIN.Layers.Logic
{
    public class UpdateMove : SAINBot
    {
        public UpdateMove(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
        }

        public void ManualUpdate()
        {
            bool randomSprint = EFT_Math.RandomBool(15f);

            if (SprintTimer < Time.time)
            {
                SprintTimer = Time.time + 3f;
                SAIN.Movement.SetSprint(randomSprint);
            }
            else
            {
                SAIN.Movement.DecideMovementSpeed();
            }

            //SAIN.MovementClass.FallBack(false);

            //BotOwner.MoveToEnemyData.TryMoveToEnemy(BotOwner.Memory.GoalEnemy.CurrPosition);
        }

        private void UpdateDoorOpener()
        {
            BotOwner.DoorOpener.Update();
        }

        private float SprintTimer = 0f;
        private readonly ManualLogSource Logger;
    }
}