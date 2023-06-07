using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.Layers.Logic;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class StandAndShootAction : CustomLogic
    {
        public StandAndShootAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            this.AimData = new GClass105(bot);
        }

        private CoverStatus CurrentCoverPointStatus;
        private CoverStatus CurrentFallBackStatus;

        private GClass105 AimData;

        public override void Update()
        {
            SAIN.Steering.ManualUpdate();

            if (SAIN.HasEnemyAndCanShoot || (SAIN.EnemyIsVisible && SAIN.EnemyCanShoot))
            {
                AimData.Update();
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

        public override void Start()
        {
            BotOwner.StopMove();

            if (SAIN.Cover.CurrentCoverPoint != null)
            {
                CurrentCoverPointStatus = SAIN.Cover.CurrentCoverPoint.Status();
            }
            if (SAIN.Cover.CurrentFallBackPoint != null)
            {
                CurrentFallBackStatus = SAIN.Cover.CurrentFallBackPoint.Status();
            }
        }

        private Vector3 PositionToHold;
        private Vector3? RightMovePos;
        private Vector3? LeftMovePos;

        public override void Stop()
        {
        }
    }
}