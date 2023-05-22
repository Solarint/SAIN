using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using System.Collections;
using UnityEngine;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Components
{
    public enum LeanSetting
    {
        None = 0,
        Left = 1,
        Right = 2
    }

    public class LeanClass : SAINBotExt
    {
        public LeanSetting LeanSetting { get; private set; }

        public LeanClass(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name + $": {BotOwner.name}: ");
            Lean = new Corners.FindDirectionToLean(BotOwner);
        }

        public void ManualUpdate()
        {
            if (BotOwner.BotState != EBotState.Active || BotOwner.IsDead)
            {
                return;
            }

            if (!AllowLeanDecision)
            {
                if (OldAllowLeanDecision)
                {
                    ResetTimer = Time.time + Random.Range(1f, 2f);
                }
                if (ResetTimer < Time.time)
                {
                    SetLean(0f, false);
                }
            }

            if (BotOwner.Memory.IsUnderFire)
            {
                if (!OldUnderFire)
                {
                    UnderFireRealTime = Time.time;
                }
                if (Time.time - UnderFireRealTime > 1f)
                {
                    SetLean(0f, false);
                }
            }

            OldUnderFire = BotOwner.Memory.IsUnderFire;

            if (AllowLeanDecision)
            {
                if (FindLeanAngleTimer < Time.time && GetTargetPosition(out var target))
                {
                    FindLeanAngleTimer = Time.time;
                    FindLeanAngleTimer += FindLeanFreq;

                    float leanAngle;
                    if (SAIN.Cover.BotIsAtCoverPoint)
                    {
                        leanAngle = LeanFromCover(SAIN.Cover.ActiveCoverPoint, target);
                    }
                    else
                    {
                        leanAngle = Lean.FindLeanAngle(target);
                        //FindLeanAngleTimer += LeanAngle == 0f ? FindLeanFreq : FindLeanFreq * 3f;
                    }

                    LeanSetting = FindSetting(leanAngle, out float angle);

                    if (DebugMode)
                    {
                        Logger.LogDebug($"Leaned. Leaned [{LeanSetting}] so [{angle}] because [{leanAngle}]");
                    }

                    SetLean(angle, true);
                }
            }

            OldAllowLeanDecision = AllowLeanDecision;
        }

        public bool GetTargetPosition(out Vector3 targetPos)
        {
            if (BotOwner.Memory.GoalEnemy != null)
            {
                targetPos = BotOwner.Memory.GoalEnemy.CurrPosition;
                return true;
            }
            else if (BotOwner.Memory.GoalTarget?.GoalTarget?.Position != null)
            {
                targetPos = BotOwner.Memory.GoalTarget.GoalTarget.Position;
                return true;
            }
            else
            {
                targetPos = Vector3.zero;
                return false;
            }
        }

        private LeanSetting FindSetting(float leanAngle, out float inputNum)
        {
            LeanSetting set;

            if (leanAngle > 0)
            {
                set = LeanSetting.Right;
                inputNum = 5f;
            }
            else if (leanAngle < 0)
            {
                set = LeanSetting.Left;
                inputNum = -5f;
            }
            else
            {
                set = LeanSetting.None;
                inputNum = 0f;
            }

            return set;
        }

        private float LeanFromCover(CoverPoint coverPoint, Vector3 targetPos)
        {
            float angle = 0f;
            if (coverPoint != null)
            {
                angle = Vector3.SignedAngle(coverPoint.PositionToCollider.normalized, targetPos, Vector3.up);
            }
            return angle;
        }

        private bool AllowLeanDecision => SAIN.CurrentDecision == SAINLogicDecision.Fight || SAIN.CurrentDecision == SAINLogicDecision.Search || SAIN.CurrentDecision == SAINLogicDecision.HoldInCover || SAIN.CurrentDecision == SAINLogicDecision.Suppress || SAIN.CurrentDecision == SAINLogicDecision.DogFight || SAIN.CurrentDecision == SAINLogicDecision.Skirmish;

        private void SetLean(float num, bool value)
        {
            if (num != LastLeanNum)
            {
                LastLeanNum = num;
                BotOwner.GetPlayer.MovementContext.SetTilt(num, false);
            }
            Lean.Leaning = value;
        }

        private bool DebugMode => DebugDynamicLean.Value;

        private float FindLeanAngleTimer = 0f;
        private const float FindLeanFreq = 0.1f;
        private float UnderFireRealTime = 0f;
        private bool OldUnderFire = false;
        private float ResetTimer = 0f;
        private bool OldAllowLeanDecision = false;
        private readonly Corners.FindDirectionToLean Lean;
        private readonly ManualLogSource Logger;
        private float LastLeanNum = 0f;
    }
}