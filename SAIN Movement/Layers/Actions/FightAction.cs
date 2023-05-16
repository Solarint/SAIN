using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.Layers.Logic;
using SAIN_Helpers;
using UnityEngine;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class FightAction : CustomLogic
    {
        public FightAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
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

        public override void Update()
        {
            //SAIN.Move.ManualUpdate();

            SAIN.Steering.ManualUpdate();

            if (SAIN.Core.Enemy.CanSee)
            {
                SAIN.Targeting.ManualUpdate();
            }
        }

        public ManualLogSource Logger;
    }
}