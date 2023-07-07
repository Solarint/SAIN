using BepInEx.Logging;
using EFT;
using UnityEngine;
using static SAIN.Editor.EditorSettings;

namespace SAIN.Classes
{
    public class SAINBotInfo : SAINBot
    {
        public SAINBotInfo(BotOwner botOwner) : base(botOwner)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource("SAIN Info");
            InfoClass = new InfoClass(botOwner);
        }

        public void Init()
        {
            BotPresetClass = new BotPresetClass(BotOwner, this);
            PersonalityClass = new PersonalityClass(BotOwner, this);

            CalcHoldGroundDelay();
            CalcTimeBeforeSearch();

            WeaponInfo = new WeaponInfo(BotOwner, this);

            PercentageBeforeExtract = Random.Range(MinPercentage.Value, MaxPercentage.Value);
            LastPower = PowerLevel;
        }

        public float DifficultyModifier => InfoClass.DifficultyModifier;
        public bool IAmBoss => InfoClass.IAmBoss;
        public bool IsFollower => InfoClass.IsFollower;
        public bool IsScav => InfoClass.IsScav;
        public bool IsPMC => InfoClass.IsPMC;

        public BotPresetClass BotPresetClass { get; private set; }
        public SAINBotPreset SAINBotPreset => BotPresetClass.DifficultyPreset;

        public InfoClass InfoClass { get; private set; }

        public BotDifficulty BotDifficulty => InfoClass.BotDifficulty;
        public WildSpawnType BotType => InfoClass.BotType;
        public float PowerLevel => InfoClass.PowerLevel;
        public EPlayerSide Faction => InfoClass.Faction;

        public readonly ManualLogSource Logger;

        public float TimeBeforeSearch { get; private set; } = 0f;
        public float PercentageBeforeExtract { get; private set; }

        private float LastPower;

        public void Update()
        {
            if (SAIN == null) return;

            if (TimeBeforeSearch == 0f || BotOwner.BotsGroup.MembersCount != GroupCount)
            {
                GroupCount = BotOwner.BotsGroup.MembersCount;
                CalcTimeBeforeSearch();
            }

            if (LastPower != PowerLevel)
            {
                LastPower = PowerLevel;
                PersonalityClass.Update();
                CalcTimeBeforeSearch();
            }

            WeaponInfo.ManualUpdate();

            if (SAIN.Squad.BotInGroup && !SAIN.Squad.IAmLeader)
            {
                var Leader = SAIN.Squad.LeaderComponent;
                if (Leader != null)
                {
                    PercentageBeforeExtract = Leader.Info.PercentageBeforeExtract;
                }
            }
        }

        private int GroupCount = 0;

        public bool CanBotTalk => TalkGlobal.Value && BotPresetClass?.DifficultyPreset?.CanTalk.GetValue(BotDifficulty) == true;

        public float HoldGroundDelay { get; private set; }

        public float CalcHoldGroundDelay()
        {
            if (Personality == SAINPersonality.Rat || Personality == SAINPersonality.Coward || Personality == SAINPersonality.Timmy)
            {
                HoldGroundDelay = 0.25f;
            }
            else if (Personality == SAINPersonality.GigaChad)
            {
                HoldGroundDelay = 2f * UnityEngine.Random.Range(0.25f, 2f);
            }
            else if (Personality == SAINPersonality.Chad)
            {
                HoldGroundDelay = 1.5f * UnityEngine.Random.Range(0.5f, 2f);
            }
            else
            {
                HoldGroundDelay = 1f * UnityEngine.Random.Range(0.66f, 1.5f);
            }
            return HoldGroundDelay;
        }

        public void CalcTimeBeforeSearch()
        {
            float searchTime;
            if (IsFollower && SAIN.Squad.BotInGroup)
            {
                searchTime = 3f;
            }
            else if (IAmBoss && SAIN.Squad.BotInGroup)
            {
                searchTime = 20f;
            }
            else
            {
                switch (Personality)
                {
                    case SAINPersonality.GigaChad:
                        searchTime = 1.5f;
                        break;

                    case SAINPersonality.Chad:
                        searchTime = 5f;
                        break;

                    case SAINPersonality.Timmy:
                        searchTime = 60f;
                        break;

                    case SAINPersonality.Rat:
                        searchTime = 240f;
                        break;

                    case SAINPersonality.Coward:
                        searchTime = 60f;
                        break;

                    default:
                        searchTime = 24f;
                        break;
                }
            }

            searchTime *= UnityEngine.Random.Range(1f - SearchRandomize, 1f + SearchRandomize);
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
            SAINBotPresetManager.PresetUpdated -= BotPresetClass.PresetUpdated;
        }
    }
}