using EFT;
using Newtonsoft.Json;
using SAIN.BotPresets;
using SAIN.Components.BotSettings;
using SAIN.Helpers;

namespace SAIN.Classes
{
    public class BotPresetClass : SAINInfoAbstract
    {
        public BotPresetClass(BotOwner owner) : base(owner)
        {
            DefaultBotSettings = new SAINBotSettingsClass(owner);
            BotSettings = new SAINBotSettingsClass(owner);
            //BotSettings = BotSettingsManager.GetBotSettings(WildSpawnType, BotDifficulty);

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

            var core = BotSettings.Core;
            core.VisibleDistance = PresetValues.VisibleDistance;
            core.VisibleAngle = PresetValues.VisibleAngle;
            core.CanGrenade = PresetValues.CanGrenade;

            var aim = BotSettings.Aiming;
            var defaultAim = DefaultBotSettings.Aiming;
            aim.MAX_AIMING_UPGRADE_BY_TIME = defaultAim.MAX_AIMING_UPGRADE_BY_TIME / PresetValues.MAX_AIMING_UPGRADE_BY_TIME;
            aim.MAX_AIM_TIME = defaultAim.MAX_AIM_TIME / PresetValues.MAX_AIM_TIME;

            BotSettings.SetNewSettings(BotOwner);
            //JsonUtility.Save.SaveBotSettings(BotSettings, BotOwner);
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

    public class SAINBotSettingsClass
    {
        [JsonConstructor]
        public SAINBotSettingsClass() { }

        public SAINBotSettingsClass(BotOwner owner)
        {
            Core = new BotCoreSettings();
            Core.SetDefaults(owner);

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

        public SAINBotSettingsClass(GClass562 settings)
        {
            Core = new BotCoreSettings();
            Core.SetDefaults(settings);

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

        public void SetNewSettings(BotOwner owner)
        {
            Core.SetNewSettings(owner);

            var settings = owner.Settings.FileSettings;
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

        [JsonProperty]
        public BotCoreSettings Core { get; private set; } = new BotCoreSettings();
        [JsonProperty]
        public BotGlobalLayData Lay { get; private set; } = new BotGlobalLayData();
        [JsonProperty]
        public BotGlobalAimingSettings Aiming { get; private set; } = new BotGlobalAimingSettings();
        [JsonProperty]
        public BotGlobalLookData Look { get; private set; } = new BotGlobalLookData();
        [JsonProperty]
        public BotGlobalShootData Shoot { get; private set; } = new BotGlobalShootData();
        [JsonProperty]
        public BotGlobalsMoveSettings Move { get; private set; } = new BotGlobalsMoveSettings();
        [JsonProperty]
        public BotGlobalsGrenadeSettings Grenade { get; private set; } = new BotGlobalsGrenadeSettings();
        [JsonProperty]
        public BotGlobalsChangeSettings Change { get; private set; } = new BotGlobalsChangeSettings();
        [JsonProperty]
        public BotGlobalsCoverSettings Cover { get; private set; } = new BotGlobalsCoverSettings();
        [JsonProperty]
        public BotGlobalPatrolSettings Patrol { get; private set; } = new BotGlobalPatrolSettings();
        [JsonProperty]
        public BotGlobasHearingSettings Hearing { get; private set; } = new BotGlobasHearingSettings();
        [JsonProperty]
        public BotGlobalsMindSettings Mind { get; private set; } = new BotGlobalsMindSettings();
        [JsonProperty]
        public BotGlobalsBossSettings Boss { get; private set; } = new BotGlobalsBossSettings();
        [JsonProperty]
        public BotGlobalsScatteringSettings Scattering { get; private set; } = new BotGlobalsScatteringSettings();

        public class BotCoreSettings
        {
            public void SetDefaults(BotOwner owner)
            {
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

            public void SetDefaults(GClass562 settings)
            {
                var core = settings.Core;
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

            public void SetNewSettings(BotOwner owner)
            {
                var core = owner.Settings.FileSettings.Core;
                core.VisibleAngle = VisibleAngle;
                core.VisibleDistance = VisibleDistance;
                core.GainSightCoef = GainSightCoef;
                core.ScatteringPerMeter = ScatteringPerMeter;
                core.DamageCoeff = DamageCoeff;
                core.HearingSense = HearingSense;
                core.CanRun = true;
                core.CanGrenade = CanGrenade;
                core.AimingType = AimingType;
                core.PistolFireDistancePref = PistolFireDistancePref;
                core.ShotgunFireDistancePref = ShotgunFireDistancePref;
                core.RifleFireDistancePref = RifleFireDistancePref;
                core.AccuratySpeed = AccuratySpeed;
            }

            public void SetNewSettings(GClass562 settings)
            {
                var core = settings.Core;
                core.VisibleAngle = VisibleAngle;
                core.VisibleDistance = VisibleDistance;
                core.GainSightCoef = GainSightCoef;
                core.ScatteringPerMeter = ScatteringPerMeter;
                core.DamageCoeff = DamageCoeff;
                core.HearingSense = HearingSense;
                core.CanRun = true;
                core.CanGrenade = CanGrenade;
                core.AimingType = AimingType;
                core.PistolFireDistancePref = PistolFireDistancePref;
                core.ShotgunFireDistancePref = ShotgunFireDistancePref;
                core.RifleFireDistancePref = RifleFireDistancePref;
                core.AccuratySpeed = AccuratySpeed;
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