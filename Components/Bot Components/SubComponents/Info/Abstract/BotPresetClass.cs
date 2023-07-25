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
            DifficultyPreset = PresetManager.TypePresets[BotType].Preset;
            UpdateSettings(DifficultyPreset);
            PresetManager.PresetUpdated += PresetUpdated;
        }

        public void PresetUpdated(WildSpawnType type, BotPreset preset)
        {
            if (BotType == type && preset != null)
            {
                UpdateSettings(preset);
            }
        }

        public void UpdateSettings(BotPreset preset)
        {
            SetProperties(preset);

            var core = BotOwner.Settings.FileSettings.Core;
            core.VisibleDistance = (float)preset.VisibleDistance.GetValue(BotDifficulty);
            core.VisibleAngle = (float)preset.VisibleAngle.GetValue(BotDifficulty);
            core.CanGrenade = (bool)preset.CanUseGrenades.GetValue(BotDifficulty);

            SAIN.Info.CalcExtractTime();
        }

        public void SetProperties(BotPreset preset)
        {
            if (preset == null)
            {
                return;
            }

            MinPercentage = (float)preset.MinPercentage.GetValue(BotDifficulty);
            MaxPercentage = (float)preset.MaxPercentage.GetValue(BotDifficulty);
            EnableExtracts = (bool)preset.EnableExtracts.GetValue(BotDifficulty);

            RecoilMultiplier = (float)preset.RecoilMultiplier.GetValue(BotDifficulty);
            AccuracyMultiplier = (float)preset.AccuracyMulti.GetValue(BotDifficulty);

            FasterCQBReactions = (bool)preset.FasterCQBReactions.GetValue(BotDifficulty);
            FasterCQBReactionsDistance = (float)preset.FasterCQBReactionsDistance.GetValue(BotDifficulty);
            FasterCQBReactionsMinimum = (float)preset.FasterCQBReactionsMinimum.GetValue(BotDifficulty);

            AudibleRangeMultiplier = (float)preset.AudibleRangeMultiplier.GetValue(BotDifficulty);
            MaxFootstepAudioDistance = (float)preset.MaxFootstepAudioDistance.GetValue(BotDifficulty);

            BurstMulti = (float)preset.BurstMulti.GetValue(BotDifficulty);
            FireratMulti = (float)preset.FireratMulti.GetValue(BotDifficulty);

            VisionSpeedModifier = (float)preset.VisionSpeedModifier.GetValue(BotDifficulty);
            CloseVisionSpeed = (float)preset.CloseVisionSpeed.GetValue(BotDifficulty);
            FarVisionSpeed = (float)preset.FarVisionSpeed.GetValue(BotDifficulty);
            CloseFarThresh = (float)preset.CloseFarThresh.GetValue(BotDifficulty);

            CanTalk = (bool)preset.CanTalk.GetValue(BotDifficulty);
            BotTaunts = (bool)preset.BotTaunts.GetValue(BotDifficulty);
            SquadTalk = (bool)preset.SquadTalk.GetValue(BotDifficulty);
            SquadMemberTalkFreq = (float)preset.SquadMemberTalkFreq.GetValue(BotDifficulty);
            SquadLeadTalkFreq = (float)preset.SquadLeadTalkFreq.GetValue(BotDifficulty);
            TalkFrequency = (float)preset.TalkFrequency.GetValue(BotDifficulty);
        }

        public float MinPercentage { get; private set; }
        public float MaxPercentage { get; private set; }
        public bool EnableExtracts { get; private set; }

        public float RecoilMultiplier { get; private set; }
        public float AccuracyMultiplier { get; private set; }

        public bool FasterCQBReactions { get; private set; }
        public float FasterCQBReactionsDistance { get; private set; }
        public float FasterCQBReactionsMinimum { get; private set; }

        public float AudibleRangeMultiplier { get; private set; }
        public float MaxFootstepAudioDistance { get; private set; }

        public float BurstMulti { get; private set; }
        public float FireratMulti { get; private set; }

        public float VisionSpeedModifier { get; private set; }
        public float CloseVisionSpeed { get; private set; }
        public float FarVisionSpeed { get; private set; }
        public float CloseFarThresh { get; private set; }

        public bool CanTalk { get; private set; }
        public bool BotTaunts { get; private set; }
        public bool SquadTalk { get; private set; }
        public float SquadMemberTalkFreq { get; private set; }
        public float SquadLeadTalkFreq { get; private set; }
        public float TalkFrequency { get; private set; }

        public BotPreset DifficultyPreset { get; private set; }
    }
}
