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
        private BotSpawner BotSpawner => BotController?.BotSpawner;

        private static readonly WildSpawnType[] ExclusionList =
        {
            WildSpawnType.bossZryachiy,
            WildSpawnType.followerZryachiy
        };

        public void Update()
        {
            if (BotSpawner != null)
            {
                if (!Subscribed && !GameEnding)
                {
                    BotSpawner.OnBotRemoved += RemoveBot;
                    BotSpawner.OnBotCreated += AddBot;
                    Subscribed = true;
                }
                if (Subscribed)
                {
                    var status = BotController?.BotGame?.Status;
                    if (status == GameStatus.Stopping || status == GameStatus.Stopped || status == GameStatus.SoftStopping)
                    {
                        BotSpawner.OnBotRemoved -= RemoveBot;
                        BotSpawner.OnBotCreated -= AddBot;
                        Subscribed = false;
                        GameEnding = true;
                    }
                }
            }
        }

        private bool GameEnding = false;
        private bool Subscribed = false;

        public void AddBot(BotOwner botOwner)
        {
            try
            {
                if (botOwner != null)
                {
                    var settings = botOwner.Profile?.Info?.Settings;
                    if (settings == null)
                    {
                        return;
                    }

                    botOwner.LeaveData.OnLeave += RemoveBot;
                    var role = settings.Role;
                    if (ExclusionList.Contains(role))
                    {
                        botOwner.GetOrAddComponent<SAINNoBushESP>().Init(botOwner);
                        return;
                    }

                    if (SAINComponentClass.TryAddSAINToBot(botOwner, out SAINComponentClass component))
                    {
                        string profileId = component.ProfileId;
                        if (SAINBotDictionary.ContainsKey(profileId))
                        {
                            SAINBotDictionary.Remove(profileId);
                        }
                        SAINBotDictionary.Add(profileId, component);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Add Component Error: {ex}");
            }
        }

        public void RemoveBot(BotOwner botOwner)
        {
            try
            {
                if (botOwner != null)
                {
                    SAINBotDictionary.Remove(botOwner.ProfileId);
                    if (botOwner.TryGetComponent(out SAINComponentClass component))
                    {
                        component.Dispose();
                    }
                    if (botOwner.TryGetComponent(out SAINNoBushESP noBush))
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
