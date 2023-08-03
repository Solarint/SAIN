using BepInEx.Logging;
using EFT;
using SAIN.Components;
using UnityEngine;

namespace SAIN.Classes.Mover
{
    public class SideStepClass : SAINBot
    {
        public SideStepSetting SideStepSetting { get; private set; }

        public SideStepClass(SAINComponent bot) : base(bot)
        {
        }

        public void ResetSideStep(float current)
        {
            SideStepSetting = SideStepSetting.None;
            if (current != 0f)
            {
                GetPlayer.MovementContext.SetSidestep(0f);
            }
        }

        private float ResetCanShoot;

        public void Update()
        {
            float currentSideStep = CurrentSideStep;
            if (SideStepSetting != SideStepSetting.None && currentSideStep == 0f)
            {
                SideStepSetting = SideStepSetting.None;
            }

            var enemy = SAIN.Enemy;
            if (enemy == null || CurrentDecision != SAINSoloDecision.HoldInCover)
            {
                ResetSideStep(currentSideStep);
                return;
            }

            if (enemy.CanShoot)
            {
                if (ResetCanShoot == -1f)
                {
                    ResetCanShoot = Time.time + 2f;
                }
                if (ResetCanShoot < Time.time)
                {
                    ResetSideStep(currentSideStep);
                }
                return;
            }
            else
            {
                ResetCanShoot = -1f;
            }

            if (SideStepTimer > Time.time)
            {
                return;
            }

            float value;
            switch (SAIN.Mover.Lean.LeanDirection)
            {
                case LeanSetting.Left:
                    value = -1f;
                    SideStepSetting = SideStepSetting.Left;
                    break;

                case LeanSetting.Right:
                    value = 1f;
                    SideStepSetting = SideStepSetting.Right;
                    break;

                default:
                    value = 0f;
                    SideStepSetting = SideStepSetting.None;
                    break;
            }

            if (value != 0f)
            {
                SideStepTimer = Time.time + 2f;
            }
            else
            {
                SideStepTimer = Time.time + 0.5f;
            }

            SetSideStep(value, currentSideStep);
        }

        public bool SideStepActive => SideStepSetting != SideStepSetting.None && CurrentSideStep != 0f;

        private float SideStepTimer = 0f;

        public void SetSideStep(float value, float current)
        {
            if (current != value)
            {
                GetPlayer.MovementContext.SetSidestep(value);
            }
        }

        public float CurrentSideStep => GetPlayer.MovementContext.GetSidestep();
    }
}