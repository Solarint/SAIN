using EFT;
using SAIN.Components;
using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Classes
{
    public class ThrowGrenadeClass : SAINBot
    {
        public ThrowGrenadeClass(BotOwner bot) : base(bot) { }

        public void Update()
        {
        }

        public void EnemyGrenadeThrown(Grenade grenade, Vector3 dangerPoint)
        {
            if (SAIN.BotActive && !SAIN.GameIsEnding)
            {
                float reactionTime = GetReactionTime(SAIN.Info.DifficultyModifier);
                var tracker = BotOwner.gameObject.AddComponent<GrenadeTracker>();
                tracker.Initialize(grenade, dangerPoint, reactionTime);
                ActiveGrenades.Add(tracker);
            }
        }

        public List<GrenadeTracker> ActiveGrenades { get; private set; } = new List<GrenadeTracker>();

        private int GrenadePositionComparerer(GrenadeTracker A, GrenadeTracker B)
        {
            if (A == null && B != null)
            {
                return 1;
            }
            else if (A != null && B == null)
            {
                return -1;
            }
            else if (A == null && B == null)
            {
                return 0;
            }
            else
            {
                float AMag = (BotPosition - A.DangerPoint).sqrMagnitude;
                float BMag = (BotPosition - B.DangerPoint).sqrMagnitude;
                return AMag.CompareTo(BMag);
            }
        }

        private static bool EnemyGrenadeHeard(Vector3 grenadePosition, Vector3 playerPosition, float distance)
        {
            return (grenadePosition - playerPosition).magnitude < distance;
        }

        private static float GetReactionTime(float diffMod)
        {
            float reactionTime = 0.33f;
            reactionTime *= diffMod;
            reactionTime *= Random.Range(0.75f, 1.25f);

            float min = 0.15f;
            float max = 0.66f;

            return Mathf.Clamp(reactionTime, min, max);
        }
    }
}