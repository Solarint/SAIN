using EFT;
using SAIN.Helpers;

namespace SAIN.SAINComponent.Classes.Info
{
    public class ProfileClass : SAINBase, ISAINClass
    {
        public ProfileClass(SAINComponentClass sain) : base(sain)
        {
            IsBoss = EnumValues.WildSpawn.IsBoss(WildSpawnType);
            IsFollower = EnumValues.WildSpawn.IsFollower(WildSpawnType);
            IsScav = EnumValues.WildSpawn.IsScav(WildSpawnType);
            IsPMC = EnumValues.WildSpawn.IsPMC(WildSpawnType);
            SetDiffModifier(BotDifficulty);
        }

        public void Init()
        {
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        public float DifficultyModifier { get; private set; }

        public bool IsBoss { get; private set; }
        public bool IsFollower { get; private set; }
        public bool IsScav { get; private set; }
        public bool IsPMC { get; private set; }

        public BotDifficulty BotDifficulty => BotOwner.Profile.Info.Settings.BotDifficulty;

        public WildSpawnType WildSpawnType => BotOwner.Profile.Info.Settings.Role;

        public float PowerLevel => BotOwner.AIData.PowerOfEquipment;

        public EPlayerSide Faction => BotOwner.Profile.Side;

        public int PlayerLevel => BotOwner.Profile.Info.Level;

        private void SetDiffModifier(BotDifficulty difficulty)
        {
            float modifier = 1f;

            if (IsBoss)
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
    }
}