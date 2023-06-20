using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN.Components;
using SAIN.Components.Helpers;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;

namespace SAIN.Patches
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
        public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref IAIDetails player, ref Vector3 position, ref float power, ref AISoundType type)
        {
            if (SAINPlugin.BotController.SAINBots.ContainsKey(___botOwner_0.ProfileId))
            {
                return false;
            }
            return true;
        }
    }

    public class BetterAudioPatch : ModulePatch
    {
        private static FieldInfo _Player;
        protected override MethodBase GetTargetMethod()
        {
            _Player = AccessTools.Field(typeof(BaseSoundPlayer), "Player");
            return AccessTools.Method(typeof(BaseSoundPlayer), "SoundEventHandler");
        }

        [PatchPrefix]
        public static void PatchPrefix(string soundName, BaseSoundPlayer __instance)
        {
            if (SAINPlugin.BotController == null)
            {
                return;
            }
            SAINSoundTypeHandler.AISoundPlayer(soundName, (Player)_Player.GetValue(__instance));
        }
    }
}
