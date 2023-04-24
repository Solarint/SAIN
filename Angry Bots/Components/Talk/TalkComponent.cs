using BepInEx.Logging;
using EFT;
using EFT.InventoryLogic;
using System.Collections;
using UnityEngine;
using static SAIN_AngryBots.Config.AngryConfig;

namespace SAIN_AngryBots.Components
{
    public class TalkComponent : MonoBehaviour
    {
        protected static ManualLogSource Logger { get; private set; }
        private BotOwner bot;

        private void Awake()
        {
            bot = GetComponent<BotOwner>();

            if (bot != null && bot?.GetPlayer != null)
            {
                StartCoroutine(TalkMonitor());
            }

            if (Logger == null)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(TalkComponent));
            }
        }
        private IEnumerator TalkMonitor()
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
                yield return new WaitForSeconds(1f);
            }
        }
    }
}