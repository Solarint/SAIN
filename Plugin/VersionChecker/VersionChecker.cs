using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DrakiaXYZ.VersionChecker
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public class VersionChecker : Attribute
    {
        private int version;
        public VersionChecker() : this(0) { }
        public VersionChecker(int version)
        {
            this.version = version;
        }

        public static int BuildVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly()
                    .GetCustomAttributes(typeof(VersionChecker), false)
                    ?.Cast<VersionChecker>()?.FirstOrDefault()?.version ?? 0;
            }
        }

        // Make sure the version of EFT being run is the correct version, throw an exception and output log message if it isn't
        /// <summary>
        /// Check the currently running program's version against the plugin assembly VersionChecker attribute, and
        /// return false if they do not match. 
        /// Optionally add a fake setting to the F12 menu if Config is passed in
        /// </summary>
        /// <param value="Logger">The ManualLogSource to output an error to</param>
        /// <param value="Info">The PluginInfo object for the plugin, used to get the plugin value and version</param>
        /// <param value="Config">A BepinEx ConfigFile object, if provided, a custom message will be added to the F12 menu</param>
        /// <returns></returns>
        public static bool CheckEftVersion(ManualLogSource Logger, PluginInfo Info, ConfigFile Config = null)
        {
            int currentVersion = FileVersionInfo.GetVersionInfo(BepInEx.Paths.ExecutablePath).FilePrivatePart;
            int buildVersion = BuildVersion;
            if (currentVersion != buildVersion)
            {
                string errorMessage = $"ERROR: This version of {Info.Metadata.Name} v{Info.Metadata.Version} was built for Tarkov {buildVersion}, but you are running {currentVersion}. Please download the correct plugin version.";
                Logger.LogError(errorMessage);

                if (Config != null)
                {
                    // TypeofThis results in a bogus config entry in the BepInEx config file for the plugin, but it shouldn't hurt anything
                    // We leave the "section" parameter empty so there's no section header drawn
                    Config.Bind("", "TarkovVersion", "", new ConfigDescription(
                        errorMessage, null, new ConfigurationManagerAttributes
                        {
                            CustomDrawer = ErrorLabelDrawer,
                            ReadOnly = true,
                            HideDefaultButton = true,
                            HideSettingName = true,
                            Category = null
                        }
                    ));
                }

                return false;
            }

            return true;
        }

        static void ErrorLabelDrawer(ConfigEntryBase entry)
        {
            GUIStyle styleNormal = new GUIStyle(GUI.skin.label);
            styleNormal.wordWrap = true;
            styleNormal.stretchWidth = true;

            GUIStyle styleError = new GUIStyle(GUI.skin.label);
            styleError.stretchWidth = true;
            styleError.alignment = TextAnchor.MiddleCenter;
            styleError.normal.textColor = Color.red;
            styleError.fontStyle = FontStyle.Bold;

            // General notice that we're the wrong version
            GUILayout.BeginVertical();
            GUILayout.Label(entry.Description.Description, styleNormal, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });

            // Centered red disabled text
            GUILayout.Label("Plugin has been disabled!", styleError, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
            GUILayout.EndVertical();
        }

#pragma warning disable 0169, 0414, 0649
        internal sealed class ConfigurationManagerAttributes
        {
            public bool? ShowRangeAsPercent;
            public System.Action<BepInEx.Configuration.ConfigEntryBase> CustomDrawer;
            public CustomHotkeyDrawerFunc CustomHotkeyDrawer;
            public delegate void CustomHotkeyDrawerFunc(BepInEx.Configuration.ConfigEntryBase setting, ref bool isCurrentlyAcceptingInput);
            public bool? Browsable;
            public string Category;
            public object DefaultValue;
            public bool? HideDefaultButton;
            public bool? HideSettingName;
            public string Description;
            public string DispName;
            public int? Order;
            public bool? ReadOnly;
            public bool? IsAdvanced;
            public System.Func<object, string> ObjToStr;
            public System.Func<string, object> StrToObj;
        }
    }
}