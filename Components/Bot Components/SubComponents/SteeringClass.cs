using BepInEx.Logging;
using EFT;
using SAIN.Classes.Mover;
using SAIN.Components;
using SAIN.Helpers;
using UnityEngine;

namespace SAIN.Classes
{
    public class SteeringClass : MonoBehaviour
    {
        private SAINComponent SAIN;
        private BotOwner BotOwner => SAIN.BotOwner;

        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        private void Update()
        {

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
            if (LookToAimTarget())
            {
                return true;
            }
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
            if (SAIN.Enemy?.TimeSinceSeen < 2f)
            {
                LookToEnemy(SAIN.Enemy);
                return true;
            }
            if (LookToEnemyLastSeenClose())
            {
                return true;
            }
            var sound = BotOwner.BotsGroup.YoungestFastPlace(BotOwner, 25f, 1f);
            if (sound != null)
            {
                LookToHearPos(sound.Position);
                return true;
            }
            if (BotOwner.DangerPointsData.HaveDangePoints && BotOwner.DangerPointsData.place != null)
            {
                LookToPoint(BotOwner.DangerPointsData.place.Position);
                return true;
            }
            if (SAIN.Enemy?.TimeSinceSeen < 12f)
            {
                LookToEnemyLastSeenPos();
                return true;
            }
            else
            {
                if (lookRandomifFalse)
                {
                    if (SAIN.ExitsToLocation != null && SAIN.ExitsToLocation.Count > 0)
                    {
                        LookToRoomExits();
                    }
                    else
                    {
                        LookToRandomPosition();
                    }
                }
                return false;
            }
        }

        private bool LookToVisibleSound()
        {
            return false;
        }

        public bool LookToEnemyLastSeenPos()
        {
            var enemy = SAIN.Enemy;
            if (enemy != null)
            {
                if (!enemy.IsVisible)
                {
                    Vector3 pos = enemy.PositionLastSeen;
                    pos += Vector3.up * 1f;
                    LookToPoint(pos);
                    return true;
                }
            }
            return false;
        }

        public bool LookToEnemyLastSeenClose()
        {
            var enemy = SAIN.Enemy;
            if (enemy != null)
            {
                if (enemy.Seen && (enemy.Position - enemy.PositionLastSeen).sqrMagnitude < 10f)
                {
                    LookToPoint(enemy.Position);
                    return true;
                }
            }
            return false;
        }

        private void LookToRoomExits()
        {
            if (RandomLookExitsTimer < Time.time)
            {
                RandomLookExitsTimer = Time.time + 3f;
                var exits = SAIN.ExitsToLocation;
                if (exits != null && exits.Count > 0)
                {
                    Vector3 corner = exits.PickRandom();
                    corner += Vector3.up * 1f;
                    LookToPoint(corner);
                }
            }
        }
        private float RandomLookExitsTimer;

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

        public void LookToMovingDirection()
        {
            BotOwner.Steering.LookToMovingDirection();
        }

        public void LookToPoint(Vector3 point)
        {
            BotOwner.Steering.LookToPoint(point);
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

        public bool LookToAimTarget()
        {
            if (SAIN.Enemy?.IsVisible == true && SAIN.Enemy?.CanShoot == true)
            {
                if (BotOwner.WeaponManager.IsReady && BotOwner.WeaponManager.HaveBullets)
                {
                    var aim = BotOwner.AimingData;
                    if (aim != null)
                    {
                        Vector3 pos = aim.EndTargetPoint;
                        LookToPoint(pos);
                        return true;
                    }
                }
            }
            return false;
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

        public void LookToEnemy(SAINEnemy enemy)
        {
            if (enemy != null)
            {
                LookToPoint(enemy.EnemyChestPosition);
            }
        }

        public void LookToEnemy()
        {
            SAINEnemy enemy = SAIN.Enemy;
            LookToEnemy(enemy);
        }

        private bool LookToCloseEnemyHear()
        {
            SAINEnemy enemy = SAIN.Enemy;
            if (enemy != null)
            {
                if (enemy.RealDistance < 30f && enemy.Person.GetPlayer.MovementContext.ActualLinearSpeed > 0.33f)
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

        private bool LookRandom;
        public void LookToRandomPosition()
        {
            if (RandomLookTimer < Time.time)
            {
                RandomLookTimer = Time.time + 2f * Random.Range(0.66f, 1.33f);
                Vector3 pointToLook = Vector3.zero;
                LookRandom = !LookRandom;
                if (LookRandom)
                {
                    var Mask = LayerMaskClass.HighPolyWithTerrainMask;
                    var Start = SAIN.HeadPosition;
                    float pointDistance = 0f;
                    for (int i = 0; i < 10; i++)
                    {
                        var random = Random.onUnitSphere * 5f;
                        random.y = 0f;
                        if (!Physics.Raycast(Start, random, out var hit, 15f, Mask))
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
                }
                else
                {
                    if (LookToPathToEnemy())
                    {
                        return;
                    }
                    if (LookToEnemyLastSeenPos())
                    {
                        return;
                    }
                    if (SAIN.CurrentTargetPosition != null)
                    {
                        pointToLook = SAIN.CurrentTargetPosition.Value;
                    }
                }
                LookToPoint(pointToLook);
            }
        }

        public bool LookToPathToEnemy()
        {
            var enemy = SAIN.Enemy;
            if (enemy != null && !enemy.IsVisible)
            {
                if (enemy.Seen && enemy.TimeSinceSeen < 10f)
                {
                    if (enemy.Path.corners.Length > 1)
                    {
                        Vector3 pos = enemy.Path.corners[1];
                        pos += Vector3.up * 1f;
                        LookToPoint(pos);
                        return true;
                    }
                }
            }
            return false;
        }

        private float RandomLookTimer = 0f;

        protected ManualLogSource Logger;
    }
}