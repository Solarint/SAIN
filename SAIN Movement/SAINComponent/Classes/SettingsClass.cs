using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.Classes
{
    public class SettingsClass : SAINBot
    {
        public SettingsClass(BotOwner bot) : base(bot)
        {
            DifficultyModifier = CalculateDifficulty(bot);
            WeaponInfo = new WeaponInfo(bot);
        }

        public void ManualUpdate()
        {
            WeaponInfo.ManualUpdate();
        }

        public WeaponInfo WeaponInfo { get; private set; }
        public float DifficultyModifier { get; private set; }

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
                return GetDifficultyMod(settings.Role, settings.BotDifficulty, SAIN.Info.IsBoss, SAIN.Info.IsFollower);
            }

            return 1f;
        }

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
    }
}
