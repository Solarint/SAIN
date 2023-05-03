using BepInEx.Logging;
using EFT;
using Movement.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using static Movement.UserSettings.DogFight;

namespace Movement.Components
{
    public class DynamicLean : MonoBehaviour
    {
        private void Awake()
        {
            bot = GetComponent<BotOwner>();

            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(DynamicLean) + $": {bot.name}: ");

            Lean = new Corners.FindDirectionToLean(bot);

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
                if (ShouldBotLean)
                {
                    StartCoroutine(ExecuteLean(0.25f));
                }
                else if (!ShouldBotLean)
                {
                    StartCoroutine(ResetLeanAfterDuration(Random.Range(1f, 2f)));
                }

                yield return new WaitForEndOfFrame();
            }
        }

        public IEnumerator ExecuteLean(float delay)
        {
            if (!StartingLean)
            {
                StartingLean = true;

                yield return new WaitForSeconds(delay);

                StartingLean = false;

                if (ShouldBotLean)
                {
                    SetLean((LeaningRight ? 5 : -5), true);
                }
            }
        }

        private IEnumerator FindLeanAngleLoop()
        {
            while (true)
            {
                if (!EnemyVisibleOrShootable)
                {
                    LeanAngle = Lean.FindLeanAngle(20f);
                    yield return new WaitForSeconds(0.2f);
                }

                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator ResetLeanAfterDuration(float duration)
        {
            if (!Resetting)
            {
                Resetting = true;

                yield return new WaitForSeconds(duration);

                Resetting = false;

                if (!ShouldBotLean)
                {
                    SetLean(0f, false);
                }
            }
        }

        private void SetLean(float num, bool value)
        {
            if (LastLeanNum != num)
            {
                LastLeanNum = num;

                bot.GetPlayer.MovementContext.SetTilt(num, false);

                Logger.LogDebug($"Leaning Right: [{LeaningRight}] so Lean Input is {LastLeanNum} because [{LeanAngle}]");
            }
            Lean.Leaning = value;
        }

        private bool ShouldBotLean => (BotActive || !GoalEnemyNull || !EnemyVisibleOrShootable || AllowLean || AllowScavLean) && !BotInCover;
        private bool EnemyFar => !GoalEnemyNull && bot.Memory.GoalEnemy.Distance > 50f;
        private bool BotActive => bot?.BotState == EBotState.Active;
        private bool AllowScavLean => ScavLeanToggle.Value && Scav;
        private bool AllowLean => LeanToggle.Value && !Scav;
        private bool Scav => bot.IsRole(WildSpawnType.assault);
        private bool EnemyVisibleOrShootable => EnemyVisible || EnemyCanShoot;
        private bool EnemyVisible => !GoalEnemyNull && bot.Memory.GoalEnemy.IsVisible;
        private bool EnemyCanShoot => !GoalEnemyNull && bot.Memory.GoalEnemy.CanShoot;
        private bool GoalEnemyNull => bot?.Memory?.GoalEnemy == null;
        private bool BotInCover => bot.Memory.IsInCover;
        public bool LeaningRight => LeanAngle > 0f;
        public float LeanAngle { get; private set; }

        private Corners.FindDirectionToLean Lean;
        private BotOwner bot;
        protected ManualLogSource Logger;
        private bool StartingLean;
        private bool Resetting;
        private float LastLeanNum = 0f;

    }
}