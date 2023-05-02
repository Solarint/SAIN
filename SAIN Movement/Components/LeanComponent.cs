using BepInEx.Logging;
using EFT;
using Movement.Helpers;
using System.Collections;
using UnityEngine;
using static Movement.UserSettings.DogFight;

namespace Movement.Components
{
    public class DynamicLean : MonoBehaviour
    {
        private bool ShouldILean
        {
            get
            {
                if (bot?.Memory?.GoalEnemy == null)
                    return false;

                if (bot?.BotState != EBotState.Active) 
                    return false;


                if (!LeanToggle.Value) 
                    return false;

                if (!ScavLeanToggle.Value && bot.IsRole(WildSpawnType.assault))
                    return false;


                if (bot.Memory.GoalEnemy.CanShoot == true || bot.Memory.IsInCover == true) 
                    return false;

                if (bot.Memory.GoalEnemy.Distance > 30f) 
                    return false;

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

                    if (!ScavLeanToggle.Value && bot.IsRole(WildSpawnType.assault))
                        return true;

                    if (!LeanToggle.Value)
                        return true;

                    if (bot?.Memory?.GoalEnemy == null)
                        return true;

                    if (bot.Memory.GoalEnemy.IsVisible == true || bot.Memory.GoalEnemy.CanShoot)
                        return true;
                }
                return false;
            }
        }
        public bool StartingLean { get; private set; }
        public bool Resetting { get; private set; }

        private void Awake()
        {
            bot = GetComponent<BotOwner>();

            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(DynamicLean) + $": {bot.name}: ");

            Resetting = false;
            StartingLean = false;
            Lean = new Corners.FindDirectionToLean(bot);

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
                    StartCoroutine(ResetLeanAfterDuration(Random.Range(1f, 2f)));
                }
                else if (ShouldILean && !StartingLean)
                {
                    StartingLean = true;
                    StartCoroutine(ExecuteLean(0.5f));
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        public IEnumerator ExecuteLean(float delay)
        {
            float leanAngle = Lean.FindLeanAngle(10f);

            yield return new WaitForSeconds(delay);

            StartingLean = false;

            if (ShouldILean && !ShouldIReset)
            {
                bot.GetPlayer.MovementContext.SetTilt((leanAngle > 0f) ? 5 : -5, false);
            }
        }

        private IEnumerator ResetLeanAfterDuration(float duration)
        {
            yield return new WaitForSeconds(duration);
            Resetting = false;

            if (!ShouldILean && ShouldIReset)
            {
                bot.GetPlayer.MovementContext.SetTilt(0f, false);

                Lean.Leaning = false;
            }
        }

        private Corners.FindDirectionToLean Lean;

        private BotOwner bot;

        protected ManualLogSource Logger;

    }
}