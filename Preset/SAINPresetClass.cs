using Aki.Common.Utils;
using Comfort.Common;
using Newtonsoft.Json;
using SAIN.Editor;
using SAIN.Editor.GUISections;
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
                GUITabs.GlobalSettingsWereEdited = false;
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
                BotPersonalityEditor.PersonalitiesWereEdited = false;
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
                BotSelectionClass.BotSettingsWereEdited = false;
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
                string[] folders = Folders(subFolder);
                SaveObjectToJson(obj, fileName, folders);
                success = true;

                if (SAINPlugin.GlobalDebugMode)
                {
                    string debugFolders = string.Empty;
                    for (int i = 0; i < folders.Length; i++)
                    {
                        debugFolders += $"/{folders[i]}";
                    }
                    Logger.LogDebug($"Type:[{obj.GetType().Name}] Name: [{fileName}] To: [{debugFolders}]");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed Export of Type [{obj.GetType().Name}] Name: [{fileName}]");
                LogExportError(ex);
            }
            return success;
        }

        public bool Import<T>(out T result, string fileName, string subFolder = null)
        {
            string[] folders = Folders(subFolder);
            if (Load.LoadJsonFile(out string json, fileName, folders))
            {
                try
                {
                    result = Load.DeserializeObject<T>(json);

                    if (SAINPlugin.GlobalDebugMode)
                    {
                        string debugFolders = string.Empty;
                        for (int i = 0; i < folders.Length; i++)
                        {
                            debugFolders += $"/{folders[i]}";
                        }
                        Logger.LogDebug($"Type:[{typeof(T).Name}] Name: [{fileName}] To: [{debugFolders}]");
                    }
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
            SAINEditor.ExceptionString = ex.ToString();
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