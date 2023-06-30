using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Profiling;

namespace SAIN.Components.BotController
{
    public class BotSpawnController : SAINControl
    {
        public BotSpawnController() { }

        public Dictionary<string, SAINComponent> SAINBotDictionary = new Dictionary<string, SAINComponent>();

        public void AddBot(BotOwner bot)
        {
            try
            {
                if (bot != null)
                {
                    string profileId = bot.ProfileId;
                    if (!SAINBotDictionary.ContainsKey(profileId))
                    {
                        bot.LeaveData.OnLeave += RemoveBot;
                        SAINBotDictionary.Add(profileId, bot.GetOrAddComponent<SAINComponent>());
                    }
                    else
                    {
                        Logger.LogError("bot is already in dictionary. cannot add components");
                    }
                }
                else
                {
                    Logger.LogError("Botowner is null. cannot add components");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Add Component Error: {ex}");
            }
        }

        public void RemoveBot(BotOwner bot)
        {
            try
            {
                if (bot != null)
                {
                    RemoveFromDictionary(bot.ProfileId);
                }
                else
                {
                    Logger.LogError("Bot is null, cannot remove it from dictionary!");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Dispose Component Error: {ex}");
            }
        }

        private void RemoveFromDictionary(string profileId)
        {
            if (SAINBotDictionary.TryGetValue(profileId, out var component))
            {
                component.BotOwner.LeaveData.OnLeave -= RemoveBot;
                component?.Dispose();
                SAINBotDictionary.Remove(profileId);
            }
        }
    }
}
