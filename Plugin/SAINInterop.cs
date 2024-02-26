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
        private static bool _SAINLoadedChecked = false;
        private static bool _SAINInteropInited = false;

        private static bool _IsSAINLoaded;
        private static Type _SAINExternalType;

        private static MethodInfo _ExtractBotMethod;
        private static MethodInfo _SetExfilForBotMethod;
        private static MethodInfo _ResetDecisionsForBotMethod;

        /**
         * Return true if SAIN is loaded in the client
         */
        public static bool IsSAINLoaded()
        {
            // Only check for SAIN once
            if (!_SAINLoadedChecked)
            {
                _SAINLoadedChecked = true;
                _IsSAINLoaded = Chainloader.PluginInfos.ContainsKey("me.sol.sain");
            }

            return _IsSAINLoaded;
        }

        /**
         * Initialize the SAIN interop class data, return true on success
         */
        public static bool Init()
        {
            if (!IsSAINLoaded()) return false;

            // Only check for the External class once
            if (!_SAINInteropInited)
            {
                _SAINInteropInited = true;

                _SAINExternalType = Type.GetType("SAIN.Plugin.External, SAIN");

                // Only try to get the methods if we have the type
                if (_SAINExternalType != null)
                {
                    _ExtractBotMethod = AccessTools.Method(_SAINExternalType, "ExtractBot");
                    _SetExfilForBotMethod = AccessTools.Method(_SAINExternalType, "TrySetExfilForBot");
                    _ResetDecisionsForBotMethod = AccessTools.Method(_SAINExternalType, "ResetDecisionsForBot");
                }
            }

            // If we found the External class, at least some of the methods are (probably) available
            return (_SAINExternalType != null);
        }

        /**
         * Force a bot into the Extract layer if SAIN is loaded. Return true if the bot was set to extract.
         */
        public static bool TryExtractBot(BotOwner botOwner)
        {
            if (!Init()) return false;
            if (_ExtractBotMethod == null) return false;

            return (bool)_ExtractBotMethod.Invoke(null, new object[] { botOwner });
        }

        /**
         * Try to select an exfil point for the bot if SAIN is loaded. Return true if an exfil was assigned to the bot.
         */
        public static bool TrySetExfilForBot(BotOwner botOwner)
        {
            if (!Init()) return false;
            if (_SetExfilForBotMethod == null) return false;

            return (bool)_SetExfilForBotMethod.Invoke(null, new object[] { botOwner });
        }

        /**
         * Force a bot to reset its decisions if SAIN is loaded. Return true if successful.
         */
        public static bool TryResetDecisionsForBot(BotOwner botOwner)
        {
            if (!Init()) return false;
            if (_ResetDecisionsForBotMethod == null) return false;

            return (bool)_ResetDecisionsForBotMethod.Invoke(null, new object[] { botOwner });
        }
    }
}
