using EFT;
using SAIN.Components;
using UnityEngine;

namespace SAIN.Classes
{
    public class GrenadeReactionClass : SAINBot
    {
        public GrenadeReactionClass(BotOwner bot) : base(bot) { }

        public void GrenadeThrown(Grenade grenade, Vector3 dangerPoint)
        {
            if (SAIN.BotActive)
            {
                if (GrenadeHeard(grenade.transform.position, BotOwner.Transform.position, 12f))
                {
                    BotOwner.BewareGrenade.AddGrenadeDanger(dangerPoint, grenade);
                }
                else
                {
                    float reactionTime = GetReactionTime(SAIN.Settings.DifficultyModifier);
                    BotOwner.gameObject.AddComponent<GrenadeTracker>().Initialize(grenade, dangerPoint, reactionTime);
                }
            }
        }

        private static bool GrenadeHeard(Vector3 grenadePosition, Vector3 playerPosition, float distance)
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