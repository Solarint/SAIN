using BepInEx.Logging;
using EFT;
using SAIN.Components;
using SAIN_Helpers;
using UnityEngine;
using static SAIN.Helpers.Corners;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers.Logic
{
    public class UpdateSteering
    {
        public UpdateSteering(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = bot;
            SAIN = bot.GetComponent<SAINCore>();
        }

        private SAINCore SAIN;
        private bool DebugMode => DebugUpdateSteering.Value;

        public void ManualUpdate()
        {
            if (CanShootEnemy)
            {
                //SetSprint(false);
                //Vector3 target = BotOwner.Memory.GoalEnemy.CurrPosition;
                //target.y += 0.75f;
                //BotOwner.Steering.LookToPoint(target);
                return;
            }

            bool randomSprint = SAIN_Math.RandomBool(15f);

            if (SprintTimer < Time.time)
            {
                SprintTimer = Time.time + 3f;
                SetSprint(randomSprint);
            }

            var corner = NextCornerPosition();

            BotOwner.Steering.LookToPoint(corner);
        }

        private float SprintTimer = 0f;

        private Vector3 NextCornerPosition()
        {
            Vector3[] corners = Processing.GetCorners(BotOwner.Transform.position, BotOwner.Memory.GoalEnemy.CurrPosition, BotOwner.LookSensor._headPoint, true, false, true, true);
            if (corners.Length > 0)
            {
                Vector3 corner = corners[1];

                if (Vector3.Distance(corners[0], corner) < 1f)
                {
                    return BotOwner.Memory.GoalEnemy.CurrPosition;
                }

                corner.y = BotOwner.LookSensor._headPoint.y;

                if (DebugMode && DebugTimer < Time.time)
                {
                    DebugTimer = Time.time + 1f;
                    SAIN_Helpers.DebugDrawer.Sphere(corner, 0.25f, Color.red, 1f);
                    SAIN_Helpers.DebugDrawer.Line(corner, BotOwner.MyHead.position, 0.1f, Color.red, 1f);
                }

                return corner;
            }
            else
            {
                return BotOwner.Memory.GoalEnemy.CurrPosition;
            }
        }

        private void SetSprint(bool value)
        {
            BotOwner.SetPose(1f);
            BotOwner.SetTargetMoveSpeed(1f);
            BotOwner.GetPlayer.EnableSprint(value);
            BotOwner.Sprint(value);
        }

        private bool CanShootEnemy => BotOwner.Memory.GoalEnemy.IsVisible;
        private bool CanSeeEnemy => BotOwner.Memory.GoalEnemy.CanShoot;
        private bool GoalEnemyNull => BotOwner.Memory.GoalEnemy == null;

        private readonly BotOwner BotOwner;
        protected ManualLogSource Logger;
        private float DebugTimer = 0f;
        private readonly CornerProcessing Processing = new CornerProcessing();
    }
}