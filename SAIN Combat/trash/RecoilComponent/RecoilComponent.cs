using Aki.Reflection.Patching;
using BepInEx;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Combat.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Solarint.VersionCheck;

// GClass545 = RecoilData in 3.5.5
// GClass546 = Shootdata in 3.5.5

namespace SAIN.Combat.Components
{
    public class ReplacementClassPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            _RecoilData = AccessTools.PropertySetter(typeof(BotOwner), "RecoilData");
            return AccessTools.Method(typeof(BotOwner), "PreActivate");
        }

        private static MethodInfo _RecoilData;

        [PatchPostfix]
        public static void PatchPostfix(ref BotOwner __instance)
        {
            if (_RecoilData != null)
            {
                RecoilComponent recoil = new RecoilComponent(__instance);
                _RecoilData.Invoke(__instance, new object[] { recoil });
            }
            else
            {
                Logger.LogWarning($"Could not add recoil component for {__instance.name}");
            }
        }
    }
    public class RecoilComponent : GClass545
    {
        public new Vector3 RecoilOffset => FinalRecoilPoint;
        public RecoilComponent(BotOwner owner) : base(owner)
        {
            bot = owner;
        }

        public Vector3 NewRecoil()
        {
            WeaponInfo stats = bot.gameObject.GetComponent<WeaponInfo>();
            float modifier = stats.FinalModifier;
            Weapon weapon = bot.WeaponManager.CurrentWeapon;

            Vector3 recoilVector = Helpers.Shoot.Recoil(NewRecoilPoint, weapon.RecoilTotal, modifier);

            float firerate = 1f / (bot.WeaponManager.CurrentWeapon.Template.bFirerate / 60);
            RecoilTimeNormalize += firerate;
            RecoilResetTime = Time.time + 0.25f;
            RecoilLastTime = Time.time;

            FinalRecoilPoint = recoilVector;
            NewRecoilPoint = recoilVector;
            return FinalRecoilPoint;
        }
        public Vector3 NewLosingRecoil()
        {
            RecoilTimeNormalize -= Time.deltaTime;
            RecoilTimeNormalize = Mathf.Clamp(RecoilTimeNormalize, 0f, RecoilTimeNormalize);
            float d = RecoilTimeNormalize / RecoilResetTime;

            Vector3 lerpvector = Vector3.Lerp(Vector3.zero, NewRecoilPoint, d);

            return lerpvector;
        }
        public Vector3 NewCheckEndRecoil()
        {
            if (!bot.ShootData.Shooting)
            {
                FinalRecoilPoint = Vector3.zero;
                NewRecoilPoint = Vector3.zero;
                RecoilTimeNormalize = 0f;
                RecoilResetTime = 0f;
                RecoilLastTime = 0f;
            }
            return Vector3.zero;
        }
        public float WeaponRecoilTotal { get; private set; }
        public float WeaponRecoilBase { get; private set; }
        public float WeaponRecoilDelta { get; private set; }

        private BotOwner bot;
        private Vector3 FinalRecoilPoint;
        private Vector3 NewRecoilPoint;
        private float RecoilTimeNormalize;
        private float RecoilResetTime;
        private float RecoilLastTime;
    }
}
