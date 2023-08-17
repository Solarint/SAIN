using SAIN.Attributes;
using SAIN.Preset.BotSettings.SAINSettings.Categories;

namespace SAIN.Preset.BotSettings.SAINSettings
{
    public class SAINSettingsClass
    {
        [NameAndDescription("Bot Aiming Settings", 
            "Anything related to a bot's Aiming settings used by default EFT Code")]
        public SAINAimingSettings Aiming = new SAINAimingSettings();

        [Advanced(IAdvancedOption.Hidden)]
        [NameAndDescription("Bot Aiming Settings", 
            "Anything related to a bot's Aiming settings used by default EFT Code")]
        public SAINBossSettings Boss = new SAINBossSettings();

        [Advanced(IAdvancedOption.Hidden)]
        [NameAndDescription("Bot Change Settings", 
            "Anything related to a bot's Change settings used by default EFT Code")]
        public SAINChangeSettings Change = new SAINChangeSettings();

        [NameAndDescription("Bot Core Settings", 
            "Anything related to a bot's Core settings used by default EFT Code")]
        public SAINCoreSettings Core = new SAINCoreSettings();

        [Advanced(IAdvancedOption.Hidden)]
        [NameAndDescription("Bot Grenade Settings", 
            "Anything related to a bot's Grenade settings used by default EFT Code")]
        public SAINGrenadeSettings Grenade = new SAINGrenadeSettings();

        [NameAndDescription("Bot Hearing Settings", 
            "Anything related to a bot's Hearing settings used by default EFT Code")]
        public SAINHearingSettings Hearing = new SAINHearingSettings();

        [Advanced(IAdvancedOption.Hidden)]
        [NameAndDescription("Bot Lay Settings", 
            "Anything related to a bot's Lay settings used by default EFT Code")]
        public SAINLaySettings Lay = new SAINLaySettings();

        [NameAndDescription("Bot Look Settings", 
            "Anything related to a bot's Look settings used by default EFT Code")]
        public SAINLookSettings Look = new SAINLookSettings();

        [NameAndDescription("Bot Mind Settings", 
            "Anything related to a bot's Mind settings used by default EFT Code")]
        public SAINMindSettings Mind = new SAINMindSettings();

        [NameAndDescription("Bot Move Settings", 
            "Anything related to a bot's Move settings used by default EFT Code")]
        public SAINMoveSettings Move = new SAINMoveSettings();

        [NameAndDescription("Bot Patrol Settings", 
            "Anything related to a bot's Patrol settings used by default EFT Code")]
        public SAINPatrolSettings Patrol = new SAINPatrolSettings();

        [NameAndDescription("Bot Scattering Settings", 
            "Anything related to a bot's Scattering settings used by default EFT Code")]
        public SAINScatterSettings Scattering = new SAINScatterSettings();

        [NameAndDescription("Bot Shoot Settings", 
            "Anything related to a bot's shoot settings used by default EFT Code")]
        public SAINShootSettings Shoot = new SAINShootSettings();
    }
}