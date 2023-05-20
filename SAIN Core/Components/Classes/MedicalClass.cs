using EFT;
using SAIN.Components;
using SAIN.Helpers;
using SAIN_Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Classes
{
    public class MedicalClass : SAINBot
    {
        public MedicalClass(BotOwner bot) : base(bot)
        {
        }

        public void Update()
        {
            BotOwner.Medecine?.Stimulators?.Refresh();
            BotOwner.Medecine?.FirstAid?.Refresh();
            CanHeal = BotOwner.Medecine.FirstAid.ShallStartUse();
        }

        public bool CanHeal = false;
        public bool HasStims => BotOwner.Medecine.Stimulators.HaveSmt;
        public bool Bleeding => BotOwner.Medecine.FirstAid.IsBleeding;
        public bool HasFirstAid => BotOwner.Medecine.FirstAid.HaveSmth2Use;
    }
}