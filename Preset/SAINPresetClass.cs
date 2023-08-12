using Aki.Common.Utils;
using Comfort.Common;
using Newtonsoft.Json;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings;
using SAIN.Preset.Personalities;
using System;
using static SAIN.Helpers.JsonUtility;

namespace SAIN.Preset
{
    public class SAINPresetClass
    {
        public SAINPresetClass(SAINPresetDefinition preset)
        {
            Info = preset;
            GlobalSettings = GlobalSettingsClass.ImportGlobalSettings(preset);
            BotSettings = new BotSettings.BotSettingsClass(this);
            PersonalityManager = new PersonalityManagerClass(this);
        }

        public void ExportAll()
        {
            ExportDefinition();
            ExportGlobalSettings();
            ExportPersonalities();
            ExportBotSettings();
        }

        public void ExportDefinition() => Export(Info, "Info");
        public void ExportGlobalSettings() => Export(GlobalSettings, "GlobalSettings");
        public void ExportPersonalities() => PersonalityManager.ExportPersonalities();
        public void ExportBotSettings() => BotSettings.ExportBotSettings();

        public bool Export(object obj, string fileName, string subFolder = null)
        {
            try
            {
                SaveObjectToJson(obj, fileName, Folders(subFolder));
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Could not export Item of Type {obj.GetType().Name}");
                Logger.LogError(ex);
                return false;
            }
        }

        public bool Import<T>(out T result, string fileName, string subFolder = null)
        {
            if (Load.LoadJsonFile(out string json, fileName, Folders(subFolder)))
            {
                try
                {
                    result = Load.DeserializeObject<T>(json);
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Could not import Item of Type {typeof(T)}");
                    Logger.LogError(ex);
                }
            }
            result = default;
            return false;
        }

        public string[] Folders(string subFolder = null)
        {
            string presets = "Presets";
            string name = Info.Name;
            string[] result;
            if (subFolder == null)
            {
                result = new string[]
                {
                    presets,
                    name
                };
            }
            else
            {
                result = new string[]
                {
                    presets,
                    name,
                    subFolder
                };
            }
            return result;
        }

        public SAINPresetDefinition Info;
        public GlobalSettingsClass GlobalSettings;
        public BotSettings.BotSettingsClass BotSettings;
        public PersonalityManagerClass PersonalityManager;
    }

    public abstract class BasePreset
    {
        public BasePreset(SAINPresetClass presetClass)
        {
            Preset = presetClass;
            Info = presetClass.Info;
        }

        public readonly SAINPresetClass Preset;
        public readonly SAINPresetDefinition Info;
    }
}