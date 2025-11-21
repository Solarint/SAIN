namespace SAIN.Components;

public abstract class GameWorldBase
{
    protected GameWorldComponent GameWorld { get; }
    public GameWorldBase(GameWorldComponent component)
    {
        GameWorld = component;
    }
}