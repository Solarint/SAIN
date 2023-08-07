using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent.Classes.Decision;
using SAIN.SAINComponent.Classes.Talk;
using SAIN.SAINComponent.Classes.WeaponFunction;
using SAIN.SAINComponent.Classes.Mover;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;
using SAIN.SAINComponent;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Text;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using SAIN.Layers.Combat.Solo;

namespace SAIN.Layers.Combat.Solo.Cover
{
    internal class ShiftCoverAction : SAINAction
    {
        public ShiftCoverAction(BotOwner bot) : base(bot, nameof(ShiftCoverAction))
        {
        }

        public override void Update()
        {
            SAIN.Steering.SteerByPriority();
            Shoot.Update();
            if (NewPoint == null)
            {
                if (FindPointToGo())
                {
                    SAIN.Mover.GoToPoint(NewPoint.Position);
                    SAIN.Mover.SetTargetMoveSpeed(1f);
                    SAIN.Mover.SetTargetPose(1f);
                }
            }
            else
            {
                if ((NewPoint.Position - BotOwner.Position).sqrMagnitude < 2f)
                {
                    SAIN.Decision.EnemyDecisions.ShiftCoverComplete = true;
                }
            }
        }

        private bool FindPointToGo()
        {
            var cover = SAIN.Cover.CoverInUse;
            if (cover != null && cover.BotIsHere)
            {
                cover.BotIsUsingThis = false;
                StartPosition = cover.Position;
                UsedPoints.Add(cover);
                var Points = SAIN.Cover.CoverFinder.CoverPoints;
                for (int i = 0; i < Points.Count; i++)
                {
                    if (!UsedPoints.Contains(Points[i]))
                    {
                        for (int j = 0; j < UsedPoints.Count; j++)
                        {
                            if ((UsedPoints[j].Position - Points[i].Position).sqrMagnitude > 9f)
                            {
                                Points[i].BotIsUsingThis = true;
                                NewPoint = Points[i];
                                SAIN.Mover.GoToPoint(NewPoint.Position);
                                SAIN.Mover.SetTargetMoveSpeed(1f);
                                SAIN.Mover.SetTargetPose(1f);
                                return true;
                            }
                        }
                    }
                }
                if (NewPoint == null)
                {
                    SAIN.Decision.EnemyDecisions.ShiftCoverComplete = true;
                }
            }
            else
            {
                StartPosition = BotOwner.Position;
            }
            return false;
        }

        public override void Start()
        {
        }

        private Vector3? StartPosition;
        private readonly List<CoverPoint> UsedPoints = new List<CoverPoint>();
        private CoverPoint NewPoint;

        public override void Stop()
        {
            NewPoint = null;
            UsedPoints.Clear();
        }

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            DebugOverlay.AddCoverInfo(SAIN, stringBuilder);
        }
    }
}