using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using System.Collections;
using UnityEngine;
using static SAIN.UserSettings.DebugConfig;
using static SAIN.UserSettings.DogFightConfig;

namespace SAIN.Components
{
    public class LeanComponent : MonoBehaviour
    {
        public LeanComponent(BotOwner bot)
        {
            BotOwner = bot;
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name + $": {bot.name}: ");
            Lean = new Corners.FindDirectionToLean();
        }

        private void Start()
        {
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
                if (BotOwner.Memory.GoalEnemy != null && BotOwner.Brain.ActiveLayerName() != "SAIN DogFight")
                {
                    HoldLean = false;
                }

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
                if (!GoalEnemyNull)
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

        private bool AllowLean
        {
            get
            {
                if (LeanToggle.Value)
                {
                    if (!BotOwner.IsRole(WildSpawnType.assault))
                    {
                        return true;
                    }
                    else if (ScavLeanToggle.Value)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private bool ShouldBotLean => BotActive && !GoalEnemyNull && !EnemyVisibleOrShootable && !BotInCover && AllowLean;
        private bool ShouldBotReset => (!BotActive || GoalEnemyNull || EnemyVisibleOrShootable || !AllowLean) && !BotInCover;
        private bool EnemyFar => !GoalEnemyNull && BotOwner.Memory.GoalEnemy.Distance > 50f;
        private bool BotActive => BotOwner.BotState == EBotState.Active;
        private bool EnemyVisibleOrShootable => EnemyVisible || EnemyCanShoot;
        private bool EnemyVisible => !GoalEnemyNull && BotOwner.Memory.GoalEnemy.IsVisible;
        private bool EnemyCanShoot => !GoalEnemyNull && BotOwner.Memory.GoalEnemy.CanShoot;
        private bool GoalEnemyNull => BotOwner?.Memory?.GoalEnemy == null;
        private bool BotInCover => BotOwner.Memory.IsInCover;
        public bool LeaningRight => LeanAngle > 0f;
        public float LeanAngle { get; private set; }
        public bool HoldLean { get; set; }
        private bool DebugMode => DebugDynamicLean.Value;

        public static Color RandomColor => new Color(Random.value, Random.value, Random.value);
        private Color BotColor;

        private Corners.FindDirectionToLean Lean;
        private BotOwner BotOwner;
        protected ManualLogSource Logger;
        private bool StartingLean;
        private bool Resetting;
        private float LastLeanNum = 0f;
    }
}