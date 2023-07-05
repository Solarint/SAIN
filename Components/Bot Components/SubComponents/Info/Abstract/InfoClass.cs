using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.Classes
{
    public class InfoClass
    {
        private readonly BotOwner BotOwner;
        public InfoClass(BotOwner owner)
        {
            BotOwner = owner;
            FindBotType();
        }

        public BotDifficulty BotDifficulty => BotOwner.Profile.Info.Settings.BotDifficulty;
        public WildSpawnType BotType => BotOwner.Profile.Info.Settings.Role;
        public float PowerLevel => BotOwner.AIData.PowerOfEquipment;
        public EPlayerSide Faction => BotOwner.Profile.Side;

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
    }
}
