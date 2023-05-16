using BepInEx.Logging;
using EFT;
using SAIN.Components;
using SAIN_Helpers;
using UnityEngine;
using static SAIN.Helpers.Corners;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers.Logic
{
    public class UpdateSteering : SAINBotExt
    {
        public UpdateSteering(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
        }

        private bool DebugMode => DebugUpdateSteering.Value;

        public void ManualUpdate()
        {
            SAIN.BotOwner.Memory.GoalEnemy.Person.MainParts.TryGetValue(BodyPartType.body, out BodyPartClass enemyChest);
            EnemyChest = enemyChest.Position;

            if (SAIN.Core.Enemy.CanSee)
            {
                BotOwner.Steering.LookToPoint(EnemyChest);
                return;
            }

            bool randomSprint = SAIN_Math.RandomBool(15f);

            if (SprintTimer < Time.time)
            {
                SprintTimer = Time.time + 3f;
                SAIN.MovementLogic.SetSprint(randomSprint);
            }

            BotOwner.Steering.LookToPoint(NextCornerPosition());
        }

        private Vector3 EnemyChest;

        private float SprintTimer = 0f;

        private Vector3 NextCornerPosition()
        {
            Vector3[] corners = Processing.GetCorners(BotOwner.Transform.position, BotOwner.Memory.GoalEnemy.CurrPosition, BotOwner.LookSensor._headPoint, true, false, true, true);
            if (corners.Length > 0)
            {
                Vector3 corner = corners[1];

                if (Vector3.Distance(corners[0], corner) < 1f)
                {
                    return EnemyChest;
                }

                corner.y = BotOwner.LookSensor._headPoint.y;

                if (DebugMode && DebugTimer < Time.time)
                {
                    DebugTimer = Time.time + 1f;
                    DebugDrawer.Sphere(corner, 0.25f, Color.red, 1f);
                    DebugDrawer.Line(corner, BotOwner.MyHead.position, 0.1f, Color.red, 1f);
                }

                return corner;
            }
            else
            {
                return EnemyChest;
            }
        }

        protected ManualLogSource Logger;
        private float DebugTimer = 0f;
        private readonly CornerProcessing Processing = new CornerProcessing();
    }
}