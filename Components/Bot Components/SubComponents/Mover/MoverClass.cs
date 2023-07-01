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
using SAIN.Components;

namespace SAIN.Classes
{
    public class MoverClass : MonoBehaviour
    {
        private SAINComponent SAIN;
        private BotOwner BotOwner => SAIN?.BotOwner;

        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);

            BlindFire = new BlindFireClass(BotOwner);
            SideStep = new SideStepClass(BotOwner);
            Lean = new LeanClass(BotOwner);
            Prone = new ProneClass(BotOwner);
            Pose = new PoseClass(BotOwner);
        }

        private void Update()
        {
            if (BotOwner == null) return;

            SetStamina();

            Pose.Update();
            Lean.Update();
            SideStep.Update();
            Prone.Update();
            BlindFire.Update();
        }

        public BlindFireClass BlindFire { get; private set; }
        public SideStepClass SideStep { get; private set; }
        public LeanClass Lean { get; private set; }
        public PoseClass Pose { get; private set; }
        public ProneClass Prone { get; private set; }

        public bool GoToPoint(Vector3 point, float reachDist = -1f, bool crawl = false)
        {
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

        public bool CanGoToPoint(Vector3 point, out Vector3 pointToGo, bool mustHaveCompletePath = false, float navSampleRange = 10f)
        {
            pointToGo = point;
            if (NavMesh.SamplePosition(point, out var navHit, navSampleRange, -1))
            {
                NavMeshPath Path = new NavMeshPath();
                if (NavMesh.CalculatePath(SAIN.Position, navHit.position, -1, Path) && Path.corners.Length > 1)
                {
                    if (mustHaveCompletePath && Path.status != NavMeshPathStatus.PathComplete)
                    {
                        return false;
                    }
                    pointToGo = navHit.position;
                    return true;
                }
            }
            return false;
        }

        private void SetStamina()
        {
            var stamina = BotOwner.GetPlayer.Physical.Stamina;
            if (SAIN.LayersActive && stamina.NormalValue < 0.1f)
            {
                BotOwner.GetPlayer.Physical.Stamina.UpdateStamina(stamina.TotalCapacity / 8f);
            }
        }

        public void SetTargetPose(float pose)
        {
            Pose.SetTargetPose(pose);
        }

        public void SetTargetMoveSpeed(float speed)
        {
            BotOwner.Mover.SetTargetMoveSpeed(speed);
        }

        public float DestMoveSpeed { get; private set; }

        public void StopMove()
        {
            BotOwner.Mover.Stop();
            if (IsSprinting)
            {
                Sprint(false);
            }
        }

        public void Sprint(bool value)
        {
            IsSprinting = value;
            BotOwner.Mover.Sprint(value);
            if (value)
            {
                SAIN.Steering.LookToMovingDirection();
                FastLean(0f);
            }
        }

        public bool IsSprinting { get; private set; }

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
            if (BotOwner.GetPlayer.MovementContext.BlindFire != value)
            {
                BotOwner.GetPlayer.MovementContext.SetBlindFire(value);
            }
        }

        public void FastLean(LeanSetting value)
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
            FastLean(num);
        }

        public void FastLean(float value)
        {
            if (BotOwner.GetPlayer.MovementContext.Tilt != value)
            {
                BotOwner.GetPlayer.MovementContext.SetTilt(value);
            }
        }

        public MovementState CurrentState => BotOwner.GetPlayer.MovementContext.CurrentState;
        public bool CanJump => BotOwner.GetPlayer.MovementContext.CanJump;

        private float JumpTimer = 0f;

        private ManualLogSource Logger;
    }
}
