using EFT;
using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings.Categories
{
    public sealed class BrainInfoClass
    {
        public string Name;
        public string Description;
        public Dictionary<Layer, int> Layers = new Dictionary<Layer, int>();
        public WildSpawnType[] UsedByWildSpawns;
    }
}