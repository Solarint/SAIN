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
    internal class RetreatAction : CustomLogic
    {
        public RetreatAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
        }

        public override void Update()
        {
            var sain = SAIN;
            var decide = sain.Decisions;
            var core = sain.Core;
            var coverFinder = sain.CoverFinder;

            if (decide.ShouldBotPopStims)
            {
                decide.BotUseStims();
            }
            if (decide.ShouldBotHeal)
            {
                decide.BotHeal();
            }
            if (decide.ShouldBotReload)
            {
                decide.BotReload();
            }

            return;

            if (coverFinder.FallBackPoint != null)
            {
                BotOwner.GoToPoint(coverFinder.FallBackPoint.CoverPosition, false);
                BotOwner.Steering.LookToMovingDirection();
                SAIN.MovementLogic.SetSprint(true);
            }
            else
            {
                sain.Steering.ManualUpdate();

                var path = core.Enemy.Path;
                if (DodgeTimer < Time.time && (path.RangeClose || path.RangeVeryClose))
                {
                    DodgeTimer = Time.time + Random.Range(0.5f, 1f);
                    sain.Dodge.Execute();
                }
                else
                {
                    sain.Move.ManualUpdate();
                }
            }

            UpdateDoorOpener();
        }

        private void UpdateDoorOpener()
        {
            BotOwner.DoorOpener.Update();
        }

        private float DodgeTimer = 0f;

        private readonly SAINComponent SAIN;

        public override void Start()
        {
            BotOwner.PatrollingData.Pause();
        }

        public override void Stop()
        {
            BotOwner.PatrollingData.Unpause();
        }

        public ManualLogSource Logger;
    }
}