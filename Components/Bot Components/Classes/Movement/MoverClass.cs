using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AI;
using UnityEngine;
using BepInEx.Logging;

namespace SAIN.Classes
{
    public class MoverClass : SAINBot
    {
        public MoverClass(BotOwner owner) : base(owner)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        public void Update()
        {
            if (NavigationPoint != null)
            {
                NavigationPoint.Update();
                if (NavigationPoint.FinishedPath)
                {
                    NavigationPoint = null;
                }
            }
            UpdateLean();
        }

        public bool GoToPoint(Vector3 point, bool forceNew = true, bool mustHaveWay = true, float reachDist = 0.5f)
        {
            if (forceNew || NavigationPoint == null)
            {
                var navPoint = new NavigationPointObject(BotOwner);
                var pathStatus = navPoint.GoToPoint(point, mustHaveWay);
                if (pathStatus != NavMeshPathStatus.PathInvalid)
                {
                    if (mustHaveWay && pathStatus != NavMeshPathStatus.PathComplete)
                    {
                        return false;
                    }
                    NavigationPoint = navPoint;
                    return true;
                }
            }
            return false;
        }

        public void StopMove()
        {
            NavigationPoint = null;
            BotOwner.StopMove();
            BotOwner.Mover.Stop();
        }

        public void StopSprint()
        {
            BotOwner.GetPlayer.EnableSprint(false);
        }

        public NavigationPointObject NavigationPoint { get; private set; }
        public bool HasDestination => NavigationPoint != null;
        public MoveToCoverObject MoveToCover { get; set; }
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
