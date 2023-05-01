using BepInEx.Logging;
using EFT;
using HarmonyLib;
using Movement.Components;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

namespace Movement.Helpers
{
    public class BotDodge
    {
        public BotDodge(BotOwner bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(BotDodge));
            this.bot = bot;
        }

        private readonly BotOwner bot;

        protected ManualLogSource Logger;

        public bool Execute()
        {
            if (FindPlace(bot, out Vector3 dodgePosition))
            {
                bot.GoToPoint(dodgePosition, false, -1, false, true, true);
                return true;
            }
            return false;
        }

        private bool FindPlace(BotOwner bot, out Vector3 position)
        {
            float ShuffleRange = 2f;
            float ArcAngle = 30.0f; // The angle of the dodge arc in degrees

            Vector3 BotPosition = bot.Transform.position;
            Vector3 TargetPosition = bot.Memory.GoalEnemy.CurrPosition;

            for (int i = 0; i < 3; i++)
            {
                FindArcPoint(BotPosition, TargetPosition, out Vector3 ArcPoint, ShuffleRange, ArcAngle, 0.25f, ShuffleRange);
                if (NavMesh.SamplePosition(ArcPoint, out NavMeshHit navmeshhit, 1f, -1))
                {
                    position = navmeshhit.position;
                    return true;
                }
            }
            position = Vector3.zero;
            return false;
        }

        private Vector3 FindArcPoint(Vector3 bot, Vector3 target, out Vector3 dodgePosition, float arcRadius, float arcAngle, float minDist, float maxDist)
        {
            bool movingRight = Random.value > 0.5f;

            Vector3 forward = target - bot;
            forward.y = 0.0f;
            forward.Normalize();

            Vector3 right = new Vector3(forward.z, 0.0f, -forward.x);

            float arcStart = movingRight ? -arcAngle / 2.0f : 180.0f - arcAngle / 2.0f;
            float arcEnd = movingRight ? arcAngle / 2.0f : 180.0f + arcAngle / 2.0f;

            arcRadius = Mathf.Clamp(arcRadius, minDist, maxDist);
            float randomAngle = Random.Range(arcStart, arcEnd);
            Vector3 direction = Quaternion.AngleAxis(randomAngle, Vector3.up) * right;
            dodgePosition = bot + direction * arcRadius;

            return dodgePosition;
        }
    }
}