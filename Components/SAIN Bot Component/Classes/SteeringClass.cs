using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using UnityEngine;

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
            if (!SAIN.BotActive || SAIN.GameIsEnding)
            {
                return;
            }

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
            else if (CanSeeAnyEnemy(out var anyPos))
            {
                BotOwner.Steering.LookToPoint(anyPos);
            }
            else if (BotOwner.Memory.IsUnderFire)
            {
                var underFirePos = SAIN.UnderFireFromPosition;
                underFirePos.y += 1f;
                BotOwner.Steering.LookToPoint(underFirePos);
                //DebugGizmos.SingleObjects.Line(BotOwner.LookSensor._headPoint, underFirePos, Color.green, 0.025f, true, 1f, true);
            }
            else if (Vector3.Distance(SAIN.LastSoundHeardPosition, BotOwner.Transform.position) < 15f)
            {
                var hearPos = SAIN.LastSoundHeardPosition;
                hearPos.y += 1f;
                BotOwner.Steering.LookToPoint(hearPos);
            }
            else if (Time.time - BotOwner.Memory.LastTimeHit < 2f)
            {
                var lastHitPos = BotOwner.Memory.LastHitPos;
                lastHitPos.y += 1f;
                BotOwner.Steering.LookToPoint(lastHitPos);
                //DebugGizmos.SingleObjects.Line(BotOwner.LookSensor._headPoint, lastHitPos, Color.red, 0.025f, true, 1f, true);
            }
            else
            {
                BotOwner.LookData.SetLookPointByHearing();
            }
        }

        private bool CanSeeAnyEnemy(out Vector3 enemyPos)
        {
            if (SAIN.Enemies.Enemies.Count > 0)
            {
                foreach (var enemy in SAIN.Enemies.Enemies.Values)
                {
                    if (enemy.IsVisible)
                    {
                        enemyPos = enemy.EnemyChestPosition.Value;
                        return true;
                    }
                }
            }
            enemyPos = Vector3.zero;
            return false;
        }

        protected ManualLogSource Logger;
    }
}