using Comfort.Common;
using EFT;
using SAIN.Components;
using UnityEngine;

namespace SAIN
{
    public abstract class SAINBot
    {
        public SAINBot(SAINComponent bot)
        {
            SAIN = bot;
            BotOwner = bot?.BotOwner;
        }

        public BotOwner BotOwner { get; private set; }
        public SAINComponent SAIN { get; private set; }
        public Player GetPlayer => BotOwner.GetPlayer;
        public Vector3 BotPosition => BotOwner.Position;
        public SAINSoloDecision CurrentDecision => SAIN.Decision.MainDecision;
        public SAINSelfDecision SelfDecision => SAIN.Decision.CurrentSelfDecision;
        public SAINSquadDecision SquadDecision => SAIN.Decision.CurrentSquadDecision;
    }
}
