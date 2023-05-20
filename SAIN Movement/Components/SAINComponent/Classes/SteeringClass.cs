using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using SAIN.Components;
using UnityEngine;
using static SAIN.Helpers.Corners;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers.Logic
{
    public class SteeringClass : SAINBotExt
    {
        public SteeringClass(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        private bool DebugMode => DebugUpdateSteering.Value;

        public void ManualUpdate()
        {
            if (SAIN.Core.Enemy.CanSee && BotOwner.Memory.GoalEnemy != null)
            {
                BotOwner.Steering.LookToDirection(BotOwner.Memory.GoalEnemy.Direction, 150f);
            }
            else
            {
                SetLookPointByHearing();
            }
        }

        private bool NextCornerPosition(out Vector3 corner)
        {
            Vector3[] corners = CornerProcessing.GetCorners(BotOwner.Transform.position, BotOwner.Memory.GoalEnemy.CurrPosition, BotOwner.LookSensor._headPoint, true, false, true);
            if (corners.Length > 2)
            {
                corner = corners[1];

                if (Vector3.Distance(BotOwner.Transform.position, corners[1]) > 5f)
                {
                    return false;
                }

                if (DebugMode && DebugTimer < Time.time)
                {
                    float freq = 0.33f;
                    DebugTimer = Time.time + freq;
                    DebugGizmos.SingleObjects.Sphere(corner, 0.25f, Color.red, true, freq);
                    DebugGizmos.SingleObjects.Line(corner, BotOwner.MyHead.position, Color.red, 0.1f, true, freq);
                }

                return true;
            }
            else
            {
                if (SAIN.Core.Enemy.EnemyChestPosition != null)
                {
                    corner = SAIN.Core.Enemy.EnemyChestPosition.Value;
                }
                else
                {
                    corner = Vector3.zero;
                }
                return false;
            }
        }

        public void SetLookPointByHearing()
        {
            bool flag = true;
            Vector3? vector = null;
            float lastSeenTimeReal = -1f;

            var hearPoint = BotOwner.BotsGroup.YoungestPlace(BotOwner, BotOwner.Settings.FileSettings.Hearing.DIST_PLACE_TO_FIND_POINT, true);

            GClass475 enemy = null;
            if (BotOwner.Memory.GoalEnemy != null)
            {
                enemy = BotOwner.Memory.GoalEnemy;
            }
            else if (BotOwner.Memory.LastEnemy != null)
            {
                enemy = BotOwner.Memory.LastEnemy;
            }

            if (enemy != null)
            {
                lastSeenTimeReal = Time.time - enemy.TimeLastSeen;
                if (lastSeenTimeReal < 3f && enemy.Distance < 20f)
                {
                    vector = new Vector3?(enemy.CurrPosition - BotOwner.Position);
                }
                else if (lastSeenTimeReal < BotOwner.Settings.FileSettings.Cover.LOOK_LAST_ENEMY_POS_MOVING)
                {
                    if (lastSeenTimeReal >= 1f && BotOwner.Memory.LastDamageDataActive)
                    {
                        vector = new Vector3?(BotOwner.Memory.LastDamageData.Postion - BotOwner.Position);
                    }
                    else
                    {
                        vector = new Vector3?(enemy.EnemyLastPosition - BotOwner.Position);
                    }
                }
                else if (BotOwner.Memory.LastDamageDataActive)
                {
                    vector = new Vector3?(BotOwner.Memory.LastDamageData.Postion - BotOwner.Position);
                }
                else if (hearPoint != null)
                {
                    bool flag2 = false;
                    if (Time.time - hearPoint.CreatedTime >= BotOwner.Settings.FileSettings.Hearing.LOOK_ONLY_DANGER_DELTA || hearPoint.Type == PlaceForCheckType.danger)
                    {
                        flag2 = true;
                    }
                    if (flag2)
                    {
                        flag = false;
                        vector = new Vector3?(CheckWallsHear(hearPoint.Position));
                    }
                }
                else if (lastSeenTimeReal < 5f)
                {
                    vector = new Vector3?(enemy.EnemyLastPosition - BotOwner.Position);
                }
            }

            if (vector == null && BotOwner.Memory.LastDamageDataActive && hearPoint != null)
            {
                vector = new Vector3?(CheckWallsHear(hearPoint.Position));
            }

            bool LookLastEnemyPos = enemy != null && Time.time - enemy.TimeLastSeen < BotOwner.Settings.FileSettings.Hearing.LOOK_ONLY_DANGER_DELTA;

            if (vector == null)
            {
                if (hearPoint != null && LookLastEnemyPos && !hearPoint.IsDanger)
                {
                    hearPoint = null;
                }

                if (hearPoint != null && Time.time - hearPoint.CreatedTime < BotOwner.Settings.FileSettings.Look.OLD_TIME_POINT)
                {
                    vector = new Vector3?(CheckWallsHear(hearPoint.Position));
                }
            }

            CoverPoint Cover = null;
            if (SAIN.Cover.CoverPointStatus == CoverStatus.InCover)
            {
                Cover = SAIN.Cover.CurrentCoverPoint;
            }
            else if (SAIN.Cover.FallBackPointStatus == CoverStatus.InCover)
            {
                Cover = SAIN.Cover.CurrentFallBackPoint;
            }

            if (vector == null)
            {
                if (BotOwner.Mover.IsMoving)
                {
                    BotOwner.Steering.LookToMovingDirection();
                }
                else
                {
                    int num2 = DirectionInt;

                    if (Time.time > ChangeDirectionTimer && Cover != null)
                    {
                        ChangeDirectionTimer = Time.time + 2f * Random.Range(0.5f, 1.5f);
                        if (num2 > 0)
                        {
                            num2 = -1;
                        }
                        else
                        {
                            num2 = 1;
                        }
                    }
                    if (Cover != null)
                    {
                        if (SAIN.Cover.InCover || SAIN.Cover.CoverPointStatus != CoverStatus.None)
                        {
                            vector = new Vector3?(RotateWallBySide(Cover.PositionToCollider, num2));
                        }
                    }
                }
            }

            if (vector == null && BotOwner.Memory.LastEnemy != null && Time.time - BotOwner.Memory.LastEnemy.TimeLastSeen < BotOwner.Settings.FileSettings.Mind.LAST_ENEMY_LOOK_TO)
            {
                vector = new Vector3?(BotOwner.Memory.LastEnemy.EnemyLastPosition - BotOwner.Transform.position);
            }

            if (vector != null)
            {
                if (Cover == null)
                {
                    return;
                }

                if (enemy == null && hearPoint != null && hearPoint.IsDanger)
                {
                    flag = false;
                }

                bool flag4 = enemy == null || Time.time - enemy.TimeLastSeen > 20f;

                if (flag && Cover != null && flag4 && (BotOwner.Transform.position - Cover.Position).sqrMagnitude < 4f)
                {
                    Vector3 vector2 = VectorHelpers.NormalizeFastSelf(vector.Value);

                    if (VectorHelpers.IsAngLessNormalized(vector2, Cover.PositionToCollider, 0.5f))
                    {
                        Vector3 directionLeft = Quaternion.Euler(0f, -90, 0f) * Cover.PositionToEnemy;
                        Vector3 directionRight = Quaternion.Euler(0f, 90, 0f) * Cover.PositionToEnemy;

                        float num5 = Vector3.Angle(directionLeft, vector2);
                        float num6 = Vector3.Angle(directionRight, vector2);

                        bool flag5 = lastSeenTimeReal > 0f && lastSeenTimeReal < 20f;

                        if (num5 < num6 && flag5)
                        {
                            vector = new Vector3?(RotateWallBySide(Cover.PositionToCollider, 1));
                        }
                        else if (flag5)
                        {
                            vector = new Vector3?(RotateWallBySide(Cover.PositionToCollider, -1));
                        }
                        else
                        {
                            vector = new Vector3?(VectorHelpers.RotateOnAngUp(-Cover.PositionToEnemy, EFT_Math.Random(-10f, 10f)));
                        }
                    }
                }

                BotOwner.Steering.LookToDirection(vector.Value, 200f);
                return;
            }
            else
            {
                BotOwner.Steering.LookToMovingDirection(200f);
            }
        }

        private Vector3 CheckWallsHear(Vector3 hearPointPos)
        {
            Vector3 direction = hearPointPos - BotOwner.MyHead.position;
            if (direction.magnitude < 5f)
            {
                return direction;
            }
            if (Physics.Raycast(BotOwner.MyHead.position, direction, 20f, LayerMaskClass.HighPolyWithTerrainMask))
            {
                return VectorHelpers.Test4Sides(direction, BotOwner.MyHead.position);
            }
            return direction;
        }

        public Vector3 RotateWallBySide(Vector3 coverPosToCollider, int side = 0)
        {
            Vector3 toWallVector = coverPosToCollider;
            if (side == 0)
            {
                DirectionInt = -DirectionInt;
                side = DirectionInt;
            }
            int offset_LOOK_ALONG_WALL_ANG = BotOwner.Settings.FileSettings.Cover.OFFSET_LOOK_ALONG_WALL_ANG;
            int num = EFT_Math.RandomInclude(90 - offset_LOOK_ALONG_WALL_ANG, 90 + offset_LOOK_ALONG_WALL_ANG);
            return VectorHelpers.RotateOnAngUp(toWallVector, (float)(side * num));
        }

        private int DirectionInt = 1;
        private float ChangeDirectionTimer;
        protected ManualLogSource Logger;
        private float DebugTimer = 0f;
    }
}