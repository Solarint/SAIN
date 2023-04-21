using BepInEx.Logging;
using EFT;
using System.Collections;
using UnityEngine;

namespace SAIN.Combat.Components
{
    public class ShootMonitor : MonoBehaviour
    {
        protected static ManualLogSource Logger { get; private set; }

        private BotOwner bot;

        private void Awake()
        {
            bot = GetComponent<BotOwner>();

            if (bot != null && bot?.GetPlayer != null)
            {
                StartCoroutine(Monitor());
            }

            if (Logger == null)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(WeaponInfo));
            }
        }

        // Runs continuously in the background to check for when recoil modifiers need to be recalculated
        private IEnumerator Monitor()
        {
            while (true)
            {
                // Check if the bot is alive before continuing
                if (bot?.GetPlayer?.HealthController?.IsAlive == false || bot.IsDead)
                {
                    StopAllCoroutines();
                    yield break;
                }

                try
                {

                }
                catch
                {
                    // Empty
                }

                // Overall Check Frequency
                yield return new WaitForSeconds(0.1f);
            }
        }
        // Use Start or OnEnable to subscribe to the OnSoundPlayed event
        private void OnEnable(ref BotOwner bot)
        {
            bot.ShootData.OnTriggerPressed += StopShootingCheck;
        }

        // Use OnDisable to unsubscribe from the OnSoundPlayed event
        private void OnDisable()
        {
            bot.ShootData.OnTriggerPressed -= StopShootingCheck;
        }
        private void StopShootingCheck()
        {
            StartCoroutine(ScatterCheck());
        }
        private string EnemyName;
        private bool ShouldIStopShooting()
        {
            return bot?.Memory?.GoalEnemy?.Nickname != EnemyName || bot?.Memory?.GoalEnemy == null;
        }
        // Runs continuously in the background to check for when recoil modifiers need to be recalculated
        private IEnumerator ScatterCheck()
        {
            while (true)
            {
                // Check if the bot is alive before continuing
                if (bot?.GetPlayer?.HealthController?.IsAlive == false || bot.IsDead)
                {
                    StopAllCoroutines();
                    yield break;
                }

                if (ShouldIStopShooting())
                {
                    bot.EnemiesController.ShootDone();
                }

                try
                {

                }
                catch
                {
                    // Empty
                }

                // Overall Check Frequency
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}