using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using UnityEngine;

namespace SAIN.Layers
{
    internal class SuppressAction : CustomLogic
    {
        public SuppressAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            AimData = new GClass105(bot);
        }

        public override void Update()
        {
            if (SAIN.HasEnemyAndCanShoot)
            {
                AimData.Update();
            }

            BotOwner.SuppressShoot.Init(BotOwner.Memory.GoalEnemy);
        }

        private readonly GClass105 AimData;

        private readonly SAINComponent SAIN;

        public override void Start()
        {
            BotOwner.PatrollingData.Pause();
        }

        public override void Stop()
        {
            BotOwner.PatrollingData.Unpause();
        }

        private ManualLogSource Logger;
    }
}