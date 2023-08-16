using EFT;
using SAIN.Helpers;
using SAIN.Helpers.SealedClass;
using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

namespace SAIN.SAINComponent.Classes
{
    public sealed class PeekPosition
    {
        public PeekPosition(Vector3 point, Vector3 danger)
        {
            Point = point;
            Vector3 direction = danger - point;
            DangerDir = direction;
            DangerDirNormal = direction.Normalize(out float magnitude);
            DangerDistance = magnitude;
        }

        public readonly Vector3 DangerDir;
        public readonly Vector3 DangerDirNormal;
        public readonly float DangerDistance;
        public readonly Vector3 Point;
    }

    public sealed class MoveDangerPoint
    {
        public MoveDangerPoint(Vector3 start, Vector3 end, Vector3 dangerPoint, Vector3 corner)
        {
            PeekStart = new PeekPosition(start, dangerPoint);
            Vector3 mid = MidPoint(start, end);
            PeekMid = new PeekPosition(mid, dangerPoint);
            PeekEnd = new PeekPosition(end, dangerPoint);

            Vector3 midDir = PeekMid.DangerDir;
            FirstLookPoint = start + midDir;
            SecondLookPoint = end + midDir;

            DangerPoint = dangerPoint;
            Corner = corner;
        }

        public PeekPosition PeekStart { get; private set; }
        public Vector3 StartPeekPosition => PeekStart.Point;
        public Vector3 FirstLookPoint { get; private set; }

        public PeekPosition PeekMid { get; private set; }
        public Vector3 PeekMidPoint => PeekMid.Point;

        public PeekPosition PeekEnd { get; private set; }
        public Vector3 EndPeekPosition => PeekEnd.Point;
        public Vector3 SecondLookPoint { get; private set; }

        public Vector3 DangerPoint { get; private set; }
        public Vector3 Corner { get; private set; }

        private Vector3 MidPoint(Vector3 A, Vector3 B)
        {
            return Vector3.Lerp(A, B, 0.5f);
        }

        private bool CheckIfLeanable(float signAngle, float limit = 1f)
        {
            return Mathf.Abs(signAngle) > limit;
        }

        public LeanSetting GetDirectionToLean(float signAngle)
        {
            if (CheckIfLeanable(signAngle))
            {
                return signAngle > 0 ? LeanSetting.Right : LeanSetting.Left;
            }
            return LeanSetting.None;
        }

        private List<Vector3> DebugVectorList;
        private List<GameObject> DebugGameObjectList;

        public void DisposeDebug()
        {
            if (DebugVectorList != null)
            {
                DebugVectorList.Clear();
                DebugVectorList = null;
            }
            if (DebugGameObjectList != null)
            {
                for (int i = 0; i < DebugGameObjectList.Count; i++)
                {
                    UnityEngine.Object.Destroy(DebugGameObjectList[i]);
                }
                DebugGameObjectList.Clear();
                DebugGameObjectList = null;
            }
        }

        public void DrawDebug()
        {
            if (SAINPlugin.DebugModeEnabled == false)
            {
                DisposeDebug();
                return;
            }
            if (DebugVectorList == null)
            {
                DebugVectorList = new List<Vector3>
                {
                    PeekStart.Point,
                    PeekMid.Point,
                    PeekEnd.Point,
                    DangerPoint,
                };
            }
            if (DebugGameObjectList == null)
            {
                DebugGameObjectList = DebugGizmos.DrawLinesBetweenPoints(0.1f, 0.05f, DebugVectorList.ToArray());
            }
        }
    }
}