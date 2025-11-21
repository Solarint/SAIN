using EFT;
using SAIN.Models.Enums;

namespace SAIN.Layers.Combat.Run;

internal class DebugLayer : SAINLayer
{
    public DebugLayer(BotOwner bot, int priority) : base(bot, priority, Name, ESAINLayer.Run)
    {
    }

    public static readonly string Name = BuildLayerName("SAIN Debug");

    public override Action GetNextAction()
    {
        if (SAINPlugin.DebugSettings.Logs.ForceBotsToRunAround)
            return new Action(typeof(RunningAction), $"RUNNING");

        if (SAINPlugin.DebugSettings.Logs.ForceBotsToTryCrawl)
            return new Action(typeof(CrawlAction), $"CRAWL");

        if (SAINPlugin.DebugSettings.Logs.TestGrenadeThrow && BotOwner.WeaponManager.Grenades.HaveGrenade)
        {
            //return new Action(typeof(GrenadeThrowTestAction), "testGrenade");
        }

        return new Action(typeof(RunningAction), $"RUNNING");
    }

    public override bool IsActive()
    {
        bool active = GetBotComponent() && SAINPlugin.DebugSettings.Logs.ForceBotsToRunAround || SAINPlugin.DebugSettings.Logs.ForceBotsToTryCrawl;
        CheckActiveChanged(active);
        return active;
    }

    public override bool IsCurrentActionEnding()
    {
        if (Bot == null) return true;
        if (base.IsCurrentActionEnding())
        {
            return true;
        }

        return false;
    }

    public ECombatDecision CurrentDecision => Bot.Decision.CurrentCombatDecision;
}