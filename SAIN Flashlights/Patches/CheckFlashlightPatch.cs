using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN_Flashlights.Components;
using System.Reflection;

namespace SAIN_Flashlights.Patches
{
    public class CheckFlashlightPatch : ModulePatch
    {
        private static FieldInfo _tacticalModesField;
        protected override MethodBase GetTargetMethod()
        {
            _tacticalModesField = AccessTools.Field(typeof(TacticalComboVisualController), "list_0");
            return AccessTools.Method(typeof(Player.FirearmController), "SetLightsState");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref Player ____player)
        {
            SAIN_Flashlight_Component flashlightComponent = ____player.gameObject.GetComponent<SAIN_Flashlight_Component>();
            if (flashlightComponent != null)
            {
                flashlightComponent.CheckDevice(____player, _tacticalModesField);
            }
            else
            {
                Logger.LogError("Could not Check Device in CheckFlashlightPatch");
            }
        }
    }
}