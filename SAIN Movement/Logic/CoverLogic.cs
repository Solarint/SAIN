using BepInEx.Logging;
using EFT;
using SAIN.Classes;
using SAIN.Helpers;
using SAIN_Helpers;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.DebugConfig;

namespace SAIN.Layers.Logic
{
    public class CoverLogic : SAINBotExt
    {
        public CoverLogic(BotOwner bot) : base(bot)
        {
        }

        public bool CheckSelfForCover(out float ratio, float minratio = 0.1f)
        {
            int rays = 0;
            int cover = 0;
            foreach (var part in BotOwner.MainParts.Values)
            {
                BotOwner.Memory.GoalEnemy.Person.MainParts.TryGetValue(BodyPartType.head, out BodyPartClass EnemyHead);
                Vector3 direction = part.Position - EnemyHead.Position;
                if (Physics.Raycast(EnemyHead.Position, direction, direction.magnitude, Components.SAINCoreComponent.SightMask))
                {
                    cover++;
                }
                rays++;
            }

            ratio = (float)cover / rays;

            return ratio > minratio;
        }

        public bool CanBotBackUp()
        {
            const float angleStep = 15f;
            const float rangeStep = 2f;
            const int max = 10;
            int i = 0;
            while (i < max)
            {
                float angleAdd = angleStep * i;
                float currentAngle = UnityEngine.Random.Range(-5f - angleAdd, 5f + angleAdd);
                float currentRange = rangeStep * i + 2f;

                Vector3 DodgeFallBack = HelperClasses.FindArcPoint(BotOwner.Transform.position, BotOwner.Memory.GoalEnemy.CurrPosition, currentRange, currentAngle);
                if (NavMesh.SamplePosition(DodgeFallBack, out NavMeshHit hit, 5f, -1))
                {
                }

                i++;
            }
            return false;
        }
    }
}