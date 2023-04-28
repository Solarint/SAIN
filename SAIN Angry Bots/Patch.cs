using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN_Grenades.Configs;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SAIN_Grenades.Patches
{
    public class PlayerTalkPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), "Say");
        }

        [PatchPrefix]
        public static void PatchPrefix(ref EPhraseTrigger @event, ref ETagStatus mask, ref bool aggressive)
        {
            if (!AngryConfig.EnableMod.Value)
                return;

            bool badvoice =
                @event == EPhraseTrigger.NeedAmmo ||
                @event == EPhraseTrigger.OnOutOfAmmo ||
                @event == EPhraseTrigger.OnWeaponReload ||
                @event == EPhraseTrigger.OnEnemyShot ||
                @event == EPhraseTrigger.CheckHim ||
                @event == EPhraseTrigger.OnEnemyGrenade ||
                @event == EPhraseTrigger.OnGrenade;

            if (badvoice)
            {
                @event = EPhraseTrigger.MumblePhrase;
                mask = ETagStatus.Combat;
            }

            if (AngryConfig.SuperAngryMode.Value)
            {
                mask = ETagStatus.Combat;
                @event = EPhraseTrigger.MumblePhrase;
                aggressive = true;
            }
        }
    }
    public class GlobalTalkSettingsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotGlobalsMindSettings), "Update");
        }

        [PatchPostfix]
        public static void PatchPostfix(BotGlobalsMindSettings __instance)
        {
            if (!AngryConfig.EnableMod.Value) return;

            __instance.TALK_WITH_QUERY = true;
            __instance.CAN_TALK = true;
        }
    }

    // Not Implemented ////////////////////////////////////////////////
    // Not Implemented ////////////////////////////////////////////////
    // Not Implemented ////////////////////////////////////////////////
    // Not Implemented ////////////////////////////////////////////////
    // Not Implemented ////////////////////////////////////////////////
    // Not Implemented ////////////////////////////////////////////////

    public class SetVisiblePatch : ModulePatch
    {
        private static PropertyInfo _GoalEnemy;

        private static MethodInfo _IsVisible;
        private static MethodInfo _FirstTimeSeen;
        private static MethodInfo _PersonalSeenTime;

        protected override MethodBase GetTargetMethod()
        {
            _GoalEnemy = AccessTools.Property(typeof(BotMemoryClass), "GoalEnemy");

            _IsVisible = AccessTools.Method(_GoalEnemy.PropertyType, "IsVisible");
            _FirstTimeSeen = AccessTools.Method(_GoalEnemy.PropertyType, "FirstTimeSeen");
            _PersonalSeenTime = AccessTools.Method(_GoalEnemy.PropertyType, "PersonalSeenTime");

            return AccessTools.Method(_GoalEnemy.PropertyType, "SetVisible");
        }

        [PatchPrefix]
        public static bool PatchPrefix(GClass475 __instance, bool value) //////////////////////////////////////////////////////////////////////////////////// GCLASS GO HERE
        {
            // save "value" as a bool that is easier to understand for readability
            bool isNowVisible = value;

            // Save the old state of "visibility" for use in logic, then set the new visible status
            bool wasVisible = __instance.IsVisible;
            _IsVisible.Invoke(__instance, new object[] { isNowVisible });
            // If the enemy was previously visible, but is now not visible then:
            if (!isNowVisible && wasVisible)
            {
                if (__instance.Owner.Memory.GoalEnemy != null && __instance.Owner.Memory.GoalEnemy == __instance)
                {
                    __instance.Owner.Memory.LoseVisionCurrentEnemy();

                    if (__instance.Owner.BotsGroup.GroupTalk.CanSay(__instance.Owner, EPhraseTrigger.LostVisual))
                    {
                        __instance.Owner.BotTalk.Say(EPhraseTrigger.OnFight, false, ETagStatus.Combat);
                    }
                }
                __instance.GroupOwner.LoseVision(__instance.Person);
            }

            // If the enemy was not previously visible, but is now visible then:
            if (isNowVisible && !wasVisible)
            {
                if (__instance.Owner.Memory.GoalEnemy != null)
                {
                    if (__instance.Owner.Memory.GoalEnemy.TimeLastSeen < Time.time + 10f && !isNowVisible)
                    {
                        __instance.Owner.BotTalk.Say(EPhraseTrigger.OnFight, false, ETagStatus.Combat);
                    }
                }

                // Test - Adding delay to every aim attempt.
                __instance.Owner.AimingData.SetNextAimingDelay(0.15f);

                if (!__instance._haveSeenPersonal)
                {
                    __instance._haveSeenPersonal = true;
                    _FirstTimeSeen.Invoke(__instance, new object[] { Time.time });

                    if (__instance.Owner.BotsGroup.GroupTalk.CanSay(__instance.Owner, EPhraseTrigger.OnFirstContact))
                    {
                        __instance.Owner.BotTalk.Say(EPhraseTrigger.OnFirstContact, false, ETagStatus.Combat);
                    }
                }

                _PersonalSeenTime.Invoke(__instance, new object[] { Time.time });

                if (__instance.Owner.Memory.GoalEnemy == null)
                {
                    __instance.Owner.BotsGroup.CalcGoalForBot(__instance.Owner);
                }

                int num = 0;
                foreach (KeyValuePair<IAIDetails, GClass475> keyValuePair in __instance.Owner.EnemiesController.EnemyInfos)
                {
                    if (keyValuePair.Value.IsVisible)
                    {
                        num++;
                    }
                }
                if (num > UnityEngine.Random.Range(1f, 2f))
                {
                    __instance.Owner.BotTalk.Say(EPhraseTrigger.Spreadout, false, ETagStatus.Combat);
                }

                // Taunt
                if (__instance.HaveSeen)
                {
                    __instance.Owner.BotTalk.Say(EPhraseTrigger.OnFight, false, ETagStatus.Combat);
                }
                else
                {
                    // Next shot miss
                    if (Vector3.Dot(__instance.Owner.LookDirection, __instance.Person.LookDirection) > 0f)
                    {
                        //__instance.Owner.AimingData.NextShotMiss();
                    }

                    // Group Talk
                    if (__instance.Owner.BotsGroup.GroupTalk.CanSay(__instance.Owner, EPhraseTrigger.OnFirstContact))
                    {
                        __instance.Owner.BotTalk.Say(EPhraseTrigger.OnFirstContact, false, ETagStatus.Combat);
                    }
                }
                __instance.Owner.BotPersonalStats.AddGetVision(__instance.Owner, __instance);
                __instance.GroupOwner.GetVision();
            }
            return false;
        }
    }
}