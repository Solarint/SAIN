using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.Components;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.VisionConfig;
using SAIN.Helpers;
using SAIN;

public class BotController : MonoBehaviour
{
    public Dictionary<string, SAINComponent> SAINBots = new Dictionary<string, SAINComponent>();
    public LineOfSightManager LineOfSightManager { get; private set; }
    public CoverManager CoverManager { get; private set; }
    public GameWorld GameWorld { get; private set; }
    public Player MainPlayer => GameWorld?.MainPlayer;


    private void Awake()
    {
        GameWorld = Singleton<GameWorld>.Instance;
        LineOfSightManager = GameWorld.GetOrAddComponent<LineOfSightManager>();
        CoverManager = GameWorld.GetOrAddComponent<CoverManager>();
    }


    private void Update()
    {
        if (Singleton<GameWorld>.Instance == null)
        {
            Dispose();
            return;
        }
    }

    private void Dispose()
    {
        StopAllCoroutines();
        SAINBots.Clear();
        Destroy(this);
    }

    public void AddBot(SAINComponent sain)
    {
        string Id = sain.ProfileId;
        if (!SAINBots.ContainsKey(Id))
        {
            SAINBots.Add(Id, sain);
        }
    }

    public void RemoveBot(SAINComponent sain)
    {
        string Id = sain.ProfileId;
        if (SAINBots.ContainsKey(Id))
        {
            SAINBots.Remove(Id);
        }
    }
}