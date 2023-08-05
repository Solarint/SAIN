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

        [JsonProperty]
        public string Name;
        [JsonProperty]
        public WildSpawnType WildSpawnType;
        [JsonProperty]
        public Dictionary<BotDifficulty, SAINSettings> Settings = new Dictionary<BotDifficulty, SAINSettings>();
    }

    public class SAINSettings
    {
        [NameAndDescription("Bot Aiming Settings", "Anything related to a bot's Aiming settings used by default EFT Code")]
        public SAINAimingSettings Aiming = new SAINAimingSettings();

        [NameAndDescription("Bot Change Settings", "Anything related to a bot's Change settings used by default EFT Code")]
        public SAINChangeSettings Change = new SAINChangeSettings();

        [NameAndDescription("Bot Core Settings", "Anything related to a bot's Core settings used by default EFT Code")]
        public SAINCoreSettings Core = new SAINCoreSettings();

        [NameAndDescription("Bot Grenade Settings", "Anything related to a bot's Grenade settings used by default EFT Code")]
        public SAINGrenadeSettings Grenade = new SAINGrenadeSettings();

        [NameAndDescription("Bot Hearing Settings", "Anything related to a bot's Hearing settings used by default EFT Code")]
        public SAINHearingSettings Hearing = new SAINHearingSettings();

        [NameAndDescription("Bot Lay Settings", "Anything related to a bot's Lay settings used by default EFT Code")]
        public SAINLaySettings Lay = new SAINLaySettings();

        [NameAndDescription("Bot Look Settings", "Anything related to a bot's Look settings used by default EFT Code")]
        public SAINLookSettings Look = new SAINLookSettings();

        [NameAndDescription("Bot Mind Settings", "Anything related to a bot's Mind settings used by default EFT Code")]
        public SAINMindSettings Mind = new SAINMindSettings();

        [NameAndDescription("Bot Move Settings", "Anything related to a bot's Move settings used by default EFT Code")]
        public SAINMoveSettings Move = new SAINMoveSettings();

        [NameAndDescription("Bot Patrol Settings", "Anything related to a bot's Patrol settings used by default EFT Code")]
        public SAINPatrolSettings Patrol = new SAINPatrolSettings();

        [NameAndDescription("Bot Scattering Settings", "Anything related to a bot's Scattering settings used by default EFT Code")]
        public SAINScatterSettings Scattering = new SAINScatterSettings();

        [NameAndDescription("Bot Shoot Settings","Anything related to a bot's shoot settings used by default EFT Code")]
        public SAINShootSettings Shoot = new SAINShootSettings();
    }
}
