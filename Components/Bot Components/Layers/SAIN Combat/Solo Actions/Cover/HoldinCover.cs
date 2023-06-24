using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
using SAIN.Layers;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class HoldinCover : CustomLogic
    {
        public HoldinCover(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            Shoot = new ShootClass(bot, SAIN);
        }

        private ShootClass Shoot;
        private bool Stopped;

        public override void Update()
        {
            if (!Stopped && !CoverInUse.Spotted && (CoverInUse.Position - BotOwner.Position).sqrMagnitude < 0.33f)
            {
                SAIN.Mover.StopMove();
                Stopped = true;
            }

            SAIN.Steering.SteerByPriority();
            Shoot.Update();
            SAIN.Cover.DuckInCover();

            var enemy = SAIN.Enemy;
            if (enemy != null)
            {
                if (enemy.TimeSinceSeen > 5f)
                {
                    if (ResetSideStepTimer < Time.time)
                    {
                        ResetSideStepTimer = Time.time + 1.5f;
                        //SAIN.Lean.SideStep.SetSideStep(0f);
                    }
                    else if (NewSideStepTimer < Time.time)
                    {
                        NewSideStepTimer = Time.time + 3f;
                        SideStepRight = !SideStepRight;
                        if (SideStepRight)
                        {
                            //SAIN.Lean.SideStep.SetSideStep(1f);
                        }
                        else
                        {
                            //SAIN.Lean.SideStep.SetSideStep(-1f);
                        }
                    }
                }
            }
        }

        private bool SideStepRight;
        private float ResetSideStepTimer;
        private float NewSideStepTimer;

        private readonly SAINComponent SAIN;
        public bool DebugMode => DebugLayers.Value;

        public ManualLogSource Logger;

        private CoverPoint CoverInUse;

        public override void Start()
        {
            CoverInUse = SAIN.Cover.CoverInUse;
            SAIN.Mover.Sprint(false);
        }

        public override void Stop()
        {
        }
    }
}