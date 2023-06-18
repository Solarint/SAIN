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
            EFTSteer = new EFTSteer(bot);
        }

        private EFTSteer EFTSteer { get; set; }

        public void Update()
        {
            UpdateAimSway();
            UpdateSmoothLook();
            UpdateSteerMode();
            if (UserSettings.DebugConfig.DebugLayers.Value)
            {
                DebugGizmos.SingleObjects.Line(SAIN.HeadPosition, CurrentSteerPoint, Color.white, 0.05f, true, Time.deltaTime, true);
            }
        }

        private void UpdateSteerMode()
        {
            switch (SteeringMode)
            {
                case EBotSteering.ToCustomPoint:
                    EFTSteer.LookToPoint(CurrentSteerPoint, 300f);
                    break;

                case EBotSteering.ToMovingDirection:
                    EFTSteer.LookToMovingDirection();
                    break;

                default:
                    EFTSteer.LookToPoint(CurrentSteerPoint, 300f);
                    break;
            }
            EFTSteer.ManualFixedUpdate();
        }

        public EBotSteering SteeringMode { get; private set; }

        private void UpdateAimSway()
        {
            if (AimSwayTimer < Time.time)
            {
                AimSwayTimer = Time.time + 0.66f;
                AimSway(RealSteerTarget);
            }
        }

        private void AimSway(Vector3 point)
        {
            RealSteerTarget = point;
            Vector3 targetDirection = (point - SAIN.HeadPosition).normalized * 5f;
            Vector3 random = Random.insideUnitSphere * 0.033f;
            Quaternion randomRotation = Quaternion.Euler(random.x, random.y, random.z);
            Vector3 randomDirection = randomRotation * targetDirection;
            SwaySteerPoint = randomDirection + SAIN.HeadPosition;
        }

        private float AimSwayTimer = 0f;

        public bool SteerComplete => Vector3.Angle(SwaySteerDirection, CurrentSteerDirection) < 1f;

        public bool SteerByPriority(bool lookRandomifFalse = true)
        {
            if (EnemyVisible())
            {
                return true;
            }
            if (Time.time - BotOwner.Memory.LastTimeHit < 1f)
            {
                LookToLastHitPos();
                return true;
            }
            if (BotOwner.Memory.IsUnderFire)
            {
                LookToUnderFirePos();
                return true;
            }
            if (EnemyVisibleGroup())
            {
                return true;
            }
            if (SAIN.Enemy?.TimeSinceSeen < 2f)
            {
                LookToEnemy(SAIN.Enemy);
                return true;
            }
            if (LookToCloseEnemyHear())
            {
                return true;
            }
            var sound = BotOwner.BotsGroup.YoungestFastPlace(BotOwner, 50f, 1f);
            if (sound != null)
            {
                LookToHearPos(sound.Position);
                return true;
            }
            else
            {
                if (lookRandomifFalse)
                {
                    LookToRandomPosition();
                }
                return false;
            }
        }

        public Vector3 RealSteerTarget { get; private set; }
        public Vector3 CurrentSteerPoint { get; private set; }
        public Vector3 SwaySteerPoint { get; private set; }
        public Vector3 CurrentSteerDirection => CurrentSteerPoint - SAIN.HeadPosition;
        public Vector3 SwaySteerDirection => SwaySteerPoint - SAIN.HeadPosition;
        public Vector3 RealSteerDirection => RealSteerTarget - SAIN.HeadPosition;

        private void UpdateSmoothLook()
        {
            float angle = Vector3.Angle(CurrentSteerDirection, SwaySteerDirection);
            float divisor = angle > 10f ? 0.25f : 0.33f;
            float lerp = Time.deltaTime / divisor;
            Vector3 pos = Vector3.Lerp(CurrentSteerPoint, SwaySteerPoint, lerp);
            CurrentSteerPoint = pos;
        }

        public void LookToMovingDirection(bool value = true)
        {
            if (value)
            {
                SteeringMode = EBotSteering.ToMovingDirection;
            }
            else
            {
                SteeringMode = EBotSteering.ToCustomPoint;
            }
        }

        public void LookToPoint(Vector3 point)
        {
            AimSway(point);
            SteeringMode = EBotSteering.ToCustomPoint;
        }

        public void LookToDirection(Vector3 direction, bool flat)
        {
            if (flat)
            {
                direction.y = 0f;
            }
            Vector3 pos = SAIN.HeadPosition + direction;
            LookToPoint(pos);
        }

        public bool EnemyVisible()
        {
            SAINEnemy enemy = SAIN.Enemy;
            if (enemy != null && enemy.IsVisible)
            {
                LookToEnemy(enemy);
                return true;
            }
            return false;
        }

        public bool EnemyVisibleGroup()
        {
            if (!SAIN.Squad.BotInGroup)
            {
                return false;
            }

            SAINEnemy enemy = SAIN.Enemy;
            if (enemy != null && enemy.InLineOfSight)
            {
                if (enemy.EnemyComponent == null)
                {
                    //System.Console.WriteLine("Bot's Enemy is not null but their enemy component is null!");
                }
                if (enemy.IsVisible || enemy.EnemyComponent?.VisibleByGroup == true)
                {
                    LookToEnemy(enemy);
                    return true;
                }
            }
            return false;
        }

        public void LookToEnemy(SAINEnemy enemy)
        {
            if (enemy != null)
            {
                LookToPoint(enemy.EnemyChestPosition);
            }
        }

        private bool LookToCloseEnemyHear()
        {
            SAINEnemy enemy = SAIN.Enemy;
            if (enemy != null)
            {
                if (enemy.RealDistance < 30f && enemy.Player.MovementContext.ActualLinearSpeed > 0.33f)
                {
                    LookToEnemy(enemy);
                    return true;
                }
            }
            return false;
        }

        public void LookToUnderFirePos()
        {
            var pos = SAIN.UnderFireFromPosition;
            pos.y += 1f;
            LookToPoint(pos);
        }

        public void LookToHearPos(Vector3 soundPos, bool visionCheck = false)
        {
            if (visionCheck)
            {
                soundPos.y += 0.1f;
                var direction = soundPos - SAIN.HeadPosition;

                if (!Physics.Raycast(SAIN.HeadPosition, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                {
                    LookToPoint(soundPos);
                }
            }
            else
            {
                LookToPoint(soundPos);
            }
        }

        public void LookToLastHitPos()
        {
            var pos = BotOwner.Memory.LastHitPos;
            pos.y += 1f;
            LookToPoint(pos);
        }

        public void LookToRandomPosition()
        {
            if (RandomLookTimer < Time.time)
            {
                RandomLookTimer = Time.time + 2f;
                var Mask = LayerMaskClass.HighPolyWithTerrainMask;
                var Start = SAIN.HeadPosition;
                Vector3 pointToLook = Vector3.zero;
                float pointDistance = 0f;
                for (int i = 0; i < 10; i++)
                {
                    var random = Random.onUnitSphere * 5f;
                    random.y = 0f;
                    if (!Physics.Raycast(Start, random, out var hit, 10f, Mask))
                    {
                        pointToLook = random + SAIN.HeadPosition;
                        break;
                    }
                    else
                    {
                        if (hit.distance > pointDistance)
                        {
                            pointDistance = hit.distance;
                            pointToLook = hit.point;
                        }
                    }
                }
                LookToPoint(pointToLook);
            }
        }

        private float RandomLookTimer = 0f;

        protected ManualLogSource Logger;
    }
}