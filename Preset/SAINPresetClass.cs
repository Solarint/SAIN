using Aki.Common.Utils;
using Comfort.Common;
using Newtonsoft.Json;
using SAIN.Helpers;
using SAIN.Plugin;
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

            ExportGlobalSettings(false);
            ExportPersonalities(false);
            ExportBotSettings(false);

            try
            {
                PresetHandler.UpdateExistingBots();
            }
            catch (Exception updateEx)
            {
                LogExportError(updateEx);
            }
        }

        public void ExportDefinition()
        {
            try
            {
                Export(Info, "Info");
            }
            catch (Exception updateEx)
            {
                LogExportError(updateEx);
            }
        }

        public bool ExportGlobalSettings(bool sendToBots = true)
        {
            bool success = false;
            try
            {
                Export(GlobalSettings, "GlobalSettings");
                if (sendToBots)
                {
                    PresetHandler.UpdateExistingBots();
                }
                success = true;
                SAINPlugin.Editor.GUITabs.GlobalSettingsWereEdited = false;
            }
            catch (Exception ex)
            {
                LogExportError(ex);
            }
            return success;
        }

        public bool ExportPersonalities(bool sendToBots = true)
        {
            bool success = false;
            try
            {
                PersonalityManager.ExportPersonalities();
                if (sendToBots)
                {
                    PresetHandler.UpdateExistingBots();
                }
                success = true;
                SAINPlugin.Editor.GUITabs.BotPersonalityEditor.PersonalitiesWereEdited = false;
            }
            catch (Exception ex)
            {
                LogExportError(ex);
            }
            return success;
        }

        public bool ExportBotSettings(bool sendToBots = true)
        {
            bool success = false;
            try
            {
                BotSettings.ExportBotSettings();
                if (sendToBots)
                {
                    PresetHandler.UpdateExistingBots();
                }
                success = true;
                SAINPlugin.Editor.GUITabs.BotSelection.BotSettingsWereEdited = false;
            }
            catch (Exception ex)
            {
                LogExportError(ex);
            }
            return success;
        }

        public bool Export(object obj, string fileName, string subFolder = null)
        {
            bool success = false;
            try
            {
                SaveObjectToJson(obj, fileName, Folders(subFolder));
                success = true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Could not export Item of Type {obj.GetType().Name}");
                LogExportError(ex);
            }
            return success;
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
                    LogExportError(ex);
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

        private void LogExportError(Exception ex)
        {
            SAINPlugin.Editor.ExceptionString = ex.ToString();
            Logger.LogError($"Export error: {ex}");
        }
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