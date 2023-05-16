using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using static SAIN.UserSettings.DebugConfig;
using static SAIN.UserSettings.DogFightConfig;

namespace SAIN.Components
{
    public class LeanComponent : MonoBehaviour
    {
        private SAINComponent SAIN;

        private void Start()
        {
            BotOwner = GetComponent<BotOwner>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name + $": {BotOwner.name}: ");
            Lean = new Corners.FindDirectionToLean();
            SAIN = BotOwner.GetComponent<SAINComponent>();

            StartCoroutine(FindLeanAngleLoop());
            StartCoroutine(LeanConditionCheckLoop());
        }

        public void Dispose()
        {
            StopAllCoroutines();
            Destroy(this);
        }

        private IEnumerator LeanConditionCheckLoop()
        {
            while (true)
            {
                if (ShouldBotLean && !StartingLean)
                {
                    float random = Random.Range(0.2f, 0.5f);

                    StartCoroutine(ExecuteLean(random));
                }
                else if (ShouldBotReset && !Resetting && !HoldLean)
                {
                    float random = Random.Range(1f, 3f);

                    StartCoroutine(ResetLeanAfterDuration(random));
                }

                yield return null;
            }
        }

        private bool ShouldBotLean
        {
            get
            {
                if (BotOwner.Memory.GoalEnemy == null)
                {
                    return false;
                }
                if (BotOwner.BotState != EBotState.Active)
                {
                    return false;
                }
                if (BotOwner.Memory.IsInCover)
                {
                    return false;
                }
                if (SAIN.Core.Enemy.CanShoot)
                {
                    return false;
                }
                return true;
            }
        }

        private bool ShouldBotReset
        {
            get
            {
                if (BotOwner.Memory.GoalEnemy == null)
                {
                    return true;
                }
                if (BotOwner.BotState != EBotState.Active)
                {
                    return true;
                }
                if (BotOwner.Memory.IsInCover)
                {
                    return true;
                }
                if (SAIN.Core.Enemy.CanShoot)
                {
                    return true;
                }
                return false;
            }
        }

        public IEnumerator ExecuteLean(float delay)
        {
            StartingLean = true;

            yield return new WaitForSeconds(delay);

            if (ShouldBotLean)
            {
                float angle = LeaningRight ? 5 : -5;

                if (DebugMode)
                {
                    Logger.LogDebug($"Leaned. Leaned Right? {LeaningRight} so {angle} because {LeanAngle}");
                }

                SetLean(angle, true);
            }

            StartingLean = false;
        }

        private IEnumerator FindLeanAngleLoop()
        {
            while (true)
            {
                if (BotOwner.Memory.GoalEnemy != null)
                {
                    LeanAngle = Lean.FindLeanAngle(BotOwner.Transform.position, BotOwner.Memory.GoalEnemy.CurrPosition, BotOwner.LookSensor._headPoint, 10f);
                    yield return new WaitForSeconds(0.25f);
                }

                yield return null;
            }
        }

        private IEnumerator ResetLeanAfterDuration(float duration)
        {
            Resetting = true;

            yield return new WaitForSeconds(duration);

            if (ShouldBotReset)
            {
                SetLean(0f, false);
            }

            Resetting = false;
        }

        private void SetLean(float num, bool value)
        {
            if (num != LastLeanNum)
            {
                LastLeanNum = num;
                BotOwner.GetPlayer.MovementContext.SetTilt(num, false);
            }

            Lean.Leaning = value;
        }

        public bool LeaningRight => LeanAngle > 0f;
        public float LeanAngle { get; private set; }
        public bool HoldLean { get; set; }
        private bool DebugMode => DebugDynamicLean.Value;

        private Corners.FindDirectionToLean Lean;
        private BotOwner BotOwner;
        protected ManualLogSource Logger;
        private bool StartingLean;
        private bool Resetting;
        private float LastLeanNum = 0f;
    }
}