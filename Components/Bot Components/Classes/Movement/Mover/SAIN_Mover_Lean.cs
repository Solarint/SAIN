using BepInEx.Logging;
using EFT;
using UnityEngine;

namespace SAIN.Classes.Mover
{
    public class SAIN_Mover_Lean : SAINBot
    {
        public LeanSetting LeanDirection { get; private set; }

        public SAIN_Mover_Lean(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        public void SetLean(float num)
        {
            SAIN.Mover.FastLean(num);
        }

        public float TiltNumber
        {
            get
            {
                float angle;
                switch (LeanDirection)
                {
                    case LeanSetting.Left:
                        angle = -5f;
                        break;

                    case LeanSetting.Right:
                        angle = 5f;
                        break;

                    default:
                        angle = 0f;
                        break;
                }
                return angle;
            }
        }

        private readonly ManualLogSource Logger;

        public LeanSetting FindLeanDirectionRayCast(Vector3 targetPos)
        {
            if (RayTimer < Time.time)
            {
                RayTimer = Time.time + 0.25f;

                DirectLineOfSight = CheckOffSetRay(targetPos, 0f, 0f, out var direct);

                RightLos = CheckOffSetRay(targetPos, 90f, 0.66f, out var rightOffset);
                if (!RightLos)
                {
                    RightLosPos = rightOffset;

                    rightOffset.y = BotOwner.Position.y;
                    float halfDist1 = (rightOffset - BotOwner.Position).magnitude / 2f;

                    RightHalfLos = CheckOffSetRay(targetPos, 90f, halfDist1, out var rightHalfOffset);
                    if (!RightHalfLos)
                    {
                        RightHalfLosPos = rightHalfOffset;
                    }
                    else
                    {
                        RightHalfLosPos = null;
                    }
                }
                else
                {
                    RightLosPos = null;
                    RightHalfLosPos = null;
                }

                LeftLos = CheckOffSetRay(targetPos, -90f, 0.66f, out var leftOffset);
                if (!LeftLos)
                {
                    LeftLosPos = leftOffset;

                    leftOffset.y = BotOwner.Position.y;
                    float halfDist2 = (leftOffset - BotOwner.Position).magnitude / 2f;

                    LeftHalfLos = CheckOffSetRay(targetPos, -90f, halfDist2, out var leftHalfOffset);

                    if (!LeftHalfLos)
                    {
                        LeftHalfLosPos = leftHalfOffset;
                    }
                    else
                    {
                        LeftHalfLosPos = null;
                    }
                }
                else
                {
                    LeftLosPos = null;
                    LeftHalfLosPos = null;
                }
            }

            var setting = GetSettingFromResults();
            LeanDirection = setting;
            return setting;
        }

        private float RayTimer = 0f;

        public LeanSetting GetSettingFromResults()
        {
            LeanSetting setting;

            if (BotOwner.Memory.GoalEnemy != null && DirectLineOfSight)
            {
                //return LeanSetting.Normal;
            }

            if ((LeftLos || LeftHalfLos) && !RightLos)
            {
                setting = LeanSetting.Left;
            }
            else if (!LeftLos && (RightLos || RightHalfLos))
            {
                setting = LeanSetting.Right;
            }
            else
            {
                setting = LeanSetting.None;
            }

            return setting;
        }

        private bool CheckOffSetRay(Vector3 targetPos, float angle, float dist, out Vector3 Point)
        {
            Vector3 startPos = BotOwner.Position;
            startPos.y = SAIN.HeadPosition.y;

            if (dist > 0f)
            {
                var dirToEnemy = (targetPos - BotOwner.Position).normalized;

                Quaternion rotation = Quaternion.Euler(0, angle, 0);

                Vector3 direction = rotation * dirToEnemy;

                Point = FindOffset(startPos, direction, dist);

                if ((Point - startPos).magnitude < dist / 3f)
                {
                    return true;
                }
            }
            else
            {
                Point = startPos;
            }

            bool LOS = LineOfSight(Point, targetPos);

            Point.y = BotOwner.Position.y;

            return LOS;
        }

        private bool LineOfSight(Vector3 start, Vector3 target)
        {
            var direction = target - start;
            float distance = Mathf.Clamp(direction.magnitude, 0f, 8f);
            return !Physics.Raycast(start, direction, distance, LayerMaskClass.HighPolyWithTerrainMask);
        }

        private Vector3 FindOffset(Vector3 start, Vector3 direction, float distance)
        {
            if (Physics.Raycast(start, direction, out var hit, distance, LayerMaskClass.HighPolyWithTerrainMask))
            {
                return hit.point;
            }
            else
            {
                return start + direction.normalized * distance;
            }
        }

        public bool DirectLineOfSight { get; set; }

        public bool LeftLos { get; set; }
        public Vector3? LeftLosPos { get; set; }

        public bool LeftHalfLos { get; set; }
        public Vector3? LeftHalfLosPos { get; set; }

        public bool RightLos { get; set; }
        public Vector3? RightLosPos { get; set; }

        public bool RightHalfLos { get; set; }
        public Vector3? RightHalfLosPos { get; set; }
    }
}