using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN_Flashlights.Components;
using SAIN_Flashlights.Helpers;
using System.Reflection;
using UnityEngine;

namespace SAIN_Flashlights.Patches
{
    public class DazzlePatch : ModulePatch
    {
        private static PropertyInfo _GoalEnemy;

        protected override MethodBase GetTargetMethod()
        {
            _GoalEnemy = AccessTools.Property(typeof(BotMemoryClass), "GoalEnemy");
            return AccessTools.Method(_GoalEnemy.PropertyType, "CheckLookEnemy");
        }
        [PatchPrefix]
        public static void Prefix(ref BotOwner ___botOwner_0, IAIDetails person, ref float ___NextTimeCheck)
        {
            if (___NextTimeCheck < Time.time)
            {
                SAIN_Flashlight_Component flashlightComponent = person.GetPlayer.gameObject.GetComponent<SAIN_Flashlight_Component>();
                if (flashlightComponent == null)
                {
                    Logger.LogError("SAIN Flashlight Dazzle: flashlightComponent is null");
                    return;
                }

                if (flashlightComponent.WhiteLight)
                {
                    FlashLight.EnemyWithFlashlight(___botOwner_0, person);
                    return;
                }

                if (flashlightComponent.Laser)
                {
                    FlashLight.EnemyWithLaser(___botOwner_0, person);
                    return;
                }

                if (___botOwner_0.NightVision.UsingNow)
                {
                    if (flashlightComponent.IRLight)
                    {
                        FlashLight.EnemyWithFlashlight(___botOwner_0, person);
                        return;
                    }

                    if (flashlightComponent.IRLaser)
                    {
                        FlashLight.EnemyWithLaser(___botOwner_0, person);
                    }
                }
            }
        }
    }
}
