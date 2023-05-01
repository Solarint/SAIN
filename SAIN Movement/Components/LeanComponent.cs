using EFT;
using Movement.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Movement.UserSettings.Debug;
using static Movement.UserSettings.DogFight;
using SAIN_Helpers;
using BepInEx.Logging;

namespace Movement.Components
{
    public class DynamicLean : MonoBehaviour
    {
        public bool StartingLean { get; private set; }
        public bool Resetting { get; private set; }

        private FindCorners.FindDirectionToLean Lean;

        private BotOwner bot;

        protected static ManualLogSource Logger { get; private set; }

        private void Awake()
        {
            bot = GetComponent<BotOwner>();

            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(DynamicLean) + $": {bot.name}: ");

            Resetting = false;
            StartingLean = false;
            Lean = new FindCorners.FindDirectionToLean(bot);

            StartCoroutine(ContinuousLean());
        }

        public IEnumerator Dispose()
        {
            StopAllCoroutines();
            Lean = null;
            yield break;
        }

        private IEnumerator ContinuousLean()
        {
            while (true)
            {
                if (ShouldIReset && !Resetting)
                {
                    Resetting = true;
                    StartCoroutine(ResetLeanAfterDuration(Random.Range(1f, 3f)));
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }

                if (ShouldILean && !StartingLean)
                {
                    StartingLean = true;
                    StartCoroutine(ExecuteLean(0.1f));
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        private bool ShouldILean
        {
            get
            {
                if (bot?.BotState != EBotState.Active) return false;

                if (!ScavDodgeToggle.Value && bot.IsRole(WildSpawnType.assault)) return false;

                if (!LeanToggle.Value) return false;

                if (bot?.Memory?.GoalEnemy?.CanShoot == true || bot?.Memory?.IsInCover == true) return false;

                if (bot?.Memory?.GoalEnemy == null) return false;

                if (bot?.Memory?.GoalEnemy.Distance > 30f) return false;

                return true;
            }
        }

        private bool ShouldIReset
        {
            get
            {
                if (Lean.Leaning)
                {
                    if (bot?.BotState != EBotState.Active) 
                        return true;

                    if (!ScavDodgeToggle.Value && bot.IsRole(WildSpawnType.assault)) 
                        return true;

                    if (!LeanToggle.Value) 
                        return true;

                    if (bot?.Memory?.GoalEnemy == null) 
                        return true;

                    if (bot?.Memory?.GoalEnemy?.IsVisible == true || bot.Memory.GoalEnemy.CanShoot) 
                        return true;
                }
                return false;
            }
        }

        public IEnumerator ExecuteLean(float delay)
        {
            yield return new WaitForSeconds(delay);
            StartingLean = false;

            if (ShouldILean)
            {
                float leanAngle = Lean.FindLeanAngle(10f);
                bot.GetPlayer.MovementContext.SetTilt((leanAngle > 0f) ? 5 : -5, false);
            }
        }

        private IEnumerator ResetLeanAfterDuration(float duration)
        {
            yield return new WaitForSeconds(duration);
            Resetting = false;

            if (ShouldIReset)
            {
                bot.GetPlayer.MovementContext.SetTilt(0f, false);

                Lean.Leaning = false;
            }
        }
    }
}