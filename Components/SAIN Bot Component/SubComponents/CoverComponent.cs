using BepInEx.Logging;
using EFT;
using SAIN.Classes;
using SAIN.Components;
using SAIN.Helpers;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.CoverConfig;

namespace SAIN.Classes
{
    public class CoverComponent : MonoBehaviour
    {
        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            CoverFinder = BotOwner.GetOrAddComponent<CoverFinderComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            FallBackPointStatus = CoverStatus.None;
            CoverPointStatus = CoverStatus.None;
        }

        private const float InCoverDist = 1f;
        private const float CloseCoverDist = 10f;
        private const float FarCoverDist = 30f;

        private void Update()
        {
            if (!SAIN.BotActive || SAIN.GameIsEnding)
            {
                CoverFinder.StopLooking();
                return;
            }

            if (BotOwner.Memory.GoalEnemy == null && BotOwner.Memory.GoalTarget?.GoalTarget == null)
            {
                CoverFinder.StopLooking();
            }
            else if (UpdateTimer < Time.time)
            {
                AssignSettings();

                UpdateTimer = Time.time + CoverUpdateFrequency.Value;

                Vector3? targetPosition = GetPointToHideFrom();

                if (targetPosition != null)
                {
                    //DebugGizmos.SingleObjects.Line(targetPosition.Value, BotOwner.MyHead.position, Color.red, 0.05f, true, CoverUpdateFrequency.Value, true);

                    CoverFinder.LookForCover(targetPosition.Value, BotOwner.Transform.position);

                    //DrawDebug(targetPosition.Value);
                }

                FallBackPointStatus = CheckCoverPointStatus(CurrentFallBackPoint, InCoverDist, CloseCoverDist, FarCoverDist);
                CoverPointStatus = CheckCoverPointStatus(CurrentCoverPoint, InCoverDist, CloseCoverDist, FarCoverDist);

                //Logger.LogWarning($"Cover Statuses: {CoverPointStatus} : {FallBackPointStatus}");

                if (CurrentFallBackPoint == null && CurrentCoverPoint == null)
                {
                    //Logger.LogError($"CurrentFallBackPoint == null && CurrentCoverPoint == null");
                }
            }
        }

        private Vector3? GetPointToHideFrom()
        {
            if (SAIN.Decisions.CurrentDecision == SAINLogicDecision.RunAwayGrenade)
            {
                return SAIN.BotOwner.BewareGrenade.GrenadeDangerPoint.DangerPoint;
            }
            else if (BotOwner.Memory.GoalEnemy != null)
            {
                return SAIN.BotOwner.Memory.GoalEnemy.CurrPosition;
            }
            else if (BotOwner.Memory.GoalTarget?.GoalTarget != null)
            {
                return BotOwner.Memory.GoalTarget.GoalTarget.Position;
            }
            else { return null; }
        }

        private CoverStatus CheckCoverPointStatus(CoverPoint cover, float inCoverDist, float closeCoverDist, float farCoverDist)
        {
            CoverStatus status = CoverStatus.None;

            if (cover != null)
            {
                if (IsBotCloseToCover(cover, inCoverDist))
                {
                    status = CoverStatus.InCover;
                }
                else if (IsBotCloseToCover(cover, closeCoverDist))
                {
                    status = CoverStatus.CloseToCover;
                }
                else if (IsBotCloseToCover(cover, farCoverDist))
                {
                    status = CoverStatus.FarFromCover;
                }
            }

            return status;
        }

        public bool IsBotCloseToCover(CoverPoint cover, float distThreshold = 15f)
        {
            if (cover != null)
            {
                if (Vector3.Distance(cover.Position, BotOwner.Transform.position) <= distThreshold)
                {
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(BotOwner.Transform.position, cover.Position, NavMesh.AllAreas, path))
                    {
                        float length = path.CalculatePathLength();

                        return length <= distThreshold * 1.5f;
                    }
                }
            }
            return false;
        }

        public bool DuckInCover(bool SetPose1ifFalse = true)
        {
            bool duckInCover = false;
            if (FallBackPointStatus == CoverStatus.InCover)
            {
                duckInCover = true;
            }
            else if (CoverPointStatus == CoverStatus.InCover)
            {
                duckInCover = true;
            }

            if (duckInCover)
            {
                SAIN.BotOwner.SetPose(0f);
            }
            else if (SetPose1ifFalse)
            {
                SAIN.BotOwner.SetPose(1f);
            }

            return duckInCover;
        }

        public bool CheckSelfForCover(float minratio = 0.1f)
        {
            int rays = 0;
            int cover = 0;


            BotOwner.Memory.GoalEnemy.Person.MainParts.TryGetValue(BodyPartType.head, out BodyPartClass EnemyHead);
            var headPos = EnemyHead.Position;

            foreach (var part in BotOwner.MainParts.Values)
            {
                rays++;

                Vector3 direction = part.Position - headPos;

                if (Physics.Raycast(headPos, direction, direction.magnitude, SAINComponent.ShootMask))
                {
                    cover++;
                }
            }

            float coverRatio = (float)cover / rays;

            return coverRatio > minratio;
        }

        private void AssignSettings()
        {
            switch (SAIN.CurrentDecision)
            {
                case SAINLogicDecision.Reload:
                case SAINLogicDecision.Surgery:
                case SAINLogicDecision.FirstAid:
                case SAINLogicDecision.RunAway:
                case SAINLogicDecision.RunAwayGrenade:
                    FallingBack = true;
                    CoverFinder.MinObstacleHeight = 1.55f;
                    break;

                default:
                    FallingBack = false;
                    CoverFinder.MinObstacleHeight = CoverMinHeight.Value;
                    break;
            }
        }

        public CoverStatus FallBackPointStatus { get; private set; }

        public CoverStatus CoverPointStatus { get; private set; }

        private BotOwner BotOwner => SAIN.BotOwner;

        private SAINComponent SAIN;

        public bool BotIsAtCoverPoint => ActiveCoverPoint != null;

        public CoverPoint ActiveCoverPoint
        {
            get
            {
                if (CoverPointStatus == CoverStatus.InCover)
                {
                    return CurrentCoverPoint;
                }
                else if (FallBackPointStatus == CoverStatus.InCover)
                {
                    return CurrentFallBackPoint;
                }
                else
                {
                    return null;
                }
            }
        }

        public CoverFinderComponent CoverFinder { get; private set; }

        public bool FallingBack = false;

        public CoverPoint CurrentCoverPoint => CoverFinder.CurrentCover;

        public CoverPoint CurrentFallBackPoint => CoverFinder.CurrentFallBackPoint;

        protected ManualLogSource Logger;

        private float UpdateTimer = 0f;

        public void Dispose()
        {
            StopAllCoroutines();
            CoverFinder.Dispose();
            Destroy(this);
        }
    }
}