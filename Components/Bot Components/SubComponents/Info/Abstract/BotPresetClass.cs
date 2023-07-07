using EFT;

namespace SAIN.Classes
{
    public class BotPresetClass : SAINInfoAbstract
    {
        public BotPresetClass(BotOwner owner, SAINBotInfo info) : base(owner, info)
        {
            BotOwner.Settings.FileSettings.Core.CanRun = true;
            PresetHandler();
            SAINBotPresetManager.PresetUpdated += PresetUpdated;
        }

        private void PresetHandler()
        {
            DifficultyPreset = SAINBotPresetManager.LoadPreset(BotType, BotDifficulty);
        }

        public void PresetUpdated(WildSpawnType type, BotDifficulty diff, SAINBotPreset preset)
        {
            if (DifficultyPreset != null)
            {
                if (DifficultyPreset.BotType == type && DifficultyPreset.Difficulty == diff)
                {
                    DifficultyPreset = preset;
                    UpdateSettings();
                }
            }
        }

        public void UpdateSettings()
        {
            var core = BotOwner.Settings.FileSettings.Core;
            core.VisibleDistance = DifficultyPreset.VisibleDistance.GetValue(BotDifficulty);
            core.VisibleAngle = DifficultyPreset.VisibleAngle.GetValue(BotDifficulty);
            core.CanGrenade = DifficultyPreset.CanUseGrenades.GetValue(BotDifficulty);
        }

        public SAINBotPreset DifficultyPreset { get; private set; }
        public BotDifficultySettingsClass Settings => BotOwner.Settings;
    }
}
