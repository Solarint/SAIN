using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN.Components.Helpers;
using SAIN.Helpers;
using System.Reflection;
using UnityEngine;

namespace SAIN.Patches.Hearing
{
    public class HearingSensorPatch : ModulePatch
    {
        private static PropertyInfo HearingSensor;

        protected override MethodBase GetTargetMethod()
        {
            HearingSensor = AccessTools.Property(typeof(BotOwner), "HearingSensor");
            return AccessTools.Method(HearingSensor.PropertyType, "method_0");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ____botOwner)
        {
            if (SAINPlugin.BotController.Bots.ContainsKey(____botOwner.ProfileId))
            {
                return false;
            }
            return true;
        }
    }

    public class TryPlayShootSoundPatch : ModulePatch
    {
        private static PropertyInfo AIFlareEnabled;

        protected override MethodBase GetTargetMethod()
        {
            AIFlareEnabled = AccessTools.Property(typeof(AIData), "Boolean_0");
            return AccessTools.Method(typeof(AIData), "TryPlayShootSound");
        }

        [PatchPrefix]
        public static bool PatchPrefix(AIData __instance, Player getPlayer, AISoundType soundType, ref float ____nextShootTime)
        {
            AIFlareEnabled.SetValue(__instance, true);

            // Limits how many sounds are played from each player for the AI
            if (____nextShootTime < Time.time)
            {
                ____nextShootTime = Time.time + 0.65f; // default 1f
                AudioHelpers.TryPlayShootSound(getPlayer, soundType);
            }
            return false;
        }
    }

    public class BetterAudioPatch : ModulePatch
    {
        private static MethodInfo _Player;
        private static FieldInfo _PlayerBridge;

        protected override MethodBase GetTargetMethod()
        {
            _PlayerBridge = AccessTools.Field(typeof(BaseSoundPlayer), "playersBridge");
            _Player = AccessTools.PropertyGetter(_PlayerBridge.FieldType, "iPlayer");
            return AccessTools.Method(typeof(BaseSoundPlayer), "SoundEventHandler");
        }

        [PatchPrefix]
        public static void PatchPrefix(string soundName, BaseSoundPlayer __instance)
        {
            if (SAINPlugin.BotController != null)
            {
                object playerBridge = _PlayerBridge.GetValue(__instance);
                Player player = _Player.Invoke(playerBridge, null) as Player;
                SAINSoundTypeHandler.AISoundPlayer(soundName, player);
            }
        }
    }
}