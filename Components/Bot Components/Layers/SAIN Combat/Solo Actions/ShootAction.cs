using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
using SAIN.Layers;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class ShootAction : CustomLogic
    {
        public ShootAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            Shoot = new ShootClass(bot, SAIN);
        }

        private ShootClass Shoot;

        private SAINComponent SAIN;

        public override void Start()
        {
        }

        public override void Stop()
        {
        }

        public override void Update()
        {
            SAIN.Steering.SteerByPriority();
            Shoot.Update();
        }

        public bool DebugMode => DebugLayers.Value;

        public ManualLogSource Logger;
    }
}