using EFT;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Components.PlayerComponentSpace.PersonClasses;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;

namespace SAIN.Components.BotController;

public class BotSpawnController : BotManagerBase
{
    public event Action<BotComponent> OnBotAdded;

    public event Action<BotComponent> OnBotRemoved;

    public BotSpawnController(BotManagerComponent botController) : base(botController)
    {
        Instance = this;
        GameWorldComponent.Instance.PlayerTracker.OnPlayerRemoved += PlayerRemoved;
    }

    private void PlayerRemoved(string id, PlayerComponent player)
    {
        if (player.BotOwner != null)
        {
            RemoveBot(player.BotOwner);
        }
    }

    public static BotSpawnController Instance { get; private set; }

    public Dictionary<string, BotComponent> BotDictionary { get; } = [];

    public static List<WildSpawnType> StrictExclusionList { get; } = new()
    {
        WildSpawnType.bossZryachiy,
        WildSpawnType.followerZryachiy,
        WildSpawnType.peacefullZryachiyEvent,
        WildSpawnType.ravangeZryachiyEvent,
        WildSpawnType.shooterBTR,
        WildSpawnType.marksman,
        WildSpawnType.infectedAssault,
        WildSpawnType.infectedCivil,
        WildSpawnType.infectedLaborant,
        WildSpawnType.infectedPmc,
        WildSpawnType.infectedTagilla
    };

    public void ManualUpdate(float currentTime, float deltaTime)
    {
        if (Subscribed &&
            GameEnding)
        {
            UnSubscribe();
        }
    }

    public bool GameEnding {
        get
        {
            var status = GameStatus;
            return status == GameStatus.Stopping || status == GameStatus.Stopped || status == GameStatus.SoftStopping;
        }
    }

    private GameStatus GameStatus {
        get
        {
            var botGame = BotController?.BotGame;
            if (botGame != null)
            {
                return botGame.Status;
            }
            return GameStatus.Starting;
        }
    }

    public void AddBot(BotOwner botOwner)
    {
        //Logger.LogDebug($"Checking {botOwner.name} for adding sain");
        PlayerComponent playerComponent = null;
        BotComponent botComponent = null;
        try
        {
            CheckExisting(botOwner);
            //Logger.LogDebug($"Checking {botOwner.name}...");
            playerComponent = botOwner.gameObject.GetComponent<PlayerComponent>();
            playerComponent.InitializeBotOwner(botOwner);

            //Logger.LogDebug($"Checking if {botOwner.name} excluded...");
            if (SAINEnableClass.IsSAINDisabledForBot(botOwner))
            {
                //Logger.LogDebug($"{botOwner.name} is excluded");
                botOwner.gameObject.AddComponent<SAINNoBushESP>().Init(botOwner);
                return;
            }

            //Logger.LogDebug($"Adding SAIN to {botOwner.name}...");
            botComponent = botOwner.gameObject.AddComponent<BotComponent>();
        }
        catch (Exception e)
        {
            Logger.LogError(e);
            return;
        }

        if (botComponent == null)
        {
#if DEBUG
            Logger.LogError($"Bot Component Null!");
#endif
            return;
        }
        if (playerComponent == null)
        {
            botComponent.Dispose();
#if DEBUG
            Logger.LogError($"Player Component Null!");
#endif
            return;
        }
        // When this botowner gets set to active is when we should initialize sain.
        botComponent.OnBotActivated += OnBotActivated;
        botComponent.ActivateIfBotActive(botOwner);
    }

    private void OnBotActivated(BotComponent bot)
    {
        bot.OnBotActivated -= OnBotActivated;

        SAINBots.Add(bot);
        if (BotGroup1.Count < BotGroup2.Count)
        {
            BotGroup1.Add(bot);
        }
        else if (BotGroup1.Count > BotGroup2.Count)
        {
            BotGroup2.Add(bot);
        }
        else
        {
            BotGroup1.Add(bot);
        }

        BotDictionary.Add(bot.ProfileId, bot);
        bot.PlayerComponent.InitializeBotComponent(bot);
        bot.BotOwner.LeaveData.OnLeave += RemoveBot;
        OnBotAdded?.Invoke(bot);
    }
    
    public HashSet<BotComponent> BotGroup1 { get; } = [];
    public HashSet<BotComponent> BotGroup2 { get; } = [];

    public HashSet<BotOwner> VanillaBots { get; } = [];
    public HashSet<BotComponent> SAINBots { get; } = [];

    public void Subscribe(BotSpawner botSpawner)
    {
        if (!Subscribed)
        {
            botSpawner.OnBotRemoved += RemoveBot;
            Subscribed = true;
        }
    }

    public void UnSubscribe()
    {
        if (Subscribed &&
            BotController?.BotSpawner != null)
        {
            BotController.BotSpawner.OnBotRemoved -= RemoveBot;
            Subscribed = false;
        }
    }

    private bool Subscribed = false;

    public BotComponent GetSAIN(BotOwner botOwner)
    {
        return GetSAIN(botOwner?.ProfileId);
    }

    public BotComponent GetSAIN(string profileId)
    {
        if (!profileId.IsNullOrEmpty() &&
            BotDictionary.TryGetValue(profileId, out BotComponent component))
        {
            return component;
        }
        return null;
    }

    private void CheckExisting(BotOwner botOwner)
    {
        string ProfileId = botOwner.ProfileId;
        if (BotDictionary.ContainsKey(ProfileId))
        {
#if DEBUG
            Logger.LogDebug($"{ProfileId} was already present in Bot Dictionary. Removing...");
#endif
            BotDictionary.Remove(ProfileId);
        }

        GameObject gameObject = botOwner.gameObject;
        // If somehow this bot already has components attached, destroy it.
        if (gameObject.TryGetComponent(out BotComponent botComponent))
        {
#if DEBUG
            Logger.LogDebug($"{ProfileId} already had a BotComponent attached. Destroying...");
#endif
            botComponent.Dispose();
        }
        if (gameObject.TryGetComponent(out SAINNoBushESP noBushComponent))
        {
#if DEBUG
            Logger.LogDebug($"{ProfileId} already had No Bush ESP attached. Destroying...");
#endif
            GameObject.Destroy(noBushComponent);
        }
    }

    public void RemoveBot(BotOwner botOwner)
    {
        try
        {
            if (botOwner != null)
            {
                if (BotDictionary.TryGetValue(botOwner.ProfileId, out BotComponent botComponent))
                {
                    OnBotRemoved?.Invoke(botComponent);
                    SAINBots.Remove(botComponent);
                    BotGroup1.Remove(botComponent);
                    BotGroup2.Remove(botComponent);
                    botComponent.Dispose();
                }
                BotDictionary.Remove(botOwner.ProfileId);
                if (botOwner.TryGetComponent(out BotComponent component) && botComponent != null)
                {
                    OnBotRemoved?.Invoke(botComponent);
                    SAINBots.Remove(botComponent);
                    BotGroup1.Remove(botComponent);
                    BotGroup2.Remove(botComponent);

                    component.Dispose();
                }
                if (botOwner.TryGetComponent(out SAINNoBushESP noBush))
                {
                    UnityEngine.Object.Destroy(noBush);
                }
            }
            else
            {
#if DEBUG
                Logger.LogError("Bot is null, cannot dispose!");
#endif
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Dispose Component Error: {ex}");
        }
    }
}