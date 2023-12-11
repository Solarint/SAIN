using BepInEx.Bootstrap;
using EFT;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.Plugin
{
    internal static class SAINInterop
    {
        private static bool _IsSAINLoaded;
        private static bool _SAINLoadedChecked = false;

        private static Type _SAINExternalType;
        private static MethodInfo _ExtractBotMethod;

        /**
         * Return true if SAIN is loaded in the client
         */
        public static bool IsSAINLoaded()
        {
            if (!_SAINLoadedChecked)
            {
                _IsSAINLoaded = Chainloader.PluginInfos.ContainsKey("me.sol.sain");
                _SAINLoadedChecked = true;
            }

            return _IsSAINLoaded;
        }

        /**
         * Initialize the SAIN interop class data, return true on success
         */
        public static bool Init()
        {
            if (!IsSAINLoaded())
            {
                return false;
            }

            if (_SAINExternalType == null)
            {
                _SAINExternalType = Type.GetType("SAIN.Plugin.External, SAIN");
                _ExtractBotMethod = AccessTools.Method(_SAINExternalType, "ExtractBot");
            }

            return true;
        }

        /**
         * Force a bot into the Extract layer if SAIN is loaded. Return true if the bot was set to extract
         */
        public static bool ExtractBot(BotOwner botOwner)
        {
            if (!Init()) return false;

            return (bool)_ExtractBotMethod.Invoke(null, new object[] { botOwner });
        }
    }
}
