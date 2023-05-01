/*
namespace Movement.UserSettings
{
    internal class Door
    {
        public static ConfigEntry<bool> DoorPatchToggle { get; private set; }
        public static void Init(ConfigFile Config)
        {
            string experimental = "6. Experimental";
            DoorPatchToggle = Config.Bind(experimental,
                "Faster Door Opening Test", false, new ConfigDescription(
                    "This doesn't do much", null, new ConfigurationManagerAttributes
                    { IsAdvanced = false, Order = 1 }));
        }
    }
}*/