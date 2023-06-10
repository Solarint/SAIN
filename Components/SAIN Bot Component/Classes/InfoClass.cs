using BepInEx.Logging;
using EFT;
using SAIN.Helpers;
using System;
using System.Linq;
using UnityEngine;
using static SAIN.UserSettings.TalkConfig;

namespace SAIN.Classes
{
    public class BotInfoClass : SAINBot
    {
        public BotInfoClass(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            Init();
            WeaponInfo = new WeaponInfo(bot);
        }

        private bool BrainWait = false;

        private void Init()
        {
            BotType = BotOwner.Profile.Info.Settings.Role;
            Faction = BotOwner.Profile.Side;

            IsBoss = CheckIsBoss(BotType);
            IsFollower = CheckIsFollower(BotType);
            IsScav = BotType == WildSpawnType.assault || BotType == WildSpawnType.cursedAssault || BotType == WildSpawnType.marksman;
            //IsPMC = BotOwner.Brain.BaseBrain.ShortName() == "PMC";

            DifficultyModifier = CalculateDifficulty(BotOwner);

            SetPersonality();
        }

        public void Update()
        {
            if (!SAIN.BotActive || SAIN.GameIsEnding)
            {
                return;
            }

            WeaponInfo.ManualUpdate();

            if (PersonalityTimer < Time.time)
            {
                PersonalityTimer = Time.time + 10f;
                SetPersonality();
            }
        }

        public bool CanBotTalk
        {
            get
            {
                if (BotOwner.Settings.FileSettings.Mind.CAN_TALK && TalkGlobal.Value)
                {
                    if (IsBoss)
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
                }
                return false;
            }
        }

        private float PersonalityTimer = 0f;
        public readonly float FightIn = 60f;
        public readonly float FightOut = 70f;

        public readonly float DogFightIn = 10f;
        public readonly float DogFightOut = 15f;

        public readonly float LowAmmoThresh0to1 = 0.3f;

        private float CalculateDifficulty(BotOwner bot)
        {
            var settings = bot.Profile.Info.Settings;

            if (settings != null)
            {
                return GetDifficultyMod(settings.Role, settings.BotDifficulty, IsBoss, IsFollower);
            }

            return 1f;
        }

        public float HoldGroundDelay
        {
            get
            {
                if (SAIN.Info.BotPersonality == BotPersonality.Rat || SAIN.Info.BotPersonality == BotPersonality.Coward || SAIN.Info.BotPersonality == BotPersonality.Timmy)
                {
                    return 0f;
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
            else if (EFT_Math.RandomBool())
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
                if (BotOwner.Profile.Info.Level < 20 && EFT_Math.RandomBool())
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

        public WildSpawnType BotType { get; private set; }

        public EPlayerSide Faction { get; private set; }

        public bool IsBoss { get; private set; }

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