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
using SAIN.Classes.Mover;

namespace SAIN.Classes
{
    public class MoverClass : SAINBot
    {
        public MoverClass(BotOwner owner) : base(owner)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            Prone = new ProneClass(owner);
            Pose = new PoseClass(owner);
        }

        public PoseClass Pose { get; private set; }
        public ProneClass Prone { get; private set; }

        public bool GoToPoint(Vector3 point, float reachDist = -1f, bool crawl = false)
        {
            if (CurrentDecision == SAINSoloDecision.HoldInCover)
            {
                return false;
            }
            if (CanGoToPoint(point, out Vector3 pointToGo))
            {
                if (reachDist < 0f)
                {
                    reachDist = BotOwner.Settings.FileSettings.Move.REACH_DIST;
                }
                BotOwner.Mover.GoToPoint(pointToGo, false, reachDist, false, false, false);
                if (crawl)
                {
                    Prone.SetProne(true);
                }
                BotOwner.DoorOpener.Update();
                return true;
            }
            return false;
        }
        public bool GoToPointWay(Vector3 point, float reachDist = -1f, bool crawl = false)
        {
            if (CurrentDecision == SAINSoloDecision.HoldInCover)
            {
                return false;
            }
            if (CanGoToPoint(point, out Vector3[] Way))
            {
                if (reachDist < 0f)
                {
                    reachDist = BotOwner.Settings.FileSettings.Move.REACH_DIST;
                }
                BotOwner.Mover.GoToByWay(Way, reachDist, BotOwner.Position);
                if (crawl)
                {
                    Prone.SetProne(true);
                }
                BotOwner.DoorOpener.Update();
                return true;
            }
            return false;
        }

        public bool CanGoToPoint(Vector3 point, out Vector3[] Way)
        {
            Way = null;
            if (CurrentDecision == SAINSoloDecision.HoldInCover)
            {
                return false;
            }
            if (NavMesh.SamplePosition(point, out var navHit, 10f, -1))
            {
                NavMeshPath Path = new NavMeshPath();
                if (NavMesh.CalculatePath(SAIN.Position, navHit.position, -1, Path) && Path.corners.Length > 1)
                {
                    Way = Path.corners;
                }
            }
            return Way != null;
        }

        public bool CanGoToPoint(Vector3 point, out Vector3 pointToGo)
        {
            pointToGo = point;
            if (CurrentDecision == SAINSoloDecision.HoldInCover)
            {
                return false;
            }
            if (NavMesh.SamplePosition(point, out var navHit, 10f, -1))
            {
                NavMeshPath Path = new NavMeshPath();
                if (NavMesh.CalculatePath(SAIN.Position, navHit.position, -1, Path) && Path.corners.Length > 1)
                {
                    pointToGo = navHit.position;
                    return true;
                }
            }
            return false;
        }

        public void Update()
        {
            if (!UpdateSprint())
            {
                Pose.Update();
                UpdateTargetMoveSpeed();
            }
        }

        private bool UpdateSprint()
        {
            if (IsSprinting)
            {
                SetTargetPose(1f);
                SetTargetMoveSpeed(1f);
                FastLean(0f);
                SAIN.Steering.LookToMovingDirection();

                CurrentState.EnableSprint(true);
                return true;
            }
            else
            {
                CurrentState.EnableSprint(false);
                return false;
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

        public void SetTargetPose(float pose)
        {
            Pose.SetTargetPose(pose);
        }

        public void SetTargetMoveSpeed(float speed)
        {
            DestMoveSpeed = speed;
        }

        public float DestMoveSpeed { get; private set; }

        public void StopMove()
        {
            NavigationPoint = null;
            BotOwner.Mover.Stop();
            Sprint(false);
        }

        public bool CanSprint => Player.Physical.CanSprint;

        public void Sprint(bool value)
        {
            IsSprinting = value;
        }

        public void ToggleSprint()
        {
            bool enable = !Player.Physical.Sprinting;
            CurrentState.EnableSprint(enable, true);
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

        public Vector3 MidPointLerp(Vector3 target)
        {
            return Vector3.Lerp(BotOwner.Position, target, 0.5f);
        }

        public bool BotIsAtPoint(Vector3 point, float reachDist = 1f)
        {
            return SqrMagToPoint(point) < reachDist;
        }

        public float SqrMagToPoint(Vector3 point)
        {
            return (point - BotOwner.Position).sqrMagnitude;
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
            Player.SlowLean(value);
        }

        private void UpdateLean()
        {
        }

        private float TargetLeanNumber = 0f;
        public float CurrentLeanNumber { get; private set; }
        public MovementState CurrentState => BotOwner.GetPlayer.MovementContext.CurrentState;
        public bool CanJump => BotOwner.GetPlayer.MovementContext.CanJump;

        private float JumpTimer = 0f;

        private readonly ManualLogSource Logger;
    }
}
