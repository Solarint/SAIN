using EFT;
using HarmonyLib;
using SAIN.BotSettings;
using SAIN.BotSettings.Categories;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAIN.Helpers
{
    public class ServerBotSettings
    {
        static void GetFields()
        {
            if (Loaded)
            {
                return;
            }

            EFTFields = typeof(GClass562).GetFields(BindingFlags.Instance | BindingFlags.Public);
            SAINFields = typeof(SAINSettings).GetFields();
            Loaded = true;
        }

        static bool Loaded = false;

        public static void Copy(BotSettingsHandler settings)
        {
            GetFields();
        }

        static void CopyClass(SAINBotSettingsClass settings)
        {
            var core = settings.Settings.Core;
            var type = core.GetType();
            foreach (var pair in SAINSettingsFields)
            {
                if (type == pair.Key)
                {
                    var fields = SAINSettingsFields[pair.Key];
                }
            }
        }

        static void CopyField(FieldInfo field, EFTSettingsPropWrapper wrapper, SAINBotSettingsClass sainSettings)
        {
            var settings = GetSettings(sainSettings.BotDifficulty, sainSettings.WildSpawnType);
            var targetType = SAINTypePairs[field.FieldType];
            foreach (var variable in wrapper.EFTProperty.Fields)
            {
                if (variable.Name == field.Name && variable.FieldType == field.FieldType)
                {
                    PropertyInfo prop = AccessTools.Property(targetType, variable.Name);
                    object eftProperty = prop.GetValue(settings);
                    var targetValue = variable.GetValue(eftProperty);
                }
            }
        }

        public static GClass566 GetSettings(BotDifficulty difficulty, WildSpawnType type)
        {
            return GClass564.GetSettings(difficulty, type);
        }

        public static Dictionary<Type, FieldInfo[]> SAINSettingsFields = new Dictionary<Type, FieldInfo[]>();
        public static Dictionary<Type, FieldInfo[]> EFTSettingsFields = new Dictionary<Type, FieldInfo[]>();
        public static FieldInfo[] EFTFields;
        public static FieldInfo[] SAINFields;

        public static readonly Type EFTCore = typeof(GClass554);
        public static Dictionary<Type, Type> SAINTypePairs = new Dictionary<Type, Type>
            {
                { typeof(SAINAimingSettings), typeof(BotGlobalAimingSettings) },
                { typeof(SAINChangeSettings), typeof(BotGlobalsChangeSettings) },
                { typeof(SAINCoreSettings), EFTCore },
                { typeof(SAINGrenadeSettings), typeof(BotGlobalsGrenadeSettings) },
                { typeof(SAINLaySettings), typeof(BotGlobalLayData) },
                { typeof(SAINLookSettings), typeof(BotGlobalLookData) },
                { typeof(SAINMindSettings), typeof(BotGlobalAimingSettings) },
                { typeof(SAINMoveSettings), typeof(BotGlobalAimingSettings) },
                { typeof(SAINPatrolSettings), typeof(BotGlobalAimingSettings) },
                { typeof(SAINScatterSettings), typeof(BotGlobalAimingSettings) },
                { typeof(SAINShootSettings), typeof(BotGlobalAimingSettings) },
            };
    }

    public class EFTSettingsPropWrapper
    {
        public EFTSettingsPropWrapper(Type eft, Type sain)
        {
            EFTProperty = new FieldWrapper(eft, ServerBotSettings.EFTFields);
            SAINProperty = new FieldWrapper(sain, ServerBotSettings.SAINFields);
        }

        public FieldWrapper EFTProperty;
        public FieldWrapper SAINProperty;
    }
    public class FieldWrapper
    {
        public FieldWrapper(Type type, FieldInfo[] containingFields)
        {
            Type = type;
            BaseField = GetField(type, containingFields);
            Fields = Reflection.GetFields(type);
        }

        public Type Type;
        public FieldInfo BaseField;
        public List<FieldInfo> Fields;

        static FieldInfo GetField(Type type, FieldInfo[] fields)
        {
            foreach (var field in fields)
            {
                if (field.FieldType == type)
                {
                    return field;
                }
            }
            return null;
        }
    }
}
