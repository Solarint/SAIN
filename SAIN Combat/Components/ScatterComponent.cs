using Aki.Reflection.Patching;
using BepInEx;
using BepInEx.Configuration;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using UnityEngine;


namespace SAIN.Combat.Components
{
    public class AddComponentPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotOwner), "PreActivate");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref BotOwner __instance)
        {
            __instance.gameObject.AddComponent<WeaponInfo>();
            //__instance.gameObject.AddComponent<WeaponScatter>();
        }
    }
    public class WeaponScatter
    {
        public Transform aimTarget;
        public float scatterRadius = 1.0f;
        public float scatterSpeed = 1.0f;
        public float timeBetweenShots = 0.1f;

        private WeaponInfo weaponinfo;
        private BotOwner bot;
        private Vector3 initialAimPosition;
        private Vector3 targetPosition;
        private float lerpTime;

        private float RateofFire
        {
            get
            {
                if (bot?.IsDead == true) return 1f;
                if (bot?.BotState != EBotState.Active) return 1f;
                if (bot?.Memory?.GoalEnemy == null) return 1f;
                if (bot?.WeaponManager?.CurrentWeapon == null) return 1f;
                else
                {
                    int roundsperminute = bot.WeaponManager.CurrentWeapon.Template.bFirerate;
                    float roundspersecond = roundsperminute / 60f;
                    float secondsperround = 1f / roundspersecond;
                    return secondsperround;
                }
            }
        }

        private void Start()
        {
            /*
            bot = GetComponent<BotOwner>();
            weaponinfo = GetComponent<WeaponInfo>();
            StartCoroutine(MonitorShooting());
            */
        }
        private void Update()
        {
            if (bot.ShootData.Shooting)
            {
                lerpTime += Time.deltaTime * scatterSpeed;
                aimTarget.position = Vector3.Lerp(aimTarget.position, targetPosition, lerpTime);
                //bot.AimingData.EndTargetPoint = 
            }
        }
        private IEnumerator MonitorShooting()
        {
            while (true)
            {
                if (bot.ShootData.Shooting)
                {
                    initialAimPosition = bot.AimingData.RealTargetPoint;

                    // Update target position and reset lerpTime
                    targetPosition = initialAimPosition + UnityEngine.Random.insideUnitSphere * scatterRadius;
                    lerpTime = 0.0f;

                    // Wait for the time between shots
                    yield return new WaitForSeconds(RateofFire);
                }
                else
                {
                    // If the bot is not shooting, just wait for the next frame
                    yield return null;
                }
            }
        }
    }
}
