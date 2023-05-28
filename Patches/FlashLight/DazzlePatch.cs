using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN.Components;
using SAIN.Helpers;
using System.Reflection;
using UnityEngine;

namespace SAIN.Patches
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
                var component = person.GetPlayer.GetComponent<FlashLightComponent>();

                if (component.WhiteLight)
                {
                    Dazzle.EnemyWithFlashlight(___botOwner_0, person);
                }
                else if (component.Laser)
                {
                    Dazzle.EnemyWithLaser(___botOwner_0, person);
                }
                else
                {
                    if (___botOwner_0.NightVision.UsingNow)
                    {
                        if (component.IRLight)
                        {
                            Dazzle.EnemyWithFlashlight(___botOwner_0, person);
                        }
                        else if (component.IRLaser)
                        {
                            Dazzle.EnemyWithLaser(___botOwner_0, person);
                        }
                    }
                }
            }
        }
    }
}
