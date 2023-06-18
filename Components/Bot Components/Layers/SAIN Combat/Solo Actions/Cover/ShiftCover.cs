using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
using SAIN.Classes;
using SAIN.Layers;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.DebugConfig;
using System.Collections.Generic;

namespace SAIN.Layers
{
    internal class ShiftCover : CustomLogic
    {
        public ShiftCover(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            Shoot = new ShootClass(bot);
        }

        private ShootClass Shoot;

        public override void Update()
        {
            SAIN.Steering.SteerByPriority();
            Shoot.Update();
            if (NewPoint != null)
            {
                if ((NewPoint.Position - BotOwner.Position).sqrMagnitude < 2f)
                {
                    SAIN.Decision.EnemyDecisions.ShiftCoverComplete = true;
                }
                else
                {
                    SAIN.Mover.GoToPoint(NewPoint.Position);
                    SAIN.Mover.SetTargetMoveSpeed(1f);
                    SAIN.Mover.SetTargetPose(1f);
                }
            }
        }

        private readonly SAINComponent SAIN;
        public bool DebugMode => DebugLayers.Value;

        public ManualLogSource Logger;

        private void FindPointToGo()
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
        }

        public override void Start()
        {
            FindPointToGo();
        }

        private Vector3? StartPosition;
        private readonly List<CoverPoint> UsedPoints = new List<CoverPoint>();
        private CoverPoint NewPoint;

        public override void Stop()
        {
            NewPoint = null;
            if (UsedPoints.Count > 5)
            {
                UsedPoints.Clear();
            }
        }
    }
}