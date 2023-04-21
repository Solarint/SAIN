using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Movement.Patches
{
    public class CoverPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass407), "TryMoveToEnemy");
        }
        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0)
        {
            BotOwner bot = ___botOwner_0;
            Vector3 botPos = bot.Transform.position;
            Vector3 enemyPos = bot.Memory.GoalEnemy.EnemyLastPosition;
            SainMemory sain = bot.gameObject.GetComponent<SainMemory>();

            if (bot.Memory.GoalEnemy.PersonalLastSeenTime + 5f < Time.time)
            {
                // Calculate a new path between bot and target
                NavMeshPath navMeshPath = new NavMeshPath();
                NavMesh.CalculatePath(botPos, enemyPos, -1, navMeshPath);

                //Vector3 firstCorner = navMeshPath.corners[1];

                Vector3 coverPoint = Vector3.RotateTowards(navMeshPath.corners[2], botPos, 10f, 999f);

                Vector3 goodcover = coverPoint.normalized;

                bot.GoToPoint(goodcover, true, 2f, false, true, true);

                sain.GoingToNewCover = true;

                return false;
            }
            return true;
        }
    }
}
