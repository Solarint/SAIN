using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Layers
{
    public class StandAndShoot : CustomLogic
    {
        public StandAndShoot(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            Shoot = new ShootClass(bot, SAIN);
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

            Shoot.Update();

            if (SAIN.Cover.BotIsAtCoverPoint())
            {
                return;
            }
            else
            {
                bool prone = SAIN.Mover.Prone.ShallProne(true);
                SAIN.Mover.Prone.SetProne(prone);
            }
        }

        private readonly SAINComponent SAIN;

        public ManualLogSource Logger;
        private float StartTime = 0f;
        private bool Stopped = false;

        public override void Start()
        {
            SAIN.Mover.Sprint(false);
            StartTime = Time.time;
        }

        public override void Stop()
        {
        }
    }
}