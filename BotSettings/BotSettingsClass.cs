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

        public SAINSettings Settings { get; private set; } = new SAINSettings();
        public BotDefaultValues BotDefaultValues { get; private set; } = new BotDefaultValues();
    }

    public class SAINSettings
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
            // Get all the fields that exist in this type
            foreach (var field in propType.GetFields())
            {
                // Save the field name and fieldInfo to a dictionary.
                FieldSections.Add(field.Name, field);
                // Get the fields of this field!
                var fields = field.FieldType.GetFields();
                // Adds that FieldArray and the field in a new dictionary.
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
            var Dictionary = BotSettingsHandler.EFTSettingsFields.Fields;
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