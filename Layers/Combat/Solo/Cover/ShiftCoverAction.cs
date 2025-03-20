using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Helpers;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SAIN.Layers.Combat.Solo.Cover
{
    internal class ShiftCoverAction : CombatAction, ISAINAction
    {
        public ShiftCoverAction(BotOwner bot) : base(bot, nameof(ShiftCoverAction))
        {
        }

        public void Toggle(bool value)
        {
            ToggleAction(value);
        }

        public override void Update(CustomLayer.ActionData data)
        {
            Bot.Steering.SteerByPriority();
            Shoot.CheckAimAndFire();
            if (NewPoint == null
                && FindPointToGo())
            {
                Bot.Mover.SetTargetMoveSpeed(GetSpeed());
                Bot.Mover.SetTargetPose(GetPose());
            }
            else if (NewPoint != null && NewPoint.StraightDistanceStatus == CoverStatus.InCover)
            {
                Bot.Decision.EnemyDecisions.ShiftCoverComplete = true;
            }
            else if (NewPoint != null)
            {
                Bot.Mover.SetTargetMoveSpeed(GetSpeed());
                Bot.Mover.SetTargetPose(GetPose());
                Bot.Mover.GoToPoint(NewPoint.Position, out _);
            }
            else
            {
                Bot.Decision.EnemyDecisions.ShiftCoverComplete = true;
            }
        }

        private float GetSpeed()
        {
            var settings = Bot.Info.PersonalitySettings;
            return Bot.HasEnemy ? settings.Cover.MoveToCoverHasEnemySpeed : settings.Cover.MoveToCoverNoEnemySpeed;
        }

        private float GetPose()
        {
            var settings = Bot.Info.PersonalitySettings;
            return Bot.HasEnemy ? settings.Cover.MoveToCoverHasEnemyPose : settings.Cover.MoveToCoverNoEnemyPose;
        }

        private bool FindPointToGo()
        {
            if (NewPoint != null)
            {
                return true;
            }

            var coverInUse = Bot.Cover.CoverInUse;
            if (coverInUse != null)
            {
                if (NewPoint == null)
                {
                    if (!UsedPoints.Contains(coverInUse))
                    {
                        UsedPoints.Add(coverInUse);
                    }

                    List<CoverPoint> coverPoints = Bot.Cover.CoverFinder.CoverPoints;

                    for (int i = 0; i < coverPoints.Count; i++)
                    {
                        CoverPoint shiftCoverTarget = coverPoints[i];

                        if (shiftCoverTarget.CoverHeight > coverInUse.CoverHeight
                            && !UsedPoints.Contains(shiftCoverTarget))
                        {
                            for (int j = 0; j < UsedPoints.Count; j++)
                            {
                                if ((UsedPoints[j].Position - shiftCoverTarget.Position).sqrMagnitude > 5f
                                    && Bot.Mover.GoToPoint(shiftCoverTarget.Position, out _))
                                {
                                    Bot.Cover.CoverInUse = shiftCoverTarget;
                                    NewPoint = shiftCoverTarget;
                                    return true;
                                }
                            }
                        }
                    }
                }
                if (NewPoint == null)
                {
                    Bot.Decision.EnemyDecisions.ShiftCoverComplete = true;
                }
            }
            return false;
        }

        public override void Start()
        {
            Toggle(true);
            Bot.Decision.EnemyDecisions.ShiftCoverComplete = false;
        }

        private readonly List<CoverPoint> UsedPoints = new List<CoverPoint>();
        private CoverPoint NewPoint;

        public override void Stop()
        {
            Toggle(false);
            Bot.Cover.CheckResetCoverInUse();
            NewPoint = null;
            UsedPoints.Clear();
        }

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine("Shift Cover Info");
            var cover = Bot.Cover;
            stringBuilder.AppendLabeledValue("CoverFinder State", $"{cover.CurrentCoverFinderState}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Cover Count", $"{cover.CoverPoints.Count}", Color.white, Color.yellow, true);
            if (Bot.CurrentTargetPosition != null)
            {
                stringBuilder.AppendLabeledValue("Current Target Position", $"{Bot.CurrentTargetPosition.Value}", Color.white, Color.yellow, true);
            }
            else
            {
                stringBuilder.AppendLabeledValue("Current Target Position", null, Color.white, Color.yellow, true);
            }

            if (NewPoint != null)
            {
                stringBuilder.AppendLine("Cover In Use");
                stringBuilder.AppendLabeledValue("Status", $"{NewPoint.StraightDistanceStatus}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Height / Value", $"{NewPoint.CoverHeight} {NewPoint.HardData.Value}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Path Length", $"{NewPoint.PathLength}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Straight Distance", $"{(NewPoint.Position - Bot.Position).magnitude}", Color.white, Color.yellow, true);
            }
        }
    }
}