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
            Properties = new List<PropertyInfo>(GetReflectionInfo.GetBotPresetProperties());
            BotTypes = new List<BotType>(BotTypeDefinitions.BotTypes);
            TypePresets = new Dictionary<WildSpawnType, BotType>();

            for (int i = 0; i < BotTypes.Count; i++)
            {
                BotType type = BotTypes[i];
                TypePresets.Add(type.WildSpawnType, type);
            }
        }

        public static void UpdatePresetsAndDict()
        {
            for (int i = 0; i < BotTypes.Count; i++)
            {
                BotTypes[i].PresetHandler();
                TypePresets[BotTypes[i].WildSpawnType] = BotTypes[i];
            }
        }

        public static Action<WildSpawnType, BotPreset> PresetUpdated { get; set; }
        public static List<BotType> BotTypes;
        public static readonly List<PropertyInfo> Properties;
        public static readonly Dictionary<WildSpawnType, BotType> TypePresets;

        public static bool GetPreset(WildSpawnType type, out BotPreset preset)
        {
            preset = null;
            if (TypePresets != null && TypePresets.ContainsKey(type))
            {
                preset = TypePresets[type].Preset;
            }
            return preset != null;
        }

        public static SAINProperty<T> GetSainProp<T>(BotType type, PropertyInfo property)
        {
            return (SAINProperty<T>)property.GetValue(type.Preset);
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