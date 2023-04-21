using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN.Movement.Components;
using System.Reflection;
using UnityEngine;
using static SAIN.Movement.Config.DogFighterConfig;

namespace SAIN.Movement.Patches
{
    public class SainMemory : MonoBehaviour
    {
        public float FallbackTimer { get; set; } = 0f;
        public bool isSeekingEnemy { get; set; } = false;
        public bool NeedtoHeal { get; set; } = false;
        public bool NeedtoReload { get; set; } = false;
        public float DodgeTimer { get; set; } = 0f;

        public bool GoingToNewCover { get; set; }
    }
    public class AddComponentPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetMethod("PreActivate", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        public static void PatchPostfix(ref BotOwner __instance)
        {
            __instance.gameObject.AddComponent<DynamicLean>();
            __instance.gameObject.AddComponent<SainMemory>();
            //bot.gameObject.AddComponent<Dodge>();
        }
    }
    public class BotGlobalsMindSettingsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotGlobalsMindSettings), "Update");
        }
        [PatchPrefix]
        public static void PatchPrefix(ref BotGlobalsMindSettings __instance)
        {
            if (DodgeToggle.Value)
            {
                __instance.DOG_FIGHT_IN = DogFighterStart.Value;
                __instance.DOG_FIGHT_OUT = DogFighterEnd.Value;
            }
        }
    }
}
