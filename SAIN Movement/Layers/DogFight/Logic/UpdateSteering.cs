using BepInEx.Logging;
using EFT;
using Movement.Helpers;
using SAIN_Helpers;
using UnityEngine;
using UnityEngine.AI;
using static Movement.UserSettings.Debug;

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

        public void Update(bool IsSprintingFallback)
        {
            if (GoalEnemyNull)
            {
                BotOwner.LookData.SetLookPointByHearing(null);
            }

            if (IsSprintingFallback)
            {
                BotOwner.Steering.LookToMovingDirection();
            }
            else if (CanShootEnemy)
            {
                Vector3 target = BotOwner.Memory.GoalEnemy.CurrPosition;
                target.y += 0.75f;
                BotOwner.Steering.LookToPoint(target);
            }
            else if (NextCornerPosition(out Vector3 corner))
            {
                BotOwner.Steering.LookToPoint(corner);
            }
            else
            {
                BotOwner.LookData.SetLookPointByHearing(null);
            }

            if (DebugMode && DebugTimer2 < Time.time)
            {
                DebugTimer2 = Time.time + 0.05f;
                DebugDrawer.Ray(BotOwner.LookSensor._headPoint, BotOwner.Steering.LookDirection, 1f, 0.025f, Color.white, 0.1f);
            }
        }

        private bool NextCornerPosition(out Vector3 corner)
        {
            Vector3[] corners = Corners.CornerProcessing.GetCorners(BotOwner.Transform.position, BotOwner.Memory.GoalEnemy.CurrPosition, true, false, true, true);
            if (corners.Length > 0)
            {
                corner = corners[1];

                if (Vector3.Distance(corners[0], corner) < 1f)
                {
                    corner = BotOwner.Memory.GoalEnemy.CurrPosition;
                    return false;
                }

                corner.y = BotOwner.LookSensor._headPoint.y;

                if (DebugMode && DebugTimer < Time.time)
                {
                    DebugTimer = Time.time + 1f;
                    DebugDrawer.Sphere(corner, 0.25f, Color.red, 1f);
                    DebugDrawer.Line(corner, BotOwner.MyHead.position, 0.1f, Color.red, 1f);
                }

                return true;
            }
            else
            {
                corner = BotOwner.Memory.GoalEnemy.CurrPosition;
                return false;
            }
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
    }
}