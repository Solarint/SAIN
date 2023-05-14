using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.Layers.Logic;
using UnityEngine;
using UnityEngine.UIElements;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class FallBackLogic : CustomLogic
    {
        public FallBackLogic(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINBotComponent>();
        }

        private SAINBotComponent SAIN;

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
            if (SAIN.Decisions.ShouldBotPopStims)
            {
                SAIN.Decisions.BotUseStims();
            }
            else if (SAIN.Decisions.ShouldBotHeal)
            {
                SAIN.Decisions.BotHeal();
            }
            else if (SAIN.Decisions.ShouldBotReload)
            {
                SAIN.Decisions.BotReload();
            }

            var coverPoint = CoverFinderComponent.FallBackPoint;

            if (coverPoint != null)
            {
                BotOwner.GoToPoint(coverPoint.CoverPosition, false);

                if (SAIN.Core.BotIsMoving)
                {
                    BotOwner.Steering.LookToMovingDirection();
                }
                else
                {
                    SAIN.Steering.ManualUpdate();

                    if (SAIN.Core.Enemy.CanSee)
                    {
                        SAIN.Targeting.ManualUpdate();
                        SAIN.Move.ManualUpdate();
                    }
                }
            }
            else
            {
                SAIN.Steering.ManualUpdate();

                if (SAIN.Core.Enemy.CanSee)
                {
                    SAIN.Targeting.ManualUpdate();
                }

                if (DodgeTimer < Time.time && (SAIN.Core.Enemy.Path.RangeClose || SAIN.Core.Enemy.Path.RangeVeryClose))
                {
                    DodgeTimer = Time.time + Random.Range(0.5f, 1f);
                    Dodge.Execute();
                }
                else
                {
                    SAIN.Move.ManualUpdate();
                }
            }

            UpdateDoorOpener();
        }

        private void UpdateDoorOpener()
        {
            BotOwner.DoorOpener.Update();
        }

        public CustomCoverPoint FallBackCoverPoint;
        private readonly BotDodge Dodge;
        public CoverFinderComponent CoverFinderComponent { get; private set; }
        public bool DebugMode => DebugLayers.Value;

        private float DodgeTimer = 0f;
        public ManualLogSource Logger;
    }
}