using EFT;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public class TransformClass : SAINBase, ISAINClass
    {
        private readonly BifacialTransform Transform;

        public TransformClass(SAINComponentClass sain) : base(sain)
        {
            Transform = sain.BotOwner.Transform;
        }

        public void Init() 
        { 
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
        }

        private const int RemoveCount = 25;
        public readonly List<HistoryPosition> MovementHistory = new List<HistoryPosition>();

        private Vector3 LastCheckPos;
        private const int MaxHistoryCount = 200;
        private const float MovementThresh = 2f;
        private const float CheckHistoryFreq = 1f;
        private float CheckHistoryTimer;

        public Vector3 Position => Transform.position;
        public Vector3 MoveDirection => BotOwner.Mover.DirCurPoint;

        public Vector3 Forward => BotOwner.LookDirection;
        public Vector3 Right => Transform.right;
        public Vector3 Left => -Right;
        public Vector3 Back => -Forward;
        public Vector3 Up => Transform.up;
        public Vector3 Down => -Up;

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