using SAIN.Models.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SAIN.Components.BotController.PeacefulActions;

public class BotGatheringController : BotManagerBase, IPeacefulActionController
{
    private const float FIND_GATHERING_FREQ = 10f;
    private const float MAX_GATHER_ENTER_DIST = 40f;
    private const float MAX_GATHER_ENTER_DIST_SQR = MAX_GATHER_ENTER_DIST * MAX_GATHER_ENTER_DIST;

    public EPeacefulAction Action { get; }
    public List<IPeacefulActionExecutor> ActiveActions { get; } = new List<IPeacefulActionExecutor>();
    public bool Active => Count > 0;
    public int Count => ActiveActions.Count;

    public void CheckExecute(BotZoneData data)
    {
        if (_nextCheckTime < Time.time)
        {
            _nextCheckTime = Time.time + FIND_GATHERING_FREQ;
            findPossibleGatherings(data);
        }
    }

    public BotGatheringController(BotManagerComponent controller, EPeacefulAction action) : base(controller)
    {
        Action = action;
    }

    private void findPossibleGatherings(BotZoneData data)
    {
        _selectedBots.Clear();
        if (PeacefulActionHelpers.findBotsForPeacefulAction(data, _localList, _selectedBots, MAX_GATHER_ENTER_DIST_SQR))
        {
            logConvoStart(data);
        }
    }

    private void selectBotsForConvo(List<BotComponent> list, BotComponent bot)
    {
    }

    private IEnumerator executeConversation(params BotComponent[] bots)
    {
        bool finishedConvo = false;
        while (!finishedConvo)
        {
            if (!recheckBots(bots))
            {
                finishedConvo = true;
                break;
            }

            // finish code
            break;
        }
        yield return null;
    }

    private bool recheckBots(params BotComponent[] bots)
    {
        foreach (BotComponent bot in bots)
            if (bot == null || bot.HasEnemy)
                return false;
        return true;
    }

    private void logConvoStart(BotZoneData data)
    {
#if DEBUG
        if (SAINPlugin.DebugMode || true)
        {
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"Gathering [{_gatherings++}]");
            stringBuilder.AppendLine($"Selected [{_selectedBots.Count}] Bots");
            stringBuilder.AppendLine($"Name: [{data.Name}]");
            stringBuilder.AppendLine($"Time: [{Time.time}]");
            for (int j = 0; j < _selectedBots.Count; j++)
            {
                var selected = _selectedBots[j];
                stringBuilder.AppendLine($"[{j + 1}] : Selected: [{selected.name}]");
            }
            Logger.LogDebug(stringBuilder.ToString());
        }
#endif
    }

    private readonly List<BotComponent> _localList = new();
    private readonly List<BotComponent> _selectedBots = new();
    private float _nextCheckTime;
#if DEBUG
    private int _gatherings;
#endif
}