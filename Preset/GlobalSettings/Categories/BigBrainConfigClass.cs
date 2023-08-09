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

        [Advanced(AdvancedEnum.Hidden)] public Brain BrainEnum;
        [Advanced(AdvancedEnum.Hidden)] public string BrainName;
        [Advanced(AdvancedEnum.Hidden)] public int SquadLayerPriority = 24;
        [Advanced(AdvancedEnum.Hidden)] public int ExtractLayerPriority = 22;
        [Advanced(AdvancedEnum.Hidden)] public int CombatSoloLayerPriority = 20;
        [Advanced(AdvancedEnum.Hidden)]
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