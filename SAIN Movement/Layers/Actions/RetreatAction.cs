using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.Layers.Logic;
using UnityEngine;
using UnityEngine.UIElements;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class RetreatAction : CustomLogic
    {
        public RetreatAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
        }

        public override void Update()
        {
            BotOwner.Steering.LookToMovingDirection();
            SAIN.MovementLogic.SetSprint(true);
            UpdateDoorOpener();
        }

        private void UpdateDoorOpener()
        {
            BotOwner.DoorOpener.Update();
        }

        private readonly SAINComponent SAIN;

        public override void Start()
        {
            BotOwner.PatrollingData.Pause();
        }

        public override void Stop()
        {
            BotOwner.PatrollingData.Unpause();
        }

        public ManualLogSource Logger;
    }
}