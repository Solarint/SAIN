using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Comfort.Common;

namespace SAIN.Combat.Patches
{
    public class TalkPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), "Say");
        }

        [PatchPrefix]
        public static bool PatchPrefix(Player __instance, ref Action<EPhraseTrigger, int> ___action_13, EPhraseTrigger @event, bool demand = false, float delay = 0f, ETagStatus mask = (ETagStatus)0, int probability = 100, bool aggressive = false)
        {
            bool badvoice = @event == EPhraseTrigger.OnEnemyShot || @event == EPhraseTrigger.CheckHim;
            bool isBotOutofAmmo = @event == EPhraseTrigger.NeedAmmo || @event == EPhraseTrigger.OnOutOfAmmo || @event == EPhraseTrigger.OnWeaponReload;

            if (isBotOutofAmmo || badvoice)
            {
                @event = EPhraseTrigger.MumblePhrase;
            }

            if (@event == EPhraseTrigger.Cooperation)
            {
                return true;
            }
            if (@event == EPhraseTrigger.MumblePhrase)
            {
                @event = ((aggressive || Time.time < __instance.Awareness) ? EPhraseTrigger.OnFight : EPhraseTrigger.OnMutter);
            }
            if (!__instance.Speaker.OnDemandOnly || demand)
            {
                if (Singleton<GClass629>.Instantiated)
                {
                    Singleton<GClass629>.Instance.SayPhrase(__instance, @event);
                }
                if (demand || probability > 99 || probability > UnityEngine.Random.Range(0, 100))
                {
                    ETagStatus etagStatus = (aggressive || __instance.Awareness > Time.time) ? ETagStatus.Combat : ETagStatus.Unaware;
                    if (delay > 0f)
                    {
                        __instance.Speaker.Queue(@event, __instance.HealthStatus | mask | etagStatus, delay, demand);
                        return false;
                    }
                    __instance.Speaker.Play(@event, __instance.HealthStatus | mask | etagStatus, demand, null);
                }
                return false;
            }
            Action<EPhraseTrigger, int> action = ___action_13;
            if (action == null)
            {
                return false;
            }
            action(@event, 5);
            return false;
        }
    }
    public class BotTalkPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            _method_1 = AccessTools.Method(typeof(GClass352), "method_1");
            _method_2 = AccessTools.Method(typeof(GClass352), "method_2");
            return AccessTools.Method(typeof(GClass352), "Say");
        }

        private static MethodInfo _method_1;
        private static MethodInfo _method_2;

        [PatchPrefix]
        public static void PatchPrefix(ref float ___float_1, ref bool ___bool_1, GClass352 __instance, ref BotOwner ___botOwner_0,
            EPhraseTrigger type, bool sayImmediately = false, ETagStatus? additionalMask = null)
        {
            if (!___bool_1)
            {
                return;
            }

            bool badvoice = type == EPhraseTrigger.OnEnemyShot || type == EPhraseTrigger.CheckHim;
            bool isBotOutofAmmo = type == EPhraseTrigger.NeedAmmo || type == EPhraseTrigger.OnOutOfAmmo || type == EPhraseTrigger.OnWeaponReload;

            if (isBotOutofAmmo || badvoice)
            {
                type = EPhraseTrigger.MumblePhrase;
            }

            object[] parameters = new object[] { additionalMask };
            ETagStatus mask = (ETagStatus)_method_1.Invoke(__instance, parameters);

            if (___botOwner_0.Settings.FileSettings.Mind.TALK_WITH_QUERY && !sayImmediately)
            {
                object[] method2param = new object[] { type, true, 0f, mask };
                _method_2.Invoke(__instance, method2param);

                return;
            }

            ___float_1 = Time.time + Helpers.MathHelpers.Random(GClass560.Core.TALK_DELAY, -GClass560.Core.TALK_DELAY);

            ___botOwner_0.GetPlayer.Say(type, true, 0f, mask, 100, false);
        }
    }
    public class SetVisiblePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // These properties are "read only" So we must use reflection
            _IsVisible = AccessTools.PropertySetter(typeof(GClass475), "IsVisible");
            _FirstTimeSeen = AccessTools.PropertySetter(typeof(GClass475), "FirstTimeSeen");
            _PersonalSeenTime = AccessTools.PropertySetter(typeof(GClass475), "PersonalSeenTime");

            return AccessTools.Method(typeof(GClass475), "SetVisible");
        }

        // Save the method info for the "read only" properties
        private static MethodInfo _IsVisible;
        private static MethodInfo _FirstTimeSeen;
        private static MethodInfo _PersonalSeenTime;

        [PatchPrefix]
        public static bool PatchPrefix(GClass475 __instance, bool value)
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
    public class UpdateRatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass115), "Update");
        }

        [PatchPrefix]
        public static void PatchPrefix(GClass115 __instance, ref float ___float_0)
        {
            if (___float_0 > Time.time + 1f) ___float_0 = Time.time + 1f;
        }
    }
}