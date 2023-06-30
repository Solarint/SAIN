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
        private BotSpawnerClass BotSpawnerClass => BotController?.BotSpawnerClass;

        public void Update()
        {
            if (BotSpawnerClass != null)
            {
                if (!Subscribed && !GameEnding)
                {
                    BotSpawnerClass.OnBotRemoved += RemoveBot;
                    BotSpawnerClass.OnBotCreated += AddBot;
                    Subscribed = true;
                }
                if (Subscribed)
                {
                    var status = BotController?.BotGame?.Status;
                    if (status == GameStatus.Stopping || status == GameStatus.Stopped || status == GameStatus.SoftStopping)
                    {
                        BotSpawnerClass.OnBotRemoved -= RemoveBot;
                        BotSpawnerClass.OnBotCreated -= AddBot;
                        Subscribed = false;
                        GameEnding = true;
                    }
                }
            }
        }

        private bool GameEnding = false;
        private bool Subscribed = false;

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
                    SAINBotDictionary.Remove(bot.ProfileId);
                    if (bot.TryGetComponent<SAINComponent>(out var component))
                    {
                        component.BotOwner.LeaveData.OnLeave -= RemoveBot;
                        component.Dispose();
                    }
                }
                else
                {
                    Logger.LogError("Bot is null, cannot dispose!");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Dispose Component Error: {ex}");
            }
        }
    }
}
