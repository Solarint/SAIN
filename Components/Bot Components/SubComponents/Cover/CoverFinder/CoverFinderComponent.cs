using BepInEx.Logging;
using EFT;
using SAIN.Classes;
using SAIN.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SAIN.Components.CoverFinder;
using static SAIN.UserSettings.CoverConfig;

namespace SAIN.Components
{
    public class CoverFinderComponent : MonoBehaviour
    {
        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            ColliderFinder = new ColliderFinder();
            CoverAnalyzer = new CoverAnalyzer(BotOwner, this);
        }

        public List<CoverPoint> CoverPoints { get; private set; } = new List<CoverPoint>();
        public CoverAnalyzer CoverAnalyzer { get; private set; }
        public ColliderFinder ColliderFinder { get; private set; }

        private Collider[] Colliders;

        private void Update()
        {
            if (DebugCoverFinder.Value)
            {
                if (CoverPoints.Count > 0)
                {
                    DebugGizmos.SingleObjects.Line(CoverPoints.PickRandom().Position, SAIN.HeadPosition, Color.yellow, 0.035f, true, 0.1f);
                }
            }
        }

        public void LookForCover(Vector3 targetPosition, Vector3 originPoint)
        {
            TargetPosition = targetPosition;
            OriginPoint = originPoint;

            if (SAIN.Decision.CurrentSelfDecision == SAINSelfDecision.RunAwayGrenade)
            {
                MinObstacleHeight = 1.5f;
                MinEnemyDist = 10f;
            }
            else
            {
                MinObstacleHeight = CoverMinHeight.Value;
                MinEnemyDist = CoverMinEnemyDistance.Value;
            }

            if (TakeCoverCoroutine == null)
            {
                TakeCoverCoroutine = StartCoroutine(FindCover());
            }
        }

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

                    if (DebugLogTimer < Time.time && DebugCoverFinder.Value)
                    {
                        DebugLogTimer = Time.time + 1f;
                        Logger.LogInfo($"[{BotOwner.name}] - Found [{CoverPoints.Count}] CoverPoints. Colliders checked: [{totalChecked}] Collider Array Size = [{hits}]");
                    }
                }
                else
                {
                    if (DebugLogTimer < Time.time && DebugCoverFinder.Value)
                    {
                        DebugLogTimer = Time.time + 1f;
                        Logger.LogWarning($"[{BotOwner.name}] - No Cover Found! Valid Colliders checked: [{totalChecked}] Collider Array Size = [{hits}]");
                    }
                }
                yield return new WaitForSeconds(CoverUpdateFrequency.Value);
            }
        }

        private void FindFallback()
        {
            if (CoverPoints.Count > 0)
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

        private float DebugLogTimer = 0f;

        public List<SpottedCoverPoint> SpottedPoints { get; private set; } = new List<SpottedCoverPoint>();
        private readonly List<SpottedCoverPoint> ReCheckList = new List<SpottedCoverPoint>();

        private void UpdateSpotted()
        {
            if (SpottedPoints.Count > 0)
            {
                ReCheckList.AddRange(SpottedPoints);
                for (int j = ReCheckList.Count - 1; j >= 0; j--)
                {
                    var spotted = ReCheckList[j];
                    if (spotted != null)
                    {
                        if (spotted.IsValidAgain)
                        {
                            SpottedPoints.RemoveAt(j);
                        }
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
                    var point = CoverPoints[i];
                    if (point != null)
                    {
                        if (point.Spotted)
                        {
                            SpottedPoints.Add(new SpottedCoverPoint(point.Position));
                            continue;
                        }
                        if (point.BotIsUsingThis)
                        {
                            if (CoverAnalyzer.CheckCollider(point.Collider, out var newPoint, MinObstacleHeight, OriginPoint, TargetPosition, MinEnemyDist))
                            {
                                PointToSave = newPoint;
                                PointToSave.BotIsUsingThis = true;
                                PointToSave.HitInCoverCount = point.HitInCoverCount;
                                if (PointToSave.Collider.bounds.size.y >= 1.5f)
                                {
                                    FallBackPoint = newPoint;
                                }
                                else
                                {
                                    FallBackPoint = null;
                                }
                            }
                        }
                    }
                }
            }
            CoverPoints.Clear();
            if (PointToSave != null)
            {
                CoverPoints.Add(PointToSave);
            }
        }

        private void GetColliders(out int hits)
        {
            const float CheckDistThresh = 100f;
            const float ColliderSortDistThresh = 25f;

            float distance = (LastCheckPos - OriginPoint).sqrMagnitude;
            if (Colliders == null || distance > CheckDistThresh)
            {
                if (Colliders == null)
                {
                    Colliders = new Collider[200];
                }
                LastCheckPos = OriginPoint;
                ColliderFinder.GetNewColliders(out hits, OriginPoint, TargetPosition, Colliders);
                LastHitCount = hits;
                return;
            }
            else if (distance > ColliderSortDistThresh)
            {
                ColliderFinder.SortArrayBotDist(Colliders);
            }

            hits = LastHitCount;
        }

        private Vector3 LastCheckPos;
        private int LastHitCount = 0;

        public void OnDestroy()
        {
            StopAllCoroutines();
        }

        private BotOwner BotOwner => SAIN.BotOwner;
        private SAINComponent SAIN;
        protected ManualLogSource Logger;
        private float MinObstacleHeight;
        private Coroutine TakeCoverCoroutine;
        private Vector3 OriginPoint;
        private Vector3 TargetPosition;
    }
}