using System.Collections.Generic;

namespace SAIN.Components.BotController.PeacefulActions;

public static class PeacefulActionHelpers
{
    public static bool findBotsForPeacefulAction(BotZoneData data, List<BotComponent> localList, List<BotComponent> selectedList, float maxRangeSqr = -1f)
    {
        List<BotComponent> allBots = data.AllContainedBots;
        int allCount = allBots.Count;
        if (allCount < 2) return false;

        Logger.LogDebug($"Currently [{allCount}] bots in BotZone [{data.Name}]");

        List<BotComponent> peaceBots = data.AllPeacefulBots;
        int peaceCount = peaceBots.Count;
        if (peaceCount < 2) return false;

        Logger.LogDebug($"Currently [{peaceCount}] peaceful bots in BotZone [{data.Name}]");

        localList.Clear();
        localList.AddRange(peaceBots);

        for (int i = allCount - 1; i >= 0; i--)
        {
            BotComponent bot = localList[i];
            if (!selectedList.findFriendlyBotsandFilter(localList, bot, maxRangeSqr))
            {
                continue;
            }
            return true;
        }
        return false;
    }

    public static bool findFriendlyBotsandFilter(this List<BotComponent> result, List<BotComponent> localList, BotComponent bot, float minSqrMag = -1f)
    {
        if (bot == null) return false;
        result.Clear();
        result.Add(bot);
        result.FindBotsOfSameSide(bot, localList);
        if (result.Count < 2)
        {
            localList.Remove(bot);
            return false;
        }
        if (minSqrMag > 0)
        {
            result.FilterBotsBySquareMagnitude(bot, minSqrMag);
            if (result.Count < 2)
            {
                localList.Remove(bot);
                return false;
            }
        }
        return true;
    }

    public static void FindBotsOfSameSide(this List<BotComponent> result, BotComponent bot, List<BotComponent> listToCheck)
    {
        int count = listToCheck.Count;
        for (int j = 0; j < count; j++)
        {
            BotComponent bot2 = listToCheck[j];
            if (!goodForSelection(bot2)) continue;
            if (isBotSame(bot, bot2)) continue;
            if (!areBotsFriendly(bot, bot2)) continue;
            result.Add(bot);
        }
    }

    public static void FilterBotsBySquareMagnitude(this List<BotComponent> result, BotComponent bot, float minSqrMag)
    {
        int count = result.Count;
        for (int j = count - 1; j >= 0; j--)
        {
            BotComponent bot2 = result[j];
            if (isBotSame(bot, bot2)) continue;
            if ((bot.Position - bot2.Position).sqrMagnitude <= minSqrMag) continue;
            result.Remove(bot2);
        }
    }

    public static bool isBotSame(BotComponent a, BotComponent b)
    {
        return a.ProfileId == b.ProfileId;
    }

    public static bool areBotsFriendly(BotComponent a, BotComponent b)
    {
        if (isEnemy(a, b)) return false;
        if (isEnemy(b, a)) return false;
        return true;
    }

    public static bool isEnemy(BotComponent a, BotComponent b)
    {
        return a.EnemyController.GetEnemy(b.ProfileId, false) != null;
    }

    public static bool goodForSelection(BotComponent bot)
    {
        return bot != null && bot.BotActive;
    }
}