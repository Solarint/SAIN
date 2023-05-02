using BepInEx.Logging;
using EFT;
using Movement.Helpers;
using SAIN_Helpers;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Movement.Layers.DogFight
{
    internal class UpdateSteering
    {
        public UpdateSteering(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            BotOwner = bot;
            Corners = new Corners.CornerProcessing(bot);
        }

        public void Update(bool fallingBack, bool debugMode = false)
        {
            if (fallingBack && HasStamina)
            {
                BotOwner.Steering.LookToMovingDirection();
            }
            else if (BotOwner.Memory.GoalEnemy != null && CanShootEnemyAndVisible)
            {
                Vector3 enemyPosition = BotOwner.Memory.GoalEnemy.Owner.GetPlayer.PlayerBones.Spine1.position;
                BotOwner.Steering.LookToPoint(enemyPosition);
            }
            else
            {
                NextCornerPosition(out Vector3 corner, debugMode);
                BotOwner.Steering.LookToPoint(corner);
            }

            if (debugMode && DebugTimer2 < Time.time)
            {
                DebugTimer2 = Time.time + 0.1f;
                DebugDrawer.Ray(BotOwner.LookSensor._headPoint, BotOwner.Steering.LookDirection, 1f, 0.025f, Color.white, 0.1f);
            }
        }

        private bool NextCornerPosition(out Vector3 corner, bool debugMode = false)
        {
            Vector3[] corners = Corners.GetCorners(true, false, true, true);
            if (corners.Length > 0)
            {
                corner = corners[1];

                if (debugMode && DebugTimer < Time.time)
                {
                    DebugTimer = Time.time + 1f;
                    DebugDrawer.Sphere(corner, 0.1f, Color.red, 1f);
                    DebugDrawer.Line(corner, BotOwner.MyHead.position, 0.1f, Color.red, 1f);
                    DebugDrawer.Line(corners[0], BotOwner.MyHead.position, 0.2f, Color.yellow, 1f);
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

        private readonly BotOwner BotOwner;
        protected ManualLogSource Logger;
        private readonly Corners.CornerProcessing Corners;
        private float DebugTimer = 0f;
        private float DebugTimer2 = 0f;
    }
}