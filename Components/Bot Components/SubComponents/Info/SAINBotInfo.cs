using BepInEx.Logging;
using EFT;
using SAIN.BotPresets;
using SAIN.BotSettings;
using SAIN.Components;
using SAIN.Plugin;
using UnityEngine;

namespace SAIN.Classes
{
    public class SAINBotInfo : SAINInfoAbstract
    {
        public SAINBotInfo(SAINComponent botOwner) : base(botOwner)
        {
            GetFileSettings();
            PersonalityClass = new PersonalityClass(botOwner);
            WeaponInfo = new WeaponInfo(botOwner);
            PresetHandler.PresetsUpdated += GetFileSettings;
        }

        public void GetFileSettings()
        {
            FileSettings = SAINPlugin.LoadedPreset.BotSettings.SAINSettings[WildSpawnType].Settings[BotDifficulty];
            UpdateExtractTime();
        }

        public SAINSettings FileSettings { get; private set; }

        public float TimeBeforeSearch { get; private set; } = 0f;

        float LastPower = -1f;

        public void Update()
        {
            if (LastPower != PowerLevel)
            {
                LastPower = PowerLevel;
                GroupCount = BotOwner.BotsGroup.MembersCount;

                PersonalityClass.Update();

                CalcTimeBeforeSearch();
                CalcHoldGroundDelay();
            }
            if (BotOwner.BotsGroup.MembersCount != GroupCount)
            {
                GroupCount = BotOwner.BotsGroup.MembersCount;
                CalcTimeBeforeSearch();
            }


            WeaponInfo.ManualUpdate();
        }

        private int GroupCount = 0;

        public float HoldGroundDelay { get; private set; }

        public void CalcHoldGroundDelay()
        {
            var settings = PersonalityClass.PersonalitySettings;

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
                searchTime = PersonalityClass.PersonalitySettings.SearchBaseTime;
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

        public float PercentageBeforeExtract { get; set; } = -1f;

        private const float SearchRandomize = 0.33f;

        public PersonalityClass PersonalityClass { get; private set; }
        public WeaponInfo WeaponInfo { get; private set; }

        public SAINPersonality Personality => PersonalityClass.Personality;

        public void Dispose()
        {
            PresetHandler.PresetsUpdated -= GetFileSettings;
        }
    }
}