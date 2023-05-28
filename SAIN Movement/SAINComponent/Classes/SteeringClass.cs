using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using SAIN.Components;
using UnityEngine;
using static SAIN.Helpers.Corners;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Classes
{
    public class SteeringClass : SAINBot
    {
        public SteeringClass(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        public void ManualUpdate()
        {
            var enemy = BotOwner.Memory.GoalEnemy;
            if (enemy == null)
            {
                if (BotOwner.Memory.LastEnemy != null)
                {
                    enemy = BotOwner.Memory.LastEnemy;
                }
            }

            if (enemy != null && enemy.CanShoot && enemy.IsVisible)
            {
                var pos = BotOwner.Memory.GoalEnemy.Person.MainParts[BodyPartType.body].Position;
                BotOwner.Steering.LookToPoint(pos);
            }
            else if (enemy != null && enemy.PersonalLastSeenTime > Time.time - 5f && enemy.HaveSeen)
            {
                var lastPos = BotOwner.Memory.GoalEnemy.EnemyLastPosition;
                lastPos.y += 1f;
                BotOwner.Steering.LookToPoint(lastPos);
                DebugGizmos.SingleObjects.Line(BotOwner.LookSensor._headPoint, lastPos, Color.blue, 0.025f, true, 1f, true);
            }
            else if (BotOwner.Memory.IsUnderFire)
            {
                var underFirePos = SAIN.UnderFireFromPosition;
                underFirePos.y += 1f;
                BotOwner.Steering.LookToPoint(underFirePos);
                DebugGizmos.SingleObjects.Line(BotOwner.LookSensor._headPoint, underFirePos, Color.green, 0.025f, true, 1f, true);
            }
            else if (Time.time - BotOwner.Memory.LastTimeHit < 2f)
            {
                var lastHitPos = BotOwner.Memory.LastHitPos;
                lastHitPos.y += 1f;
                BotOwner.Steering.LookToPoint(lastHitPos);
                DebugGizmos.SingleObjects.Line(BotOwner.LookSensor._headPoint, lastHitPos, Color.red, 0.025f, true, 1f, true);
            }
            else
            {
                BotOwner.LookData.SetLookPointByHearing();
            }
        }

        protected ManualLogSource Logger;
    }
}