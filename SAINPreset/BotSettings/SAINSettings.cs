using EFT;
using Newtonsoft.Json;
using SAIN.BotSettings.Categories;
using SAIN.SAINPreset.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.BotSettings
{
    public class SAINSettingsGroup
    {
        [JsonConstructor]
        public SAINSettingsGroup() { }

        public SAINSettingsGroup(string name, WildSpawnType type, BotDifficulty[] difficulties)
        {
            Name = name;
            WildSpawnType = type;
            foreach (var difficulty in difficulties)
            {
                Settings.Add(difficulty, new SAINSettings());
            }
        }

        public readonly string Name;
        public readonly WildSpawnType WildSpawnType;
        public readonly Dictionary<BotDifficulty, SAINSettings> Settings = new Dictionary<BotDifficulty, SAINSettings>();
    }

    public class SAINSettings
    {
        [Name("Bot Aiming Settings")]
        [Description("Anything related to a bot's Aiming settings used by default EFT Code")]
        public SAINAimingSettings Aiming = new SAINAimingSettings();

        [Name("Bot Change Settings")]
        [Description("Anything related to a bot's Change settings used by default EFT Code")]
        public SAINChangeSettings Change = new SAINChangeSettings();

        [Name("Bot Core Settings")]
        [Description("Anything related to a bot's Core settings used by default EFT Code")]
        public SAINCoreSettings Core = new SAINCoreSettings();

        [Name("Bot Grenade Settings")]
        [Description("Anything related to a bot's Grenade settings used by default EFT Code")]
        public SAINGrenadeSettings Grenade = new SAINGrenadeSettings();

        [Name("Bot Hearing Settings")]
        [Description("Anything related to a bot's Hearing settings used by default EFT Code")]
        public SAINHearingSettings Hearing = new SAINHearingSettings();

        [Name("Bot Lay Settings")]
        [Description("Anything related to a bot's Lay settings used by default EFT Code")]
        public SAINLaySettings Lay = new SAINLaySettings();

        [Name("Bot Look Settings")]
        [Description("Anything related to a bot's Look settings used by default EFT Code")]
        public SAINLookSettings Look = new SAINLookSettings();

        [Name("Bot Mind Settings")]
        [Description("Anything related to a bot's Mind settings used by default EFT Code")]
        public SAINMindSettings Mind = new SAINMindSettings();

        [Name("Bot Move Settings")]
        [Description("Anything related to a bot's Move settings used by default EFT Code")]
        public SAINMoveSettings Move = new SAINMoveSettings();

        [Name("Bot Patrol Settings")]
        [Description("Anything related to a bot's Patrol settings used by default EFT Code")]
        public SAINPatrolSettings Patrol = new SAINPatrolSettings();

        [Name("Bot Scattering Settings")]
        [Description("Anything related to a bot's Scattering settings used by default EFT Code")]
        public SAINScatterSettings Scattering = new SAINScatterSettings();

        [Name("Bot Shoot Settings")]
        [Description("Anything related to a bot's shoot settings used by default EFT Code")]
        public SAINShootSettings Shoot = new SAINShootSettings();
    }
}
