using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;
using SAIN.SAINComponent;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using SAIN.Layers.Combat.Solo;

namespace SAIN.Layers.Combat.Solo.Cover
{
    internal class HoldinCoverAction : SAINAction
    {
        public HoldinCoverAction(BotOwner bot) : base(bot, nameof(HoldinCoverAction))
        {
        }

        private bool Stopped;

        public override void Update()
        {
            if (CoverInUse == null)
            {
                return;
            }

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
                        //SAIN.Lean.SideStepClass.SetSideStep(0f);
                    }
                    else if (NewSideStepTimer < Time.time)
                    {
                        NewSideStepTimer = Time.time + 3f;
                        SideStepRight = !SideStepRight;
                        if (SideStepRight)
                        {
                            //SAIN.Lean.SideStepClass.SetSideStep(1f);
                        }
                        else
                        {
                            //SAIN.Lean.SideStepClass.SetSideStep(-1f);
                        }
                    }
                }
            }
        }

        private bool SideStepRight;
        private float ResetSideStepTimer;
        private float NewSideStepTimer;

        private CoverPoint CoverInUse;

        public override void Start()
        {
            CoverInUse = SAIN.Cover.CoverInUse;
        }

        public override void Stop()
        {
        }

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            DebugOverlay.AddCoverInfo(SAIN, stringBuilder);
        }
    }
}