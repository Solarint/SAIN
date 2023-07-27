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

        public BotTypeEnum BotType { get; private set; }

        public bool IAmBoss => BotType == BotTypeEnum.Boss;
        public bool IsFollower => BotType == BotTypeEnum.Follower;
        public bool IsScav => BotType == BotTypeEnum.Scav;
        public bool IsPMC => BotType == BotTypeEnum.PMC;

        void SetDiffModifier(BotTypeEnum type, BotDifficulty difficulty)
        {
            float modifier;
            switch (type)
            {
                case BotTypeEnum.Boss:
                    modifier = 0.85f; break;
                case BotTypeEnum.Follower:
                    modifier = 0.95f; break;
                case BotTypeEnum.Scav:
                    modifier = 1.25f; break;
                case BotTypeEnum.PMC:
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
        public int PlayerLevel => BotOwner.Profile.Info.Level;

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

        public static BotTypeEnum FindType(WildSpawnType wildSpawnType)
        {
            string wildString = wildSpawnType.ToString();
            if (CheckIsBoss(wildSpawnType))
            {
                return BotTypeEnum.Boss;
            }
            else if (CheckIsFollower(wildSpawnType))
            {
                return BotTypeEnum.Follower;
            }
            else if (wildSpawnType == WildSpawnType.assault || wildSpawnType == WildSpawnType.cursedAssault || wildSpawnType == WildSpawnType.marksman)
            {
                return BotTypeEnum.Scav;
            }
            else if (wildString == "sptUsec" || wildString == "sptBear")
            {
                return BotTypeEnum.PMC;
            }
            else
            {
                return BotTypeEnum.None;
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

    public enum BotTypeEnum
    {
        None,
        Scav,
        PMC,
        Boss,
        Follower
    }
}
