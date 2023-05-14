using BepInEx.Logging;
using EFT;
using SAIN.Components;
using SAIN.Helpers;
using UnityEngine;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers.Logic
{
    public class UpdateMove
    {
        public UpdateMove(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = bot;
            Cover = new CoverLogic(bot);
            Dodge = new BotDodge(bot);
            MovementSpeed = new MovementLogic(bot);
            SAIN = bot.GetComponent<SAINCore>();
        }

        private SAINCore SAIN;

        private readonly MovementLogic MovementSpeed; 
        private readonly BotDodge Dodge;
        private readonly CoverLogic Cover;
        private const float UpdateFrequency = 0.1f;

        public void ManualUpdate()
        {
            if (ReactionTimer < Time.time)
            {
                UpdateDoorOpener();

                ReactionTimer = Time.time + UpdateFrequency;

                if (BotOwner.Memory.GoalEnemy != null)
                {
                    MovementSpeed.DecideMovementSpeed();

                    if (Cover.TakeCover())
                    {
                        return;
                    }

                    MovementSpeed.SetSprint(false);

                    BotOwner.MoveToEnemyData.TryMoveToEnemy(BotOwner.Memory.GoalEnemy.CurrPosition);
                }
            }
        }

        private void UpdateDoorOpener()
        {
            BotOwner.DoorOpener.Update();
        }

        private bool DebugMode => DebugUpdateMove.Value;
        public LeanComponent DynamicLean { get; private set; }

        private float ReactionTimer = 0f;
        private readonly BotOwner BotOwner;
        private readonly ManualLogSource Logger;
    }
}