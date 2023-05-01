using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using Flashlights.Components;
using Flashlights.Helpers;
using System.Reflection;
using UnityEngine;

namespace Flashlights.Patches
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
                    Dazzle.EnemyWithFlashlight(___botOwner_0, person);
                    return;
                }

                if (flashlightComponent.Laser)
                {
                    Dazzle.EnemyWithLaser(___botOwner_0, person);
                    return;
                }

                if (___botOwner_0.NightVision.UsingNow)
                {
                    if (flashlightComponent.IRLight)
                    {
                        Dazzle.EnemyWithFlashlight(___botOwner_0, person);
                        return;
                    }

                    if (flashlightComponent.IRLaser)
                    {
                        Dazzle.EnemyWithLaser(___botOwner_0, person);
                    }
                }
            }
        }
    }
}
