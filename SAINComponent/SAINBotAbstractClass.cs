using BepInEx.Logging;
using EFT;
using SAIN.BotSettings;
using SAIN.SAINPreset.GlobalSettings;

namespace SAIN.SAINComponent
{
    public abstract class SAINBase : SAINComponentAbstract
    {
        public SAINBase(SAINComponentClass sain) : base (sain)
        {
            BotOwner = sain.BotOwner;
            Player = sain.Player;
            Logger = sain.Logger;
        }

        public BotOwner BotOwner { get; private set; }
        public Player Player { get; private set; }
        public ManualLogSource Logger { get; private set; }
        public GlobalSettingsClass GlobalSAINSettings => SAINPlugin.LoadedPreset?.GlobalSettings;
    }

    public class SAINComponentAbstract
    {
        public SAINComponentAbstract(SAINComponentClass sain)
        {
            SAIN = sain;
        }

        public SAINComponentClass SAIN { get; private set; }
    }
}