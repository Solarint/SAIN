using Aki.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Combat.Components;
using SAIN.Combat.Helpers;
using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SAIN.Combat.Configs.AimingConfig;
using static SAIN.Combat.Configs.DebugConfig;

namespace SAIN.Combat.Patches
{
    public class AimOffsetPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass544), "method_13");
        }

        [PatchPrefix]
        public static bool PatchPrefix(GClass544 __instance, ref BotOwner ___botOwner_0, ref Vector3 ___vector3_5, ref Vector3 ___vector3_4, ref float ___float_13)
        {
            __instance.EndTargetPoint = __instance.RealTargetPoint + ___vector3_5 + ___float_13 * (___vector3_4 + (___botOwner_0.RecoilData.RecoilOffset * ScatterMultiplier.Value));

            if (DebugRecoil.Value && ___botOwner_0.ShootData.Shooting)
            {
                GameObject lineObject = new GameObject();
                float sphereSize = 0.05f;
                GameObject sphereObject = DebugDrawer.Sphere(__instance.EndTargetPoint, sphereSize, Color.red);

                float lineWidth = 0.005f;
                GameObject lineObjectSegment = DebugDrawer.Line(___botOwner_0.WeaponRoot.position, __instance.EndTargetPoint, lineWidth, Color.blue);
                lineObjectSegment.transform.parent = lineObject.transform;

                sphereObject.transform.parent = lineObject.transform;

                DebugDrawer.DestroyAfterDelay(lineObject, 2f);
            }
            return false;
        }
    }
    public class RecoilPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass545), "Recoil");
        }

        [PatchPrefix]
        public static bool PatchPrefix(GClass545 __instance, ref Vector3 ___vector3_0, ref BotOwner ___botOwner_0, ref Vector3 ___vector3_1, ref float ___float_0, ref float ___float_1, ref float ___float_2)
        {
            WeaponInfo stats = ___botOwner_0.gameObject.GetComponent<WeaponInfo>();
            float modifier = stats.ShootModifier;
            float recoil = ___botOwner_0.WeaponManager.CurrentWeapon.RecoilTotal;

            Vector3 recoilVector = ShootPatchHelpers.Recoil(___vector3_0, recoil, modifier);
            ___vector3_0 = recoilVector;

            return false;
        }
    }
    public class LoseRecoilPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass545), "LosingRecoil");
        }
        [PatchPrefix]
        public static bool PatchPrefix(GClass545 __instance, ref Vector3 ___vector3_0, ref BotOwner ___botOwner_0, ref Vector3 ___vector3_1, ref float ___float_0, ref float ___float_1, ref float ___float_2)
        {
            ___vector3_1 = Vector3.Lerp(Vector3.zero, ___vector3_0, 0.5f);
            ___vector3_0 = ___vector3_1;

            return false;
        }
    }
    public class EndRecoilPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass545), "CheckEndRecoil");
        }
        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return true;
        }
    }
}
/*
namespace SAIN.Combat.Patches
{
    public class AimOffsetPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass544), "method_13");
        }

        [PatchPrefix]
        public static bool PatchPrefix(GClass544 __instance, ref BotOwner ___botOwner_0, ref Vector3 ___vector3_5, ref Vector3 ___vector3_4, ref float ___float_13)
        {
            WeaponInfo stats = ___botOwner_0.gameObject.GetComponent<WeaponInfo>();
            float modifier = stats.ShootModifier;

            float recoil = ___botOwner_0.WeaponManager.CurrentWeapon.RecoilTotal;

            Vector3 recoilVector = Vector3.zero;
            if (___botOwner_0.ShootData.Shooting)
            {
                recoilVector = ShootPatchHelpers.Recoil(Vector3.zero, recoil, modifier);
            }

            __instance.EndTargetPoint = __instance.RealTargetPoint + ___vector3_5 + ___float_13 * (___vector3_4 + recoilVector);

            if (DebugRecoil.Value)
            {
                Draw.Sphere(__instance.EndTargetPoint, 0.05f, Color.white);
            }

            return false;
        }
    }
    public class RecoilPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass545), "Recoil");
        }

        [PatchPrefix]
        public static bool PatchPrefix(GClass545 __instance, ref Vector3 ___vector3_0)
        {
            if (__instance is RecoilComponent recoilComponent)
            {
                ___vector3_0 = recoilComponent.NewRecoil();
                return false;
            }
            return true;
        }
    }
    public class LoseRecoilPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass545), "LosingRecoil");
        }
        [PatchPrefix]
        public static bool PatchPrefix(GClass545 __instance)
        {
            if (__instance is RecoilComponent recoilComponent)
            {
                recoilComponent.NewLosingRecoil();
                return false;
            }
            return true;
        }
    }
    public class EndRecoilPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass545), "CheckEndRecoil");
        }
        [PatchPrefix]
        public static bool PatchPrefix(GClass545 __instance)
        {
            if (__instance is RecoilComponent recoilComponent)
            {
                recoilComponent.NewCheckEndRecoil();
                return false;
            }
            return true;
        }
    }
}*/