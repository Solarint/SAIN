using BepInEx.Logging;
using EFT;
using JetBrains.Annotations;
using SAIN.Components;
using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Classes
{
    public class EFTSteer : SAINBot
    {
        public EFTSteer(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            bifacialTransform_0 = bot.Transform;
            FirstTurnSpeed = BotOwner.Settings.FileSettings.Move.FIRST_TURN_SPEED;
            FirstTurnBigSpeed = BotOwner.Settings.FileSettings.Move.FIRST_TURN_BIG_SPEED;
            TurnSpeedSprint = BotOwner.Settings.FileSettings.Move.TURN_SPEED_ON_SPRINT;
        }

        public float Speed { get; private set; }

        public Vector3 LookDirection
        {
            get
            {
                return vector3_0;
            }
        }

        public EBotSteering SteeringMode { get; private set; }

        public void ManualFixedUpdate()
        {
            method_2();
        }

        public void SetYAngle(float angle)
        {
            if (bool_0)
            {
                return;
            }
            float num = Mathf.DeltaAngle(Player.Rotation.y, angle);
            BotOwner.AimingData.RotateY(num);
            Player.Rotate(new Vector2(0f, num), false);
        }

        public void LookToPoint(Vector3 point)
        {
            LookToPoint(point, BotOwner.Settings.FileSettings.Move.BASE_ROTATE_SPEED);
        }

        public void LookToPathDestPoint()
        {
            LookToPathDestPoint(BotOwner.Settings.FileSettings.Move.BASE_ROTATE_SPEED);
        }

        public void LookToMovingDirection()
        {
            LookToMovingDirection(BotOwner.Settings.FileSettings.Move.BASE_ROTATE_SPEED);
        }

        public void LookToPoint(Vector3 point, float rotateSpeed)
        {
            method_0(rotateSpeed);
            SteeringMode = EBotSteering.ToCustomPoint;
            vector3_1 = point;
        }

        public void LookToMovingDirection(float rotateSpeed)
        {
            method_0(rotateSpeed);
            SteeringMode = EBotSteering.ToMovingDirection;
        }

        public void LookToDirection(Vector3 dir)
        {
            LookToDirection(dir, BotOwner.Settings.FileSettings.Move.BASE_ROTATE_SPEED);
        }

        public void LookToDirection(Vector3 dir, float rotateSpeed)
        {
            method_0(rotateSpeed);
            SteeringMode = EBotSteering.Direction;
            if (dir.sqrMagnitude > 0f)
            {
                vector3_2 = dir;
            }
        }

        public void LookToPathDestPoint(float rotateSpeed)
        {
            method_0(rotateSpeed);
            SteeringMode = EBotSteering.ToDestPoint;
        }

        public void SetYByDir(Vector3 dir)
        {
            float magnitude = dir.magnitude;
            float num = -dir.y / magnitude;
            num = Mathf.Clamp(num, -1f, 1f);
            num = 57.29578f * Mathf.Asin(num);
            BotOwner.SetYAngle(-Mathf.Abs(num) * Mathf.Sign(dir.y));
        }

        private void method_0(float rotateSpeed)
        {
            Speed = rotateSpeed;
        }

        private void method_1(float degPerSec)
        {
            if (bool_0)
            {
                return;
            }
            float target;
            if (BotOwner.LookedTransform != null)
            {
                Vector3 normalized = (BotOwner.LookedTransform.position - BotOwner.WeaponRoot.position).normalized;
                target = 57.29578f * Mathf.Atan2(normalized.x, normalized.z);
            }
            else
            {
                target = 57.29578f * Mathf.Atan2(vector3_0.x, vector3_0.z);
            }
            float num = Mathf.DeltaAngle(Player.Rotation.x, target);
            if (BotOwner.BotLay.IsLay && num > BotOwner.Settings.FileSettings.Look.ANGLE_FOR_GETUP)
            {
                BotOwner.BotLay.GetUp(true);
            }
            float num2 = degPerSec * Time.deltaTime;
            float num3;
            if (num > 0f)
            {
                num3 = Mathf.Clamp(num, 0f, num2);
            }
            else
            {
                num3 = Mathf.Clamp(num, -num2, 0f);
            }
            BotOwner.AimingData?.RotateX(num3);
            Player.Rotate(new Vector2(num3, 0f), true);
        }

        private void method_2()
        {
            Vector3 vector = vector3_0;
            bool flag = false;
            bool flag2 = false;
            if (BotOwner.Mover.Sprinting && BotOwner.Mover.HasPathAndNoComplete)
            {
                vector3_0 = BotOwner.Mover.DirCurPoint;
                if (BotOwner.Mover.ShallSlowAtStart && !BotOwner.Mover.FirstTurnComplete)
                {
                    if (BotOwner.Mover.FirstTurnBigSpeed)
                    {
                        method_0(FirstTurnBigSpeed);
                    }
                    else
                    {
                        method_0(FirstTurnSpeed);
                    }
                }
                else if (BotOwner.Mover.IsMoving)
                {
                    method_0(TurnSpeedSprint);
                }
            }
            else
            {
                switch (SteeringMode)
                {
                    case EBotSteering.ToDestPoint:
                        {
                            Vector3 vector2 = BotOwner.Destination - bifacialTransform_0.position;
                            if (vector2.sqrMagnitude > 0f)
                            {
                                vector3_0 = vector2;
                            }
                            if (Mathf.Abs(vector3_0.y) < 0.001f)
                            {
                                flag = true;
                            }
                            break;
                        }
                    case EBotSteering.ToMovingDirection:
                        if (!method_3())
                        {
                            if (Player.MovementContext.Velocity.magnitude < 0.04f)
                            {
                                return;
                            }
                            flag = true;
                            vector3_0 = BotOwner.Mover.DirCurPoint;
                        }
                        break;
                    case EBotSteering.ToCustomPoint:
                        flag2 = true;
                        vector3_0 = vector3_1 - BotOwner.WeaponRoot.position;
                        break;
                    case EBotSteering.Direction:
                        vector3_0 = vector3_2;
                        if (Mathf.Abs(vector3_0.y) < 0.001f)
                        {
                            flag = true;
                        }
                        break;
                }
            }
            Vector3 vector3 = vector3_0;
            if (Mathf.Abs(vector3.x) <= Mathf.Epsilon && Mathf.Abs(vector3.z) <= Mathf.Epsilon)
            {
                if (!bool_1)
                {
                    bool_1 = true;
                }
                vector3_0 = vector;
            }
            method_1(Speed);
            if (flag)
            {
                SetYAngle(0f);
                return;
            }
            if (flag2)
            {
                SetYByDir(vector3_0);
                return;
            }
            SetYByDir(vector3_0);
        }

        private bool method_3()
        {
            if (BotOwner.CurrPath == null)
            {
                return false;
            }
            List<Corner> corners = BotOwner.CurrPath.Corners;
            if (corners.Count < 3)
            {
                return false;
            }
            float num = 2f;
            float num2 = (bifacialTransform_0.position - corners[1].Pos).magnitude;
            if (num2 >= num)
            {
                return false;
            }
            Vector3 a = corners[2].Pos - corners[1].Pos;
            if (a.magnitude < 0.5f && (float)corners.Count > 3f)
            {
                a = (corners[3].Pos - corners[2].Pos).normalized;
            }
            if (a.magnitude < 1f && (float)corners.Count > 4f)
            {
                a = (corners[4].Pos - corners[3].Pos).normalized;
            }
            a.Normalize();
            num2 = ((num2 < 0.01f) ? 0.01f : num2);
            float d = num / num2 - 1f;
            vector3_0 = a * d;
            return true;
        }

        public float FirstTurnSpeed = 160f;
        public float FirstTurnBigSpeed = 320f;
        public float TurnSpeedSprint = 200f;
        private Vector3 vector3_0 = Vector3.one;
        private Vector3 vector3_1;
        private Vector3 vector3_2;
        private readonly BifacialTransform bifacialTransform_0;
        private readonly bool bool_0;
        private bool bool_1;

        protected ManualLogSource Logger;
    }
}