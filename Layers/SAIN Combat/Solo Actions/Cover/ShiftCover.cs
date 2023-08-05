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

namespace SAIN.Layers
{
    internal class ShiftCover : CustomLogic
    {
        public ShiftCover(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponentClass>();
            Shoot = new ShootClass(bot, SAIN);
        }

        private ShootClass Shoot;

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

        private readonly SAINComponentClass SAIN;

        public ManualLogSource Logger;

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
            if (UsedPoints.Count > 5)
            {
                UsedPoints.Clear();
            }
        }

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"SAIN Info:");
            stringBuilder.AppendLabeledValue("Personality", $"{SAIN.Info.Personality}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("BotType", $"{SAIN.Info.Profile.WildSpawnType}", Color.white, Color.yellow, true);
            if (NewPoint != null)
            {
                stringBuilder.AppendLine($"SAIN Cover Info:");
                stringBuilder.AppendLabeledValue("Cover Position", $"{NewPoint.Position}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover Distance", $"{NewPoint.Distance}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover Spotted?", $"{NewPoint.Spotted}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover Path Length", $"{NewPoint.Distance}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover ID", $"{NewPoint.Id}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover Status", $"{NewPoint.CoverStatus}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover HitInCoverCount", $"{NewPoint.HitInCoverCount}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover HitInCoverUnknownCount", $"{NewPoint.HitInCoverUnknownCount}", Color.white, Color.yellow, true);
            }
        }
    }
}