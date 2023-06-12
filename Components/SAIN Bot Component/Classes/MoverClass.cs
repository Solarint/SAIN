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
            if (JumpTimer < Time.time)
            {
                JumpTimer = Time.time + 1f;
                BotOwner.GetPlayer.MovementContext.TryJump();
            }
        }

        private float JumpTimer = 0f;

        private readonly ManualLogSource Logger;
    }
}
