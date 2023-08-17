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

        [Advanced(IAdvancedOption.Hidden)] public Brain BrainEnum;
        [Advanced(IAdvancedOption.Hidden)] public string BrainName;
        [Advanced(IAdvancedOption.Hidden)] public int SquadLayerPriority = 24;
        [Advanced(IAdvancedOption.Hidden)] public int ExtractLayerPriority = 22;
        [Advanced(IAdvancedOption.Hidden)] public int CombatSoloLayerPriority = 20;
        [Advanced(IAdvancedOption.Hidden)]
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