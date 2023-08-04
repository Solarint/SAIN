using BepInEx.Logging;
using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.Components
{
    public interface ISAINComponent
    {
        bool Init(Player player, BotOwner botOwner);
        BotOwner BotOwner { get; }
        Player Player { get; }
        ManualLogSource Logger { get; }
    }

    public interface ISAINSubComponent
    {
        void Init(SAINComponent sain);
        SAINComponent SAIN { get; }
        BotOwner BotOwner { get; }
        Player Player { get; }
        ManualLogSource Logger { get; }
    }
}
