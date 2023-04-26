using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN_Flashlights.Components;
using System.Reflection;
using static SAIN_Flashlights.Config.DazzleConfig;

namespace SAIN_Flashlights.Patches
{
    public class CheckFlashlightPatch : ModulePatch
    {
        private static FieldInfo _tacticalModesField;
        private static MethodInfo _UsingLight;
        protected override MethodBase GetTargetMethod()
        {
            _UsingLight = AccessTools.PropertySetter(typeof(AiDataClass), "UsingLight");

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

                if (!flashlightComponent.WhiteLight && !flashlightComponent.Laser)
                {
                    _UsingLight.Invoke(____player.AIData, new object[] { false });

                    if (DebugFlash.Value)
                        Logger.LogDebug($"Updated Using Light for {____player.Profile.Nickname}, now set to {____player.AIData.UsingLight}");
                }
            }
            else
            {
                Logger.LogError("Could not Check Device in CheckFlashlightPatch");
            }
        }
    }
}