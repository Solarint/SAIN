using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
using System.Text;
using UnityEngine;

namespace SAIN.Layers
{
    internal class WalkToCover : CustomLogic
    {
        public WalkToCover(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            Shoot = new ShootClass(bot, SAIN);
        }

        private readonly ManualLogSource Logger;

        public override void Update()
        {
            if (CoverDestination != null)
            {
                if (!SAIN.Cover.CoverPoints.Contains(CoverDestination) || CoverDestination.Spotted)
                {
                    CoverDestination = null;
                }
            }

            SAIN.Mover.SetTargetMoveSpeed(1f);
            SAIN.Mover.SetTargetPose(1f);

            if (CoverDestination == null)
            {
                if (FindTargetCover())
                {
                    if (SAIN.Mover.Prone.ShallProne(CoverDestination, true) || SAIN.Mover.Prone.IsProne)
                    {
                        SAIN.Mover.Prone.SetProne(true);
                        SAIN.Mover.StopMove();
                    }
                    else
                    {
                        RecalcPathTimer = Time.time + 2f;
                        MoveTo(DestinationPosition);
                    }
                }
            }
            if (CoverDestination != null && RecalcPathTimer < Time.time)
            {
                RecalcPathTimer = Time.time + 2f;
                MoveTo(DestinationPosition);
            }

            EngageEnemy();
        }

        private float RecalcPathTimer = 0f;

        private bool FindTargetCover()
        {
            var coverPoint = SAIN.Cover.ClosestPoint;
            if (coverPoint != null && !coverPoint.Spotted)
            {
                if (CanMoveTo(coverPoint, out Vector3 pointToGo))
                {
                    coverPoint.BotIsUsingThis = true;
                    CoverDestination = coverPoint;
                    DestinationPosition = pointToGo;
                    return true;
                }
                else
                {
                    coverPoint.BotIsUsingThis = false;
                }
            }
            return false;
        }

        private void MoveTo(Vector3 position)
        {
            CoverDestination.BotIsUsingThis = true;
            SAIN.Mover.GoToPoint(position);
            SAIN.Mover.SetTargetMoveSpeed(1f);
            SAIN.Mover.SetTargetPose(1f);
        }

        private bool CanMoveTo(CoverPoint coverPoint, out Vector3 pointToGo)
        {
            if (coverPoint != null && SAIN.Mover.CanGoToPoint(coverPoint.Position, out pointToGo))
            {
                return true;
            }
            pointToGo = Vector3.zero;
            return false;
        }

        private CoverPoint CoverDestination;
        private Vector3 DestinationPosition;

        private void EngageEnemy()
        {
            if (SAIN.Enemy?.IsVisible == false && SAIN.Enemy.Seen && SAIN.Enemy.TimeSinceSeen < 5f && SAIN.Enemy.LastCornerToEnemy != null && SAIN.Enemy.CanSeeLastCornerToEnemy)
            {
                Vector3 corner = SAIN.Enemy.LastCornerToEnemy.Value;
                corner += Vector3.up * 1f;
                SAIN.Steering.LookToPoint(corner);
                if (BotOwner.WeaponManager.HaveBullets)
                {
                    BotOwner.ShootData.Shoot();
                }
            }
            else
            {
                SAIN.Steering.SteerByPriority(false);
                if (SAIN.Enemy != null)
                {
                    SAIN.Steering.LookToEnemy(SAIN.Enemy);
                }
                Shoot.Update();
            }
        }

        private readonly ShootClass Shoot;

        public override void Start()
        {
            SAIN.Mover.Sprint(false);
        }

        public override void Stop()
        {
            CoverDestination = null;
        }
        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"SAIN Info:");
            stringBuilder.AppendLabeledValue("Personality", $"{SAIN.Info.BotPersonality}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("BotType", $"{SAIN.Info.BotType}", Color.white, Color.yellow, true);
            CoverPoint cover = SAIN.Cover.CoverInUse;
            if (cover != null)
            {
                stringBuilder.AppendLine($"SAIN Cover Info:");
                stringBuilder.AppendLabeledValue("Cover Position", $"{cover.Position}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover Distance", $"{cover.Distance}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover Spotted?", $"{cover.Spotted}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover Path Length", $"{cover.PathDistance}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover ID", $"{cover.Id}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover Status", $"{cover.CoverStatus}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover HitInCoverCount", $"{cover.HitInCoverCount}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Cover HitInCoverUnknownCount", $"{cover.HitInCoverUnknownCount}", Color.white, Color.yellow, true);
            }
        }

        private readonly SAINComponent SAIN;
    }
}