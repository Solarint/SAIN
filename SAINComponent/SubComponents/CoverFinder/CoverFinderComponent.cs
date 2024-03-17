using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using SAIN.SAINComponent;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SAIN.SAINComponent.SubComponents.CoverFinder;

namespace SAIN.SAINComponent.SubComponents.CoverFinder
{
    public class CoverFinderComponent : MonoBehaviour, ISAINSubComponent
    {
        public void Init(SAINComponentClass sain)
        {
            SAIN = sain;
            Player = sain.Player;
            BotOwner = sain.BotOwner;

            ColliderFinder = new ColliderFinder();
            CoverAnalyzer = new CoverAnalyzer(SAIN, this);
        }

        public SAINComponentClass SAIN { get; private set; }
        public Player Player { get; private set; }
        public BotOwner BotOwner { get; private set; }

        public List<CoverPoint> CoverPoints { get; private set; } = new List<CoverPoint>();
        public CoverAnalyzer CoverAnalyzer { get; private set; }
        public ColliderFinder ColliderFinder { get; private set; }

        private Collider[] Colliders = new Collider[200];

        private void Update()
        {
            if (SAIN == null || BotOwner == null || Player == null)
            {
                Dispose();
                return;
            }
            if (DebugCoverFinder)
            {
                if (CoverPoints.Count > 0)
                {
                    DebugGizmos.Line(CoverPoints.PickRandom().Position, SAIN.Transform.Head, Color.yellow, 0.035f, true, 0.1f);
                }
            }
        }
        static bool DebugCoverFinder => SAINPlugin.LoadedPreset.GlobalSettings.Cover.DebugCoverFinder;

        public void LookForCover(Vector3 targetPosition, Vector3 originPoint)
        {
            TargetPosition = targetPosition;
            OriginPoint = originPoint;

            if (SAIN.Decision.CurrentSelfDecision == SelfDecision.RunAwayGrenade)
            {
                MinObstacleHeight = 1.5f;
                MinEnemyDist = 10f;
            }
            else
            {
                MinObstacleHeight = CoverMinHeight;
                MinEnemyDist = CoverMinEnemyDistance;
            }

            if (TakeCoverCoroutine == null)
            {
                TakeCoverCoroutine = StartCoroutine(FindCover());
            }
        }

        static float CoverMinHeight => SAINPlugin.LoadedPreset.GlobalSettings.Cover.CoverMinHeight;
        static float CoverMinEnemyDistance => SAINPlugin.LoadedPreset.GlobalSettings.Cover.CoverMinEnemyDistance;
        public float MinEnemyDist { get; private set; }

        public void StopLooking()
        {
            if (TakeCoverCoroutine != null)
            {
                StopCoroutine(TakeCoverCoroutine);
                TakeCoverCoroutine = null;
            }
        }

        private readonly Collider[] SinglePointColliders = new Collider[50];
        public CoverPoint SinglePoint { get; private set; }

        public bool FindSinglePoint(Vector3 origin, Vector3 target, out CoverPoint coverpoint)
        {
            if (SinglePointFinder == null)
            {
                SinglePoint = null;
                SinglePointFinder = StartCoroutine(FindSingle(origin, target));
            }
            coverpoint = SinglePoint;
            bool found = coverpoint != null;
            if (found && SinglePointFinder != null)
            {
                StopCoroutine(SinglePointFinder);
                SinglePointFinder = null;
            }
            return coverpoint != null;
        }

        private Coroutine SinglePointFinder;

        private IEnumerator FindSingle(Vector3 origin, Vector3 target)
        {
            var Counter = new FrameCounter(5);
            ColliderFinder.GetNewColliders(out int hits, origin, target, SinglePointColliders, 15f, 10f);
            for (int i = 0; i < hits; i++)
            {
                if (CoverAnalyzer.CheckCollider(SinglePointColliders[i], out var newPoint, 0.66f, origin, target, 10f))
                {
                    SinglePoint = newPoint;
                    break;
                }

                // Every X colliders checked, wait until the next frame before continuing.
                if (Counter.FrameWait)
                {
                    yield return null;
                }
            }
        }

        public CoverPoint FallBackPoint { get; private set; }

        private IEnumerator FindCover()
        {
            while (true)
            {
                UpdateSpotted();
                ClearOldPoints();
                GetColliders(out int hits);
                int totalChecked = 0;
                var Counter = new FrameCounter(5);
                for (int i = 0; i < hits; i++)
                {
                    totalChecked++;
                    if (CoverAnalyzer.CheckCollider(Colliders[i], out var newPoint, MinObstacleHeight, OriginPoint, TargetPosition, MinEnemyDist))
                    {
                        CoverPoints.Add(newPoint);
                    }
                    if (Counter.FrameWait)
                    {
                        yield return null;
                    }
                    if (CoverPoints.Count > 2)
                    {
                        break;
                    }
                }

                if (CoverPoints.Count > 0)
                {
                    FindFallback();

                    if (DebugLogTimer < Time.time && DebugCoverFinder)
                    {
                        DebugLogTimer = Time.time + 1f;
                        Logger.LogInfo($"[{BotOwner.name}] - Found [{CoverPoints.Count}] CoverPoints. Colliders checked: [{totalChecked}] Collider Array Size = [{hits}]");
                    }
                }
                else
                {
                    if (DebugLogTimer < Time.time && DebugCoverFinder)
                    {
                        DebugLogTimer = Time.time + 1f;
                        Logger.LogWarning($"[{BotOwner.name}] - No Cover Found! Valid Colliders checked: [{totalChecked}] Collider Array Size = [{hits}]");
                    }
                }
                yield return new WaitForSeconds(CoverUpdateFrequency);
            }
        }

        static float CoverUpdateFrequency => SAINPlugin.LoadedPreset.GlobalSettings.Cover.CoverUpdateFrequency;

        private void FindFallback()
        {
            CheckResetFallback();

            if (CoverPoints.Count > 0 && FallBackPoint == null)
            {
                float highest = 0f;
                int highestIndex = 0;
                for (int i = 0; i < CoverPoints.Count; i++)
                {
                    float Y = CoverPoints[i].Collider.bounds.size.y;
                    if (Y > highest)
                    {
                        highest = Y;
                        highestIndex = i;
                    }
                }
                FallBackPoint = CoverPoints[highestIndex];
            }
        }

        static float FallBackPointResetDistance = 35;
        static float FallBackPointNextAllowedResetDelayTime = 3f;
        static float FallBackPointNextAllowedResetTime = 0;
        
        private void CheckResetFallback()
        {
            if (FallBackPoint == null || Time.time < FallBackPointNextAllowedResetTime)
            {
                return;
            }

            if ((BotOwner.Position - FallBackPoint.Position).magnitude > FallBackPointResetDistance)
            {
                if (SAINPlugin.DebugMode)
                    Logger.LogInfo($"Resetting fallback point for {BotOwner.name}...");

                FallBackPoint = null;
                FallBackPointNextAllowedResetTime = Time.time + FallBackPointNextAllowedResetDelayTime;
            }
        }

        private float DebugLogTimer = 0f;

        public List<SpottedCoverPoint> SpottedPoints { get; private set; } = new List<SpottedCoverPoint>();
        private readonly List<SpottedCoverPoint> ReCheckList = new List<SpottedCoverPoint>();

        private void UpdateSpotted()
        {
            if (SpottedPoints.Count > 0)
            {
                ReCheckList.AddRange(SpottedPoints);
                for (int j = SpottedPoints.Count - 1; j >= 0; j--)
                {
                    var spotted = SpottedPoints[j];
                    if (spotted != null && spotted.IsValidAgain)
                    {
                        SpottedPoints.RemoveAt(j);
                    }
                }
                ReCheckList.Clear();
            }
        }

        private void ClearOldPoints()
        {
            CoverPoint PointToSave = null;

            if (CoverPoints.Count > 0)
            {
                for (int i = 0; i < CoverPoints.Count; i++)
                {
                    var coverPoint = CoverPoints[i];
                    if (coverPoint != null && Recheck(coverPoint))
                    {
                        PointToSave = coverPoint;
                    }
                }
            }
            CoverPoints.Clear();
            if (PointToSave != null)
            {
                CoverPoints.Add(PointToSave);
            }
        }

        private bool Recheck(CoverPoint coverPoint)
        {
            if (!PointIsSpotted(coverPoint) && coverPoint.BotIsUsingThis && CheckPoint(coverPoint, out var newPoint))
            {
                coverPoint.Position = newPoint.Position;
                return true;
            }
            return false;
        }

        private bool PointIsSpotted(CoverPoint point)
        {
            if (point == null)
            {
                return true;
            }
            if (point.Spotted)
            {
                SpottedPoints.Add(new SpottedCoverPoint(point.Position));
            }
            return point.Spotted;
        }

        private bool CheckPoint(CoverPoint point, out CoverPoint newPoint)
        {
            return CoverAnalyzer.CheckCollider(point.Collider, out newPoint, MinObstacleHeight, OriginPoint, TargetPosition, MinEnemyDist);
        }

        private void GetColliders(out int hits)
        {
            const float CheckDistThresh = 10f * 10f;
            const float ColliderSortDistThresh = 3f * 3f;

            float distance = (LastCheckPos - OriginPoint).sqrMagnitude;
            if (distance > CheckDistThresh)
            {
                LastCheckPos = OriginPoint;
                ColliderFinder.GetNewColliders(out hits, OriginPoint, TargetPosition, Colliders);
                LastHitCount = hits;
            }
            if (distance > ColliderSortDistThresh)
            {
                ColliderFinder.SortArrayBotDist(Colliders);
            }

            hits = LastHitCount;
        }

        private Vector3 LastCheckPos = Vector3.zero + Vector3.down * 100f;
        private int LastHitCount = 0;

        public void Dispose()
        {
            try {
                StopAllCoroutines();
                Destroy(this); }
            catch { }
        }

        private float MinObstacleHeight;
        private Coroutine TakeCoverCoroutine;
        private Vector3 OriginPoint;
        private Vector3 TargetPosition;
    }
}