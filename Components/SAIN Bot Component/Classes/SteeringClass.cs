using BepInEx.Logging;
using EFT;
using HarmonyLib;
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

            if (enemy != null && enemy.CanShoot && enemy.IsVisible)
            {
                LookToGoalEnemyPos();
            }
            else if (enemy != null && SAIN.EnemyIsVisible)
            {
                LookToGoalEnemyPos();
            }
            else if (BotOwner.Memory.LastTimeHit > Time.time - 1f)
            {
                LookToLastHitPos();
            }
            else if (BotOwner.Memory.IsUnderFire)
            {
                LookToUnderFirePos();
            }
            else if (SAIN.LastSoundHeardTime > Time.time - 2f)
            {
                LookToHearPos();
            }
            else
            {
                BotOwner.LookData.SetLookPointByHearing();
            }
        }

        public void LookToGoalEnemyPos()
        {
            var enemy = BotOwner.Memory.GoalEnemy;
            if (enemy != null && enemy.CanShoot && enemy.IsVisible)
            {
                var pos = enemy.Person.MainParts[BodyPartType.body].Position;
                BotOwner.Steering.LookToPoint(pos);
            }
        }

        public void LookToPriorityEnemyPos()
        {
            var priority = SAIN.Enemy.SAINEnemy;
            if (priority != null && priority.IsVisible && priority.EnemyChestPosition != null)
            {
                var enemyPos = priority.EnemyChestPosition;
                BotOwner.Steering.LookToPoint(enemyPos);
            }
        }

        public void LookToUnderFirePos()
        {
            var underFirePos = SAIN.UnderFireFromPosition;
            underFirePos.y += 1f;
            BotOwner.Steering.LookToPoint(underFirePos);
        }

        public void LookToHearPos(bool visionCheck = false)
        {
            var soundPos = SAIN.LastSoundHeardPosition;

            if (visionCheck)
            {
                soundPos.y += 0.1f;
                var direction = soundPos - SAIN.HeadPosition;

                if (!Physics.Raycast(SAIN.HeadPosition, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                {
                    BotOwner.Steering.LookToPoint(soundPos);
                }
            }
            else
            {
                BotOwner.Steering.LookToPoint(soundPos);
            }
        }

        public void LookToLastHitPos()
        {
            var lastHitPos = BotOwner.Memory.LastHitPos;
            lastHitPos.y += 1f;
            BotOwner.Steering.LookToPoint(lastHitPos);
        }


        protected ManualLogSource Logger;
    }
}