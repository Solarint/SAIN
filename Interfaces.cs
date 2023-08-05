using BepInEx.Logging;
using EFT;
using SAIN.SAINComponent;

namespace SAIN
{
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
