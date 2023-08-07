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
        }

        private void CalculateSettings()
        {
            Personality = GetPersonality();
            PersonalitySettings = SAINPlugin.LoadedPreset.PersonalityManager.Personalities[Personality];

            UpdateExtractTime();
            CalcTimeBeforeSearch();
            CalcHoldGroundDelay();
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
                            eftVarField.SetValue(eftCategory, sainValue);
                        }
                    }
                }
                yield return null;
            }

            CalculateSettings();
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
            var persSettings = SAINPlugin.LoadedPreset.GlobalSettings.Personality;
            if (persSettings.AllGigaChads || CanBePersonality(SAINPersonality.GigaChad))
            {
                return SAINPersonality.GigaChad;
            }
            if (persSettings.AllChads || CanBePersonality(SAINPersonality.Chad))
            {
                return SAINPersonality.Chad;
            }
            if (persSettings.AllRats || CanBePersonality(SAINPersonality.Rat))
            {
                return SAINPersonality.Rat;
            }

            if (CanBePersonality(SAINPersonality.Custom1))
            {
                return SAINPersonality.Custom1;
            }
            if (CanBePersonality(SAINPersonality.Custom2))
            {
                return SAINPersonality.Custom2;
            }
            if (CanBePersonality(SAINPersonality.Custom3))
            {
                return SAINPersonality.Custom3;
            }
            if (CanBePersonality(SAINPersonality.Custom4))
            {
                return SAINPersonality.Custom4;
            }
            if (CanBePersonality(SAINPersonality.Custom5))
            {
                return SAINPersonality.Custom5;
            }

            if (CanBePersonality(SAINPersonality.Timmy))
            {
                return SAINPersonality.Timmy;
            }
            if (CanBePersonality(SAINPersonality.Coward))
            {
                return SAINPersonality.Coward;
            }
            return SAINPersonality.Normal;
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
        public PersonalitySettingsClass PersonalitySettings { get; private set; }

        public float PercentageBeforeExtract { get; set; } = -1f;

        private const float SearchRandomize = 0.33f;

        public WeaponInfoClass WeaponInfo { get; private set; }
    }
}