using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using System;
using System.Linq;
using UnityEngine;
using static SAIN.UserSettings.TalkConfig;
using static SAIN.UserSettings.DebugConfig;
using static SAIN.UserSettings.ExtractConfig;
using static SAIN.UserSettings.EditorSettings;
using SAIN.Components;

namespace SAIN.Classes
{
    public class PersonalityClass : SAINBot
    {
        public PersonalityClass(BotOwner owner) : base(owner) 
        {
            SetPersonality();
        }

        public void Update()
        {
            SetPersonality();
        }

        public void SetPersonality()
        {
            if (CanBeGigaChad)
            {
                Personality = SAINPersonality.GigaChad;
            }
            else if (CanBeChad)
            {
                Personality = SAINPersonality.Chad;
            }
            else if (CanBeRat)
            {
                Personality = SAINPersonality.Rat;
            }
            else if (CanBeCoward)
            {
                Personality = SAINPersonality.Coward;
            }
            else if (CanBeTimmy)
            {
                Personality = SAINPersonality.Timmy;
            }
            else
            {
                Personality = SAINPersonality.Normal;
            }
        }

        private bool CanBeGigaChad
        {
            get
            {
                if (AllGigaChads.Value)
                {
                    return true;
                }
                if (Info.PowerLevel > 110f && Info.IsPMC && EFTMath.RandomBool(75))
                {
                    return true;
                }
                if (EFTMath.RandomBool(RandomGigaChadChance.Value))
                {
                    return true;
                }
                return false;
            }
        }

        private SAINBotInfo Info => SAIN.Info;

        private bool CanBeChad
        {
            get
            {
                if (AllChads.Value)
                {
                    return true;
                }
                if (Info.PowerLevel > 85f && Info.IsPMC && EFTMath.RandomBool(65))
                {
                    return true;
                }
                if (EFTMath.RandomBool(RandomChadChance.Value))
                {
                    return true;
                }
                return false;
            }
        }

        private bool CanBeTimmy
        {
            get
            {
                if (BotOwner.Profile.Info.Level <= 10 && Info.PowerLevel < 25f)
                {
                    return true;
                }
                return false;
            }
        }

        private bool CanBeRat
        {
            get
            {
                if (AllRats.Value)
                {
                    return true;
                }
                if (BotOwner.Profile.Info.Level < 40 && EFTMath.RandomBool(RandomRatChance.Value))
                {
                    return true;
                }
                return false;
            }
        }

        private bool CanBeCoward
        {
            get
            {
                if (EFTMath.RandomBool(RandomCowardChance.Value))
                {
                    return true;
                }
                return false;
            }
        }

        public SAINPersonality Personality { get; private set; }
    }
}