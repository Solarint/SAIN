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

        public BotOwner BotOwner { get; private set; }
        public SAINComponent SAIN { get; private set; }

        public SAINSoloDecision CurrentDecision => SAIN.Decision.MainDecision;
        public SAINSelfDecision SelfDecision => SAIN.Decision.CurrentSelfDecision;
        public SAINSquadDecision SquadDecision => SAIN.Decision.CurrentSquadDecision;
    }
}
