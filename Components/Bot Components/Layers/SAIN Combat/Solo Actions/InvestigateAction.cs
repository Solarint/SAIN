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
    internal class InvestigateAction : CustomLogic
    {
        public InvestigateAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            Shoot = new ShootClass(bot);
        }

        private readonly ShootClass Shoot;

        public override void Update()
        {
            Sound = BotOwner.BotsGroup.YoungestFastPlace(BotOwner, 50f, 10f);
            if (SAIN.Enemy == null)
            {
                if (Sound != null)
                {
                    SAIN.Steering.SteerByPriority();
                    MovePos = Sound.Position;
                    SAIN.Mover.GoToPoint(MovePos, false, false);
                }
                else
                {
                    Sound = BotOwner.BotsGroup.YoungestPlace(BotOwner, 200f, false);
                    SAIN.Steering.LookToRandomPosition();
                }
            }
            else
            {
                SAIN.Steering.SteerByPriority();
                Shoot.Update();
                if (SAIN.CurrentTargetPosition != null)
                {
                    SAIN.Mover.GoToPoint(SAIN.CurrentTargetPosition.Value, false, false);
                }
            }
        }

        private Vector3 MovePos;

        private readonly SAINComponent SAIN;
        public bool DebugMode => DebugLayers.Value;

        public ManualLogSource Logger;

        public override void Start()
        {
        }

        private GClass270 Sound;

        public override void Stop()
        {
        }
    }
}