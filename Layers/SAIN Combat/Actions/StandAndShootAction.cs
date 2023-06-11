using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
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
            Shoot = new GClass105(bot);
        }

        private CoverStatus CurrentCoverPointStatus;
        private CoverStatus CurrentFallBackStatus;

        private GClass105 Shoot;

        public override void Update()
        {
            SAIN.Steering.ManualUpdate();

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
                if (CurrentCoverPointStatus != CoverStatus.InCover && CurrentCoverPointStatus != CoverStatus.CloseToCover && CurrentFallBackStatus != CoverStatus.InCover && CurrentFallBackStatus != CoverStatus.CloseToCover)
                {
                    if (BotOwner.BotLay.CanProne && (BotOwner.Position - BotOwner.Memory.GoalEnemy.CurrPosition).magnitude > 20f)
                    {
                        BotOwner.BotLay.TryLay();
                    }
                    else
                    {
                        BotOwner.SetPose(0f);
                    }
                }
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
            CurrentCoverPointStatus = SAIN.Cover.CoverPointStatus;
            CurrentFallBackStatus = SAIN.Cover.FallBackPointStatus;
        }

        public override void Stop()
        {
        }
    }
}