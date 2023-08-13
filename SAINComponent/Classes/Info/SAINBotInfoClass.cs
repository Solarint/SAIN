using BepInEx.Logging;
using EFT;
using SAIN.Preset;
using SAIN.SAINComponent.Classes.Decision;
using SAIN.SAINComponent.Classes.Talk;
using SAIN.SAINComponent.Classes.WeaponFunction;
using SAIN.SAINComponent.Classes.Mover;
using SAIN.SAINComponent.SubComponents;
using SAIN.Plugin;
using SAIN.Preset.Personalities;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Collections;
using SAIN.Helpers;
using SAIN.Preset.BotSettings.SAINSettings;
using static SAIN.Preset.Personalities.PersonalitySettingsClass;
using Comfort.Common;
using static Mono.Security.X509.X520;

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

        static FieldInfo[] EFTSettingsCategories;
        static FieldInfo[] SAINSettingsCategories;

        static readonly Dictionary<FieldInfo, FieldInfo[]> EFTSettingsFields = new Dictionary<FieldInfo, FieldInfo[]>();
        static readonly Dictionary<FieldInfo, FieldInfo[]> SAINSettingsFields = new Dictionary<FieldInfo, FieldInfo[]>();

        public void GetFileSettings()
        {
            FileSettings = SAINPlugin.LoadedPreset.BotSettings.GetSAINSettings(WildSpawnType, BotDifficulty);
            SAIN.StartCoroutine(SetConfigValuesCoroutine(FileSettings));
            CalculateSettings();
        }

        private void CalculateSettings()
        {
            Personality = GetPersonality();
            PersonalitySettingsClass = SAINPlugin.LoadedPreset.PersonalityManager.Personalities[Personality];

            UpdateExtractTime();
            CalcTimeBeforeSearch();
            CalcHoldGroundDelay();
            ManualSettingsUpdate();
        }

        private void ManualSettingsUpdate()
        {
            var sainSettings = FileSettings;
            var defaultSettings = HelpersGClass.GetEFTSettings(WildSpawnType, BotDifficulty);

            var botAim = BotOwner.Settings.FileSettings.Aiming;
            var botScatter = BotOwner.Settings.FileSettings.Scattering;

            float difficultyModifier = GlobalSAINSettings.General.GlobalDifficultyModifier;

            float multiplier = sainSettings.Aiming.AccuracySpreadMulti * GlobalSAINSettings.Aiming.AccuracySpreadMultiGlobal / difficultyModifier;
            multiplier = Mathf.Round(multiplier * 100f) / 100f;

            botAim.BASE_SHIEF = MultiplySetting(
                defaultSettings.Aiming.BASE_SHIEF, 
                multiplier, 
                "BASE_SHIEF");
            botAim.BOTTOM_COEF = MultiplySetting(
                defaultSettings.Aiming.BOTTOM_COEF, 
                multiplier, 
                "BOTTOM_COEF");

            multiplier = sainSettings.Scattering.ScatterMultiplier * GlobalSAINSettings.Shoot.GlobalScatterMultiplier / difficultyModifier;
            multiplier = Mathf.Round(multiplier * 100f) / 100f;

            botAim.XZ_COEF = MultiplySetting(
                defaultSettings.Aiming.XZ_COEF, 
                multiplier, 
                "XZ_COEF");
            botAim.XZ_COEF_STATIONARY_BULLET = MultiplySetting(
                defaultSettings.Aiming.XZ_COEF_STATIONARY_BULLET, 
                multiplier, 
                "XZ_COEF_STATIONARY_BULLET");
            botAim.XZ_COEF_STATIONARY_GRENADE = MultiplySetting(
                defaultSettings.Aiming.XZ_COEF_STATIONARY_GRENADE, 
                multiplier, 
                "XZ_COEF_STATIONARY_GRENADE");

            botScatter.MinScatter = MultiplySetting(
                defaultSettings.Scattering.MinScatter, 
                multiplier, 
                "MinScatter");
            botScatter.MaxScatter = MultiplySetting(
                defaultSettings.Scattering.MaxScatter, 
                multiplier, 
                "MaxScatter");
            botScatter.WorkingScatter = MultiplySetting(
                defaultSettings.Scattering.WorkingScatter, 
                multiplier, 
                "WorkingScatter");

            if (BotOwner.WeaponManager?.WeaponAIPreset != null)
            {
                BotOwner.WeaponManager.WeaponAIPreset.XZ_COEF = botAim.XZ_COEF;
                BotOwner.WeaponManager.WeaponAIPreset.BaseShift = botAim.BASE_SHIEF;
            }

            BotOwner.Settings.FileSettings.Core.VisibleDistance = MultiplySetting(
                defaultSettings.Core.VisibleDistance, 
                GlobalSAINSettings.Look.GlobalVisionDistanceMultiplier, 
                "VisibleDistance");
        }

        private float MultiplySetting(float defaultValue, float multiplier, string name)
        {
            float result = Mathf.Round(defaultValue * multiplier * 100f) / 100f;
            if (SAINPlugin.DebugModeEnabled)
            {
                Logger.LogInfo($"{name} Default {defaultValue} Multiplier: {multiplier} Result: {result}");
            }
            return result;
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
                yield return null;
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
                                Logger.LogInfo($"{eftVarField.Name} Default {eftVarField.GetValue(eftCategory)} NewValue: {sainValue}");
                            }

                            eftVarField.SetValue(eftCategory, sainValue);
                        }
                    }
                }
                yield return null;
            }
        }

        public SAINSettingsClass FileSettings { get; private set; }

        public float TimeBeforeSearch { get; private set; } = 0f;

        public float HoldGroundDelay { get; private set; }

        public void CalcHoldGroundDelay()
        {
            var settings = PersonalitySettings;

            float baseTime = settings.HoldGroundBaseTime;
            baseTime *= AggressionMultiplier;
            float min = settings.HoldGroundMinRandom;
            float max = settings.HoldGroundMaxRandom;

            float result = baseTime * Random.Range(min, max);
            HoldGroundDelay = Mathf.Round(result * 100f) / 100f;
        }

        private float AggressionMultiplier => FileSettings.Mind.Aggression * GlobalSAINSettings.Mind.GlobalAggression;

        public void CalcTimeBeforeSearch()
        {
            float searchTime;
            if (Profile.IsFollower && SAIN.Squad.BotInGroup)
            {
                searchTime = 6f;
            }
            else
            {
                searchTime = PersonalitySettings.SearchBaseTime;
            }

            searchTime /= AggressionMultiplier;
            searchTime *= Random.Range(1f - SearchRandomize, 1f + SearchRandomize);
            searchTime = Mathf.Round(searchTime * 10f) / 10f;
            if (searchTime < 0.25f)
            {
                searchTime = 0.25f;
            }

            TimeBeforeSearch = searchTime;
        }

        void UpdateExtractTime()
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

        private bool CanBePersonality(SAINPersonality personality)
        {
            var Personalities = SAINPlugin.LoadedPreset.PersonalityManager.Personalities;
            if (Personalities == null || !Personalities.ContainsKey(personality))
            {
                return false;
            }
            return Personalities[personality].CanBePersonality(WildSpawnType, PowerLevel, PlayerLevel);
        }

        public WildSpawnType WildSpawnType => Profile.WildSpawnType;
        public float PowerLevel => Profile.PowerLevel;
        public int PlayerLevel => Profile.PlayerLevel;
        public BotDifficulty BotDifficulty => Profile.BotDifficulty;

        public SAINPersonality Personality { get; private set; }
        public PersonalityVariablesClass PersonalitySettings => PersonalitySettingsClass?.Variables;
        public PersonalitySettingsClass PersonalitySettingsClass { get; private set; }

        public float PercentageBeforeExtract { get; set; } = -1f;

        private const float SearchRandomize = 0.33f;

        public WeaponInfoClass WeaponInfo { get; private set; }
    }
}