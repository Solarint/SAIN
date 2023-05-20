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
    public class BotInfoClass : SAINBot
    {
        public BotInfoClass(BotOwner bot) : base(bot)
        {
            BotType = bot.Profile.Info.Settings.Role;

            IsBoss = CheckIsBoss(BotType);
            IsFollower = CheckIsFollower(BotType);

            Faction = bot.Profile.Side;

            SetPersonality();

            Console.WriteLine($"[{BotPersonality}] Power Level [{PowerLevel}] Player Level [{BotOwner.Profile.Info.Level}]");
        }

        public void SetPersonality()
        {
            if (CanBeTimmy)
            {
                BotPersonality = BotPersonality.Timmy;
            }
            else if (CanBeGigaChad)
            {
                BotPersonality = BotPersonality.GigaChad;
            }
            else if (CanBeChad)
            {
                BotPersonality = BotPersonality.Chad;
            }
            else if (CanBeRat)
            {
                BotPersonality = BotPersonality.Rat;
            }
            else if (SAIN_Math.RandomBool())
            {
                BotPersonality = BotPersonality.Coward;
            }
            else
            {
                BotPersonality = BotPersonality.None;
            }
        }

        private bool CanBeChad
        {
            get
            {
                if (PowerLevel > 90f)
                {
                    return true;
                }
                return false;
            }
        }

        private bool CanBeGigaChad
        {
            get
            {
                if (PowerLevel > 120f)
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
                if (BotOwner.Profile.Info.Level <= 10 && PowerLevel < 30f)
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
                if (BotOwner.Profile.Info.Level < 20 && SAIN_Math.RandomBool())
                {
                    return true;
                }
                return false;
            }
        }

        public BotPersonality BotPersonality { get; private set; }
        public float PowerLevel => BotOwner.AIData.PowerOfEquipment;
        public WildSpawnType BotType { get; private set; }
        public EPlayerSide Faction { get; private set; }
        public bool IsBoss { get; private set; }
        public bool IsFollower { get; private set; }

        public static bool CheckIsBoss(WildSpawnType bottype)
        {
            WildSpawnType[] bossTypes = new WildSpawnType[0];
            foreach (WildSpawnType type in Enum.GetValues(typeof(WildSpawnType)))
            { // loop over all enum values
                if (type.ToString().StartsWith("boss"))
                {
                    Array.Resize(ref bossTypes, bossTypes.Length + 1);
                    bossTypes[bossTypes.Length - 1] = type;
                }
            }
            if (bossTypes.Contains(bottype))
            {
                return true;
            }

            return false;
        }

        public static bool CheckIsFollower(WildSpawnType bottype)
        {
            WildSpawnType[] followerTypes = new WildSpawnType[0];
            foreach (WildSpawnType type in Enum.GetValues(typeof(WildSpawnType)))
            {
                if (type.ToString().StartsWith("follower"))
                {
                    Array.Resize(ref followerTypes, followerTypes.Length + 1);
                    followerTypes[followerTypes.Length - 1] = type;
                }
            }
            if (followerTypes.Contains(bottype))
            {
                return true;
            }

            return false;
        }
    }
}