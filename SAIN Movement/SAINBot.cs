using EFT;
using SAIN.Components;
using SAIN.Layers.Logic;
using SAIN.Helpers;
using UnityEngine.UIElements;

namespace SAIN
{
    public abstract class SAINBotLayers : SAINBot
    {
        public SAINBotLayers(BotOwner bot) : base(bot)
        {
            SAIN = bot.GetOrAddComponent<SAINBotComponent>();
        }

        protected SAINBotComponent SAIN { get; private set; }
    }
}
