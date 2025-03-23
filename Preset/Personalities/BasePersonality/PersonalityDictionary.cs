using EFT;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent.Classes.Info;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Preset.Personalities
{
    public class PersonalityDictionary : Dictionary<EPersonality, PersonalitySettingsClass>
    {
        static PersonalityDictionary()
        {
            if (!JsonUtility.Load.LoadObject(out _nicknames, "NicknamePersonalities")) {
                _nicknames = new NickNames();
                JsonUtility.SaveObjectToJson(_nicknames, "NicknamePersonalities");
            }
        }

        private class NickNames
        {
            public string Description = "Names are not case sensitive. Any bot nick name that contains one of the entries here will be forced to use the matching personality.";

            public Dictionary<string, EPersonality> NicknamePersonalityMatches = new Dictionary<string, EPersonality>() {
                { "steve", EPersonality.Wreckless},
                { "solarint", EPersonality.GigaChad},
                { "lvndmark", EPersonality.SnappingTurtle},
                { "chomp", EPersonality.Chad},
                { "senko", EPersonality.Chad},
                { "kaeno", EPersonality.Timmy},
                { "justnu", EPersonality.Timmy},
                { "ratthew", EPersonality.Rat},
                { "choccy", EPersonality.Rat},
            };
        }

        private static readonly NickNames _nicknames;

        public EPersonality GetPersonality(SAINBotInfoClass infoClass, out PersonalitySettingsClass settings)
        {
            if (checkForcePersonality(out EPersonality result)) {
                settings = this[result];
                return result;
            }

            result = setNicknamePersonality(infoClass.Profile.NickName);
            if (result != EPersonality.Normal) {
                settings = this[result];
                return result;
            }

            result = setBossPersonality(infoClass.Profile.WildSpawnType);
            if (result != EPersonality.Normal) {
                settings = this[result];
                return result;
            }

            foreach (var setting in this) {
                if (canBotBePersonality(infoClass, setting.Key)) {
                    settings = setting.Value;
                    return setting.Key;
                }
            }

            if (infoClass.Profile.IsPMC && EFTMath.RandomBool(33))
                result = EPersonality.Chad;
            else
                result = EPersonality.Normal;

            settings = this[result];
            return result;
        }

        public PersonalitySettingsClass GetSettings(EPersonality personality)
        {
            if (this.TryGetValue(personality, out var result)) {
                return result;
            }
            return null;
        }

        private bool checkForcePersonality(out EPersonality personality)
        {
            foreach (var item in SAINPlugin.LoadedPreset.GlobalSettings.Mind.ForcePersonality) {
                if (item.Value == true) {
                    personality = item.Key;
                    return true;
                }
            }
            personality = EPersonality.Normal;
            return false;
        }

        public Dictionary<EPersonality, List<string>> Nickname_Personalities = new Dictionary<EPersonality, List<string>>();

        private EPersonality setNicknamePersonality(string nickname)
        {
            if (nickname.IsNullOrEmpty()) {
                return EPersonality.Normal;
            }
            string lowerNick = nickname.ToLower();
            foreach (KeyValuePair<string, EPersonality> kvp in _nicknames.NicknamePersonalityMatches) {
                if (lowerNick.Contains(kvp.Key.ToLower())) {
                    return kvp.Value;
                }
            }
            return EPersonality.Normal;
        }

        private EPersonality setBossPersonality(WildSpawnType wildSpawnType)
        {
            if (GlobalSettingsClass.Instance.Mind.PERS_BOSSES.TryGetValue(wildSpawnType, out EPersonality bossPersonality)) {
                return bossPersonality;
            }
            return EPersonality.Normal;
        }

        private bool canBotBePersonality(SAINBotInfoClass infoClass, EPersonality personality)
        {
            if (!this.TryGetValue(personality, out var settings)) {
                return false;
            }
            var assignment = settings.Assignment;
            if (!assignment.Enabled) {
                return false;
            }
            if (checkRandomAssignment(settings)) {
                return true;
            }
            if (meetsRequirements(infoClass, settings)) {
                float assignmentChance = getChance(infoClass.Profile.PowerLevel, settings);
                if (EFTMath.RandomBool(assignmentChance)) {
                    return true;
                }
            }
            return false;
        }

        private bool checkRandomAssignment(PersonalitySettingsClass settings)
        {
            return settings.Assignment.CanBeRandomlyAssigned && EFTMath.RandomBool(settings.Assignment.RandomlyAssignedChance);
        }

        private bool meetsRequirements(SAINBotInfoClass infoClass, PersonalitySettingsClass settings)
        {
            var assignment = settings.Assignment;
            return assignment.AllowedTypes.Contains(infoClass.Profile.WildSpawnType)
                && infoClass.Profile.PowerLevel <= assignment.PowerLevelMax
                && infoClass.Profile.PowerLevel > assignment.PowerLevelMin
                && infoClass.Profile.PlayerLevel <= assignment.MaxLevel
                && infoClass.Profile.PlayerLevel > assignment.MinLevel;
        }

        private float getChance(float powerLevel, PersonalitySettingsClass settings)
        {
            var assignment = settings.Assignment;
            powerLevel = Mathf.Clamp(powerLevel, 0, 1000);
            float modifier0to1 = (powerLevel - assignment.PowerLevelScaleStart) / (assignment.PowerLevelScaleEnd - assignment.PowerLevelScaleStart);
            if (assignment.InverseScale) {
                modifier0to1 = 1f - modifier0to1;
            }
            float result = assignment.MaxChanceIfMeetRequirements * modifier0to1;
            result = Mathf.Clamp(result, 0f, 100f);
            //Logger.LogDebug($"Result: [{result}] Power: [{powerLevel}] PowerLevelScaleStart [{PowerLevelScaleStart}] PowerLevelScaleEnd [{PowerLevelScaleEnd}] MaxChanceIfMeetRequirements [{MaxChanceIfMeetRequirements}]");
            return result;
        }
    }
}