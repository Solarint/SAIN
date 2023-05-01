using BepInEx.Configuration;

namespace Movement.UserSettings
{
    internal class Debug
    {
        public static ConfigEntry<bool> DebugDodge { get; private set; }
        public static ConfigEntry<bool> DebugLean { get; private set; }
        //public static ConfigEntry<bool> DebugDoor { get; private set; }
        public static ConfigEntry<bool> DebugNavMesh { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string debugmode = "7. Debug Logs";

            DebugDodge = Config.Bind(debugmode,
                "Dodge",
                false, new ConfigDescription(
                "", null, new ConfigurationManagerAttributes
                { IsAdvanced = true, Order = 11 }));

            DebugLean = Config.Bind(debugmode,
                "Lean", false, new ConfigDescription(
                    "", null, new ConfigurationManagerAttributes
                    { IsAdvanced = true, Order = 10 }));

            DebugNavMesh = Config.Bind(debugmode,
                "Draw Corners",
                false, new ConfigDescription(
                "Draws Spheres at corners, will murder performance", null, new ConfigurationManagerAttributes
                { IsAdvanced = true, Order = 5 }));
        }
    }
}