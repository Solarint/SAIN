using EFT;
using UnityEngine.AI;
using UnityEngine;
using BepInEx.Logging;
using SAIN.Components;
using DrakiaXYZ.BigBrain.Brains;
using SAIN.Helpers;
using System.Collections.Generic;
using SAIN.Classes;

namespace SAIN.Layers.Actions
{
    public class JumpAction : CustomLogic
    {
        public JumpAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
        }

        private readonly ManualLogSource Logger;
        private readonly SAINComponent SAIN;
        private NavigationPointObject Move;

        public override void Update()
        {
            Vector3 playerPos = Plugin.MainPlayerPosition;
            if (Move.FinishedPath || Move.ActivePath == null)
            {
                Move = new NavigationPointObject(BotOwner);
                Move.GoToPoint(Plugin.MainPlayerPosition, false);
            }
            if (Move.FinalDestination != null && (playerPos - Move.FinalDestination.Point).magnitude > 3f)
            {
                Move = new NavigationPointObject(BotOwner);
                Move.GoToPoint(Plugin.MainPlayerPosition, false);
            }

            Move.Update();
            playerPos.y += 1.4f;
            BotOwner.Steering.LookToPoint(playerPos);
        }

        private bool Stopped = false;
        private float JumpTimer = 0f;

        public override void Start()
        {
            BotOwner.SetTargetMoveSpeed(1f);
            BotOwner.SetPose(1f);
            Move = new NavigationPointObject(BotOwner);
            Move.GoToPoint(Plugin.MainPlayerPosition, false);
        }

        public override void Stop()
        {
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
                    DirectionForJump = result - BotOwner.Position;
                    DirectionForJump.y = BotOwner.Position.y;
                    return true;
                }
            }
            return false;
        }

        Vector3 DirectionForJump;

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
                    if (hitDown.distance > 0.5f)
                    {
                        if (NavMesh.SamplePosition(hitDown.point, out var NavHit, 0.25f, -1))
                        {
                            result = NavHit.position;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
