using BepInEx.Logging;
using EFT;
using SAIN.BotPresets;
using System.Linq;
using System;

namespace SAIN.Classes
{
    public abstract class SAINInfoAbstract : SAINBot
    {
        public SAINInfoAbstract(BotOwner owner) : base(owner)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource("SAIN Info");
            BotType = FindBotType.FindType(WildSpawnType);
            SetDiffModifier(BotType, BotDifficulty);
        }

        public BotType BotType { get; private set; }

        public bool IAmBoss => BotType == BotType.Boss;
        public bool IsFollower => BotType == BotType.Follower;
        public bool IsScav => BotType == BotType.Scav;
        public bool IsPMC => BotType == BotType.PMC;

        void SetDiffModifier(BotType type, BotDifficulty difficulty)
        {
            float modifier;
            switch (type)
            {
                case BotType.Boss:
                    modifier = 0.85f; break;
                case BotType.Follower:
                    modifier = 0.95f; break;
                case BotType.Scav:
                    modifier = 1.25f; break;
                case BotType.PMC:
                    modifier = 0.75f; break;
                default:
                    modifier = 1f; break;
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

            DifficultyModifier = modifier;
        }

        public float DifficultyModifier { get; private set; }

        public BotDifficulty BotDifficulty => BotOwner.Profile.Info.Settings.BotDifficulty;
        public WildSpawnType WildSpawnType => BotOwner.Profile.Info.Settings.Role;
        public float PowerLevel => BotOwner.AIData.PowerOfEquipment;
        public EPlayerSide Faction => BotOwner.Profile.Side;

        public readonly ManualLogSource Logger;

    }

    public class FindBotType
    {
        static FindBotType()
        {
            Types = (WildSpawnType[])Enum.GetValues(typeof(WildSpawnType));

            BossTypes = new WildSpawnType[0];
            foreach (WildSpawnType type in Types)
            { // loop over all enum values
                if (type.ToString().StartsWith("boss"))
                {
                    Array.Resize(ref BossTypes, BossTypes.Length + 1);
                    BossTypes[BossTypes.Length - 1] = type;
                }
            }

            FollowerTypes = new WildSpawnType[0];
            foreach (WildSpawnType type in Types)
            {
                if (type.ToString().StartsWith("follower"))
                {
                    Array.Resize(ref FollowerTypes, FollowerTypes.Length + 1);
                    FollowerTypes[FollowerTypes.Length - 1] = type;
                }
            }
        }

        static WildSpawnType[] Types;
        static WildSpawnType[] BossTypes;
        static WildSpawnType[] FollowerTypes;

        public static BotType FindType(WildSpawnType wildSpawnType)
        {
            string wildString = wildSpawnType.ToString();
            if (CheckIsBoss(wildSpawnType))
            {
                return BotType.Boss;
            }
            else if (CheckIsFollower(wildSpawnType))
            {
                return BotType.Follower;
            }
            else if (wildSpawnType == WildSpawnType.assault || wildSpawnType == WildSpawnType.cursedAssault || wildSpawnType == WildSpawnType.marksman)
            {
                return BotType.Scav;
            }
            else if (wildString == "sptUsec" || wildString == "sptBear")
            {
                return BotType.PMC;
            }
            else
            {
                return BotType.None;
            }
        }

        public static bool CheckIsBoss(WildSpawnType bottype)
        {
            return BossTypes.Contains(bottype) || bottype == WildSpawnType.sectantPriest;
        }

        public static bool CheckIsFollower(WildSpawnType bottype)
        {
            return FollowerTypes.Contains(bottype) || bottype == WildSpawnType.sectantWarrior;
        }
    }

    public enum BotType
    {
        None,
        Scav,
        PMC,
        Boss,
        Follower
    }
}
