using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using System;
using System.Linq;
using UnityEngine;
using static SAIN.UserSettings.TalkConfig;
using static SAIN.UserSettings.DebugConfig;
using static SAIN.UserSettings.ExtractConfig;
using static SAIN.UserSettings.DifficultyConfig;
using SAIN.Components;

namespace SAIN.Classes
{
    public class BotInfoClass : MonoBehaviour
    {
        private SAINComponent SAIN;
        private BotOwner BotOwner => SAIN.BotOwner;

        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            Init();
            WeaponInfo = new WeaponInfo(BotOwner);
        }

        public float TimeBeforeSearch { get; private set; } = 0f;

        public float PercentageBeforeExtract { get; private set; }

        private void Init()
        {
            IAmBoss = CheckIsBoss(BotType);
            IsFollower = CheckIsFollower(BotType);
            IsScav = BotType == WildSpawnType.assault || BotType == WildSpawnType.cursedAssault || BotType == WildSpawnType.marksman;
            string botTypeString = BotType.ToString();
            IsPMC = botTypeString == "sptUsec" || botTypeString == "sptBear";

            SetPersonality();
            DifficultyModifier = CalculateDifficulty(BotOwner);
            PercentageBeforeExtract = UnityEngine.Random.Range(MinPercentage.Value, MaxPercentage.Value);
            LastPower = PowerLevel;
        }

        private float LastPower;

        private void Update()
        {
            if (BotOwner == null || SAIN == null) return;
            if (TimeBeforeSearch == 0f || BotOwner.BotsGroup.MembersCount != GroupCount)
            {
                GroupCount = BotOwner.BotsGroup.MembersCount;
                GetTimeBeforeSearch();
            }

            if (LastPower != PowerLevel)
            {
                LastPower = PowerLevel;
                SetPersonality();
                GetTimeBeforeSearch();
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

        private float CalculateDifficulty(BotOwner bot)
        {
            var settings = bot.Profile.Info.Settings;

            if (settings != null)
            {
                return GetDifficultyMod(settings.Role, settings.BotDifficulty, IAmBoss, IsFollower);
            }

            return 1f;
        }

        public float HoldGroundDelay
        {
            get
            {
                if (SAIN.Info.BotPersonality == BotPersonality.Rat || SAIN.Info.BotPersonality == BotPersonality.Coward || SAIN.Info.BotPersonality == BotPersonality.Timmy)
                {
                    return 0.25f;
                }

                if (StandAndShootRandomTimer < Time.time)
                {
                    StandAndShootRandomTimer = Time.time + 1f;

                    if (SAIN.Info.BotPersonality == BotPersonality.GigaChad)
                    {
                        ShootDelay = 2f * UnityEngine.Random.Range(0.25f, 2f);
                    }
                    else if (SAIN.Info.BotPersonality == BotPersonality.Chad)
                    {
                        ShootDelay = 1.5f * UnityEngine.Random.Range(0.5f, 2f);
                    }
                    else
                    {
                        ShootDelay = 1f * UnityEngine.Random.Range(0.66f, 1.5f);
                    }
                }

                return ShootDelay;
            }
        }

        private float StandAndShootRandomTimer = 0f;
        private float ShootDelay = 0f;

        public void GetTimeBeforeSearch()
        {
            var group = SAIN.Squad;

            float searchTime;

            if (IsFollower && group.BotInGroup)
            {
                searchTime = 3f;
            }
            else if (IAmBoss && group.BotInGroup)
            {
                searchTime = 20f;
            }
            else
            {
                switch (BotPersonality)
                {
                    case BotPersonality.GigaChad:
                        searchTime = 1.5f;
                        break;

                    case BotPersonality.Chad:
                        searchTime = 5f;
                        break;

                    case BotPersonality.Timmy:
                        searchTime = 60f;
                        break;

                    case BotPersonality.Rat:
                        searchTime = 240f;
                        break;

                    case BotPersonality.Coward:
                        searchTime = 40f;
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

            if (DebugBotInfo.Value)
            {
                Logger.LogDebug($"Search Time = [{searchTime}] because: IsBoss? [{IAmBoss}] IsFollower? [{IsFollower}] Personality [{BotPersonality}] SquadLead? [{group.IAmLeader}] Squad Members: [{group.SquadMembers.Count}]");
            }

            TimeBeforeSearch = searchTime;
        }

        private const float SearchRandomize = 0.33f;

        private static float GetDifficultyMod(WildSpawnType bottype, BotDifficulty difficulty, bool isBoss, bool isFollower)
        {
            float modifier = 1f;
            if (isBoss)
            {
                modifier = 0.85f;
            }
            else if (isFollower)
            {
                modifier = 0.95f;
            }
            else
            {
                switch (bottype)
                {
                    case WildSpawnType.assault:
                        modifier *= 1.25f;
                        break;

                    case WildSpawnType.pmcBot:
                        modifier *= 1.1f;
                        break;

                    case WildSpawnType.cursedAssault:
                        modifier *= 1.35f;
                        break;

                    case WildSpawnType.exUsec:
                        modifier *= 1.1f;
                        break;

                    default:
                        modifier *= 0.75f;
                        break;
                }
            }

            switch (difficulty)
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

        public void SetPersonality()
        {
            if (AllGigaChads.Value)
            {
                BotPersonality = BotPersonality.GigaChad;
            }
            else if (AllChads.Value)
            {
                BotPersonality = BotPersonality.Chad;
            }
            else if (AllRats.Value)
            {
                BotPersonality = BotPersonality.Rat;
            }
            else if (CanBeTimmy)
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
            else if (EFTMath.RandomBool())
            {
                BotPersonality = BotPersonality.Coward;
            }
            else
            {
                BotPersonality = BotPersonality.Normal;
            }
        }

        private bool CanBeChad
        {
            get
            {
                if (PowerLevel > 85f && IsPMC && EFTMath.RandomBool(65))
                {
                    return true;
                }
                if (EFTMath.RandomBool(3))
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
                if (PowerLevel > 110f && IsPMC && EFTMath.RandomBool(75))
                {
                    return true;
                }
                if (EFTMath.RandomBool(3))
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
                if (BotOwner.Profile.Info.Level <= 10 && PowerLevel < 25f)
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
                if (BotOwner.Profile.Info.Level < 40 && EFTMath.RandomBool(40))
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsScav { get; private set; }

        public WeaponInfo WeaponInfo { get; private set; }

        public float DifficultyModifier { get; private set; }

        public BotPersonality BotPersonality { get; private set; }

        public float PowerLevel => BotOwner.AIData.PowerOfEquipment;

        public WildSpawnType BotType => BotOwner.Profile.Info.Settings.Role;

        public EPlayerSide Faction => BotOwner.Profile.Side;

        public bool IAmBoss { get; private set; }

        public bool IsFollower { get; private set; }

        public bool IsPMC { get; private set; }

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