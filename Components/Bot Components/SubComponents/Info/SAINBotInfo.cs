using BepInEx.Logging;
using EFT;
using SAIN.BotPresets;
using UnityEngine;

namespace SAIN.Classes
{
    public class SAINBotInfo : SAINInfoAbstract
    {
        public SAINBotInfo(BotOwner botOwner) : base(botOwner)
        {
            GlobalSettings.Update(BotOwner);

            BotPresetClass = new BotPresetClass(BotOwner);
            PersonalityClass = new PersonalityClass(BotOwner);
            WeaponInfo = new WeaponInfo(BotOwner);
        }

        public PresetValues FileSettings => BotPresetClass.PresetValues;

        public bool EnableExtracts => FileSettings.EnableExtracts;

        public float RecoilMultiplier => FileSettings.RecoilMultiplier;
        public float AimUpgradeByTimeMin => FileSettings.MAX_AIMING_UPGRADE_BY_TIME;
        public float MaxAimTime => FileSettings.MAX_AIM_TIME;

        public bool FasterCQBReactions => FileSettings.FasterCQBReactions;
        public float FasterCQBReactionsDistance => FileSettings.FasterCQBReactionsDistance;
        public float FasterCQBReactionsMinimum => FileSettings.FasterCQBReactionsMinimum;

        public float AudibleRangeMultiplier => FileSettings.AudibleRangeMultiplier;
        public float MaxFootstepAudioDistance => FileSettings.MaxFootstepAudioDistance;

        public float BurstMulti => FileSettings.BurstMulti;
        public float FireratMulti => FileSettings.FireratMulti;

        public float VisionSpeedModifier => FileSettings.VisionSpeedModifier;
        public float CloseVisionSpeed => FileSettings.CloseVisionSpeed;
        public float FarVisionSpeed => FileSettings.FarVisionSpeed;
        public float CloseFarThresh => FileSettings.CloseFarThresh;

        public bool CanTalk => FileSettings.CanTalk;
        public bool BotTaunts => FileSettings.BotTaunts;
        public bool SquadTalk => FileSettings.SquadTalk;
        public float SquadMemberTalkFreq => FileSettings.SquadMemberTalkFreq;
        public float SquadLeadTalkFreq => FileSettings.SquadLeadTalkFreq;
        public float TalkFrequency => FileSettings.TalkFrequency;

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
                searchTime = 6f;
            }
            else if (IAmBoss && SAIN.Squad.BotInGroup)
            {
                searchTime = 30f;
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
            PresetManager.PresetUpdated -= BotPresetClass.PresetUpdated;
        }
    }
}