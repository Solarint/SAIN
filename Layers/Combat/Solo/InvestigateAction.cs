using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;
using SAIN.SAINComponent;
using UnityEngine;
using UnityEngine.AI;
using SAIN.Helpers;

namespace SAIN.Layers.Combat.Solo
{
    internal class InvestigateAction : SAINAction
    {
        public InvestigateAction(BotOwner bot) : base(bot, nameof(InvestigateAction))
        {
        }

        public override void Update()
        {
            if (SAIN.Enemy?.IsVisible == false && SAIN.Decision.SelfActionDecisions.LowOnAmmo(0.66f))
            {
                SAIN.SelfActions.TryReload();
            }

            SAIN.Mover.SetTargetMoveSpeed(0.7f);
            SAIN.Mover.SetTargetPose(1f);

            Shoot.Update();

            if (SearchPoint == null)
            {
                SAIN.Decision.ResetDecisions();
                return;
            }

            SAIN.Steering.SteerByPriority();

            if (RecalcPathtimer < Time.time)
            {
                RecalcPathtimer = Time.time + 4f;
                SAIN.Mover.GoToPoint(SearchPoint.Point.Position);
            }
            if ((BotOwner.Position - SearchPoint.Point.Position).sqrMagnitude < 10f)
            {
                Vector3 headPos = SAIN.Transform.Head;
                Vector3 searchPoint = SearchPoint.Point.Position;
                Vector3 direction = searchPoint - headPos;
                if (!Physics.SphereCast(headPos, 0.1f, direction, out var hit, LayerMaskClass.HighPolyWithTerrainMaskAI))
                {
                    SearchPoint.Point.IsCome = true;
                    SearchPoint.Point = null;
                    return;
                }
            }
        }

        private float RecalcPathtimer = 0f;

        private readonly SearchPoint SearchPoint = new SearchPoint();

        public override void Start()
        {
            SearchPoint.Point = BotOwner.BotsGroup.YoungestFastPlace(BotOwner, 200f, 60f);
        }

        public override void Stop()
        {
        }
    }
}