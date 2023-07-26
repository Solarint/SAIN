using EFT;
using SAIN.BotPresets;
using System;

namespace SAIN.Classes
{
    public class BotPresetClass : SAINInfoAbstract
    {
        public BotPresetClass(BotOwner owner) : base(owner)
        {
            DefaultBotSettings = new BotSettingsClass(owner);
            BotSettings = new BotSettingsClass(owner);

            if (PresetManager.GetPreset(WildSpawnType, out var preset))
            {
                UpdateSettings(preset);
            }

            PresetManager.PresetUpdated += PresetUpdated;
        }

        public PresetValues PresetValues { get; set; }
        public BotSettingsClass DefaultBotSettings { get; private set; }
        public BotSettingsClass BotSettings { get; private set; }

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

            var core = BotSettings.Core;
            core.VisibleDistance = PresetValues.VisibleDistance;
            core.VisibleAngle = PresetValues.VisibleAngle;
            core.CanGrenade = PresetValues.CanGrenade;

            var aim = BotSettings.Aiming;
            var defaultAim = DefaultBotSettings.Aiming;
            aim.MAX_AIMING_UPGRADE_BY_TIME = defaultAim.MAX_AIMING_UPGRADE_BY_TIME / PresetValues.MAX_AIMING_UPGRADE_BY_TIME;
            aim.MAX_AIM_TIME = defaultAim.MAX_AIM_TIME / PresetValues.MAX_AIM_TIME;

            BotSettings.AssignSettings();
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

    public class BotSettingsClass
    {
        public readonly BotOwner BotOwner;

        public BotSettingsClass(BotOwner owner)
        {
            BotOwner = owner;

            Core = new BotCoreSettings(owner);

            var settings = owner.Settings.FileSettings.Copy();
            Aiming = settings.Aiming;
            Look = settings.Look;
            Shoot = settings.Shoot;
            Move = settings.Move;
            Grenade = settings.Grenade;
            Cover = settings.Cover;
            Patrol = settings.Patrol;
            Hearing = settings.Hearing;
            Mind = settings.Mind;
            Lay = settings.Lay;
            Boss = settings.Boss;
            Change = settings.Change;
            Scattering = settings.Scattering;
        }

        public void AssignSettings()
        {
            Core.AssignSettings();

            var settings = BotOwner.Settings.FileSettings;
            settings.Aiming = Aiming;
            settings.Look = Look;
            settings.Shoot = Shoot;
            settings.Move = Move;
            settings.Grenade = Grenade;
            settings.Cover = Cover;
            settings.Patrol = Patrol;
            settings.Hearing = Hearing;
            settings.Mind = Mind;
            settings.Lay = Lay;
            settings.Boss = Boss;
            settings.Change = Change;
            settings.Scattering = Scattering;
        }

        void SettingsPatches()
        {

        }

        public BotCoreSettings Core { get; private set; }
        public BotGlobalLayData Lay { get; private set; }
        public BotGlobalAimingSettings Aiming { get; private set; }
        public BotGlobalLookData Look { get; private set; }
        public BotGlobalShootData Shoot { get; private set; }
        public BotGlobalsMoveSettings Move { get; private set; }
        public BotGlobalsGrenadeSettings Grenade { get; private set; }
        public BotGlobalsChangeSettings Change { get; private set; }
        public BotGlobalsCoverSettings Cover { get; private set; }
        public BotGlobalPatrolSettings Patrol { get; private set; }
        public BotGlobasHearingSettings Hearing { get; private set; }
        public BotGlobalsMindSettings Mind { get; private set; }
        public BotGlobalsBossSettings Boss { get; private set; }
        public BotGlobalsScatteringSettings Scattering { get; private set; }

        public class BotCoreSettings
        {
            public readonly BotOwner BotOwner;

            public BotCoreSettings(BotOwner owner)
            {
                BotOwner = owner;

                var core = owner.Settings.FileSettings.Core;

                VisibleAngle = core.VisibleAngle;
                VisibleDistance = core.VisibleDistance;
                GainSightCoef = core.GainSightCoef;
                ScatteringPerMeter = core.ScatteringPerMeter;
                DamageCoeff = core.DamageCoeff;
                HearingSense = core.HearingSense;
                CanRun = true;
                CanGrenade = core.CanGrenade;
                AimingType = core.AimingType;
                PistolFireDistancePref = core.PistolFireDistancePref;
                ShotgunFireDistancePref = core.ShotgunFireDistancePref;
                RifleFireDistancePref = core.RifleFireDistancePref;
                AccuratySpeed = core.AccuratySpeed;
            }

            public void AssignSettings()
            {
                var settings = BotOwner.Settings.FileSettings;

                settings.Core.VisibleAngle = VisibleAngle;
                settings.Core.VisibleDistance = VisibleDistance;
                settings.Core.GainSightCoef = GainSightCoef;
                settings.Core.ScatteringPerMeter = ScatteringPerMeter;
                settings.Core.DamageCoeff = DamageCoeff;
                settings.Core.HearingSense = HearingSense;
                settings.Core.CanRun = true;
                settings.Core.CanGrenade = CanGrenade;
                settings.Core.AimingType = AimingType;
                settings.Core.PistolFireDistancePref = PistolFireDistancePref;
                settings.Core.ShotgunFireDistancePref = ShotgunFireDistancePref;
                settings.Core.RifleFireDistancePref = RifleFireDistancePref;
                settings.Core.AccuratySpeed = AccuratySpeed;
            }

            public float VisibleAngle = 110f;

            public float VisibleDistance = 137f;

            public float GainSightCoef = 0.2f;

            public float ScatteringPerMeter = 0.08f;

            public float ScatteringClosePerMeter = 0.12f;

            public float DamageCoeff = 1.2f;

            public float HearingSense = 0.65f;

            public bool CanRun = true;

            public bool CanGrenade = true;

            public AimingType AimingType;

            public float PistolFireDistancePref = 35f;

            public float ShotgunFireDistancePref = 50f;

            public float RifleFireDistancePref = 100f;

            public float AccuratySpeed = 0.3f;

            public float WaitInCoverBetweenShotsSec = 2f;
        }
    }
}