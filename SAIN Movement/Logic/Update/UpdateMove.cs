using BepInEx.Logging;
using EFT;
using SAIN.Components;
using UnityEngine;
using SAIN_Helpers;

namespace SAIN.Layers.Logic
{
    public class UpdateMove : SAINBotExt
    {
        public UpdateMove(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
        }

        public void ManualUpdate()
        {
            bool randomSprint = SAIN_Math.RandomBool(15f);

            if (SprintTimer < Time.time)
            {
                SprintTimer = Time.time + 3f;
                SAIN.MovementLogic.SetSprint(randomSprint);
            }
            else
            {
                SAIN.MovementLogic.DecideMovementSpeed();
            }

            //SAIN.MovementLogic.SetSprint(false);

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