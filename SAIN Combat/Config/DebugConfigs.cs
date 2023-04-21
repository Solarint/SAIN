using BepInEx.Configuration;

namespace SAIN.Combat.Configs
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
        public static ConfigEntry<bool> DebugRecoil { get; private set; }
        public static ConfigEntry<bool> DebugTalk { get; private set; }

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
                "Fullauto", false, new ConfigDescription(
                    "", null, new ConfigurationManagerAttributes
                    { IsAdvanced = true, Order = 8 }));

            DebugFire = Config.Bind(debugmode,
                "Semiauto", false, new ConfigDescription(
                    "", null, new ConfigurationManagerAttributes
                    { IsAdvanced = true, Order = 7 }));

            DebugShootHelpers = Config.Bind(debugmode,
                "Weapon Helpers", false, new ConfigDescription(
                    "Recoil and Ergo Modifiers Calculations", null, new ConfigurationManagerAttributes
                    { IsAdvanced = true, Order = 6 }));

            DebugRecoil = Config.Bind(debugmode,
                "Debug Recoil",
                false, new ConfigDescription(
                "Draws lines showing recoil movements", null, new ConfigurationManagerAttributes
                { IsAdvanced = true, Order = 5 }));

            DebugTalk = Config.Bind(debugmode,
                "Debug Bot Talk",
                false, new ConfigDescription(
                "", null, new ConfigurationManagerAttributes
                { IsAdvanced = true, Order = 5 }));

            DebugNavMesh = Config.Bind(debugmode,
                "Draw Corners",
                false, new ConfigDescription(
                "Draws Spheres at corners", null, new ConfigurationManagerAttributes
                { IsAdvanced = true, Order = 4 }));
        }
    }
}