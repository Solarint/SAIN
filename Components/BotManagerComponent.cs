using Comfort.Common;
using EFT;
using EFT.EnvironmentEffect;
using SAIN.BotController.Classes;
using SAIN.Components.BotController;
using SAIN.Components.BotControllerSpace.Classes;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Components;

public class BotManagerComponent : MonoBehaviour
{
    public static BotManagerComponent Instance { get; private set; }

    public Dictionary<string, BotComponent> Bots => BotSpawnController.Bots;
    public GameWorld GameWorld => SAINGameWorld.GameWorld;
    public IBotGame BotGame => Singleton<IBotGame>.Instance;

    public BotEventHandler BotEventHandler {
        get
        {
            if (_eventHandler == null)
            {
                _eventHandler = Singleton<BotEventHandler>.Instance;
                if (_eventHandler != null)
                {
                    GrenadeController.Subscribe(_eventHandler);
                }
            }
            return _eventHandler;
        }
    }

    private BotEventHandler _eventHandler;

    public GameWorldComponent SAINGameWorld { get; private set; }
    public BotsController DefaultController { get; set; }

    public BotSpawner BotSpawner {
        get
        {
            return _spawner;
        }
        set
        {
            BotSpawnController.Subscribe(value);
            _spawner = value;
        }
    }

    private BotSpawner _spawner;
    public GrenadeController GrenadeController { get; private set; }
    public BotJobsClass BotJobs { get; private set; }
    public BotExtractManager BotExtractManager { get; private set; }
    public TimeClass TimeVision { get; private set; }
    public BotController.SAINWeatherClass WeatherVision { get; private set; }
    public BotSpawnController BotSpawnController { get; private set; }
    public BotSquads BotSquads { get; private set; }
    public BotHearingClass BotHearing { get; private set; }

    public void PlayerEnviromentChanged(string profileID, IndoorTrigger trigger)
    {
        SAINGameWorld.PlayerTracker.GetPlayerComponent(profileID)?.AIData.PlayerLocation.UpdateEnvironment(trigger);
    }

    public void Activate(GameWorldComponent gameWorldComp)
    {
        Instance = this;
        SAINGameWorld = gameWorldComp;
        BotSpawnController = new BotSpawnController(this);
        BotExtractManager = new BotExtractManager(this);
        TimeVision = new TimeClass(this);
        WeatherVision = new BotController.SAINWeatherClass(this);
        BotSquads = new BotSquads(this);
        BotHearing = new BotHearingClass(this);
        BotJobs = new BotJobsClass(this);
        GrenadeController = new GrenadeController(this);
        GameWorld.OnDispose += Dispose;
    }

    public void ManualUpdate(float currentTime, float deltaTime)
    {
        BotSpawnController.ManualUpdate(currentTime, deltaTime);
        BotExtractManager.Update(currentTime, deltaTime);
        TimeVision.Update(currentTime, deltaTime);
        WeatherVision.Update(currentTime, deltaTime);
        BotSquads.Update(currentTime, deltaTime);

        HashSet<BotComponent> BotsArray = BotSpawnController.SAINBots;
            foreach (BotComponent BotComponent in BotsArray)
                if (BotComponent != null)
                    BotComponent.ManualUpdate(currentTime, deltaTime);
    }

    private void drawCover()
    {
        if (_coverDrawn)
        {
            return;
        }
        var coverData = DefaultController?.CoversData;
        if (coverData == null)
        {
            return;
        }
        foreach (var point in coverData.Points)
        {
            DebugGizmos.DrawSphere(point.Position, 0.1f, Color.white);
            if (point.AltPosition != null)
                DebugGizmos.DrawSphere(point.AltPosition.Value, 0.1f, Color.yellow);
            DebugGizmos.DrawSphere(point.FirePosition, 0.1f, Color.red);
            if (point.CorePointInGame != null)
                DebugGizmos.DrawSphere(point.CorePointInGame.Position, 0.1f, Color.blue);
        }
        _coverDrawn = coverData.Points.Count > 0;
    }

    private bool _coverDrawn;

    public void BotDeath(BotOwner bot)
    {
        if (bot?.GetPlayer != null && bot.IsDead)
        {
            DeadBots.Add(bot.GetPlayer);
        }
    }

    public List<Player> DeadBots { get; private set; } = new List<Player>();

    public List<BotDeathObject> DeathObstacles { get; private set; } = new List<BotDeathObject>();

    private readonly List<int> IndexToRemove = new();

    public void AddNavObstacles()
    {
        if (DeadBots.Count > 0)
        {
            const float ObstacleRadius = 1.5f;

            for (int i = 0; i < DeadBots.Count; i++)
            {
                var bot = DeadBots[i];
                if (bot == null || bot.GetPlayer == null)
                {
                    IndexToRemove.Add(i);
                    continue;
                }
                bool enableObstacle = true;
                Collider[] players = Physics.OverlapSphere(bot.Position, ObstacleRadius, LayerMaskClass.PlayerMask);
                foreach (var p in players)
                {
                    if (p == null) continue;
                    if (p.TryGetComponent<Player>(out var player))
                    {
                        if (player.IsAI && player.HealthController.IsAlive)
                        {
                            enableObstacle = false;
                            break;
                        }
                    }
                }
                if (enableObstacle)
                {
                    if (bot != null && bot.GetPlayer != null)
                    {
                        var obstacle = new BotDeathObject(bot);
                        obstacle.Activate(ObstacleRadius);
                        DeathObstacles.Add(obstacle);
                    }
                    IndexToRemove.Add(i);
                }
            }

            foreach (var index in IndexToRemove)
            {
                DeadBots.RemoveAt(index);
            }

            IndexToRemove.Clear();
        }
    }

    private void UpdateObstacles()
    {
        if (DeathObstacles.Count > 0)
        {
            for (int i = 0; i < DeathObstacles.Count; i++)
            {
                var obstacle = DeathObstacles[i];
                if (obstacle?.TimeSinceCreated > 30f)
                {
                    obstacle?.Dispose();
                    IndexToRemove.Add(i);
                }
            }

            foreach (var index in IndexToRemove)
            {
                DeathObstacles.RemoveAt(index);
            }

            IndexToRemove.Clear();
        }
    }

    public List<string> Groups = new();

    public void OnDestroy()
    {
    }

    public void Dispose()
    {
        try
        {
            GameWorld.OnDispose -= Dispose;
            StopAllCoroutines();
            BotJobs.Dispose();
            BotSpawnController.UnSubscribe();

            if (BotEventHandler != null)
            {
                GrenadeController.UnSubscribe(BotEventHandler);
            }

            if (Bots != null && Bots.Count > 0)
            {
                foreach (var bot in Bots.Values)
                {
                    bot?.Dispose();
                }
            }

            Bots?.Clear();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Dispose SAIN BotController Error: {ex}");
        }

        Destroy(this);
    }

    public bool GetSAIN(BotOwner botOwner, out BotComponent bot)
    {
        bot = BotSpawnController.GetSAIN(botOwner);
        return bot != null;
    }
}

public class BotDeathObject
{
    public BotDeathObject(Player player)
    {
        Player = player;
        NavMeshObstacle = player.gameObject.AddComponent<NavMeshObstacle>();
        NavMeshObstacle.carving = false;
        NavMeshObstacle.enabled = false;
        Position = player.Position;
        TimeCreated = Time.time;
    }

    public void Activate(float radius = 2f)
    {
        if (NavMeshObstacle != null)
        {
            NavMeshObstacle.enabled = true;
            NavMeshObstacle.carving = true;
            NavMeshObstacle.radius = radius;
        }
    }

    public void Dispose()
    {
        if (NavMeshObstacle != null)
        {
            NavMeshObstacle.carving = false;
            NavMeshObstacle.enabled = false;
            GameObject.Destroy(NavMeshObstacle);
        }
    }

    public NavMeshObstacle NavMeshObstacle { get; private set; }
    public Player Player { get; private set; }
    public Vector3 Position { get; private set; }
    public float TimeCreated { get; private set; }
    public float TimeSinceCreated => Time.time - TimeCreated;
    public bool ObstacleActive => NavMeshObstacle.carving;
}