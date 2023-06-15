using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN.Components;
using System.Reflection;
using static SAIN.UserSettings.DazzleConfig;

namespace SAIN.Patches
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
            var component = ____player.gameObject.GetComponent<FlashLightComponent>();

            if (component != null)
            {
                component.CheckDevice(____player, _tacticalModesField);

                if (!component.WhiteLight && !component.Laser)
                {
                    _UsingLight.Invoke(____player.AIData, new object[] { false });

                    if (DebugFlash.Value)
                        Logger.LogDebug($"Updated Using Light for {____player.Profile.Nickname}, now set to {____player.AIData.UsingLight}");
                }
            }
        }
    }
}