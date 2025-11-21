using EFT;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings.Categories;

public sealed class LayerInfoClass
{
    public string Name;
    public bool ConvertedToString;
    public string Description;
    public Dictionary<EBrain, int> UsedByBrains = new();
    public WildSpawnType[] UsedByWildSpawns;
}