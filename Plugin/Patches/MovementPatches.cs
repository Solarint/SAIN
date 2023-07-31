using Aki.Reflection.Patching;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using SAIN.Components;
using SAIN.Helpers;
using System.Reflection;
using UnityEngine;
using DrakiaXYZ.BigBrain.Brains;
using UnityEngine.AI;
using SAIN.Layers;
using System;

namespace SAIN.Patches.Generic
{
    public class KickPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("DoorOpener")?.PropertyType?.GetMethod("Interact", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static void PatchPrefix(ref BotOwner ___botOwner_0, Door door, ref EInteractionType Etype)
        {
            if (SAINPlugin.BotController.GetBot(___botOwner_0.ProfileId, out var bot))
            {
                if (bot.Enemy == null)
                {
                    if (Etype == EInteractionType.Breach)
                    {
                        Etype = EInteractionType.Open;
                    }

                    return;
                }

                if (Etype == EInteractionType.Open || Etype == EInteractionType.Breach)
                {
                    bool enemyClose = Vector3.Distance(bot.Position, bot.Enemy.CurrPosition) < 30f;

                    if (enemyClose || ___botOwner_0.Memory.IsUnderFire)
                    {
                        var breakInParameters = door.GetBreakInParameters(___botOwner_0.Position);

                        if (door.BreachSuccessRoll(breakInParameters.InteractionPosition))
                        {
                            Etype = EInteractionType.Breach;
                        }
                        else
                        {
                            Etype = EInteractionType.Open;
                        }
                    }
                    else
                    {
                        Etype = EInteractionType.Open;
                    }
                }
            }
        }
    }
}
