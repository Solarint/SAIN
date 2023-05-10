using BepInEx.Logging;
using EFT;
using Movement.Helpers;
using SAIN_Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static Movement.UserSettings.DebugConfig;
using static Movement.UserSettings.DogFightConfig;

namespace Movement.Components
{
    public class LeanComponent : MonoBehaviour
    {
        private void Awake()
        {
            bot = GetComponent<BotOwner>();
            BotColor = RandomColor;
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name + $": {bot.name}: ");

            Lean = new Corners.FindDirectionToLean();

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
                if (bot.Memory.GoalEnemy != null && bot.Brain.ActiveLayerName() != "SAIN DogFight")
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
                    LeanAngle = Lean.FindLeanAngle(bot.Transform.position, bot.Memory.GoalEnemy.CurrPosition, bot.LookSensor._headPoint, 10f);
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
                bot.GetPlayer.MovementContext.SetTilt(num, false);
            }

            Lean.Leaning = value;
        }

        private bool AllowLean
        {
            get
            {
                if (LeanToggle.Value)
                {
                    if (!bot.IsRole(WildSpawnType.assault))
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
        private bool EnemyFar => !GoalEnemyNull && bot.Memory.GoalEnemy.Distance > 50f;
        private bool BotActive => bot.BotState == EBotState.Active;
        private bool EnemyVisibleOrShootable => EnemyVisible || EnemyCanShoot;
        private bool EnemyVisible => !GoalEnemyNull && bot.Memory.GoalEnemy.IsVisible;
        private bool EnemyCanShoot => !GoalEnemyNull && bot.Memory.GoalEnemy.CanShoot;
        private bool GoalEnemyNull => bot?.Memory?.GoalEnemy == null;
        private bool BotInCover => bot.Memory.IsInCover;
        public bool LeaningRight => LeanAngle > 0f;
        public float LeanAngle { get; private set; }
        public bool HoldLean { get; set; }
        private bool DebugMode => DebugDynamicLean.Value;

        public static Color RandomColor => new Color(Random.value, Random.value, Random.value);
        private Color BotColor;

        private Corners.FindDirectionToLean Lean;
        private BotOwner bot;
        protected ManualLogSource Logger;
        private bool StartingLean;
        private bool Resetting;
        private float LastLeanNum = 0f;
    }
}