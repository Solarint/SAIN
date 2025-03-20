using Comfort.Common;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using SAIN.Preset.GlobalSettings;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using PathFinderClass = GClass485;

namespace SAIN.Patches.Movement
{
    public class GlobalLookPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotGlobalLookData), "Update");
        }

        [PatchPostfix]
        public static void PatchPrefix(BotGlobalLookData __instance)
        {
            __instance.SHOOT_FROM_EYES = false;
        }
    }

    public class GlobalShootSettingsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotGlobalShootData), "Update");
        }

        [PatchPostfix]
        public static void PatchPrefix(BotGlobalShootData __instance)
        {
            __instance.CAN_STOP_SHOOT_CAUSE_ANIMATOR = false;
            __instance.MAX_DIST_COEF = 100f;
        }
    }

    public class PoseStaminaPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(PlayerPhysicalClass), "ConsumePoseLevelChange");
        }

        [PatchPrefix]
        public static bool PatchPrefix(Player ___player_0)
        {
            if (___player_0.IsAI) {
                return false;
            }
            return true;
        }
    }

    public class AimStaminaPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(PlayerPhysicalClass), "Aim");
        }

        [PatchPrefix]
        public static bool PatchPrefix(Player ___player_0)
        {
            if (___player_0.IsAI) {
                return false;
            }
            return true;
        }
    }

    public class CrawlPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(PathFinderClass), "method_0");
        }

        [PatchPrefix]
        public static bool PatchPrefix(PathFinderClass __instance, BotOwner ___botOwner_0, Vector3 pos, bool slowAtTheEnd, bool getUpWithCheck)
        {
            if (SAINPlugin.IsBotExluded(___botOwner_0)) {
                return true;
            }
            if (___botOwner_0.BotLay.IsLay &&
                getUpWithCheck) {
                Vector3 vector = pos - ___botOwner_0.Position;
                if (vector.y < 0.5f) {
                    vector.y = 0f;
                }
                if (vector.sqrMagnitude > 0.2f) {
                    ___botOwner_0.BotLay.GetUp(getUpWithCheck);
                }
                if (___botOwner_0.BotLay.IsLay) {
                    return false;
                }
            }
            ___botOwner_0.WeaponManager.Stationary.StartMove();
            __instance.SlowAtTheEnd = slowAtTheEnd;
            return true;
        }
    }

    public class CrawlPatch2 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotMover), "DoProne");
        }

        [PatchPrefix]
        public static bool PatchPrefix(BotOwner ___botOwner_0, bool val)
        {
            if (!val) {
                return true;
            }
            if (SAINPlugin.IsBotExluded(___botOwner_0)) {
                return true;
            }
            ___botOwner_0.GetPlayer.MovementContext.IsInPronePose = true;
            return false;
        }
    }

    public class EncumberedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BasePhysicalClass), "UpdateWeightLimits");
        }

        [PatchPrefix]
        public static bool PatchPrefix(bool ___bool_7, BasePhysicalClass.IObserverToPlayerBridge ___iobserverToPlayerBridge_0, BasePhysicalClass __instance)
        {
            if (___bool_7) {
                return true;
            }

            IPlayer player = ___iobserverToPlayerBridge_0.iPlayer;
            if (player == null) {
                Logger.LogWarning($"Player is Null, can't set weight limits for AI.");
                return true;
            }

            if (!player.IsAI) {
                return true;
            }

            if (SAINPlugin.IsBotExluded(player.AIData.BotOwner)) {
                return true;
            }

            var stamina = Singleton<BackendConfigSettingsClass>.Instance.Stamina;

            float carryWeightModifier = ___iobserverToPlayerBridge_0.Skills.CarryingWeightRelativeModifier;
            float d = carryWeightModifier * carryWeightModifier;

            float absoluteWeightModifier = ___iobserverToPlayerBridge_0.iPlayer.HealthController.CarryingWeightAbsoluteModifier;
            Vector2 b = new Vector2(absoluteWeightModifier, absoluteWeightModifier);

            var inertia = Singleton<BackendConfigSettingsClass>.Instance.Inertia;
            float strength = (float)___iobserverToPlayerBridge_0.Skills.Strength.SummaryLevel;
            Vector3 b2 = new Vector3(inertia.InertiaLimitsStep * strength, inertia.InertiaLimitsStep * strength, 0f);

            __instance.BaseInertiaLimits = inertia.InertiaLimits + b2;
            __instance.WalkOverweightLimits = stamina.WalkOverweightLimits * d + b;
            __instance.BaseOverweightLimits = stamina.BaseOverweightLimits * d + b;
            __instance.SprintOverweightLimits = stamina.SprintOverweightLimits * d + b;
            __instance.WalkSpeedOverweightLimits = stamina.WalkSpeedOverweightLimits * d + b;

            return false;
        }
    }

    public class DoorOpenerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotDoorOpener), nameof(BotDoorOpener.Update));
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ____owner, ref bool __result)
        {
            var settings = GlobalSettingsClass.Instance.General.Doors;
            if (settings.DisableAllDoors) {
                __result = false;
                return false;
            }
            if (settings.NewDoorOpening &&
                SAINEnableClass.GetSAIN(____owner, out var botComponent) &&
                botComponent.SAINLayersActive) {
                __result = botComponent.DoorOpener.FindDoorsToOpen();
                return false;
            }
            return true;
        }
    }

    public class DoorDisabledPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(WorldInteractiveObject), "method_3");
        }

        [PatchPrefix]
        public static bool PatchPrefix(WorldInteractiveObject __instance)
        {
            if (!__instance.enabled || !__instance.gameObject.activeInHierarchy) {
                return false;
            }
            return true;
        }
    }
}