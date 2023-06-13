using EFT;
using SAIN.Components;
using UnityEngine;

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
        public Player Player => BotOwner.GetPlayer;
        public Vector3 BotPosition => BotOwner.Position;

        public SAINSoloDecision CurrentDecision => SAIN.Decision.MainDecision;
        public SAINSelfDecision SelfDecision => SAIN.Decision.CurrentSelfDecision;
        public SAINSquadDecision SquadDecision => SAIN.Decision.CurrentSquadDecision;
    }
}
