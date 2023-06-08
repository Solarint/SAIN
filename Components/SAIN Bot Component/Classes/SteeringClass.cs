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

            if (UpdateSteerTimer < Time.time)
            {
                UpdateSteerTimer = Time.time + 0.25f;

                var enemy = BotOwner.Memory.GoalEnemy;
                if (enemy != null && enemy.CanShoot && enemy.IsVisible)
                {
                    LookToGoalEnemyPos();
                }
                else if (SAIN.HasEnemy && SAIN.EnemyIsVisible)
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
                else if (SAIN.LastHeardSound != null && SAIN.LastHeardSound.TimeSinceHeard < 2f && (SAIN.LastHeardSound.Position - BotOwner.Position).magnitude < 50f)
                {
                    LookToHearPos();
                }
                else
                {
                    BotOwner.LookData.SetLookPointByHearing();
                }
            }
        }

        private float UpdateSteerTimer = 0f;

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
            var soundPos = SAIN.LastHeardSound.Position;

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