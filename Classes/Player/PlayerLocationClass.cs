using EFT.EnvironmentEffect;

namespace SAIN.Components.PlayerComponentSpace.Classes;

public class PlayerLocationClass : AIDataBase
{
    public float BunkerDepth { get; private set; }
    public bool InBunker { get; private set; }

    public PlayerLocationClass(SAINAIData aiData) : base(aiData)
    {
    }

    public void UpdateEnvironment(IndoorTrigger trigger)
    {
        InBunker = trigger?.IsBunker == true;
        BunkerDepth = InBunker ? trigger.BunkerDepth : 0f;
        if (InBunker)
        {
            Logger.LogDebug($"In Bunker. BunkerDepth: {BunkerDepth}");
        }
    }
}