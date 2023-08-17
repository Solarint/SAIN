using Newtonsoft.Json;
using SAIN.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace SAIN.Preset.GlobalSettings.Categories
{
    public class BigBrainSettings
    {
        static BigBrainSettings()
        {
            foreach (Brain brain in BotBrains.AllBrains)
            {
                DefaultBrains.Add(new BigBrainConfigClass(brain));
            }
        }

        [JsonConstructor]
        public BigBrainSettings() { }

        public BigBrainSettings(List<BigBrainConfigClass> brainsToAdd)
        {
            foreach (BigBrainConfigClass brain in brainsToAdd)
            {
                BrainSettings.Add(brain);
            }
        }


        [Advanced(IAdvancedOption.Hidden)]
        public List<BigBrainConfigClass> BrainSettings = new List<BigBrainConfigClass>();
        [Advanced(IAdvancedOption.Hidden)]
        public static readonly List<BigBrainConfigClass> DefaultBrains = new List<BigBrainConfigClass>();
    }
}