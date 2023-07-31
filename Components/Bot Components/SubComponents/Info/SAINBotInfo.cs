using BepInEx.Logging;
using EFT;
using SAIN.BotPresets;
using SAIN.Components;
using UnityEngine;

namespace SAIN.Classes
{
    public class SAINBotInfo : SAINInfoAbstract
    {
        public SAINBotInfo(SAINComponent botOwner) : base(botOwner)
        {
            BotPresetClass = new BotPresetClass(botOwner);
            PersonalityClass = new PersonalityClass(botOwner);
            WeaponInfo = new WeaponInfo(botOwner);
        }

        public PresetValues FileSettings => BotPresetClass.PresetValues;

        public BotPresetClass BotPresetClass { get; private set; }

        public float TimeBeforeSearch { get; private set; } = 0f;
        public float PercentageBeforeExtract => BotPresetClass.PercentageBeforeExtract;

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

        private const float SearchRandomize = 0.33f;

        public PersonalityClass PersonalityClass { get; private set; }
        public WeaponInfo WeaponInfo { get; private set; }

        public SAINPersonality Personality => PersonalityClass.Personality;

        public void Dispose()
        {
            PresetManager.PresetUpdated -= BotPresetClass.PresetUpdated;
        }
    }
}