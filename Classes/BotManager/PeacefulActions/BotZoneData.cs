using System.Collections.Generic;

namespace SAIN.Components.BotController.PeacefulActions;

public class BotZoneData
{
    public string Name { get; }
    public BotZone Zone { get; }

    public BotZoneData(BotZone botZone)
    {
        Zone = botZone;
        Name = botZone.NameZone;
    }


    public void AddBot(BotComponent bot)
    {
        bot.GlobalEvents.OnEnterPeace += botEnterPeace;
        bot.GlobalEvents.OnExitPeace += botExitPeace;
        AllContainedBots.Add(bot);
    }

    public void RemoveBot(BotComponent bot)
    {
        bot.GlobalEvents.OnEnterPeace += botEnterPeace;
        bot.GlobalEvents.OnExitPeace += botExitPeace;
        AllContainedBots.Remove(bot);
        AllPeacefulBots.Remove(bot);
    }

    private void botEnterPeace(BotComponent bot)
    {
        AllPeacefulBots.Add(bot);
    }

    private void botExitPeace(BotComponent bot)
    {
        AllPeacefulBots.Remove(bot);
    }

    public List<BotComponent> AllContainedBots { get; } = new List<BotComponent>();
    public List<BotComponent> AllPeacefulBots { get; } = new List<BotComponent>();
}