using BepInEx.Logging;
using EFT;

namespace SAIN.Classes
{
    public abstract class SAINInfoAbstract : SAINBot
    {
        public SAINInfoAbstract(BotOwner owner, SAINBotInfo sainInfo) : base(owner)
        {
            SAINInfo = sainInfo;
            Logger = BepInEx.Logging.Logger.CreateLogSource("SAIN Info");
        }

        public float DifficultyModifier => InfoClass.DifficultyModifier;
        public bool IAmBoss => InfoClass.IAmBoss;
        public bool IsFollower => InfoClass.IsFollower;
        public bool IsScav => InfoClass.IsScav;
        public bool IsPMC => InfoClass.IsPMC;

        public BotPresetClass BotPresetClass { get; private set; }
        public SAINBotPreset SAINBotPreset => BotPresetClass.DifficultyPreset;

        public SAINBotInfo SAINInfo { get; private set; }
        private InfoClass InfoClass => SAINInfo.InfoClass;

        public BotDifficulty BotDifficulty => InfoClass.BotDifficulty;
        public WildSpawnType BotType => InfoClass.BotType;
        public float PowerLevel => InfoClass.PowerLevel;
        public EPlayerSide Faction => InfoClass.Faction;

        public readonly ManualLogSource Logger;

    }
}
