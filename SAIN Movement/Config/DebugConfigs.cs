using BepInEx.Configuration;

namespace SAIN.Movement.Config
{
    internal class DebugConfig
    {
        public static ConfigEntry<bool> DebugDodge { get; private set; }
        public static ConfigEntry<bool> DebugLean { get; private set; }
        //public static ConfigEntry<bool> DebugDoor { get; private set; }
        public static ConfigEntry<bool> DebugNavMesh { get; private set; }
        public static ConfigEntry<bool> DebugBurst { get; private set; }
        public static ConfigEntry<bool> DebugFire { get; private set; }
        public static ConfigEntry<bool> DebugShootHelpers { get; private set; }

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

            DebugBurst = Config.Bind(debugmode,
                "Burst Length", false, new ConfigDescription(
                    "", null, new ConfigurationManagerAttributes
                    { IsAdvanced = true, Order = 8 }));

            DebugFire = Config.Bind(debugmode,
                "Firerate", false, new ConfigDescription(
                    "", null, new ConfigurationManagerAttributes
                    { IsAdvanced = true, Order = 7 }));

            DebugShootHelpers = Config.Bind(debugmode,
                "Weapon Helpers", false, new ConfigDescription(
                    "Recoil and Ergo Modifiers Calculations", null, new ConfigurationManagerAttributes
                    { IsAdvanced = true, Order = 6 }));

            DebugNavMesh = Config.Bind(debugmode,
                "Draw Corners",
                false, new ConfigDescription(
                "Draws Spheres at corners, will murder performance", null, new ConfigurationManagerAttributes
                { IsAdvanced = true, Order = 5 }));
        }
    }
}