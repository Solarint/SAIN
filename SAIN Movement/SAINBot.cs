using EFT;
using SAIN.Components;

namespace SAIN
{
    public abstract class SAINBot
    {
        public SAINBot(BotOwner bot)
        {
            BotOwner = bot;
            SAIN = bot.GetOrAddComponent<SAINComponent>();
        }

        protected BotOwner BotOwner { get; private set; }
        protected SAINComponent SAIN { get; private set; }
    }
}
