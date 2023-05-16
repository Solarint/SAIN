using EFT;
using SAIN.Components;
using SAIN.Layers.Logic;
using SAIN.Helpers;
using UnityEngine.UIElements;

namespace SAIN
{
    public abstract class SAINBotExt : SAINBot
    {
        public SAINBotExt(BotOwner bot) : base(bot)
        {
            SAIN = bot.GetOrAddComponent<SAINComponent>();
        }

        protected SAINComponent SAIN { get; private set; }
    }
}
