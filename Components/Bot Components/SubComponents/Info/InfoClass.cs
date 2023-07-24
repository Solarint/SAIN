using BepInEx.Logging;
using EFT;
using System;
using System.Linq;
using UnityEngine;
using static SAIN.UserSettings.TalkConfig;
using static SAIN.UserSettings.ExtractConfig;

namespace SAIN.Classes
{
    public class SAINBotInfo : SAINBot
    {
        public SAINBotInfo(BotOwner botOwner) : base(botOwner)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);

            FindBotType();
            PersonalityClass = new PersonalityClass(BotOwner, this);
            CalcHoldGroundDelay();
            CalcTimeBeforeSearch();

            WeaponInfo = new WeaponInfo(BotOwner);

            PercentageBeforeExtract = UnityEngine.Random.Range(MinPercentage.Value, MaxPercentage.Value);
            LastPower = PowerLevel;
        }

        public float TimeBeforeSearch { get; private set; } = 0f;
        public float PercentageBeforeExtract { get; private set; }

        private void FindBotType()
        {
            if (CheckIsBoss(BotType))
            {
                IAmBoss = true;
            }
            else if (CheckIsFollower(BotType))
            {
                IsFollower = true;
            }
            else if (BotType == WildSpawnType.assault || BotType == WildSpawnType.cursedAssault || BotType == WildSpawnType.marksman)
            {
                IsScav = true;
            }
            else if (BotType.ToString() == "sptUsec" || BotType.ToString() == "sptBear")
            {
                IsPMC = true;
            }
        }

        private float LastPower;

        public void Update()
        {
            if (SAIN == null) return;

            if (TimeBeforeSearch == 0f || BotOwner.BotsGroup.MembersCount != GroupCount)
            {
                GroupCount = BotOwner.BotsGroup.MembersCount;
                CalcTimeBeforeSearch();
            }

            if (LastPower != PowerLevel)
            {
                LastPower = PowerLevel;
                PersonalityClass.Update();
                CalcTimeBeforeSearch();
            }

            WeaponInfo.ManualUpdate();

            if (SAIN.Squad.BotInGroup && !SAIN.Squad.IAmLeader)
            {
                var Leader = SAIN.Squad.LeaderComponent;
                if (Leader != null)
                {
                    PercentageBeforeExtract = Leader.Info.PercentageBeforeExtract;
                }
            }
        }

        private int GroupCount = 0;

        public bool CanBotTalk
        {
            get
            {
                if (TalkGlobal.Value)
                {
                    if (IAmBoss)
                    {
                        return BossTalk.Value;
                    }
                    if (IsFollower)
                    {
                        return FollowerTalk.Value;
                    }
                    if (IsScav)
                    {
                        return ScavTalk.Value;
                    }
                    if (IsPMC)
                    {
                        return PMCTalk.Value;
                    }
                    if (BotOwner.Settings.FileSettings.Mind.CAN_TALK)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public float HoldGroundDelay { get; private set; }

        public float CalcHoldGroundDelay()
        {
            if (Personality == SAINPersonality.Rat || Personality == SAINPersonality.Coward || Personality == SAINPersonality.Timmy)
            {
                HoldGroundDelay = 0.25f;
            }
            else if (Personality == SAINPersonality.GigaChad)
            {
                HoldGroundDelay = 2f * UnityEngine.Random.Range(0.25f, 2f);
            }
            else if (Personality == SAINPersonality.Chad)
            {
                HoldGroundDelay = 1.5f * UnityEngine.Random.Range(0.5f, 2f);
            }
            else
            {
                HoldGroundDelay = 1f * UnityEngine.Random.Range(0.66f, 1.5f);
            }
            return HoldGroundDelay;
        }

        public void CalcTimeBeforeSearch()
        {
            float searchTime;
            if (IsFollower && SAIN.Squad.BotInGroup)
            {
                searchTime = 3f;
            }
            else if (IAmBoss && SAIN.Squad.BotInGroup)
            {
                searchTime = 20f;
            }
            else
            {
                switch (Personality)
                {
                    case SAINPersonality.GigaChad:
                        searchTime = 1.5f;
                        break;

                    case SAINPersonality.Chad:
                        searchTime = 5f;
                        break;

                    case SAINPersonality.Timmy:
                        searchTime = 60f;
                        break;

                    case SAINPersonality.Rat:
                        searchTime = 240f;
                        break;

                    case SAINPersonality.Coward:
                        searchTime = 60f;
                        break;

                    default:
                        searchTime = 24f;
                        break;
                }
            }

            searchTime *= UnityEngine.Random.Range(1f - SearchRandomize, 1f + SearchRandomize);
            searchTime = Mathf.Round(searchTime * 10f) / 10f;
            if (searchTime < 0.25f)
            {
                searchTime = 0.25f;
            }

            TimeBeforeSearch = searchTime;
        }

        private const float SearchRandomize = 0.33f;

        public float DifficultyModifier
        {
            get
            {
                float modifier;
                if (IAmBoss)
                {
                    modifier = 0.85f;
                }
                else if (IsFollower)
                {
                    modifier = 0.95f;
                }
                else if (IsPMC)
                {
                    modifier = 0.75f;
                }
                else if (IsScav)
                {
                    modifier = 1.25f;
                }
                else
                {
                    modifier = 1.1f;
                }

                switch (BotDifficulty)
                {
                    case BotDifficulty.easy:
                        modifier *= 1.25f;
                        break;

                    case BotDifficulty.normal:
                        modifier *= 1.0f;
                        break;

                    case BotDifficulty.hard:
                        modifier *= 0.85f;
                        break;

                    case BotDifficulty.impossible:
                        modifier *= 0.75f;
                        break;

                    default:
                        modifier *= 1f;
                        break;
                }

                return modifier;
            }
        }

        public BotDifficulty BotDifficulty => BotOwner.Profile.Info.Settings.BotDifficulty;

        public PersonalityClass PersonalityClass { get; private set; }
        public WeaponInfo WeaponInfo { get; private set; }

        public SAINPersonality Personality => PersonalityClass.Personality;
        public float PowerLevel => BotOwner.AIData.PowerOfEquipment;
        public WildSpawnType BotType => BotOwner.Profile.Info.Settings.Role;
        public EPlayerSide Faction => BotOwner.Profile.Side;

        public bool IAmBoss { get; private set; } = false;
        public bool IsFollower { get; private set; } = false;
        public bool IsScav { get; private set; } = false;
        public bool IsPMC { get; private set; } = false;

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

            return bossTypes.Contains(bottype) || bottype == WildSpawnType.sectantPriest;
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
            return followerTypes.Contains(bottype) || bottype == WildSpawnType.sectantWarrior;
        }

        private ManualLogSource Logger;
    }
}