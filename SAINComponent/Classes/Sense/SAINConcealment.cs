using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Sense
{
    public class SAINConcealment : SAINBase, ISAINClass
    {
        public SAINConcealment(SAINComponentClass sain) : base(sain)
        {
        }
        public void Init()
        {
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        public bool IsConcealed { get; set; }

        public float ConcealmentAmount { get; set; }

        public const float MaxConcealmentDistance = 60f;
        public const float MaxFarConcealmentDistance = 100f;

        public const float ProneConcealmentMulti = 2f;
        public const float CrouchConcealmentMulti = 1.25f;
        public const float HighGroundMulti = 0.66f;
        public const float LowGroundMulti = 1.33f;

        public const float HeightDifferenceThreshold = 1f;
        public const float HeightAngleThreshold = 10;

        private bool CheckConcealement(Player observer, Player target, ConcealmentResults results, float threshold = 5f)
        {
            if (observer == null || target == null)
            {
                Logger.LogError("Observer Or Target Is Null");
                return false;
            }
            if (results == null)
            {
                results = new ConcealmentResults(observer, target);
            }
            if (results.Observer == null || results.Target == null)
            {
                Logger.LogError("Observer Or Target Is Null in Results");
                results = new ConcealmentResults(observer, target);
                return false;
            }

            results.TargetPosition = target.Position;
            results.ObserverPosition = observer.Position;
            results.TargetDirection = target.Position - observer.Position;
            results.TargetDirectionNormalized = results.TargetDirection.normalized;
            results.Distance = results.TargetDirection.magnitude;

            CheckConcealmentRayCast(results);
            float finalResult = results.RaycastConcealment;

            // if the target is far away, multiply the concealment by the scaled amount, which is a number between 0 and 1, add 1 so its 1 to 2.
            // so if they are at max base range, it would be concealment * 2, and if at max far range, it would be an extra * 2
            results.ConcealmentDistanceMulti = 1f + (results.Distance / Mathf.Clamp(results.Distance, 0f, MaxConcealmentDistance));
            finalResult *= results.ConcealmentDistanceMulti;

            if (results.Distance > MaxConcealmentDistance)
            {
                float ExtraDistance = Mathf.Clamp(results.Distance, MaxConcealmentDistance, MaxFarConcealmentDistance);
                results.FarConcealmentDistanceMulti = 1f + (ExtraDistance / MaxFarConcealmentDistance - MaxConcealmentDistance);
                finalResult *= results.FarConcealmentDistanceMulti;
            }

            results.isProne = target.IsInPronePose;
            results.isCrouched = target.PoseLevel < 0.5;

            if (results.isProne)
            {
                finalResult *= ProneConcealmentMulti;
            }
            else if (results.isCrouched)
            {
                finalResult *= CrouchConcealmentMulti;
            }

            results.HeightAngle = FindHeightAngle(observer, target);

            if (results.HeightAngle > HeightAngleThreshold)
            {
                results.HeightDifference = results.ObserverPosition.y - results.TargetPosition.y;

                if (results.HeightDifference > HeightDifferenceThreshold)
                {
                    // Observer has highground on target
                    finalResult *= HighGroundMulti;
                }
                else if (results.HeightDifference < -HeightDifferenceThreshold)
                {
                    // Observer has lowground on target
                    finalResult *= LowGroundMulti;
                }
            }

            results.Concealment = finalResult;

            return finalResult >= threshold;
        }

        public const float HardCover = 1f;
        public const float SoftCover = 1.5f;

        private void CheckConcealmentRayCast(ConcealmentResults results)
        {
            float concealment = 1f;
            LayerMask BaseMask = LayerMaskClass.HighPolyWithTerrainMask;
            LayerMask FoliageMask = LayerMaskClass.AI;

            Vector3 observerOrigin = results.Observer.MainParts[BodyPartType.head].Position;
            foreach (var bodyPart in results.Target.MainParts.Values)
            {
                Vector3 partPos = bodyPart.Position;
                Vector3 partDir = partPos - observerOrigin;
                Vector3 partDirNormalized = partDir.normalized;
                float distance = partDir.magnitude;
                if (Physics.Raycast(observerOrigin, partDirNormalized, distance, FoliageMask))
                {
                    concealment += SoftCover;
                }
                else if (Physics.Raycast(observerOrigin, partDirNormalized, distance, BaseMask))
                {
                    concealment += HardCover;
                }
            }
            results.RaycastConcealment = concealment;
        }

        private float FindHeightAngle(Player observer, Player target)
        {
            Vector3 observerPos = observer.Position;
            Vector3 targetPos = target.Position;

            Vector3 targetDir = targetPos - observerPos;
            Vector3 targetDirStraight = targetDir;
            targetDirStraight.y = 0f;

            return Vector3.Angle(targetDirStraight, targetDir);
        }

        public sealed class ConcealmentResults
        {
            public ConcealmentResults(Player observer, Player target)
            {
                Target = target;
                TargetName = target.Profile.Nickname;
                TargetId = target.Profile.Id;

                Observer = observer;
                ObserverName = observer.Profile.Nickname;
                ObserverId = observer.Profile.Id;
            }

            public readonly Player Target;
            public readonly string TargetName;
            public readonly string TargetId;
            public Vector3 TargetPosition;
            public Vector3 TargetDirection;
            public Vector3 TargetDirectionNormalized;

            public readonly Player Observer;
            public readonly string ObserverName;
            public readonly string ObserverId;
            public Vector3 ObserverPosition;

            public float Concealment;
            public float RaycastConcealment;

            public float Distance;
            public float ConcealmentDistanceMulti;
            public float FarConcealmentDistanceMulti;

            public bool isProne;
            public bool isCrouched;
            public float HeightDifference;
            public float HeightAngle;

            public void Log()
            {
                Logger.LogDebug($"Concealment: [{Concealment}] for target: [{TargetName}] for observer: [{ObserverName}] at distance: [{Distance}] " +
                    $"Info: Is Prone? [{isProne}] is Crouched: [{isCrouched}] " +
                    $"ConcealmentDistanceMulti: [{ConcealmentDistanceMulti}] FarConcealmentDistanceMulti: [{FarConcealmentDistanceMulti}] " +
                    $"HeightDifference: [{HeightDifference}] HeightAngle: [{HeightAngle}]");
            }

            public void OnGUI()
            {

            }
        }
    }
}
