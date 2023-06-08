using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Components;
using SAIN.Layers.Logic;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers
{
    internal class HoldInCoverAction : CustomLogic
    {
        public HoldInCoverAction(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponent>();
            this.AimData = new GClass105(bot);
        }

        private GClass105 AimData;

        public override void Update()
        {
            SAIN.Steering.ManualUpdate();

            if (SAIN.HasEnemyAndCanShoot)
            {
                AimData.Update();
            }

            SAIN.Cover.DuckInCover();

            var enemy = BotOwner.Memory.GoalEnemy;
            if (enemy != null)
            {
                if (Time.time - enemy.TimeLastSeenReal > 5f && !SAIN.HasEnemyAndCanShoot)
                {
                    if (RightMovePos == null && LeftMovePos == null)
                    {
                        SetMovePoints();
                    }
                    MoveAround();
                }
                else
                {
                    if (SAIN.CurrentTargetPosition != null)
                    {
                        SAIN.ShiftAwayFromCloseWall(SAIN.CurrentTargetPosition.Value);
                    }
                }
            }

            if (BotOwner.Memory.IsUnderFire)
            {
                BotOwner.SetPose(0f);
            }
            else
            {
                BotOwner.SetPose(0.66f);
            }
        }

        private readonly SAINComponent SAIN;
        public bool DebugMode => DebugLayers.Value;

        public ManualLogSource Logger;

        private void MoveAround()
        {
            if (SAIN.HasEnemyAndCanShoot)
            {
                BotOwner.SetTargetMoveSpeed(1f);
                BotOwner.GoToPoint(PositionToHold, true, -1, false, false);
                return;
            }
            else
            {
                BotOwner.SetPose(0.66f);
                BotOwner.SetTargetMoveSpeed(0f);
            }

            if (MoveTimer < Time.time)
            {
                MoveTimer = Time.time + 3f;

                if (MovingRight)
                {
                    if (RightMovePos != null)
                    {
                        BotOwner.GoToPoint(RightMovePos.Value, true, 0.15f, false, false);
                    }
                    else
                    {
                        BotOwner.GoToPoint(PositionToHold, true, 0.15f, false, false);
                    }
                }
                else
                {
                    if (LeftMovePos != null)
                    {
                        BotOwner.GoToPoint(LeftMovePos.Value, true, 0.15f, false, false);
                    }
                    else
                    {
                        BotOwner.GoToPoint(PositionToHold, true, 0.15f, false, false);
                    }
                }

                MovingRight = !MovingRight;
            }
        }

        private float MoveTimer = 0f;
        private bool MovingRight = true;

        private void SetMovePoints()
        {
            var right = RotatedDirection(90f);
            var rightMove = RayCastDir(right);
            if (NavSamplePos(rightMove, out var rightPos))
            {
                RightMovePos = rightPos;
            }

            var left = RotatedDirection(-90f);
            var leftMove = RayCastDir(left);
            if (NavSamplePos(leftMove, out var leftPos))
            {
                LeftMovePos = leftPos;
            }
        }

        private Vector3 RayCastDir(Vector3 direction)
        {
            direction.y = 0f;

            Vector3 result;

            var origin = PositionToHold;
            origin.y += 0.33f;

            if (Physics.Raycast(origin, direction, out var ray, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
            {
                result = ray.point;
                result.y -= 0.33f;

                return result;
            }
            else
            {
                return direction + PositionToHold;
            }
        }

        private bool NavSamplePos(Vector3 pos, out Vector3 navHitPos)
        {
            if (NavMesh.SamplePosition(pos, out var hit, 1f, -1))
            {
                navHitPos = hit.position;
                return true;
            }
            navHitPos = Vector3.zero;
            return false;
        }

        private Vector3 RotatedDirection(float angle)
        {
            var enemy = BotOwner.Memory.GoalEnemy;
            var direction = (enemy.CurrPosition - PositionToHold).normalized * 3f;
            direction.y = 0f;
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
            return rotation * direction;
        }

        public override void Start()
        {
            BotOwner.PatrollingData.Pause();

            BotOwner.MovementPause(0.1f);

            PositionToHold = BotOwner.Position;
        }

        private Vector3 PositionToHold;
        private Vector3? RightMovePos;
        private Vector3? LeftMovePos;

        public override void Stop()
        {
            BotOwner.PatrollingData.Unpause();
        }
    }
}