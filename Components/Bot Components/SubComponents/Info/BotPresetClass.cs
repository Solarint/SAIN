using EFT;
using Newtonsoft.Json;
using SAIN.BotPresets;
using SAIN.BotSettings;
using SAIN.Helpers;

namespace SAIN.Classes
{
    public class BotPresetClass : SAINInfoAbstract
    {
        public BotPresetClass(BotOwner owner) : base(owner)
        {
            BotSettings = BotSettingsHandler.GetSettings(owner);

            if (PresetManager.GetPreset(WildSpawnType, out var preset))
            {
                UpdateSettings(preset);
            }

            PresetManager.PresetUpdated += PresetUpdated;
        }

        public PresetValues PresetValues { get; set; }
        public SAINBotSettingsClass DefaultBotSettings { get; private set; }
        public SAINBotSettingsClass BotSettings { get; private set; }

        public void PresetUpdated(WildSpawnType type, BotPreset preset)
        {
            if (WildSpawnType == type && preset != null)
            {
                UpdateSettings(preset);
            }
        }

        public void UpdateSettings(BotPreset preset)
        {
            DifficultyPreset = preset;
            PresetValues = preset.GetValues(BotDifficulty);
            UpdateExtractTime();

            var core = BotSettings.Settings.Core;
            core.VisibleDistance = PresetValues.VisibleDistance;
            core.VisibleAngle = PresetValues.VisibleAngle;
            core.CanGrenade = PresetValues.CanGrenade;

            var aim = BotSettings.Settings.Aiming;
            var defaultAim = BotSettings.BotDefaultValues.DefaultSettings.Aiming;
            aim.MAX_AIMING_UPGRADE_BY_TIME = defaultAim.MAX_AIMING_UPGRADE_BY_TIME / PresetValues.MAX_AIMING_UPGRADE_BY_TIME;
            aim.MAX_AIM_TIME = defaultAim.MAX_AIM_TIME / PresetValues.MAX_AIM_TIME;

        }

        void UpdateExtractTime()
        {
            if (PresetValues == null && DifficultyPreset != null)
            {
                PresetValues = DifficultyPreset.GetValues(BotDifficulty);
            }
            if (PresetValues == null)
            {
                return;
            }

            float percentage = UnityEngine.Random.Range(PresetValues.MinPercentage, PresetValues.MaxPercentage);

            var squad = SAIN?.Squad;
            var members = squad?.SquadMembers;
            if (squad != null && squad.BotInGroup && members != null && members.Count > 0)
            {
                if (squad.IAmLeader)
                {
                    PercentageBeforeExtract = percentage;
                    foreach (var member in members)
                    {
                        var presetClass = member.Value?.Info?.BotPresetClass;
                        if (presetClass != null)
                        {
                            presetClass.PercentageBeforeExtract = percentage;
                        }
                    }
                }
                else if (PercentageBeforeExtract == -1f)
                {
                    var Leader = squad?.LeaderComponent?.Info;
                    if (Leader != null)
                    {
                        PercentageBeforeExtract = Leader.PercentageBeforeExtract;
                    }
                }
            }
            else
            {
                PercentageBeforeExtract = percentage;
            }
        }

        public float PercentageBeforeExtract { get; set; } = -1f;
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