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

        public void Update()
        {
            UpdateAimSway();
            UpdateSmoothLook();
        }

        private void UpdateAimSway()
        {
            if (AimSwayTimer < Time.time)
            {
                AimSwayTimer = Time.time + 0.66f;
                AimSway();
            }
        }

        private void AimSway()
        {
            Vector3 random = Random.insideUnitSphere * 0.5f;
            Quaternion randomRotation = Quaternion.Euler(random.x, random.y, random.z);
            TargetSteerPoint = randomRotation * RealSteerTarget;
        }

        private float AimSwayTimer = 0f;

        public bool SteerComplete => (TargetSteerPoint - RealSteerTarget).sqrMagnitude < 1f;

        public bool Steer(bool useDefaultHear = true)
        {
            if (!SAIN.BotActive || SAIN.GameIsEnding)
            {
                return false;
            }

            if (SAIN.Enemy?.IsVisible == true)
            {
                LookToEnemy();
                return true;
            }
            if (BotOwner.Memory.LastTimeHit > Time.time - 1f)
            {
                LookToLastHitPos();
                return true;
            }
            if (BotOwner.Memory.IsUnderFire)
            {
                LookToUnderFirePos();
                return true;
            }
            var sound = BotOwner.BotsGroup.YoungestPlace(BotOwner, 100f, false);
            if (sound != null && Time.time - sound.CreatedTime < 2f)
            {
                LookToHearPos(sound.Position);
                return true;
            }
            else
            {
                if (useDefaultHear)
                {
                    BotOwner.LookData.SetLookPointByHearing();
                }
                else
                {
                    LookToRandomPosition();
                }
                return false;
            }
        }

        private Vector3 RealSteerTarget;

        public Vector3 CurrentSteerPoint { get; private set; }

        public Vector3 TargetSteerPoint { get; private set; }

        public Vector3 CurrentSteerDirection => CurrentSteerPoint - BotPosition;

        public Vector3 TargetSteerDirection => TargetSteerPoint - BotPosition;

        private void UpdateSmoothLook()
        {
            float angle = Vector3.Angle(CurrentSteerDirection, TargetSteerDirection);
            float divisor = angle > 10f ? 0.5f : 0.66f;
            float lerp = Time.deltaTime / divisor;
            Vector3 pos = Vector3.Lerp(CurrentSteerPoint, TargetSteerPoint, lerp);
            CurrentSteerPoint = pos;
            BotOwner.Steering.LookToPoint(pos, 999f);
        }

        public void LookToPoint(Vector3 point)
        {
            RealSteerTarget = point;
            AimSway();
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

        public void LookToRandomPosition()
        {
            if (RandomLookTimer < Time.time)
            {
                var Mask = LayerMaskClass.HighPolyWithTerrainMask;
                var Start = SAIN.HeadPosition;
                for (int i = 0; i < 10;  i++)
                {
                    var random = Random.onUnitSphere * 5f;
                    if (!Physics.Raycast(Start, random, 3f, Mask))
                    {
                        RandomLookTimer = Time.time + 2f * Random.Range(0.5f, 1.5f);
                        SAIN.Steering.LookToDirection(random, true);
                        break;
                    }
                }
            }
        }

        private float RandomLookTimer = 0f;

        public void LookToEnemy()
        {
            var enemy = SAIN.Enemy;
            if (enemy != null && enemy.CanShoot && enemy.IsVisible)
            {
                var pos = enemy.Person.MainParts[BodyPartType.body].Position;
                TargetSteerPoint = pos;
            }
        }

        public void LookToPriorityEnemyPos()
        {
            var priority = SAIN.Enemy;
            if (priority != null && priority.IsVisible && priority.EnemyChestPosition != null)
            {
                var pos = priority.EnemyChestPosition;
                LookToPoint(pos);
            }
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


        protected ManualLogSource Logger;
    }
}