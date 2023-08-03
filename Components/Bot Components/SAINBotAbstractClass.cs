using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.Components;
using UnityEngine;

namespace SAIN
{
    public abstract class SAINBot
    {
        static SAINBot()
        {
            StaticLogger = BepInEx.Logging.Logger.CreateLogSource("SAIN Bot Component");
        }

        public SAINBot(SAINComponent bot)
        {
            SAIN = bot;
        }

        public BotOwner BotOwner => SAIN?.BotOwner;
        public SAINComponent SAIN { get; private set; }
        public Player GetPlayer => BotOwner.GetPlayer;
        public Vector3 BotPosition => BotOwner.Position;
        public SAINSoloDecision CurrentDecision => SAIN.Decision.MainDecision;
        public SAINSelfDecision SelfDecision => SAIN.Decision.CurrentSelfDecision;
        public SAINSquadDecision SquadDecision => SAIN.Decision.CurrentSquadDecision;

        public static ManualLogSource StaticLogger;
        public ManualLogSource Logger => StaticLogger;
    }
}
