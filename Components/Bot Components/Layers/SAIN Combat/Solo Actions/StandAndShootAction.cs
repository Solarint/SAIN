using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    public class StandAndShootAction : CustomLogic
    {
        public StandAndShootAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            Shoot = new ShootClass(bot);
        }

        private readonly ShootClass Shoot;

        public override void Update()
        {
            SAIN.Steering.SteerByPriority();

            if ((!Stopped && Time.time - StartTime > 1f) || SAIN.Cover.CheckLimbsForCover())
            {
                Stopped = true;
                BotOwner.StopMove();
            }

            if (SAIN.Enemy?.IsVisible == true)
            {
                Shoot.Update();
            }

            if (SAIN.Cover.BotIsAtCoverPoint)
            {
                return;
            }
            else
            {
                bool prone = SAIN.Mover.ShallProne(true);
                SAIN.Mover.SetBotProne(prone);
            }
        }

        private readonly SAINComponent SAIN;
        public bool DebugMode => DebugLayers.Value;

        public ManualLogSource Logger;
        private float StartTime = 0f;
        private bool Stopped = false;

        public override void Start()
        {
            StartTime = Time.time;
        }

        public override void Stop()
        {
        }
    }
}