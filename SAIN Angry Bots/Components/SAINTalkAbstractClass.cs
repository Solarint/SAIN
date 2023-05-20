using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.Components;

namespace SAIN.Talk.Components
{
    public abstract class SAINTalk : SAINBot
    {
        public SAINTalk(BotOwner bot) : base(bot)
        {
            SAIN = bot.GetComponent<SAINCoreComponent>();

            SAINLayers = bot.GetComponent<SAINComponent>();

            BotTalkComponent = bot.GetComponent<BotTalkComponent>();

            PlayerComponent = Singleton<GameWorld>.Instance.MainPlayer.GetComponent<PlayerTalkComponent>();

            Logger = BepInEx.Logging.Logger.CreateLogSource("SAIN Talk");
        }

        protected BotTalkComponent BotTalkComponent { get; private set; }
        protected ManualLogSource Logger { get; private set; }
        protected SAINCoreComponent SAIN { get; private set; }
        protected SAINComponent SAINLayers { get; private set; }
        protected PlayerTalkComponent PlayerComponent { get; private set; }
    }
}