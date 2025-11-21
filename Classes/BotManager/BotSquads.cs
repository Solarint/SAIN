using EFT;
using SAIN.Components;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SAIN.BotController.Classes;

public class BotSquads(BotManagerComponent botController) : BotManagerBase(botController)
{
    public Dictionary<string, Squad> Squads { get; } = [];
    public HashSet<Squad> SquadArray { get; } = [];

    public void Update(float currentTime, float deltaTime)
    {
        foreach (Squad squad in SquadArray)
            squad?.Update(currentTime, deltaTime);

        ClearEmptySquads();
        
#if DEBUG
        if (SAINPlugin.DebugMode && DebugTimer < Time.time)
            LogDebug();
#endif
    }

    private void ClearEmptySquads()
    {
        foreach (Squad squad in _squadsToRemove)
        {
            if (squad != null)
            {
                Squads.Remove(squad.GUID);
                SquadArray.Remove(squad);
            }
        }
        _squadsToRemove.Clear();
    }

    private void LogDebug()
    {
        int count = 0;
        foreach (Squad squad in SquadArray)
        {
            if (squad != null)
                LogDebug(count, squad);
            count++;
        }
    }

    private void LogDebug(int count, Squad squad)
    {
        DebugTimer = Time.time + 60f;

        StringBuilder sb = new();
        sb.AppendLine($"Squad [{count}]: " +
            $"ID: [{squad.GetId()}] " +
            $"Count: [{squad.Members.Count}] " +
            $"Power: [{squad.SquadPowerLevel}] " +
            $"Members:");

        foreach (var member in squad.MemberInfos.Values)
            sb.AppendLine($" [{member.Nickname}, {member.PowerLevel}]");

        Logger.LogDebug(sb);
    }

    public Squad GetSquad(BotOwner botOwner)
    {
        Squad result = null;
        var group = botOwner.BotsGroup;
        if (group != null)
        {
            int groupCount = group.MembersCount;
            
#if DEBUG
            if (SAINPlugin.DebugMode)
                Logger.LogDebug($"Member Count: {groupCount} Checking for existing squad object");
#endif

            for (int i = 0; i < groupCount; i++)
            {
                var defaultMember = group.Member(i);
                if (defaultMember != null &&
                    defaultMember.ProfileId != botOwner.ProfileId &&
                    BotController.GetSAIN(defaultMember, out var sainComponent))
                {
#if DEBUG
                    if (SAINPlugin.DebugMode)
                        Logger.LogInfo($"Found SAIN Bot for squad");
#endif

                    result = sainComponent.Squad.SquadInfo;
                    if (result != null)
                    {
#if DEBUG
                        if (SAINPlugin.DebugMode)
                            Logger.LogInfo($"Adding bot to squad [{result.GUID}]");
#endif

                        break;
                    }
                }
            }
        }

        if (result == null)
        {
            result = new Squad();
#if DEBUG
            if (SAINPlugin.DebugMode)
                Logger.LogWarning($"Created New Squad [{result.GUID}]");
#endif

            if (!Squads.ContainsKey(result.GUID))
            {
                result.OnSquadEmpty += RemoveSquad;
                Squads.Add(result.GUID, result);
                SquadArray.Add(result);
            }
        }
        return result;
    }

    private void RemoveSquad(Squad squad)
    {
        if (squad != null)
        {
            squad.OnSquadEmpty -= RemoveSquad;
            squad.Dispose();
            _squadsToRemove.Add(squad);
        }
    }

    private readonly HashSet<Squad> _squadsToRemove = [];
    private float DebugTimer = 0f;
}