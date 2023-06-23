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
    public class JumpClass : SAINBot
    {
        public JumpClass(BotOwner owner) : base(owner)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        private readonly ManualLogSource Logger;

        public void Update()
        {
            if (!JumpingOff && JumpOffLedgeTimer < Time.time && LookForJumpableLedges())
            {
                Logger.LogWarning($"JUMPIN OFF");
                JumpingOff = true;
                JumpOffLedgeTimer = Time.time + 6f;
                NewPathAfterJumpTimer = Time.time + 4f;
                return;
            }

            if (JumpingOff)
            {
                if (NewPathAfterJumpTimer < Time.time)
                {
                    Logger.LogWarning($"JUMP DONE");
                    JumpingOff = false;
                }
            }
        }

        private float JumpOffLedgeTimer = 0f;
        private float NewPathAfterJumpTimer = 0f;
        bool JumpingOff = false;

        private bool LookForJumpableLedges()
        {
            for (int i = 0; i < 50; i++)
            {
                if (FindJumpPoint())
                {
                    return true;
                }
            }
            return false;
        }

        private bool FindJumpPoint()
        {
            Vector3 randomPos = UnityEngine.Random.onUnitSphere * 3f;
            randomPos.y = BotOwner.Position.y;
            Vector3 head = SAIN.HeadPosition;
            Vector3 direction = randomPos - head;
            var Ray = new Ray(head, direction);
            if (!Physics.SphereCast(Ray, 0.1f, direction.magnitude))
            {
                direction = randomPos - BotOwner.Position;
                if (CanJumpOffLedge(direction, out Vector3 result))
                {
                    BotOwner.GetPlayer.Move(direction);
                    return true;
                }
            }
            return false;
        }

        private bool FindNavEdge(Vector3 origin, out NavMeshHit edge)
        {
            edge = new NavMeshHit();
            if (NavMesh.SamplePosition(origin, out var navHit, 0.05f, -1))
            {
                if (NavMesh.FindClosestEdge(navHit.position, out edge, -1))
                {
                    return true;
                }
            }
            return false;
        }

        private bool CanJumpOffLedge(Vector3 direction, out Vector3 result)
        {
            Vector3 start = SAIN.BodyPosition;
            var Mask = LayerMaskClass.HighPolyCollider;
            var Ray = new Ray(start, direction);
            result = Vector3.zero;
            if (Physics.SphereCast(Ray, 0.15f, out var hit, 3f, Mask))
            {
                Vector3 hitDir = (hit.point - start) * 0.85f;
                Vector3 CheckDownPos = start + hitDir;
                Ray = new Ray(CheckDownPos, Vector3.down);
                if (Physics.SphereCast(Ray, 0.15f, out var hitDown, 20f, Mask))
                {
                    if (NavMesh.SamplePosition(hitDown.point, out var NavHit, 0.25f, -1))
                    {
                        result = NavHit.position;
                        return true;
                    }
                }

            }
            return false;
        }
    }
}
