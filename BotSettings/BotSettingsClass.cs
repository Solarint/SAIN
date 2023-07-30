using EFT;
using Newtonsoft.Json;
using SAIN.BotSettings.Categories;
using SAIN.BotSettings.Categories.Util;
using SAIN.Helpers;
using System.Collections.Generic;

namespace SAIN.BotSettings
{
    public class BotSettingsHandler
    {
        static readonly Dictionary<WildSpawnType, SAINDictionary<SAINBotSettingsClass>> GlobalSettingsDictionary = new Dictionary<WildSpawnType, SAINDictionary<SAINBotSettingsClass>>();

        static SAINDictionary<SAINBotSettingsClass> GetSettings(WildSpawnType type)
        {
            if (!GlobalSettingsDictionary.ContainsKey(type))
            {
                string currentPreset = JsonUtility.Save.SelectedPresetName;
                string[] path = JsonUtility.EFTBotConfigFolders(currentPreset);

                var BotSettings = new SAINDictionary<SAINBotSettingsClass>(type.ToString(), path);
                if (BotSettings.NewFileCreated)
                {
                    BotSettings.Add(BotDifficulty.easy, new SAINBotSettingsClass(type, BotDifficulty.easy));
                    BotSettings.Add(BotDifficulty.normal, new SAINBotSettingsClass(type, BotDifficulty.normal));
                    BotSettings.Add(BotDifficulty.hard, new SAINBotSettingsClass(type, BotDifficulty.hard));
                    BotSettings.Add(BotDifficulty.impossible, new SAINBotSettingsClass(type, BotDifficulty.impossible));
                    BotSettings.Export();
                }
                GlobalSettingsDictionary.Add(type, BotSettings);
            }
            return GlobalSettingsDictionary[type];
        }

        public static SAINBotSettingsClass GetSettings(BotOwner owner)
        {
            var type = owner.Profile.Info.Settings.Role;
            var diff = owner.Profile.Info.Settings.BotDifficulty;

            var Result = GetSettings(type).Get(diff);
            Result.BotDefaultValues.Init(owner);

            return Result;
        }
    }

    public class SAINBotSettingsClass
    {
        public SAINBotSettingsClass(WildSpawnType type, BotDifficulty diff)
        {
            WildSpawnType = type;
            BotDifficulty = diff;
        }

        public WildSpawnType WildSpawnType { get; private set; }
        public BotDifficulty BotDifficulty { get; private set; }

        public BotSettingsGroup Settings { get; private set; } = new BotSettingsGroup();
        public BotDefaultValues BotDefaultValues { get; private set; } = new BotDefaultValues();

        public void SetBotVariablesFromSAIN(BotOwner owner)
        {
            var Aiming = owner.Settings.FileSettings.Aiming;
            Aiming.MAX_AIMING_UPGRADE_BY_TIME = Settings.Aiming.MAX_AIMING_UPGRADE_BY_TIME;

            var Core = owner.Settings.FileSettings.Core;
            Core.VisibleAngle = Settings.Core.VisibleAngle;
            Core.VisibleDistance = Settings.Core.VisibleDistance;
            Core.GainSightCoef = Settings.Core.GainSightCoef;
            Core.ScatteringPerMeter = Settings.Core.ScatteringPerMeter;
            Core.DamageCoeff = Settings.Core.DamageCoeff;
            Core.CanRun = true;
            Core.CanGrenade = Settings.Core.CanGrenade;
        }
    }

    public class BotSettingsGroup
    {
        public SAINAimingSettings Aiming = new SAINAimingSettings();
        public SAINChangeSettings Change = new SAINChangeSettings();
        public SAINCoreSettings Core = new SAINCoreSettings();
        public SAINGrenadeSettings Grenade = new SAINGrenadeSettings();
        public SAINHearingSettings Hearing = new SAINHearingSettings();
        public SAINLaySettings Lay = new SAINLaySettings();
        public SAINLookSettings Look = new SAINLookSettings();
        public SAINMindSettings Mind = new SAINMindSettings();
        public SAINMoveSettings Move = new SAINMoveSettings();
        public SAINPatrolSettings Patrol = new SAINPatrolSettings();
        public SAINScatterSettings Scattering = new SAINScatterSettings();
        public SAINShootSettings Shoot = new SAINShootSettings();
    }
}