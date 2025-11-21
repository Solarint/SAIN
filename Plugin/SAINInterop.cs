using BepInEx.Bootstrap;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Plugin;

internal static class SAINInterop
{
    private static bool _SAINLoadedChecked = false;
    private static bool _SAINInteropInited = false;

    private static bool _IsSAINLoaded;
    private static Type _SAINExternalType;

    private static MethodInfo _ExtractBotMethod;
    private static MethodInfo _SetExfilForBotMethod;
    private static MethodInfo _IsPathTowardEnemyMethod;
    private static MethodInfo _TimeSinceSenseEnemyMethod;
    private static MethodInfo _CanBotQuestMethod;
    private static MethodInfo _GetExtractedBotsMethod;
    private static MethodInfo _GetExtractionInfosMethod;
    private static MethodInfo _IgnoreHearingMethod;
    private static MethodInfo _GetPersonalityMethod;

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
                _IsPathTowardEnemyMethod = AccessTools.Method(_SAINExternalType, "IsPathTowardEnemy");
                _TimeSinceSenseEnemyMethod = AccessTools.Method(_SAINExternalType, "TimeSinceSenseEnemy");
                _CanBotQuestMethod = AccessTools.Method(_SAINExternalType, "CanBotQuest");
                _GetExtractedBotsMethod = AccessTools.Method(_SAINExternalType, "GetExtractedBots");
                _GetExtractionInfosMethod = AccessTools.Method(_SAINExternalType, "GetExtractionInfos");

                _IgnoreHearingMethod = AccessTools.Method(_SAINExternalType, "IgnoreHearing");
                _GetPersonalityMethod = AccessTools.Method(_SAINExternalType, "GetPersonality");
            }
        }

        // If we found the External class, at least some of the methods are (probably) available
        return (_SAINExternalType != null);
    }

    /// <summary>
    /// Sets this bot to ignore hearing for a specified duration, or until a bot sees an enemy;
    /// </summary>
    /// <param name="value">Set Ignore to On or Off</param>
    /// <param name="ignoreUnderFire">Set bot to ignore being under fire (shots being ~2m or closer to them by default)</param>
    /// <param name="duration">if greater than 0, stop ignoring hearing after that time has passed. 0 means they will ignore hearing forever until they see an enemy.</param>
    /// <returns>True if the bot was successfully set to ignore hearing</returns>
    public static bool IgnoreHearing(BotOwner botOwner, bool value, bool ignoreUnderFire, float duration = 0)
    {
        if (botOwner == null) return false;
        if (!Init()) return false;
        if (_IgnoreHearingMethod == null) return false;

        return (bool)_IgnoreHearingMethod.Invoke(null, [botOwner, value, ignoreUnderFire, duration]);
    }

    /// <summary>
    /// Get the Current Personality of a bot;
    /// </summary>
    /// <param name="botOwner">The bot to check</param>
    /// <returns>A string of a bot's personality, returns string.Empty if it could not be found.</returns>
    public static string GetPersonality(BotOwner botOwner)
    {
        string result = string.Empty;
        if (botOwner == null) return result;
        if (!Init()) return result;
        if (_GetPersonalityMethod == null) return result;

        result = (string)_GetPersonalityMethod.Invoke(null, [botOwner]);
        return result;
    }

    /// <summary>
    /// Get a list of all "Player.ProfileID"s of bots that have extracted. The list must not be null. The list is cleared before adding all extracted ProfileIDs;
    /// </summary>
    /// <param name="list">An already existing list to add to</param>
    /// <returns>True if the list was successfully updated</returns>
    public static bool GetExtractedBots(List<string> list)
    {
        if (list == null) return false;
        if (!Init()) return false;
        if (_GetExtractedBotsMethod == null) return false;

        _GetExtractedBotsMethod.Invoke(null, [list]);
        return true;
    }

    /// <summary>
    /// Get a list of all "Player.ProfileID"s of bots that have extracted. The list must not be null. The list is cleared before adding all extracted ProfileIDs;
    /// </summary>
    /// <param name="list">An already existing list to add to</param>
    /// <returns>True if the list was successfully updated</returns>
    public static bool GetExtractedBots(List<ExtractionInfo> list)
    {
        if (list == null) return false;
        if (!Init()) return false;
        if (_GetExtractionInfosMethod == null) return false;

        _GetExtractionInfosMethod.Invoke(null, [list]);
        return true;
    }

    /**
     * Force a bot into the Extract layer if SAIN is loaded. Return true if the bot was set to extract.
     */
    public static bool TryExtractBot(BotOwner botOwner)
    {
        if (!Init()) return false;
        if (_ExtractBotMethod == null) return false;

        return (bool)_ExtractBotMethod.Invoke(null, [botOwner]);
    }

    /**
     * Try to select an exfil point for the bot if SAIN is loaded. Return true if an exfil was assigned to the bot.
     */
    public static bool TrySetExfilForBot(BotOwner botOwner)
    {
        if (!Init()) return false;
        if (_SetExfilForBotMethod == null) return false;

        return (bool)_SetExfilForBotMethod.Invoke(null, [botOwner]);
    }

    /// <summary>
    /// Compare a NavMeshPath to the pre-calculated NavMeshPath that leads directly to a bot's Active Enemy.
    /// </summary>
    /// <param name="path">The Path To Test</param>
    /// <param name="botOwner">The Bot in Question</param>
    /// <param name="ratioSameOverAll">How many nodes along a path are allowed to be the same divided by the total nodes in the Path To Test. Example: 3 nodes are the same, with 10 total nodes = 0.3 ratio, so if the input value is 0.25, this will return false.</param>
    /// <param name="sqrDistCheck">How Close a node can be to be considered the same.</param>
    /// <returns>True if the path leads in the same direction as their active enemy.</returns>
    public static bool IsPathTowardEnemy(NavMeshPath path, BotOwner botOwner, float ratioSameOverAll = 0.25f, float sqrDistCheck = 0.05f)
    {
        if (!Init()) return false;
        if (_IsPathTowardEnemyMethod == null) return false;

        return (bool)_IsPathTowardEnemyMethod.Invoke(null, [path, botOwner, ratioSameOverAll, sqrDistCheck]);
    }


    /// <summary>
    /// Compare a NavMeshPath to the pre-calculated NavMeshPath that leads directly to a bot's Active Enemy.
    /// </summary>
    /// <param name="path">The Path To Test</param>
    /// <param name="botOwner">The Bot in Question</param>
    /// <param name="ratioSameOverAll">How many nodes along a path are allowed to be the same divided by the total nodes in the Path To Test. Example: 3 nodes are the same, with 10 total nodes = 0.3 ratio, so if the input value is 0.25, this will return false.</param>
    /// <param name="sqrDistCheck">How Close a node can be to be considered the same.</param>
    /// <returns>True if the path leads in the same direction as their active enemy.</returns>
    public static bool CanBotQuest(BotOwner botOwner, Vector3 questPosition, float dotThreshold = 0.33f)
    {
        if (!Init()) return false;
        if (_CanBotQuestMethod == null) return false;

        return (bool)_CanBotQuestMethod.Invoke(null, [botOwner, questPosition, dotThreshold]);
    }

    public static float TimeSinceSenseEnemy(BotOwner botOwner)
    {
        if (!Init()) return float.MaxValue;
        if (_TimeSinceSenseEnemyMethod == null) return float.MaxValue;

        return (float)_TimeSinceSenseEnemyMethod.Invoke(null, [botOwner]);
    }

}

public class ExtractionInfo
{
    public ExtractionInfo(BotOwner bot, string reason, ExfiltrationPoint exfil)
    {
        BotNickname = bot.Profile.Nickname;
        ProfileID = bot.GetPlayer.ProfileId;
        Reason = reason;
        TimeExtracted = Time.time;
        ExtractionPoint = exfil.Settings.Name;
    }

    public readonly string BotNickname;
    public readonly string ProfileID;
    public readonly string Reason;
    public readonly string ExtractionPoint;
    public readonly float TimeExtracted;
}
