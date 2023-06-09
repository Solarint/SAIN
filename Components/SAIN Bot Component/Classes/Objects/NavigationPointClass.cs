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
    public class NavigationPointObject : SAINBot
    {
        private readonly NavMeshAgent Agent;

        public NavigationPointObject(BotOwner bot, NavMeshAgent agent) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            Agent = agent;
        }

        public bool Update(float reachDist = -1f)
        {
            if (CornerDestination != null && CurrentFinalDestination != null)
            {
                if (reachDist > 0)
                {
                    ReachDistance = reachDist;
                }

                if (CheckMoveTimer < Time.time)
                {
                    CheckMoveTimer = Time.time + 0.1f;
                    Vector3 FinalDest = CurrentFinalDestination.Value;

                    if (BotIsStuck && UnstuckMoveTimer < Time.time)
                    {
                        UnstuckMoveTimer = Time.time + 1f;
                        var pathStatus = GoToPoint(FinalDest, false);
                        if (pathStatus == NavMeshPathStatus.PathInvalid)
                        {
                            return false;
                        }
                    }

                    Vector3 CornerDest = CornerDestination.Value;
                    if (BotIsAtPoint(FinalDest, 1f, true))
                    {
                        Logger.LogDebug("Bot Arrived at Final Destination");
                        return false;
                    }

                    if (!BotIsAtPoint(CornerDest))
                    {
                        if (!AgentDestinationCompare(CornerDest))
                        {
                            Agent.SetDestination(CornerDest);
                        }
                    }
                    else
                    {
                        Logger.LogDebug("Bot Arrived at Corner, Moving to Next");
                        MoveNext();
                    }
                }
                return true;
            }
            return false;
        }

        private float UnstuckMoveTimer = 0f;
        public bool BotIsStuck => SAIN.BotStuck.BotIsStuck;
        private float CheckMoveTimer = 0f;

        private void MoveNext()
        {
            PathWayIndex++;
            if (PathWayIndex <= PathWayIndexMax)
            {
                CornerDestination = PathWay[PathWayIndex];
            }
        }

        public bool AgentDestinationCompare(Vector3 point)
        {
            return (Agent.destination - point).sqrMagnitude < 0.01f;
        }

        public NavMeshPathStatus GoToPoint(Vector3 point, bool MustHavePath = true, float reachDist = 0.5f)
        {
            NavMeshPath Path = new NavMeshPath();
            NavMesh.CalculatePath(BotOwner.Position, point, -1, Path);
            if (Path.status != NavMeshPathStatus.PathInvalid)
            {
                if (Path.corners.Length < 2)
                {
                    Logger.LogError($"Corners Length too low! [{Path.corners.Length}]");
                }
                else if (Path.status == NavMeshPathStatus.PathComplete || !MustHavePath)
                {
                    ReachDistance = reachDist > 0 ? reachDist : 0.5f;
                    PathWay = Path.corners;
                    PathWayIndex = 1;
                    PathWayIndexMax = Path.corners.Length - 1;
                    CornerDestination = Path.corners[1];
                    CurrentFinalDestination = Path.corners[Path.corners.Length - 1];
                    Logger.LogWarning($"{Path.status}");
                    return Path.status;
                }
            }

            Logger.LogWarning($"{Path.status}");
            return Path.status;
        }

        public bool BotIsAtPoint()
        {
            return CornerDestDistance < 0.5f;
        }

        public bool BotIsAtPoint(float reachDist = 1f, bool Sqr = true)
        {
            if (Sqr)
            {
                return CornerDestSqrDistance < reachDist;
            }
            return CornerDestDistance < reachDist;
        }

        public bool BotIsAtPoint(Vector3 point, float reachDist = 1f, bool Sqr = true)
        {
            if (Sqr)
            {
                return DistanceToDestinationSqr(point) < reachDist;
            }
            return DistanceToDestination(point) < reachDist;
        }

        public float DistanceToDestinationSqr(Vector3 point)
        {
            return (point - BotOwner.Transform.position).sqrMagnitude;
        }

        public float DistanceToDestination(Vector3 point)
        {
            return (point - BotOwner.Transform.position).magnitude;
        }

        public float ReachDistance { get; private set; }
        public Vector3? CurrentFinalDestination { get; set; }
        public Vector3? CornerDestination { get; set; }
        public float CornerDestDistance => (BotOwner.Position - CornerDestination.Value).magnitude;
        public float CornerDestSqrDistance => (BotOwner.Position - CornerDestination.Value).sqrMagnitude;
        public Vector3[] PathWay { get; private set; }
        public int PathWayIndex { get; private set; }
        public int PathWayIndexMax { get; private set; }

        private readonly ManualLogSource Logger;
    }
}
