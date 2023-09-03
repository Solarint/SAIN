using Newtonsoft.Json;
using SAIN.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings.Categories
{
    public class BigBrainConfigClass
    {
        [JsonConstructor]
        public BigBrainConfigClass(float donotuse)
        {

        }

        public BigBrainConfigClass(Brain brain)
        {
            BrainEnum = brain;
            BrainName = brain.ToString();
        }

        private readonly string DONOTEDIT = "Do not Edit These";

        [Hidden] public Brain BrainEnum;
        [Hidden] public string BrainName;
        [Hidden] public int SquadLayerPriority = 24;
        [Hidden] public int ExtractLayerPriority = 22;
        [Hidden] public int CombatSoloLayerPriority = 20;
        [Hidden]
        public List<string> LayersToRemove = new List<string>
            {
                "Help",
                "AdvAssaultTarget",
                "Hit",
                "Pmc",
                "AssaultHaveEnemy",
                "Request"
            };
    }
}