using BepInEx.Configuration;

namespace SAIN.UserSettings
{
    internal class DogFightConfig
    {
        public static ConfigEntry<bool> LeanToggle { get; private set; }
        public static ConfigEntry<bool> ScavLeanToggle { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string dogfighter = "Dynamic Lean";

            LeanToggle = Config.Bind(dogfighter,
                "Dynamic Lean",
                true, new ConfigDescription(
                "Enables Fully Dynamic Lean based on corner angles between a bot and their enemy.", null, new ConfigurationManagerAttributes
                { IsAdvanced = false, Order = 8 }));

            ScavLeanToggle = Config.Bind(dogfighter,
                "Scav Dynamic Lean",
                false, new ConfigDescription(
                "Enables Scavs using Dynamic Lean", null, new ConfigurationManagerAttributes
                { IsAdvanced = false, Order = 7 }));
        }
    }
}