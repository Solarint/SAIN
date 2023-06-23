using BepInEx.Logging;
using EFT;
using SAIN.Components;
using static SAIN.UserSettings.VisionConfig;
using SAIN.Helpers;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace SAIN.Classes
{
    public class SAINEnemy_Vision : SAINBot
    {
        public SAINEnemy_Vision(BotOwner bot) : base(bot)
        {
        }
    }
}