using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AI;
using UnityEngine;
using BepInEx.Logging;
using System.Reflection;
using HarmonyLib;

namespace SAIN.Classes
{
    public class MoverClass : SAINBot
    {
        public MoverClass(BotOwner owner) : base(owner)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            var botlay = AccessTools.Property(typeof(BotOwner), "BotLay");
            BotLayProperty = botlay.PropertyType.GetProperty("IsLay");
        }

        private readonly PropertyInfo BotLayProperty;

        public bool GoToPoint(Vector3 point, bool slowAtTheEnd = false, float reachDist = -1f, bool crawl = false)
        {
            if (NavMesh.SamplePosition(point, out var navHit, 0.1f, -1))
            {
                NavMeshPath Path = new NavMeshPath();
                if (NavMesh.CalculatePath(SAIN.Position, navHit.position, -1, Path) && Path.corners.Length > 1)
                {
                    if (reachDist < 0f)
                    {
                        reachDist = BotOwner.Settings.FileSettings.Move.REACH_DIST;
                    }
                    //BotOwner.Mover.GoToByWay(Path.corners, reachDist, Vector3.zero);
                    BotOwner.Mover.GoToPoint(navHit.position, false, reachDist, false, false, !crawl);
                    if (crawl)
                    {
                        SetBotProne(true);
                    }
                    return true;
                }
            }

            Logger.LogWarning($"{NavMeshPathStatus.PathInvalid}");
            return false;
        }

        public void SetBotProne(bool value)
        {
            if (BotLay.IsLay != value)
            {
                BotLayProperty.SetValue(BotLay, value);
            }
        }

        public void Update()
        {
            //SetBotProne(true);
            UpdateTargetMoveSpeed();
            UpdateTargetPose();
            UpdateLean();

            if (NavigationPoint != null)
            {
                //NavigationPoint.Update();
                if (NavigationPoint.FinishedPath)
                {
                    NavigationPoint = null;
                }
            }
        }

        private void UpdateTargetMoveSpeed()
        {
            if (BotOwner.DoorOpener.NearDoor)
            {
                DestMoveSpeed = 0.5f;
            }
            float num = DestMoveSpeed - Player.Speed;
            if (Math.Abs(num) >= 1E-45f)
            {
                Player.ChangeSpeed(num); 
            }
        }

        private void UpdateTargetPose()
        {
            if (BotOwner.BotLay.IsLay)
            {
                return;
            }
            float poseDiff = TargetPose - Player.PoseLevel;
            float num = Math.Abs(poseDiff);
            if (num >= 1E-45f)
            {
                Player.ChangePose(0.05f * poseDiff); 
            }
        }

        public bool ShallProne(CoverPoint point, bool withShoot)
        {
            var status = point.CoverStatus;
            if (status == CoverStatus.FarFromCover || status == CoverStatus.None)
            {
                if (Player.MovementContext.CanProne)
                {
                    var enemy = SAIN.Enemy;
                    if (enemy != null)
                    {
                        float distance = (enemy.Position - BotPosition).magnitude;
                        if (distance > 30f)
                        {
                            if (withShoot)
                            {
                                return CanShootFromProne(enemy.Position);
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool ShallProne(bool withShoot, float mindist = 30f)
        {
            if (Player.MovementContext.CanProne)
            {
                var enemy = SAIN.Enemy;
                if (enemy != null)
                {
                    float distance = (enemy.Position - BotPosition).magnitude;
                    if (distance > mindist)
                    {
                        if (withShoot)
                        {
                            return CanShootFromProne(enemy.Position);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public bool ShallProneHide(float mindist = 30f)
        {
            if (Player.MovementContext.CanProne)
            {
                var enemy = SAIN.Enemy;
                if (enemy != null)
                {
                    float distance = (enemy.Position - BotPosition).magnitude;
                    if (distance > mindist)
                    {
                        return !CanShootFromProne(enemy.Position);
                    }
                }
            }
            return false;
        }

        public bool CanShootFromProne(Vector3 target)
        {
            Vector3 vector = BotPosition + Vector3.up * 0.14f;
            Vector3 vector2 = target + Vector3.up - vector;
            Vector3 from = vector2;
            from.y = vector.y;
            float num = Vector3.Angle(from, vector2);
            float lay_DOWN_ANG_SHOOT = GClass560.Core.LAY_DOWN_ANG_SHOOT;
            return num <= Mathf.Abs(lay_DOWN_ANG_SHOOT) && GClass252.CanShootToTarget(new ShootPointClass(target, 1f), vector, BotOwner.LookSensor.Mask, true);
        }

        public BotLayClass BotLay => BotOwner.BotLay;

        public void SetTargetPose(float pose)
        {
            TargetPose = pose;
        }

        public void SetTargetMoveSpeed(float speed)
        {
            DestMoveSpeed = speed;
        }

        public float TargetPose { get; private set; }
        public float DestMoveSpeed { get; private set; }

        private void PoseChangeScatter(float obj)
        {
        }

        public bool GoToPoint(Vector3 point, bool forceNew = true, bool mustHaveWay = true, float reachDist = 0.5f)
        {
            if (forceNew || NavigationPoint == null)
            {
                var navPoint = new NavigationPointObject(BotOwner);
                if (navPoint.GoToPoint(point, reachDist))
                {
                    NavigationPoint = navPoint;
                    return true;
                }
            }
            return false;
        }

        public void StopMove()
        {
            NavigationPoint = null;
            BotOwner.Mover.Stop();
        }

        public bool CanSprint => Player.Physical.CanSprint;

        public void Sprint(bool value)
        {
            if (IsSprinting != value || Player.Physical.Sprinting != value)
            {
                Player.EnableSprint(value);
                IsSprinting = value;
            }
            if (value)
            {
                SAIN.Steering.LookToMovingDirection();
            }
        }

        public bool IsSprinting { get; private set; }

        public NavigationPointObject NavigationPoint { get; private set; }
        public bool HasDestination => NavigationPoint != null;
        public CoverPoint CoverDestination { get; private set; }

        public bool ShiftAwayFromCloseWall(Vector3 target, out Vector3 newPos)
        {
            const float closeDist = 0.75f;

            if (CheckTooCloseToWall(target, out var rayHit, closeDist))
            {
                var direction = (BotOwner.Position - rayHit.point).normalized * 0.8f;
                direction.y = 0f;
                var movePoint = BotOwner.Position + direction;
                if (NavMesh.SamplePosition(movePoint, out var hit, 0.1f, -1))
                {
                    newPos = hit.position;
                    return true;
                }
            }
            newPos = Vector3.zero;
            return false;
        }

        public bool CheckTooCloseToWall(Vector3 target, out RaycastHit rayHit, float checkDist = 0.75f)
        {
            Vector3 botPos = BotOwner.Position;
            Vector3 direction = target - botPos;
            botPos.y = SAIN.WeaponRoot.y;
            return Physics.Raycast(BotOwner.Position, direction, out rayHit, checkDist, LayerMaskClass.HighPolyWithTerrainMask);
        }

        public Vector3 MidPoint(Vector3 target, float lerpVal = 0.5f)
        {
            return Vector3.Lerp(BotOwner.Position, target, lerpVal);
        }

        public bool BotIsAtPoint(Vector3 point, float reachDist = 1f)
        {
            return DistanceToDestination(point) < reachDist;
        }

        public float DistanceToDestination(Vector3 point)
        {
            return Vector3.Distance(point, BotOwner.Transform.position);
        }

        public void TryJump()
        {
            if (JumpTimer < Time.time && CanJump)
            {
                JumpTimer = Time.time + 1f;
                CurrentState.Jump();
            }
        }

        public void SetBlindFire(BlindFireSetting value)
        {
            SetBlindFire((int)value);
        }

        public void SetBlindFire(int value)
        {
            BotOwner.GetPlayer.MovementContext.SetBlindFire(value);
        }

        public void FastLean(LeanSetting value)
        {

        }

        public void FastLean(float value)
        {
            TargetLeanNumber = value;
            Player.MovementContext.SetTilt(value);
        }

        public void SlowLean(LeanSetting value)
        {
            float num;
            switch (value)
            {
                case LeanSetting.Left:
                    num = -5f; break;
                case LeanSetting.Right:
                    num = 5f; break;
                default:
                    num = 0f; break;
            }
            SlowLean(num);
        }

        public void SlowLean(float value)
        {
            TargetLeanNumber = value;
        }

        private void UpdateLean()
        {
            float target = TargetLeanNumber;
            float current = CurrentLeanNumber;
            if (target == current)
            {
                return;
            }
            if (Mathf.Abs(target - current) < 0.1f)
            {
                current = target;
            }
            else
            {
                if (target < 0f)
                {
                    current -= target / 30f;
                }
                else if (target > 0f)
                {
                    current += target / 30f;
                }
                else
                {
                    if (current < 0f)
                    {
                        current += 1f / 30f;
                    }
                    else if (current > 0f)
                    {
                        current -= 1f / 30f;
                    }
                }
            }
            Player.MovementContext.SetTilt(current);
        }

        private float TargetLeanNumber = 0f;
        public float CurrentLeanNumber => Player.MovementContext.Tilt;
        public MovementState CurrentState => BotOwner.GetPlayer.MovementContext.CurrentState;
        public bool CanJump => BotOwner.GetPlayer.MovementContext.CanJump;

        private float JumpTimer = 0f;

        private readonly ManualLogSource Logger;
    }
}
