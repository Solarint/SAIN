using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SAIN
{
    public static class PluginInfo
    {
        public const string GUID = "me.sol.sain";
        public const string Name = "SAIN";
        public const string Version = "3.5.0";

        public static class SPT
        {
            public const string GUID = "com.spt-aki.core";
            public const string Name = nameof(SPT);
            public const string Version = "3.6.0";
            public const string Creator = "SPT-AKI Team";
        }

        public static class BigBrain
        {
            public const string GUID = "xyz.drakia.bigbrain";
            public const string Name = nameof(BigBrain);
            public const string Version = "0.2.0";
            public const string Creator = "DrakiaXYZ";

            public static readonly PluginInfoClass Plugin = new PluginInfoClass
            {
                Name = Name,
                Version = Version,
                Creator = Creator,
                GUID = GUID
            };
        }

        public static class Waypoints
        {
            public const string GUID = "xyz.drakia.waypoints";
            public const string Name = nameof(Waypoints);
            public const string Version = "1.2.0";
            public const string Creator = "DrakiaXYZ";

            public static readonly PluginInfoClass Plugin = new PluginInfoClass
            {
                Name = Name,
                Version = Version,
                Creator = Creator,
                GUID = GUID
            };
        }

        public static class LootingBots
        {
            public const string GUID = "me.skwizzy.lootingbots";
            public const string Name = nameof(LootingBots);
            public const string Version = "1.0.0";
            public const string Creator = "Skwizzy";

            public static readonly PluginInfoClass Plugin = new PluginInfoClass
            {
                Name = Name,
                Version = Version,
                Creator = Creator,
                GUID = GUID
            };
        }

        public static class RealismMod
        {
            public const string GUID = "RealismMod";
            public const string Name = nameof(RealismMod);
            public const string Version = "1.0.0";
            public const string Creator = "Fontaine";

            public static readonly PluginInfoClass Plugin = new PluginInfoClass
            {
                Name = Name,
                Version = Version,
                Creator = Creator,
                GUID = GUID
            };
        }

        public static readonly Dictionary<string, PluginInfoClass> Plugins = new Dictionary<string, PluginInfoClass>
        {
            { BigBrain.Name, BigBrain.Plugin },
            { Waypoints.Name, Waypoints.Plugin },
            { RealismMod.Name, RealismMod.Plugin },
            { LootingBots.Name, LootingBots.Plugin },
        };
    }

    public sealed class PluginInfoClass
    {
        public string GUID;
        public string Name;
        public string Version;
        public string Creator;
        public bool Loaded;
    }

    public static class AssemblyInfo
    {
        public const string Title = PluginInfo.Name;
        public const string Description = "Full Revamp of Escape from Tarkov's AI System.";
        public const string Configuration = SPTVersion;
        public const string Company = "";
        public const string Product = PluginInfo.Name;
        public const string Copyright = "Copyright © 2023 Solarint";
        public const string Trademark = "";
        public const string Culture = "";

        // spt 3.6.0 == 25206
        public const int TarkovVersion = 25206;

        public const string SAINVersion = PluginInfo.Version;
        public const string SPTVersion = PluginInfo.SPT.Version;
    }
}