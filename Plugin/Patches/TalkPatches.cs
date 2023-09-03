using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN.Components;
using System;
using System.Collections.Generic;
using System.Reflection;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.Decision;
using SAIN.SAINComponent.Classes.Talk;
using SAIN.SAINComponent.Classes.WeaponFunction;
using SAIN.SAINComponent.Classes.Mover;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;
using Comfort.Common;

namespace SAIN.Patches.Talk
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
                SAINPlugin.BotController?.PlayerTalk(@event, mask, __instance);
                return true;
            }
            else
            {
                var stackTrace = Environment.StackTrace;
                bool fromSAIN = stackTrace.Contains(nameof(SAINComponentClass.Talk)) || stackTrace.Contains(nameof(SAINBotTalkClass));
                if (!fromSAIN && PatchHelpers.BadTriggers.Contains(@event))
                {
                    if (SAINPlugin.DebugMode)
                    {
                        Logger.LogInfo($"PlayerTalkPatch: Blocked {@event}");
                    }
                    return false;
                }

                if (PatchHelpers.CheckTalkEvent(__instance, @event))
                {
                    if (SAINPlugin.DebugMode)
                    {
                        Logger.LogInfo($"PlayerTalkPatch: Allowed {@event}");
                    }

                    SAINPlugin.BotController?.PlayerTalk(@event, mask, __instance);
                    return true;
                }

                if (SAINPlugin.DebugMode)
                {
                    Logger.LogInfo($"PlayerTalkPatch: Blocked {@event}");
                }

                return false;
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
        public static bool PatchPrefix(ref BotOwner ___botOwner_0, EPhraseTrigger type, ETagStatus? additionalMask = null)
        {
            if (___botOwner_0.HealthController?.IsAlive == false)
            {
                return false;
            }

            return PatchHelpers.AllowDefaultBotTalk(___botOwner_0, type, additionalMask);
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

        public static bool CheckTalkEvent(Player player, EPhraseTrigger trigger)
        {
            bool result = player.IsAI && (BotInGroup(player.AIData.BotOwner) || GoodSoloTriggers.Contains(trigger));

            return result;
        }

        public static bool AllowDefaultBotTalk(BotOwner botOwner, EPhraseTrigger trigger, ETagStatus? mask)
        {
            var component = botOwner.GetComponent<SAINComponentClass>();
            if (component == null)
            {
                return true;
            }
            if (BotInGroup(botOwner))
            {
                if (GoodGroupTriggers.Contains(trigger))
                {
                    component?.Talk.Say(trigger, mask);
                }
            }
            else
            {
                if (GoodSoloTriggers.Contains(trigger))
                {
                    component?.Talk.Say(trigger, mask);
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