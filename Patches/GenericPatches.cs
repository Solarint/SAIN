using Comfort.Common;
using EFT;
using EFT.EnvironmentEffect;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Components;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Helpers;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using ExecuteRequestAction = GClass81;
using FoodAndMedsEquipCallbackType = GInterface203;
using QuickGrenadeUseCallbackType = GInterface206;
using SetInHandsMedsStruct = GStruct382<EBodyPart>;

namespace SAIN.Patches.Generic
{
    public static class GenericHelpers
    {
        public static bool CheckNotNull(BotOwner botOwner)
        {
            return botOwner != null &&
                botOwner.gameObject != null &&
                botOwner.gameObject.transform != null &&
                botOwner.Transform != null &&
                !botOwner.IsDead;
        }
    }

    namespace SetInHands
    {
        public static class Helpers
        {
            public static void SetItemEquiped(IPlayer Player, Item Item)
            {
                if (GameWorldComponent.TryGetPlayerComponent(Player, out PlayerComponent PlayerComponent))
                {
                    PlayerComponent.SetItemEquippedInHands(Item);
                }
            }
        }

        public class SetInHands_Empty : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(Player), nameof(Player.SetEmptyHands));
            }

            [PatchPostfix]
            public static void Patch(Player __instance)
            {
                Helpers.SetItemEquiped(__instance, null);
            }
        }

        public class SetInHands_Weapon_Patch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                System.Type[] Params = [typeof(Weapon), typeof(Callback<IFirearmHandsController>)];
                return AccessTools.Method(typeof(Player), nameof(Player.SetInHands), Params);
            }

            [PatchPostfix]
            public static void Patch(Player __instance, Weapon weapon)
            {
                Helpers.SetItemEquiped(__instance, weapon);
            }
        }

        public class SetInHands_Weapon_Stationary_Patch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(Player), nameof(Player.SetStationaryWeapon));
            }

            [PatchPostfix]
            public static void Patch(Player __instance, Weapon weapon)
            {
                Helpers.SetItemEquiped(__instance, weapon);
            }
        }

        public class SetInHands_Knife_Patch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                System.Type[] Params = [typeof(KnifeComponent), typeof(Callback<IKnifeController>)];
                return AccessTools.Method(typeof(Player), nameof(Player.SetInHands), Params);
            }

            [PatchPostfix]
            public static void Patch(Player __instance, KnifeComponent knife)
            {
                Helpers.SetItemEquiped(__instance, knife?.Item);
            }
        }

        public class SetInHands_Grenade_Patch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                System.Type[] Params = [typeof(ThrowWeapItemClass), typeof(Callback<IHandsThrowController>)];
                return AccessTools.Method(typeof(Player), nameof(Player.SetInHands), Params);
            }

            [PatchPrefix]
            public static void PatchPrefix(Player __instance, ThrowWeapItemClass throwWeap)
            {
                float range = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_GrenadePinDraw;
                BotManagerComponent.Instance?.BotHearing.PlayAISound(__instance.ProfileId, SAINSoundType.GrenadeDraw, __instance.Position, range, 1f);
                Helpers.SetItemEquiped(__instance, throwWeap);
            }
        }

        public class SetInHands_Food_Patch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                System.Type[] Params = [typeof(FoodDrinkItemClass), typeof(float), typeof(int), typeof(Callback<FoodAndMedsEquipCallbackType>)];
                return AccessTools.Method(typeof(Player), nameof(Player.SetInHands), Params);
            }

            [PatchPrefix]
            public static void PatchPrefix(Player __instance, FoodDrinkItemClass foodDrink)
            {
                float range = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_EatDrink;
                BotManagerComponent.Instance?.BotHearing.PlayAISound(__instance.ProfileId, SAINSoundType.Food, __instance.Position, range, 1f);
                Helpers.SetItemEquiped(__instance, foodDrink);
            }
        }

        public class SetInHands_Meds_Patch1 : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                System.Type[] Params = [typeof(MedsItemClass), typeof(EBodyPart), typeof(int), typeof(Callback<FoodAndMedsEquipCallbackType>)];
                return AccessTools.Method(typeof(Player), nameof(Player.SetInHands), Params);
            }

            [PatchPrefix]
            public static void PatchPrefix(MedsItemClass meds, Player __instance)
            {
                SAINSoundType soundType;
                float range;
                if (meds != null && meds.HealthEffectsComponent.AffectsAny([EDamageEffectType.DestroyedPart]))
                {
                    soundType = SAINSoundType.Surgery;
                    range = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Surgery;
                }
                else
                {
                    soundType = SAINSoundType.Heal;
                    range = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Healing;
                }
                BotManagerComponent.Instance?.BotHearing.PlayAISound(__instance.ProfileId, soundType, __instance.Position, range, 1f);
                Helpers.SetItemEquiped(__instance, meds);
            }
        }

        public class SetInHands_Meds_Patch2 : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                System.Type[] Params = [typeof(MedsItemClass), typeof(SetInHandsMedsStruct), typeof(int), typeof(Callback<FoodAndMedsEquipCallbackType>)];
                return AccessTools.Method(typeof(Player), nameof(Player.SetInHands), Params);
            }

            [PatchPrefix]
            public static void PatchPrefix(MedsItemClass meds, Player __instance)
            {
                SAINSoundType soundType;
                float range;
                if (meds != null && meds.HealthEffectsComponent.AffectsAny([EDamageEffectType.DestroyedPart]))
                {
                    soundType = SAINSoundType.Surgery;
                    range = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Surgery;
                }
                else
                {
                    soundType = SAINSoundType.Heal;
                    range = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Healing;
                }
                BotManagerComponent.Instance?.BotHearing.PlayAISound(__instance.ProfileId, soundType, __instance.Position, range, 1f);
                Helpers.SetItemEquiped(__instance, meds);
            }
        }

        public class SetInHands_QuickUse_Patch1 : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                System.Type[] Params = [typeof(Item), typeof(Callback<IOnHandsUseCallback>)];
                return AccessTools.Method(typeof(Player), nameof(Player.SetInHandsForQuickUse), Params);
            }

            [PatchPrefix]
            public static void PatchPrefix(Item quickUseItem, Player __instance)
            {
                Helpers.SetItemEquiped(__instance, quickUseItem);
            }
        }

        public class SetInHands_QuickUse_Patch2 : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                System.Type[] Params = [typeof(ThrowWeapItemClass), typeof(Callback<QuickGrenadeUseCallbackType>)];
                return AccessTools.Method(typeof(Player), nameof(Player.SetInHandsForQuickUse), Params);
            }

            [PatchPrefix]
            public static void PatchPrefix(ThrowWeapItemClass throwWeap, Player __instance)
            {
                Helpers.SetItemEquiped(__instance, throwWeap);
            }
        }
    }

    /// <summary>
    /// This function is called when a bot is activated, we are patching SAIN bots to remove any global painkiller effects or health regen.
    /// </summary>
    public class BotOwnerActivatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotOwner), nameof(BotOwner.method_11));
        }

        [PatchPrefix]
        public static bool PatchPrefix(BotOwner __instance)
        {
            if (SAINEnableClass.IsSAINDisabledForBot(__instance))
            {
                return true;
            }
            // ORIGINAL METHOD:
            //if (__instance.Settings.FileSettings.Boss.EFFECT_PAINKILLER)
            //{
            //	__instance.GetPlayer.ActiveHealthController.DoPainKiller();
            //}
            //if (__instance.Settings.FileSettings.Boss.DISABLE_METABOLISM)
            //{
            //	__instance.GetPlayer.HealthController.DisableMetabolism();
            //}
            //if (__instance.Settings.FileSettings.Boss.EFFECT_REGENERATION_PER_MIN > 0f)
            //{
            //	__instance.GetPlayer.ActiveHealthController.DoScavRegeneration(__instance.Settings.FileSettings.Boss.EFFECT_REGENERATION_PER_MIN);
            //}
            // END ORIGINAL METHOD
            __instance.GetPlayer.HealthController.DisableMetabolism();
            return false;
        }
    }

    public class StopRequestExecutePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass84), nameof(GClass84.method_13));
        }

        [PatchPrefix]
        public static bool Patch(GClass84 __instance, ref bool __result)
        {
            var request = __instance.BotOwner_0.BotRequestController.CurRequest;
            if (request == null)
            {
                __result = false;
                return false;
            }
            if (request.Requester != null && 
                request.Requester.IsAI && 
                SAINEnableClass.GetSAIN(__instance.BotOwner_0.ProfileId, out var sain) && 
                sain.SAINLayersActive)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    public class SetEnvironmentPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass591), nameof(GClass591.SetEnvironment));
        }

        [PatchPostfix]
        public static void Patch(GClass591 __instance, IndoorTrigger trigger)
        {
            BotManagerComponent.Instance?.PlayerEnviromentChanged(__instance?.Player?.ProfileId, trigger);
        }
    }

    public class AddPointToSearchPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.AddPointToSearch));
        }

        [PatchPrefix]
        public static bool PatchPrefix(BotsGroup __instance, BotOwner owner)
        {
            if (SAINEnableClass.IsBotInCombat(owner)) return false;
            return true;
        }
    }

    public class SetPanicPointPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotMemoryClass), nameof(BotMemoryClass.SetPanicPoint));
        }

        [PatchPrefix]
        public static bool PatchPrefix(BotMemoryClass __instance)
        {
            return !SAINEnableClass.GetSAIN(__instance.BotOwner_0.ProfileId, out _);
        }
    }

    public class HaveSeenEnemyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(EnemyInfo), nameof(EnemyInfo.HaveSeen));
        }

        [PatchPostfix]
        public static void PatchPostfix(ref bool __result, EnemyInfo __instance)
        {
            if (__result == true)
            {
                return;
            }
            if (SAINEnableClass.GetSAIN(__instance.Owner.ProfileId, out var sain)
                //&& sain.Info.Profile.IsPMC
                && sain.EnemyController.CheckAddEnemy(__instance.Person)?.EnemyKnown == true)
            {
                __result = true;
            }
        }
    }

    public class TurnDamnLightOffPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotWeaponSelector), nameof(BotWeaponSelector.TryChangeToSlot));
        }

        [PatchPrefix]
        public static void PatchPrefix(BotWeaponSelector __instance)
        {
            // Try to turn a gun's light off before swapping weapon.
            __instance.BotOwner_0?.BotLight?.TurnOff(false, true);
        }
    }

    internal class ShallKnowEnemyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EnemyInfo), nameof(EnemyInfo.ShallKnowEnemy));
        }

        [PatchPostfix]
        public static void PatchPostfix(EnemyInfo __instance, ref bool __result)
        {
            if (!SAINEnableClass.GetSAIN(__instance.Owner.ProfileId, out var botComponent))
            {
                return;
            }
            var enemy = botComponent.EnemyController.CheckAddEnemy(__instance.Person);
            __result = enemy?.EnemyKnown == true;
        }
    }

    internal class ShallKnowEnemyLatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EnemyInfo), nameof(EnemyInfo.ShallKnowEnemyLate));
        }

        [PatchPostfix]
        public static void PatchPostfix(EnemyInfo __instance, ref bool __result)
        {
            if (!SAINEnableClass.GetSAIN(__instance.Owner.ProfileId, out var botComponent))
            {
                return;
            }
            var enemy = botComponent.EnemyController.CheckAddEnemy(__instance.Person);
            __result = enemy?.EnemyKnown == true;
        }
    }

    public class GrenadeThrownActionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsController), nameof(BotsController.method_5));
        }

        [PatchPrefix]
        public static bool PatchPrefix(BotsController __instance, Grenade grenade, Vector3 position, Vector3 force, float mass)
        {
            Vector3 danger = Vector.DangerPoint(position, force, mass);
            foreach (BotOwner bot in __instance.Bots.BotOwners)
            {
                if (!SAINEnableClass.GetSAIN(bot.ProfileId, out _))
                {
                    bot.BewareGrenade.AddGrenadeDanger(danger, grenade);
                }
            }
            return false;
        }
    }

    public class GrenadeExplosionActionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsController), nameof(BotsController.method_3));
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return false;
        }
    }

    /// <summary>
    /// This function is called when a voice line or gesture is activated.
    /// We dont want bot voicelines or gestures triggering behavior here in other ai.
    /// </summary>
    public class BlockVoiceRequestsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotReceiver), nameof(BotReceiver.method_5));
        }

        [PatchPrefix]
        public static bool Patch(BotReceiver __instance, IPlayer player)
        {
            if (!SAINEnableClass.IsBotInCombat(__instance.BotOwner_0)) return true;
            if (player?.IsAI == true) return false;
            return true;
        }
    }

    public class BlockGrenadeThrowRequestsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotRequestController), nameof(BotRequestController.TryActivateThrowGrenadeRequestToPlace));
        }

        [PatchPrefix]
        public static bool Patch(BotRequestController __instance, Player targetToThrow, ref bool __result)
        {
            if (!SAINEnableClass.IsBotInCombat(__instance.Owner)) return true;
            __result = false;
            return false;
        }
    }

    public class BlockGrenadeThrowRequestsPatch2 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotRequestController), nameof(BotRequestController.TryActivateThrowGrenadeRequest));
        }

        [PatchPrefix]
        public static bool Patch(BotRequestController __instance, Player targetToThrow, ref bool __result)
        {
            if (!SAINEnableClass.IsBotInCombat(__instance.Owner)) return true;
            __result = false;
            return false;
        }
    }

    public class BlockRequestPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(AIDataRequestController), nameof(AIDataRequestController.TryAdd));
        }

        [PatchPrefix]
        public static bool Patch(BotRequest request, AIDataRequestController __instance, ref bool __result)
        {
            if (!SAINEnableClass.IsBotInCombat(__instance.AiData?.Player_0)) return true;
            if (request.Requester?.IsAI == true)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}