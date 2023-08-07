using EFT;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public class OldTransformClass : SAINBase
    {
        private readonly BifacialTransform Transform;

        public OldTransformClass(SAINComponentClass sain) : base(sain)
        {
            Transform = sain.BotOwner.Transform;
        }

        public void Update()
        {
            if (CheckHistoryTimer < Time.time)
            {
                CheckHistoryTimer = Time.time + CheckHistoryFreq;
                float sqrMag = (Position - LastCheckPos).sqrMagnitude;
                if (sqrMag > MovementThresh)
                {
                    LastCheckPos = Position;
                    MovementHistory.Add(new HistoryPosition(Position));
                }
                if (MovementHistory.Count > MaxHistoryCount)
                {
                    MovementHistory.RemoveRange(0, RemoveCount);
                }
            }
        }

        public void Dispose()
        {
            MovementHistory.Clear();
        }

        private const int RemoveCount = 25;
        public readonly List<HistoryPosition> MovementHistory = new List<HistoryPosition>();

        private Vector3 LastCheckPos;
        private const int MaxHistoryCount = 200;
        private const float MovementThresh = 2f;
        private const float CheckHistoryFreq = 1f;
        private float CheckHistoryTimer;

        public Vector3 Position => Transform.position;

        public sealed class HistoryPosition
        {
            public HistoryPosition(Vector3 pos)
            {
                Position = pos;
                TimeCreated = Time.time;
            }

            public readonly Vector3 Position;
            public readonly float TimeCreated;
        }
    }
}