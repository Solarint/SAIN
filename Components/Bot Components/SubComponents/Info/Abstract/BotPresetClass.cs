using EFT;
using SAIN.BotPresets;
using static HBAO_Core;

namespace SAIN.Classes
{
    public class BotPresetClass : SAINInfoAbstract
    {
        public BotPresetClass(BotOwner owner, SAINBotInfo info) : base(owner, info)
        {
            BotOwner.Settings.FileSettings.Core.CanRun = true;
            var preset = PresetManager.TypePresets[BotType].Preset;
            UpdateSettings(preset);
            PresetManager.PresetUpdated += PresetUpdated;
        }

        public void PresetUpdated(WildSpawnType type, BotPreset preset)
        {
            if (BotType == type)
            {
                UpdateSettings(preset);
            }
        }

        public void UpdateSettings(BotPreset preset)
        {
            DifficultyPreset = preset;
            var core = BotOwner.Settings.FileSettings.Core;
            core.VisibleDistance = (float)DifficultyPreset.VisibleDistance.GetValue(BotDifficulty);
            core.VisibleAngle = (float)DifficultyPreset.VisibleAngle.GetValue(BotDifficulty);
            core.CanGrenade = (bool)DifficultyPreset.CanUseGrenades.GetValue(BotDifficulty);
        }

        public BotPreset DifficultyPreset { get; private set; }
    }
}
