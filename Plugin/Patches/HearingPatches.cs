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
        public static bool PatchPrefix(ref BotOwner ___botOwner_0)
        {
            if (SAINPlugin.BotController.Bots.ContainsKey(___botOwner_0.ProfileId))
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
            AIFlareEnabled = AccessTools.Property(typeof(AiDataClass), "Boolean_0");
            return AccessTools.Method(typeof(AiDataClass), "TryPlayShootSound");
        }

        [PatchPrefix]
        public static bool PatchPrefix(AiDataClass __instance, Player getPlayer, AISoundType soundType, ref float ___float_0)
        {
            AIFlareEnabled.SetValue(__instance, true);

            // Limits how many sounds are played from each player for the AI
            if (___float_0 < Time.time)
            {
                ___float_0 = Time.time + 0.65f; // default 1f
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