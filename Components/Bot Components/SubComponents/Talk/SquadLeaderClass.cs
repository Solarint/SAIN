using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using EFT;
using SAIN.Components;
using UnityEngine.AI;
using BepInEx.Logging;

namespace SAIN.Classes
{
    public class SquadLeaderClass : SAINBot
    {
        public SquadLeaderClass(SAINComponent owner) : base(owner)
        {
        }

        public void ManualUpdate()
        {
            if (!SAIN.BotActive || SAIN.GameIsEnding)
            {
                return;
            }

            if (!SAIN.Squad.BotInGroup)
            {
                return;
            }
        }
    }
}
