using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;
using SAIN.SAINComponent;
using System.Text;
using UnityEngine;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using SAIN.Layers.Combat.Solo;

namespace SAIN.Layers.Combat.Solo.Cover
{
    internal class RunToCoverAction : SAINAction
    {
        public RunToCoverAction(BotOwner bot) : base(bot, nameof(RunToCoverAction))
        {
        }

        public override void Update()
        {
            SAIN.Mover.SetTargetMoveSpeed(1f);
            SAIN.Mover.SetTargetPose(1f);

            if (RecalcTimer < Time.time)
            {
                if (FindTargetCover())
                {
                    RecalcTimer = Time.time + 2f;
                    BotOwner.BotRun.Run(CoverDestination.Position, false, 0.6f);
                }
                else
                {
                    RecalcTimer = Time.time + 0.5f;
                }
            }
            if (CoverDestination == null || CoverDestination.BotIsHere)
            {
                EngageEnemy();
            }
        }

        private float RecalcTimer;

        private bool FindTargetCover()
        {
            if (CoverDestination != null)
            {
                CoverDestination.BotIsUsingThis = false;
                CoverDestination = null;
            }

            CoverPoint coverPoint = SelectPoint();
            if (coverPoint != null && !coverPoint.Spotted)
            {
                if (SAIN.Mover.CanGoToPoint(coverPoint.Position, out Vector3 pointToGo))
                {
                    coverPoint.Position = pointToGo;
                    coverPoint.BotIsUsingThis = true;
                    CoverDestination = coverPoint;
                    return true;
                }
            }
            return false;
        }

        private CoverPoint SelectPoint()
        {
            CoverPoint fallback = SAIN.Cover.FallBackPoint;
            if (SAIN.Memory.Decisions.Main.Current == SoloDecision.Retreat && fallback != null)
            {
                return fallback;
            }
            else
            {
                return SAIN.Cover.ClosestPoint;
            }
        }

        private CoverPoint CoverDestination;

        private void EngageEnemy()
        {
            SAIN.Steering.SteerByPriority();
            Shoot.Update();
        }

        public override void Start()
        {
            if (SAIN.Decision.CurrentSelfDecision == SelfDecision.RunAwayGrenade)
            {
                SAIN.Talk.Say(EPhraseTrigger.OnEnemyGrenade, ETagStatus.Combat);
            }
        }

        public override void Stop()
        {
            CoverDestination = null;
        }

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            DebugOverlay.AddCoverInfo(SAIN, stringBuilder);
        }
    }
}