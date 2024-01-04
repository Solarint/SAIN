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
using System.Text;
using UnityEngine;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using SAIN.Layers.Combat.Solo;

namespace SAIN.Layers.Combat.Solo.Cover
{
    internal class WalkToCoverAction : SAINAction
    {
        public WalkToCoverAction(BotOwner bot) : base(bot, nameof(WalkToCoverAction))
        {
            
        }

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
        private float SuppressTimer;

        private void EngageEnemy()
        {
            if (SAIN.Enemy?.IsVisible == false && SAIN.Enemy.Seen && SAIN.Enemy.TimeSinceSeen < 5f && SAIN.Enemy.LastCornerToEnemy != null && SAIN.Enemy.CanSeeLastCornerToEnemy)
            {
                Vector3 corner = SAIN.Enemy.LastCornerToEnemy.Value;
                corner += Vector3.up * 1f;
                SAIN.Steering.LookToPoint(corner);
                if (SuppressTimer < Time.time && BotOwner.WeaponManager.HaveBullets)
                {
                    SuppressTimer = Time.time + 0.5f * Random.Range(0.66f, 1.25f);
                    SAIN.Shoot();
                }
            }
            else
            {
                if (!BotOwner.ShootData.Shooting)
                {
                    SAIN.Steering.SteerByPriority(false);
                }
                Shoot.Update();
            }
        }


        public override void Start()
        {
            SAIN.Mover.Sprint(false);

            if (SAIN.HasEnemy)
            {
                Logger.LogInfo($"The current enemy of {BotOwner.name} is {SAIN.Enemy.EnemyPerson.BotOwner.name}");
            }
            else
            {
                Logger.LogInfo($"The current target of {BotOwner.name} is {SAIN.CurrentTargetPosition.ToString()}");
            }

        }

        public override void Stop()
        {
            CoverDestination = null;
        }

        public override void BuildDebugText(StringBuilder stringBuilder)
        {
            DebugOverlay.AddCoverInfo(SAIN, stringBuilder);
        }
    }
}