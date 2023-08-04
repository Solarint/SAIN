using BepInEx.Logging;
using EFT;
using SAIN.BotPresets;
using SAIN.BotSettings;
using SAIN.Components;
using SAIN.Plugin;
using SAIN.SAINPreset.Personalities;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Collections;

namespace SAIN.Classes
{
    public class SAINBotInfo : SAINInfoAbst
    {
        public SAINBotInfo(SAINComponent sain) : base(sain)
        {
            GetFileSettings();
            WeaponInfo = new WeaponInfo(sain);
            PresetHandler.PresetsUpdated += GetFileSettings;
        }

        static FieldInfo[] EFTSettingsCategories;
        static FieldInfo[] SAINSettingsCategories;

        static readonly Dictionary<FieldInfo, FieldInfo[]> EFTSettingsFields = new Dictionary<FieldInfo, FieldInfo[]>();
        static readonly Dictionary<FieldInfo, FieldInfo[]> SAINSettingsFields = new Dictionary<FieldInfo, FieldInfo[]>();

        public void GetFileSettings()
        {
            FileSettings = SAINPlugin.LoadedPreset.BotSettings.Get(WildSpawnType, BotDifficulty);
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

        public IEnumerator SetConfigValuesCoroutine(SAINSettings sainFileSettings)
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
                FieldInfo eftCategoryField = FindFieldByName(sainCategoryField.Name, EFTSettingsCategories);
                if (eftCategoryField != null)
                {
                    object sainCategory = sainCategoryField.GetValue(sainFileSettings);
                    object eftCategory = eftCategoryField.GetValue(eftFileSettings);

                    FieldInfo[] sainFields = SAINSettingsFields[sainCategoryField];
                    FieldInfo[] eftFields = EFTSettingsFields[eftCategoryField];
                    foreach (FieldInfo sainVarField in sainFields)
                    {
                        FieldInfo eftVarField = FindFieldByName(sainVarField.Name, eftFields);
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

        static FieldInfo FindFieldByName(string name, FieldInfo[] fields)
        {
            foreach (FieldInfo field in fields)
            {
                if (field.Name == name)
                {
                    return field;
                }
            }
            return null;
        }

        public SAINSettings FileSettings { get; private set; }

        public float TimeBeforeSearch { get; private set; } = 0f;

        public void ManualUpdate()
        {
            WeaponInfo.ManualUpdate();
        }

        public float HoldGroundDelay { get; private set; }

        public void CalcHoldGroundDelay()
        {
            var settings = PersonalitySettings;

            float baseTime = settings.HoldGroundBaseTime;
            float min = settings.HoldGroundMinRandom;
            float max = settings.HoldGroundMaxRandom;

            HoldGroundDelay = baseTime * Random.Range(min, max);
        }

        public void CalcTimeBeforeSearch()
        {
            float searchTime;
            if (IsFollower && SAIN.Squad.BotInGroup)
            {
                searchTime = 6f;
            }
            else if (IAmBoss && SAIN.Squad.BotInGroup)
            {
                searchTime = 60f;
            }
            else
            {
                searchTime = PersonalitySettings.SearchBaseTime;
            }

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
            float percentage = Random.Range(FileSettings.Mind.MinExtractPercentage, FileSettings.Mind.MaxExtractPercentage) / 100f;

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

        public SAINPersonality Personality { get; private set; }
        public PersonalitySettingsClass PersonalitySettings { get; private set; }

        public float PercentageBeforeExtract { get; set; } = -1f;

        private const float SearchRandomize = 0.33f;

        public WeaponInfo WeaponInfo { get; private set; }

        public void Dispose()
        {
            PresetHandler.PresetsUpdated -= GetFileSettings;
        }
    }
}