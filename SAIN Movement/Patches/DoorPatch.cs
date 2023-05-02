using Aki.Reflection.Patching;
using EFT;
using EFT.Interactive;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

namespace Movement.Patches
{
    namespace DoorPatch
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
                if (___botOwner_0.Memory.GoalEnemy == null)
                {
                    if (Etype == EInteractionType.Breach)
                        Etype = EInteractionType.Open;

                    return;
                }

                if (Etype == EInteractionType.Open || Etype == EInteractionType.Breach)
                {
                    NavMeshPath navMeshPath = new NavMeshPath();
                    NavMesh.CalculatePath(___botOwner_0.Transform.position, ___botOwner_0.Memory.GoalEnemy.CurrPosition, -1, navMeshPath);

                    bool enemyClose = navMeshPath.CalculatePathLength() < 15f;

                    if (enemyClose || ___botOwner_0.Memory.IsUnderFire)
                    {
                        var breakInParameters = door.GetBreakInParameters(___botOwner_0.Position);

                        if (door.BreachSuccessRoll(breakInParameters.InteractionPosition))
                        {
                            Etype = EInteractionType.Breach;
                        }
                        else Etype = EInteractionType.Open;
                    }
                    else Etype = EInteractionType.Open;
                }
            }
        }
    }
}