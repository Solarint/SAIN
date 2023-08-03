using BepInEx.Logging;
using EFT;
using SAIN.BotPresets;
using SAIN.BotSettings;
using SAIN.Components;
using SAIN.Plugin;
using SAIN.SAINPreset.Personalities;
using UnityEngine;

namespace SAIN.Classes
{
    public class SAINBotInfo : SAINInfoAbstract
    {
        public SAINBotInfo(SAINComponent sain) : base(sain)
        {
            GetFileSettings();
            WeaponInfo = new WeaponInfo(sain);
            PresetHandler.PresetsUpdated += GetFileSettings;
        }

        public void GetFileSettings()
        {
            FileSettings = SAINPlugin.LoadedPreset.BotSettings.SAINSettings[WildSpawnType].Settings[BotDifficulty];

            Personality = GetPersonality();
            PersonalitySettings = SAINPlugin.LoadedPreset.PersonalityManager.Personalities[Personality];

            UpdateExtractTime();
            CalcTimeBeforeSearch();
            CalcHoldGroundDelay();
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