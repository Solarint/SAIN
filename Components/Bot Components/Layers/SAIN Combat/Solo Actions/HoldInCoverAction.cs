using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.Classes.CombatFunctions;
using SAIN.Components;
using SAIN.Layers;
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
            Shoot = new ShootClass(bot);
        }

        private ShootClass Shoot;

        public override void Update()
        {
            SAIN.Steering.Steer();

            Shoot.Update();

            SAIN.Cover.DuckInCover();

            var enemy = BotOwner.Memory.GoalEnemy;
            if (enemy != null)
            {
                if (Time.time - enemy.PersonalLastShootTime > 5f && !SAIN.HasEnemyAndCanShoot)
                {
                    if (RightMovePos == null && LeftMovePos == null)
                    {
                        SetMovePoints();
                    }
                    MoveAround();
                }
            }

            if (BotOwner.Memory.IsUnderFire)
            {
                SAIN.Mover.SetTargetPose(0f);
            }
            else
            {
                SAIN.Mover.SetTargetPose(0.66f);
            }
        }

        private readonly SAINComponent SAIN;
        public bool DebugMode => DebugLayers.Value;

        public ManualLogSource Logger;

        private void MoveAround()
        {
            if (SAIN.HasEnemyAndCanShoot)
            {
                SAIN.Mover.SetTargetMoveSpeed(1f);
                BotOwner.GoToPoint(PositionToHold, false, -1, false, false);
                return;
            }
            else
            {
                SAIN.Mover.SetTargetPose(0.66f);
                SAIN.Mover.SetTargetMoveSpeed(0f);
            }

            if (MoveTimer < Time.time)
            {
                MoveTimer = Time.time + 3f;

                if (MovingRight)
                {
                    if (RightMovePos != null)
                    {
                        BotOwner.GoToPoint(RightMovePos.Value, false, 0.15f, false, false);
                    }
                    else
                    {
                        BotOwner.GoToPoint(PositionToHold, false, 0.15f, false, false);
                    }
                }
                else
                {
                    if (LeftMovePos != null)
                    {
                        BotOwner.GoToPoint(LeftMovePos.Value, false, 0.15f, false, false);
                    }
                    else
                    {
                        BotOwner.GoToPoint(PositionToHold, false, 0.15f, false, false);
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
            BotOwner.StopMove();

            var cover = SAIN.Cover.ClosestPoint;
            if (cover != null)
            {
                PositionToHold = SAIN.Cover.ClosestPoint.Position;
            }
            else
            {
                PositionToHold = BotOwner.Position;
            }
        }

        private Vector3 PositionToHold;
        private Vector3? RightMovePos;
        private Vector3? LeftMovePos;

        public override void Stop()
        {
        }
    }
}