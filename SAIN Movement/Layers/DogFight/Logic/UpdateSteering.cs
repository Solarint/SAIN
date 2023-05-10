using BepInEx.Logging;
using EFT;
using Movement.Helpers;
using SAIN_Helpers;
using UnityEngine;
using UnityEngine.AI;
using static Movement.Helpers.Corners;
using static Movement.UserSettings.DebugConfig;
using static UnityEngine.UI.GridLayoutGroup;

namespace SAIN.Movement.Layers.DogFight
{
    internal class UpdateSteering
    {
        public UpdateSteering(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = bot;
        }
        private bool DebugMode => DebugUpdateSteering.Value;

        public void ManualUpdate()
        {
            if (GoalEnemyNull)
            {
                BotOwner.LookData.SetLookPointByHearing(null);
            }

            if (BotOwner.WeaponManager.Reload.Reloading || BotOwner.Medecine.FirstAid.Using || !BotOwner.WeaponManager.HaveBullets)
            {
                SetSprint(true);
                BotOwner.Steering.LookToMovingDirection();
                return;
            }

            if (CanShootEnemy)
            {
                SetSprint(false);
                Vector3 target = BotOwner.Memory.GoalEnemy.CurrPosition;
                target.y += 0.75f;
                BotOwner.Steering.LookToPoint(target);
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
                    DebugDrawer.Sphere(corner, 0.25f, Color.red, 1f);
                    DebugDrawer.Line(corner, BotOwner.MyHead.position, 0.1f, Color.red, 1f);
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

        private bool CanShootEnemyAndVisible => CanShootEnemy && CanSeeEnemy;
        private bool CanShootEnemy => BotOwner.Memory.GoalEnemy.IsVisible;
        private bool CanSeeEnemy => BotOwner.Memory.GoalEnemy.CanShoot;
        private bool HasStamina => BotOwner.GetPlayer.Physical.Stamina.NormalValue > 0f;
        private bool GoalEnemyNull => BotOwner.Memory.GoalEnemy == null;

        private readonly BotOwner BotOwner;
        protected ManualLogSource Logger;
        private float DebugTimer = 0f;
        private float DebugTimer2 = 0f;
        private readonly CornerProcessing Processing = new CornerProcessing();
    }
}