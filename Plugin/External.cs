using EFT;
using SAIN.Components;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Plugin;

public static class External
{
    public static bool IgnoreHearing(BotOwner bot, bool value, bool ignoreUnderFire, float duration)
    {
        var component = GetBotComponent(bot);
        if (component == null)
        {
            return false;
        }

        bool result = component.Hearing.SoundInput.SetIgnoreHearingExternal(value, ignoreUnderFire, duration, out string reason);
        return result;
    }

    public static string GetPersonality(BotOwner bot)
    {
        var component = GetBotComponent(bot);
        if (component == null)
        {
            return string.Empty;
        }
        return component.Info.Personality.ToString();
    }

    private static BotComponent GetBotComponent(BotOwner bot)
    {
        if (BotManagerComponent.Instance?.GetSAIN(bot, out BotComponent botComponent) == true)
        {
            return botComponent;
        }
        return bot.GetComponent<BotComponent>();
    }

    public static bool ExtractBot(BotOwner bot)
    {
        var component = GetBotComponent(bot);
        if (component == null)
        {
            return false;
        }

        component.Info.ForceExtract = true;

        return true;
    }

    public static void GetExtractedBots(List<string> list)
    {
        var botController = BotManagerComponent.Instance;
        if (botController == null)
        {
#if DEBUG
            Logger.LogWarning("SAIN Bot Controller is Null, cannot retrieve Extracted Bots List.");
#endif
            return;
        }
        var extractedBots = botController.BotExtractManager?.ExtractedBots;
        if (extractedBots == null)
        {
#if DEBUG
            Logger.LogWarning("List of extracted bots is null! Cannot copy list.");
#endif
            return;
        }
        list.Clear();
        list.AddRange(extractedBots);
    }

    public static void GetExtractionInfos(List<ExtractionInfo> list)
    {
        var botController = BotManagerComponent.Instance;
        if (botController == null)
        {
#if DEBUG
            Logger.LogWarning("SAIN Bot Controller is Null, cannot retrieve Extracted Bots List.");
#endif
            return;
        }
        var extractedBots = botController.BotExtractManager?.BotExtractionInfos;
        if (extractedBots == null)
        {
#if DEBUG
            Logger.LogWarning("List of extracted bots is null! Cannot copy list.");
#endif
            return;
        }
        list.Clear();
        list.AddRange(extractedBots);
    }

    public static bool TrySetExfilForBot(BotOwner bot)
    {
        var component = GetBotComponent(bot);
        if (component == null)
        {
            return false;
        }
        
#if DEBUG
        if (!Components.BotController.BotExtractManager.IsBotAllowedToExfil(component))
        {
            Logger.LogWarning($"{bot.name} is not allowed to use extracting logic.");
        }
#endif

        if (!BotManagerComponent.Instance.BotExtractManager.TryFindExfilForBot(component))
        {
            return false;
        }

        return true;
    }

    private static bool DebugExternal => SAINPlugin.DebugSettings.Logs.DebugExternal;

    public static float TimeSinceSenseEnemy(BotOwner botOwner)
    {
        var component = GetBotComponent(botOwner);
        if (component == null)
        {
            return float.MaxValue;
        }

        Enemy enemy = component.GoalEnemy;
        if (enemy == null)
        {
            return float.MaxValue;
        }

        return enemy.TimeSinceLastKnownUpdated;
    }

    public static bool IsPathTowardEnemy(NavMeshPath path, BotOwner botOwner, float ratioSameOverAll = 0.25f, float sqrDistCheck = 0.05f)
    {
        var component = GetBotComponent(botOwner);
        if (component == null)
        {
            return false;
        }

        Enemy enemy = component.GoalEnemy;
        if (enemy == null)
        {
            return false;
        }

        // Compare the corners in both paths, and check if the nodes used in each are the same.
        if (SAINBotSpaceAwareness.ArePathsDifferent(path, enemy.Path.PathToEnemy, ratioSameOverAll, sqrDistCheck))
        {
            return false;
        }

        return true;
    }

    public static bool CanBotQuest(BotOwner botOwner, Vector3 questPosition, float dotProductThresh = 0.33f)
    {
        var component = GetBotComponent(botOwner);
        if (component == null)
        {
            return false;
        }
        if (IsBotInCombat(component, out var reason))
        {
            if (DebugExternal)
                Logger.LogInfo($"{botOwner.name} is currently engaging an enemy, cannot quest. Reason: [{reason}]");

            return false;
        }
        if (IsBotSearching(component))
        {
            if (DebugExternal)
                Logger.LogInfo($"{botOwner.name} is currently searching and hasn't cleared last known position, cannot quest.");

            return false;
        }
        return true;
    }

    public static bool IsQuestTowardTarget(BotComponent component, Vector3 questPosition, float dotProductThresh)
    {
        Vector3? currentTarget = component.GoalEnemy?.LastKnownPosition;
        if (currentTarget == null)
        {
            return false;
        }

        Vector3 botPosition = component.Position;
        Vector3 targetDirection = currentTarget.Value - botPosition;
        Vector3 questDirection = questPosition - botPosition;

        return Vector3.Dot(targetDirection.normalized, questDirection.normalized) > dotProductThresh;
    }

    private static bool IsBotSearching(BotComponent component)
    {
        if (component.Decision.CurrentCombatDecision == ECombatDecision.Search || component.Decision.CurrentSquadDecision == ESquadDecision.Search)
        {
            return !component.Search.PathFinder.SearchedTargetPosition;
        }
        return false;
    }

    private static bool IsBotInCombat(BotComponent component, out ECombatReason reason)
    {
        const float TimeSinceSeenThreshold = 10f;
        const float TimeSinceHeardThreshold = 5f;
        const float TimeSinceUnderFireThreshold = 10f;

        reason = ECombatReason.None;
        Enemy enemy = component?.GoalEnemy;
        if (enemy == null)
        {
            return false;
        }
        if (enemy.IsVisible)
        {
            reason = ECombatReason.EnemyVisible;
            return true;
        }
        if (enemy.TimeSinceSeen < TimeSinceSeenThreshold)
        {
            reason = ECombatReason.EnemySeenRecently;
            return true;
        }
        if (enemy.TimeSinceHeard < TimeSinceHeardThreshold)
        {
            reason = ECombatReason.EnemyHeardRecently;
            return true;
        }
        BotMemoryClass memory = component.BotOwner.Memory;
        if (memory.IsUnderFire)
        {
            reason = ECombatReason.UnderFireNow;
            return true;
        }
        if (memory.UnderFireTime + TimeSinceUnderFireThreshold < Time.time)
        {
            reason = ECombatReason.UnderFireRecently;
            return true;
        }
        return false;
    }

    public enum ECombatReason
    {
        None = 0,
        EnemyVisible = 1,
        EnemyHeardRecently = 2,
        EnemySeenRecently = 3,
        UnderFireNow = 4,
        UnderFireRecently = 5,
    }
}
