using EFT;
using SAIN.Helpers;
using SAIN.Plugin;
using SAIN.Preset.BotSettings.SAINSettings;
using SAIN.Preset.Personalities;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using static SAIN.Preset.Personalities.PersonalitySettingsClass;
using Random = UnityEngine.Random;

namespace SAIN.SAINComponent.Classes.Info
{
    public class SAINBotInfoClass : SAINBase, ISAINClass
    {
        public SAINBotInfoClass(SAINComponentClass sain) : base(sain)
        {
            Profile = new ProfileClass(sain);
            WeaponInfo = new WeaponInfoClass(sain);
        }

        public void Init()
        {
            GetFileSettings();
            PresetHandler.PresetsUpdated += GetFileSettings;
            WeaponInfo.Init();
            Profile.Init();
        }

        public void Update()
        {
            WeaponInfo.Update();
            Profile.Update();
        }

        public void Dispose()
        {
            PresetHandler.PresetsUpdated -= GetFileSettings;
            WeaponInfo.Dispose();
            Profile.Dispose();
        }

        public ProfileClass Profile { get; private set; }

        private static FieldInfo[] EFTSettingsCategories;
        private static FieldInfo[] SAINSettingsCategories;

        private static readonly Dictionary<FieldInfo, FieldInfo[]> EFTSettingsFields = new Dictionary<FieldInfo, FieldInfo[]>();
        private static readonly Dictionary<FieldInfo, FieldInfo[]> SAINSettingsFields = new Dictionary<FieldInfo, FieldInfo[]>();

        public void GetFileSettings()
        {
            FileSettings = SAINPlugin.LoadedPreset.BotSettings.GetSAINSettings(WildSpawnType, BotDifficulty);

            Personality = GetPersonality();
            PersonalitySettingsClass = SAINPlugin.LoadedPreset.PersonalityManager.Personalities[Personality];
            SearchMoveSpeed = PersonalitySettingsClass.Variables.BaseSearchMoveSpeed.Randomize(0.9f, 1.1f).Round100();

            UpdateExtractTime();
            CalcTimeBeforeSearch();
            CalcHoldGroundDelay();

            SAIN.StartCoroutine(SetConfigValuesCoroutine(FileSettings));
        }

        public IEnumerator SetConfigValuesCoroutine(SAINSettingsClass sainFileSettings)
        {
            var eftFileSettings = BotOwner.Settings.FileSettings;
            if (EFTSettingsCategories == null)
            {
                var flags = BindingFlags.Instance | BindingFlags.Public;

                EFTSettingsCategories = eftFileSettings.GetType().GetFields(flags);
                foreach (FieldInfo field in EFTSettingsCategories)
                {
                    EFTSettingsFields.Add(field, field.FieldType.GetFields(flags));
                }

                SAINSettingsCategories = sainFileSettings.GetType().GetFields(flags);
                foreach (FieldInfo field in SAINSettingsCategories)
                {
                    SAINSettingsFields.Add(field, field.FieldType.GetFields(flags));
                }
            }

            foreach (FieldInfo sainCategoryField in SAINSettingsCategories)
            {
                FieldInfo eftCategoryField = Reflection.FindFieldByName(sainCategoryField.Name, EFTSettingsCategories);
                if (eftCategoryField != null)
                {
                    object sainCategory = sainCategoryField.GetValue(sainFileSettings);
                    object eftCategory = eftCategoryField.GetValue(eftFileSettings);

                    FieldInfo[] sainFields = SAINSettingsFields[sainCategoryField];
                    FieldInfo[] eftFields = EFTSettingsFields[eftCategoryField];
                    foreach (FieldInfo sainVarField in sainFields)
                    {
                        FieldInfo eftVarField = Reflection.FindFieldByName(sainVarField.Name, eftFields);
                        if (eftVarField != null)
                        {
                            object sainValue = sainVarField.GetValue(sainCategory);
                            if (SAINPlugin.DebugModeEnabled)
                            {
                                // Logger.LogInfo($"{eftVarField.Value} Default {eftVarField.GetValue(eftCategory)} NewValue: {sainValue}");
                            }

                            eftVarField.SetValue(eftCategory, sainValue);
                        }
                    }
                }
                yield return null;
            }
            UpdateSettingClass.ManualSettingsUpdate(WildSpawnType, BotDifficulty, BotOwner, FileSettings);
        }

        public SAINSettingsClass FileSettings { get; private set; }

        public float TimeBeforeSearch { get; private set; } = 0f;

        public float HoldGroundDelay { get; private set; }

        public void CalcHoldGroundDelay()
        {
            var settings = PersonalitySettings;
            float baseTime = settings.HoldGroundBaseTime * AggressionMultiplier;

            float min = settings.HoldGroundMinRandom;
            float max = settings.HoldGroundMaxRandom;
            HoldGroundDelay = baseTime.Randomize(min, max).Round100();
        }

        private float AggressionMultiplier => (FileSettings.Mind.Aggression * GlobalSAINSettings.Mind.GlobalAggression * PersonalitySettings.AggressionMultiplier).Round100();

        public void CalcTimeBeforeSearch()
        {
            float searchTime;
            if (Profile.IsFollower && SAIN.Squad.BotInGroup)
            {
                searchTime = 5f;
            }
            else
            {
                searchTime = PersonalitySettings.SearchBaseTime;
            }

            searchTime = (searchTime.Randomize(0.66f, 1.33f) / AggressionMultiplier).Round10();
            if (searchTime < 0.2f)
            {
                searchTime = 0.2f;
            }

            TimeBeforeSearch = searchTime;
            float random = 30f.Randomize(0.75f, 1.25f).Round100();
            float forgetTime = searchTime + random;
            if (forgetTime < 45f)
            {
                forgetTime = 45f.Randomize(0.85f, 1.15f).Round100();
            }
            BotOwner.Settings.FileSettings.Mind.TIME_TO_FORGOR_ABOUT_ENEMY_SEC = forgetTime;
        }

        private void UpdateExtractTime()
        {
            float percentage = Random.Range(FileSettings.Mind.MinExtractPercentage, FileSettings.Mind.MaxExtractPercentage);

            var squad = SAIN?.Squad;
            var members = squad?.SquadMembers;
            if (squad != null && squad.BotInGroup && members != null && members.Count > 0)
            {
                if (squad.IAmLeader)
                {
                    PercentageBeforeExtract = percentage;
                    foreach (var member in members)
                    {
                        var infocClass = member.Value?.Info;
                        if (infocClass != null)
                        {
                            infocClass.PercentageBeforeExtract = percentage;
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

        public SAINPersonality GetPersonality()
        {
            if (!SAINPlugin.LoadedPreset.GlobalSettings.Personality.CheckForForceAllPers(out SAINPersonality result))
            {
                foreach (PersonalitySettingsClass setting in SAINPlugin.LoadedPreset.PersonalityManager.Personalities.Values)
                {
                    if (setting.CanBePersonality(WildSpawnType, PowerLevel, PlayerLevel))
                    {
                        result = Personality;
                        break;
                    }
                }
            }
            return result;
        }

        public WildSpawnType WildSpawnType => Profile.WildSpawnType;
        public float PowerLevel => Profile.PowerLevel;
        public int PlayerLevel => Profile.PlayerLevel;
        public BotDifficulty BotDifficulty => Profile.BotDifficulty;

        public SAINPersonality Personality { get; private set; }
        public PersonalityVariablesClass PersonalitySettings => PersonalitySettingsClass?.Variables;
        public PersonalitySettingsClass PersonalitySettingsClass { get; private set; }

        public float PercentageBeforeExtract { get; set; } = -1f;
        public float SearchMoveSpeed { get; private set; }

        public WeaponInfoClass WeaponInfo { get; private set; }
    }
}