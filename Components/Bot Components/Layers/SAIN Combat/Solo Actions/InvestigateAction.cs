using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class InvestigateAction : CustomLogic
    {
        public InvestigateAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            Shoot = new ShootClass(bot, SAIN);
        }

        private readonly ShootClass Shoot;

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
            if (!SAIN.Steering.SteerByPriority(false))
            {
                SAIN.Steering.LookToMovingDirection();
            }
            if (RecalcPathtimer < Time.time)
            {
                RecalcPathtimer = Time.time + 4f;
                SAIN.Mover.GoToPoint(SearchPoint.Position);
            }
            if ((BotOwner.Position - SearchPoint.Position).sqrMagnitude < 10f)
            {
                Vector3 searchPoint = SearchPoint.Position;
                Vector3 direction = searchPoint - SAIN.HeadPosition;
                if (!Physics.SphereCast(SAIN.HeadPosition, 0.1f, direction, out var hit, LayerMaskClass.HighPolyWithTerrainMaskAI))
                {
                    SearchPoint.IsCome = true;
                    SearchPoint = null;
                    return;
                }
            }
        }

        private float RecalcPathtimer = 0f;

        private readonly SAINComponent SAIN;

        public ManualLogSource Logger;
        private GClass273 SearchPoint;

        public override void Start()
        {
            SearchPoint = BotOwner.BotsGroup.YoungestFastPlace(BotOwner, 200f, 60f);
        }

        public override void Stop()
        {
        }
    }
}