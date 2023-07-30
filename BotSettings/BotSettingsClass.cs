using EFT;
using HarmonyLib;
using SAIN.BotSettings.Categories;
using SAIN.BotSettings.Categories.Util;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAIN.BotSettings
{
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

    public class FieldWrapper
    {
        public FieldWrapper(Type propType, string property, params string[] fieldStrings)
        {
            PropertyInfo _property = AccessTools.Property(propType, property);
            if (_property != null)
            {
                foreach (string field in fieldStrings)
                {
                    FieldInfo section = AccessTools.Field(_property.PropertyType, field);
                    if (section != null)
                    {
                        FieldSections.Add(section.Name, section);
                        var fields = section.FieldType.GetFields();
                        Fields.Add(section, fields);
                    }
                    else
                    {
                        Console.WriteLine($"Field: {field} Not Found");
                    }
                }

                Console.WriteLine($"{_property.Name}: Sections: {FieldSections.Count} Fields: {Fields.Count}");
            }
            else
            {
                Console.WriteLine($"{property} Not Found");
            }
        }

        public FieldWrapper(Type propType)
        {
            foreach (var field in propType.GetFields())
            {
                FieldSections.Add(field.Name, field);
                var fields = field.FieldType.GetFields();
                Fields.Add(field, fields);
            }
        }

        public readonly Dictionary<string, FieldInfo> FieldSections = new Dictionary<string, FieldInfo>();
        public readonly Dictionary<FieldInfo, FieldInfo[]> Fields = new Dictionary<FieldInfo, FieldInfo[]>();
    }

    public class BotOwnerSettings
    {
        public BotOwnerSettings(BotOwner owner)
        {
            BotOwner = owner;
            UpdateValue(nameof(SAINCoreSettings.VisibleDistance), 100f);
        }

        public void UpdateValue(string key, object value)
        {
            bool complete = false;
            var Dictionary = BotSettingsHandler.EFTBotSettingsFields.Fields;
            foreach (var keyPair in Dictionary)
            {
                var Settings = keyPair.Key.GetValue(BotOwner.Settings.FileSettings);
                var fields = keyPair.Value;
                foreach (var field in fields)
                {
                    if (field.Name == key)
                    {
                        complete = true;
                        field.SetValue(Settings, value);
                        break;
                    }
                }
                if (complete)
                {
                    break;
                }
            }
        }

        public readonly BotOwner BotOwner;
    }
}