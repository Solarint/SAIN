using BepInEx.Logging;
using EFT;

namespace SAIN.Classes.Mover
{
    public class SAIN_Mover_SideStep : SAINBot
    {
        public SideStepSetting CurrentSideStep { get; private set; }

        public SAIN_Mover_SideStep(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        public void Update()
        {
            if (!SAIN.BotActive)
            {
                return;
            }

            if (BotOwner.Memory.GoalEnemy == null)
            {
                return;
            }

            var lean = SAIN.Mover.Lean.LeanDirection;
            var move = BotOwner.GetPlayer.MovementContext;
            var enemy = BotOwner.Memory.GoalEnemy;

            float value = 0f;
            SideStepSetting setting = SideStepSetting.None;

            if (!enemy.CanShoot && !enemy.IsVisible)
            {
                switch (lean)
                {
                    case LeanSetting.Left:
                        value = -1f;
                        setting = SideStepSetting.Left;
                        break;

                    case LeanSetting.Right:
                        value = 1f;
                        setting = SideStepSetting.Right;
                        break;

                    default:
                        break;
                }
            }

            SetSideStep(value);

            CurrentSideStep = setting;
        }

        public void SetSideStep(float value)
        {
            BotOwner.GetPlayer.MovementContext.SetSidestep(value);
        }

        private readonly ManualLogSource Logger;
    }
}