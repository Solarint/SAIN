using BepInEx.Logging;
using EFT;
using SAIN.BotPresets;
using System.Linq;
using System;
using SAIN.Components;
using SAIN.Helpers;

namespace SAIN.Classes
{
    public abstract class SAINInfoAbstract : SAINBot
    {
        public SAINInfoAbstract(SAINComponent owner) : base(owner)
        {
            IAmBoss = EnumValues.WildSpawn.IsBoss(WildSpawnType);
            IsFollower = EnumValues.WildSpawn.IsFollower(WildSpawnType);
            IsScav = EnumValues.WildSpawn.IsScav(WildSpawnType);
            IsPMC = EnumValues.WildSpawn.IsPMC(WildSpawnType);
            SetDiffModifier(BotDifficulty);
        }

        public readonly bool IAmBoss;
        public readonly bool IsFollower;
        public readonly bool IsScav;
        public readonly bool IsPMC;

        void SetDiffModifier(BotDifficulty difficulty)
        {
            float modifier = 1f;

            if (IAmBoss)
            {
                modifier = 0.85f;
            }
            if (IsFollower)
            {
                modifier = 0.95f;
            }
            if (IsScav)
            {
                modifier = 1.25f;
            }
            if (IsPMC)
            {
                modifier = 0.75f;
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
    }
}
