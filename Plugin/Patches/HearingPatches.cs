using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
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

    public class InitiateShotPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.FirearmController), "InitiateShot");
        }
        [PatchPostfix]
        public static void PatchPostfix(Player.FirearmController __instance, IWeapon weapon, BulletClass ammo)
        {
            Player playerInstance = AccessTools.FieldRefAccess<Player.FirearmController, Player>(__instance, "_player");
            GunshotRange.OnMakingShot(weapon, playerInstance, ammo);
        }
    }

    public class TryPlayShootSoundPatch : ModulePatch
    {
        private static PropertyInfo _boolean_0;
        protected override MethodBase GetTargetMethod()
        {
            _boolean_0 = AccessTools.Property(typeof(AiDataClass), "Boolean_0");

            return AccessTools.Method(typeof(AiDataClass), "TryPlayShootSound");
        }
        [PatchPrefix]
        public static bool PatchPrefix(AiDataClass __instance, ref float ___float_0)
        {
            bool flag = ___float_0 < Time.time;

            _boolean_0.SetValue(__instance, true);

            if (flag)
            {
                ___float_0 = Time.time + 1f;
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
            if (SAINPlugin.BotController == null)
            {
                return;
            }

            object playerBridge = _PlayerBridge.GetValue(__instance);
            Player player = _Player.Invoke(playerBridge, null) as Player;

            SAINSoundTypeHandler.AISoundPlayer(soundName, player);
        }
    }
}
