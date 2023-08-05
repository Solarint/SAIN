using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.Decision;
using SAIN.SAINComponent.Classes.Talk;
using SAIN.SAINComponent.Classes.WeaponFunction;
using SAIN.SAINComponent.Classes.Mover;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;

namespace SAIN.Components.BotController
{
    public class BotSpawnController : SAINControl
    {
        public BotSpawnController() { }

        public Dictionary<string, SAINComponentClass> SAINBotDictionary = new Dictionary<string, SAINComponentClass>();
        private BotSpawnerClass BotSpawnerClass => BotController?.BotSpawnerClass;

        private static readonly WildSpawnType[] ExclusionList =
        {
            WildSpawnType.bossZryachiy,
            WildSpawnType.followerZryachiy
        };

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
                    var settings = bot.Profile?.Info?.Settings;
                    if (settings == null)
                    {
                        return;
                    }

                    bot.LeaveData.OnLeave += RemoveBot;
                    var role = settings.Role;
                    if (ExclusionList.Contains(role))
                    {
                        bot.GetOrAddComponent<NoBushESP>().Init(bot);
                        return;
                    }

                    SAINComponentClass component = SAINComponentHandler.AddComponent(bot);
                    if (component != null)
                    {
                        string profileId = bot.ProfileId;
                        if (!SAINBotDictionary.ContainsKey(profileId))
                        {
                            SAINBotDictionary.Add(profileId, component);
                        }
                        else
                        {
                            Logger.LogError("bot is already in dictionary. cannot add components");
                        }
                    }
                    else
                    {
                        Logger.LogError("Component could not be attached. Aborting");
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
                    if (bot.TryGetComponent<SAINComponentClass>(out var component))
                    {
                        component.Dispose();
                    }
                    if (bot.TryGetComponent<NoBushESP>(out var noBush))
                    {
                        UnityEngine.Object.Destroy(noBush);
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
