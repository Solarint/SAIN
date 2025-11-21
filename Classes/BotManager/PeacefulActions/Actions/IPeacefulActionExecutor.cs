namespace SAIN.Components.BotController.PeacefulActions;

public interface IPeacefulActionExecutor
{
    bool Finished { get; }
    void Execute();
    bool RecheckBots();
}