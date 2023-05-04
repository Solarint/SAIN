using BepInEx.Logging;
using EFT;
using Movement.Helpers;
using System.Collections;
using UnityEngine;
using static Movement.UserSettings.DogFight;
using static Movement.UserSettings.Debug;
using SAIN_Helpers;

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

            StartCoroutine(CheckVisibleLoop());
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
                    StartCoroutine(ExecuteLean(0.25f));
                }
                else if (ShouldBotReset && !Resetting)
                {
                    //StartCoroutine(ResetLeanAfterDuration(Random.Range(1f, 2f)));
                }

                yield return new WaitForEndOfFrame();
            }
        }

        public IEnumerator ExecuteLean(float delay)
        {
            StartingLean = true;

            yield return new WaitForSeconds(delay);

            if (ShouldBotLean)
            {
                SetLean((LeaningRight ? 5 : -5), true);
            }

            StartingLean = false;
        }

        private IEnumerator CheckVisibleLoop()
        {
            while (true)
            {
                if (!GoalEnemyNull)
                {
                    if (CheckEnemyParts())
                    {
                    }
                    yield return new WaitForSeconds(1f);
                }

                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator FindLeanAngleLoop()
        {
            while (true)
            {
                if (!GoalEnemyNull && !EnemyVisible)
                {
                    LeanAngle = Lean.FindLeanAngle(20f, DebugMode);
                    yield return new WaitForSeconds(0.2f);
                }

                yield return new WaitForEndOfFrame();
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
            if (LastLeanNum != num)
            {
                LastLeanNum = num;

                bot.GetPlayer.MovementContext.SetTilt(num, false);

                if (DebugMode)
                {
                    Logger.LogDebug($"Leaning Right: [{LeaningRight}] so Lean Input is {LastLeanNum} because [{LeanAngle}]");
                }
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

        private bool NewEnemyIsVisible;

        private bool CheckLineOfSight
        {
            get
            {
                if (GoalEnemyNull)
                {
                    NewEnemyIsVisible = false;
                    return false;
                }

                if (Physics.Raycast(bot.LookSensor._headPoint, bot.Memory.GoalEnemy.Direction, out RaycastHit hit, bot.Memory.GoalEnemy.Distance, LayerMaskClass.HighPolyWithTerrainMaskAI))
                {
                    if (DebugMode)
                    {
                        DebugDrawer.Line(bot.LookSensor._headPoint, hit.point, 0.05f, Color.white, 0.25f);
                    }
                    NewEnemyIsVisible = false;
                    return false;
                }

                if (DebugMode)
                {
                    DebugDrawer.Ray(bot.LookSensor._headPoint, bot.Memory.GoalEnemy.Direction, bot.Memory.GoalEnemy.Distance, 0.05f, Color.red, 0.25f);
                }
                NewEnemyIsVisible = true;
                return true;
            }
        }

        private bool CheckEnemyParts()
        {
            var parts = bot.Memory.GoalEnemy.Owner.MainParts;
            foreach (BodyPartClass part in parts.Values)
            {
                if (!Physics.Linecast(bot.LookSensor._headPoint, part.Position, LayerMaskClass.HighPolyWithTerrainMaskAI))
                {
                    if (DebugMode)
                    {
                        DebugDrawer.Line(bot.LookSensor._headPoint, part.Position, 0.1f, Color.red, 1f);
                    }

                    return true;
                }
            }
            return false;
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

        private bool DebugMode => DebugDynamicLean.Value;

        private Corners.FindDirectionToLean Lean;
        private BotOwner bot;
        protected ManualLogSource Logger;
        private bool StartingLean;
        private bool Resetting;
        private float LastLeanNum = 0f;
    }
}