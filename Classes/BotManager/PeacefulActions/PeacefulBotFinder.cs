using System.Collections.Generic;

namespace SAIN.Components.BotController.PeacefulActions;

public class PeacefulBotFinder : BotManagerBase, IBotControllerClass
{
    public Dictionary<string, BotZoneData> ZoneDatas = new();

    public PeacefulBotFinder(BotManagerComponent controller) : base(controller)
    {
    }

    public void Init()
    {
        BotController.BotSpawnController.OnBotAdded += botAdded;
        BotController.BotSpawnController.OnBotRemoved += botRemoved;
    }

    public void Update()
    {
    }

    public void Dispose()
    {
        BotController.BotSpawnController.OnBotAdded -= botAdded;
        BotController.BotSpawnController.OnBotRemoved -= botRemoved;
    }

    private void botAdded(BotComponent bot)
    {
        BotZone botZone = bot.BotOwner.BotsGroup.BotZone;
        if (botZone == null)
        {
#if DEBUG
            Logger.LogWarning($"Null BotZone for [{bot.BotOwner.name}]");
#endif
            return;
        }

        if (!ZoneDatas.TryGetValue(botZone.NameZone, out BotZoneData data))
        {
            data = new BotZoneData(botZone);
            ZoneDatas.Add(data.Name, data);
        }

        data.AddBot(bot);
    }

    private void botRemoved(BotComponent bot)
    {
        if (bot == null)
        {
#if DEBUG
            Logger.LogWarning($"Null BotComponent");
#endif
            return;
        }
        if (bot.BotOwner == null)
        {
#if DEBUG
            Logger.LogWarning($"Null BotOwner [{bot.Info.Profile.Name}]");
#endif
            return;
        }
        if (bot.BotOwner.BotsGroup == null)
        {
#if DEBUG
            Logger.LogWarning($"Null BotGroup [{bot.Info.Profile.Name}]");
#endif
            return;
        }
        BotZone botZone = bot.BotOwner.BotsGroup.BotZone;
        if (botZone == null)
        {
#if DEBUG
            Logger.LogWarning($"Null BotZone for [{bot.BotOwner.name}]");
#endif
            return;
        }
        BotZoneData data;
        if (!ZoneDatas.TryGetValue(botZone.NameZone, out data))
        {
            return;
        }
        data.RemoveBot(bot);
    }
}