using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN.Components;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SAIN.Patches
{
    public class PlayerTalkPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), "Say");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref Player __instance, ref EPhraseTrigger @event, ref ETagStatus mask, ref bool aggressive)
        {
            if (__instance.HealthController?.IsAlive == false)
            {
                return false;
            }

            if (__instance.IsYourPlayer)
            {
                var component = __instance.GetComponent<PlayerTalkComponent>();
                component?.TalkEvent(@event, mask, aggressive);
                return true;
            }
            else
            {
                if (!Environment.StackTrace.Contains("BotTalk") && PatchHelpers.BadTriggers.Contains(@event))
                {
                    //Logger.LogError($"Blocked [{@event}] with Mask [{mask}] Aggressive: [{aggressive}]");
                    return false;
                }

                return PatchHelpers.CheckTalkEvent(__instance, @event, mask, aggressive);
            }
        }
    }

    public class TalkDisablePatch1 : ModulePatch
    {
        private static PropertyInfo BotTalk;

        protected override MethodBase GetTargetMethod()
        {
            BotTalk = AccessTools.Property(typeof(BotOwner), "BotTalk");
            return AccessTools.Method(BotTalk.PropertyType, "Say");
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0, EPhraseTrigger type, bool sayImmediately = false, ETagStatus? additionalMask = null)
        {
            if (___botOwner_0.HealthController?.IsAlive == false)
            {
                return false;
            }

            PatchHelpers.AllowDefaultBotTalk(___botOwner_0, type, additionalMask);

            return false;
        }
    }

    public class TalkDisablePatch2 : ModulePatch
    {
        private static PropertyInfo BotTalk;

        protected override MethodBase GetTargetMethod()
        {
            BotTalk = AccessTools.Property(typeof(BotOwner), "BotTalk");
            return AccessTools.Method(BotTalk.PropertyType, "method_5");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return false;
        }
    }

    public class TalkDisablePatch3 : ModulePatch
    {
        private static PropertyInfo BotTalk;

        protected override MethodBase GetTargetMethod()
        {
            BotTalk = AccessTools.Property(typeof(BotOwner), "BotTalk");
            return AccessTools.Method(BotTalk.PropertyType, "method_4");
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return false;
        }
    }

    public class TalkDisablePatch4 : ModulePatch
    {
        private static PropertyInfo BotTalk;

        protected override MethodBase GetTargetMethod()
        {
            BotTalk = AccessTools.Property(typeof(BotOwner), "BotTalk");
            return AccessTools.Method(BotTalk.PropertyType, "TrySay", new Type[] { typeof(EPhraseTrigger), typeof(ETagStatus?), typeof(bool) });
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return false;
        }
    }

    public class PatchHelpers
    {
        public static bool BotInGroup(BotOwner botOwner)
        {
            return botOwner?.BotsGroup?.MembersCount > 1;
        }

        public static bool CheckTalkEvent(Player player, EPhraseTrigger trigger, ETagStatus mask, bool aggressive)
        {
            if (player.IsAI)
            {
                if (BotInGroup(player.AIData.BotOwner) || GoodSoloTriggers.Contains(trigger))
                {
                    var component = player.AIData.BotOwner.GetComponent<SAINComponent>();
                    component?.Talk.TalkEvent(trigger, mask, aggressive);
                    return true;
                }
            }

            return false;
        }

        public static bool AllowDefaultBotTalk(BotOwner botOwner, EPhraseTrigger trigger, ETagStatus? mask)
        {
            if (BotInGroup(botOwner))
            {
                if (GoodGroupTriggers.Contains(trigger))
                {
                    var component = botOwner.GetComponent<SAINComponent>();
                    component?.Talk.Talk.Say(trigger, mask);
                }
            }
            else
            {
                if (GoodSoloTriggers.Contains(trigger))
                {
                    var component = botOwner.GetComponent<SAINComponent>();
                    component?.Talk.Talk.Say(trigger, mask);
                }
            }

            return false;
        }

        public static List<EPhraseTrigger> GoodGroupTriggers = new List<EPhraseTrigger>()
        {
            EPhraseTrigger.OnEnemyGrenade,
            EPhraseTrigger.OnFriendlyDown,
            EPhraseTrigger.OnFirstContact,
            EPhraseTrigger.FriendlyFire,
            EPhraseTrigger.EnemyDown,
            EPhraseTrigger.OnAgony,
            EPhraseTrigger.SniperPhrase,
            EPhraseTrigger.MumblePhrase,
            EPhraseTrigger.OnDeath
        };

        public static List<EPhraseTrigger> GoodSoloTriggers = new List<EPhraseTrigger>()
        {
            EPhraseTrigger.OnAgony,
            EPhraseTrigger.OnFight,
            EPhraseTrigger.OnDeath
        };

        public static List<EPhraseTrigger> BadTriggers = new List<EPhraseTrigger>()
        {
            EPhraseTrigger.OnWeaponReload,
            EPhraseTrigger.OnOutOfAmmo,
            EPhraseTrigger.NeedAmmo,
            EPhraseTrigger.EnemyHit,
            EPhraseTrigger.OnEnemyShot
        };
    }
}