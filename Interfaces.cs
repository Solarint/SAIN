using BepInEx.Logging;
using EFT;
using SAIN.Attributes;
using SAIN.SAINComponent;
using System.Collections.Generic;

namespace SAIN
{
    public abstract class Logging
    {
        public static readonly Dictionary<string, ManualLogSource> LogSources = new Dictionary<string, ManualLogSource>();

        public Logging(string name)
        {
            if (LogSources.ContainsKey(name))
            {
                Logger = LogSources[name];
            }
            else
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(name);
                LogSources.Add(name, Logger);
            }
        }

        [AdvancedOptions(false, true)]
        public readonly ManualLogSource Logger;
    }

    public interface ISAINSubComponent
    {
        void Init(SAINComponentClass sain);
        BotOwner BotOwner { get; }
        Player Player { get; }
        ManualLogSource Logger { get; }
    }

    public interface IBotComponent
    {
        bool Init(Player player, BotOwner botOwner);
        BotOwner BotOwner { get; }
        Player Player { get; }
        ManualLogSource Logger { get; }
    }

    public interface IPlayerComponent
    {
        void Init(Player player);
        Player Player { get; }
        ManualLogSource Logger { get; }
    }

    public interface ISAINClass
    {
        void Init();
        void Update();
        void Dispose();
    }
}
