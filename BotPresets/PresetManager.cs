using EFT;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAIN.BotPresets
{
    public class PresetManager
    {
        static PresetManager()
        {
            Properties = new List<PropertyInfo>();
            PropertyInfo[] properties = typeof(BotPreset).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                Type propertyType = property.PropertyType;
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(SAINProperty<>))
                {
                    Properties.Add(property);
                }
            }

            BotTypes = BotTypeDefinitions.BotTypes;
            TypePresets = new Dictionary<WildSpawnType, BotType>();
            for (int i = 0; i < BotTypes.Length; i++)
            {
                BotType type = BotTypes[i];
                TypePresets.Add(type.WildSpawnType, type);
            }
        }

        public static Action<WildSpawnType, BotPreset> PresetUpdated { get; set; }
        public static BotType[] BotTypes;
        public static readonly List<PropertyInfo> Properties;
        public static readonly Dictionary<WildSpawnType, BotType> TypePresets;


        public static object GetPresetValue(BotType type, PropertyInfo property, BotDifficulty difficulty)
        {
            object sourcePropValue = property.GetValue(type.Preset);
            if (sourcePropValue is SAINProperty<float> floatProp)
            {
                return floatProp.GetValue(difficulty);
            }
            if (sourcePropValue is SAINProperty<bool> boolProp)
            {
                return boolProp.GetValue(difficulty);
            }
            return null;
        }

        public static SAINProperty<T> GetSainProp<T>(BotType type, PropertyInfo property)
        {
            return (SAINProperty<T>)property.GetValue(type.Preset);
        }

        public static void SetPresetValue(object value, List<BotType> types, PropertyInfo property, BotDifficulty difficulty)
        {
            foreach (var type in types)
            {
                SetPresetValue(value, type, property, difficulty);
            }
        }

        public static void SetPresetValue(object value, List<BotType> types, PropertyInfo property, List<BotDifficulty> difficulties)
        {
            foreach (var difficulty in difficulties)
            {
                SetPresetValue(value, types, property, difficulty);
            }
        }

        public static void SetPresetValue(object value, BotType type, PropertyInfo property, BotDifficulty difficulty)
        {
            object targetPropValue = property.GetValue(type.Preset);
            //if (targetPropValue is SAINProperty<object> targetProp && targetProp is ISAINProperty target)
            //{
            //    target.SetValue(difficulty, value);
            //    UpdatePreset(type.Preset);
            //}
        }

        public static void UpdatePreset(BotPreset preset)
        {
            JsonUtility.Save.SavePreset(preset);
            if (SAINPlugin.BotController != null && SAINPlugin.BotController.Bots.Count > 0)
            {
                PresetUpdated(preset.WildSpawnType, preset);
            }
        }

        public static WildSpawnType GetType(string type)
        {
            return (WildSpawnType)Enum.Parse(typeof(WildSpawnType), type);
        }

        public static BotDifficulty GetDiff(string diff)
        {
            return (BotDifficulty)Enum.Parse(typeof(BotDifficulty), diff);
        }

    }
}