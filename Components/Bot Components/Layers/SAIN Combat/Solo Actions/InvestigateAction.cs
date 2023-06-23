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
            Shoot = new ShootClass(bot, SAIN);
        }

        private readonly ShootClass Shoot;

        public override void Update()
        {
            BotOwner.SearchData.UpdateByNode();
            //SAIN.Steering.SteerByPriority();
            Shoot.Update();
        }

        private Vector3 MovePos;

        private readonly SAINComponent SAIN;

        public ManualLogSource Logger;

        public override void Start()
        {
        }

        public override void Stop()
        {
        }
    }
}