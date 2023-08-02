using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
using SAIN.Layers;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

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

        private readonly SAINComponent SAIN;

        public ManualLogSource Logger;

        private CoverPoint CoverInUse;

        public override void Start()
        {
            CoverInUse = SAIN.Cover.CoverInUse;
            //SAIN.Mover.Sprint(false);
        }

        public override void Stop()
        {
        }

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"SAIN Info:");
            stringBuilder.AppendLabeledValue("Personality", $"{SAIN.Info.Personality}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("BotType", $"{SAIN.Info.WildSpawnType}", Color.white, Color.yellow, true);
            CoverPoint cover = CoverInUse;
            if (cover != null)
            {
                stringBuilder.AppendLine($"SAIN Cover Info:");
                stringBuilder.AppendLabeledValue("Cover Position", $"{cover.Position}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover Distance", $"{cover.Distance}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover Spotted?", $"{cover.Spotted}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover Path Length", $"{cover.Distance}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover ID", $"{cover.Id}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover Status", $"{cover.CoverStatus}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover HitInCoverCount", $"{cover.HitInCoverCount}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover HitInCoverUnknownCount", $"{cover.HitInCoverUnknownCount}", Color.white, Color.yellow, true);
            }
        }
    }
}