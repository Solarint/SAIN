using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using Movement.Helpers;
using SAIN_Helpers;
using UnityEngine;
using UnityEngine.AI;
using static SAIN_Helpers.DebugDrawer;
using static SAIN_Helpers.SAIN_Math;

namespace SAIN.Movement.Layers
{
    namespace DogFight
    {
        internal class DogFightLogic : CustomLogic
        {
            public DogFightLogic(BotOwner bot) : base(bot)
            {
                Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
                updateTarget_0 = new UpdateTarget(bot);
                updateMove_0 = new UpdateMove(bot);
            }

            public override void Start()
            {
                BotOwner.PatrollingData.Pause();
            }

            public override void Stop()
            {
                BotOwner.PatrollingData.Unpause();
            }

            //private float AimReactionTimer = 0f;

            public override void Update()
            {
                var goalEnemy = BotOwner.Memory.GoalEnemy;

                Fight();

                if (goalEnemy != null && goalEnemy.CanShoot && goalEnemy.IsVisible)
                {
                    BotOwner.Steering.LookToPoint(goalEnemy.CurrPosition);

                    updateTarget_0.Update();

                    return;
                }
            }

            //private readonly BotOwner BotOwner;
            private float ReactionTimer = 0f;

            public void Fight()
            {
                if (ReactionTimer < Time.time)
                {
                    ReactionTimer = Time.time + 0.25f;

                    updateMove_0.Update();
                }
            }

            public bool CheckEnemyDistance(out Vector3 trgPos)
            {
                Vector3 a = -NormalizeFastSelf(BotOwner.Memory.GoalEnemy.Direction);

                trgPos = Vector3.zero;

                float num = 0f;
                if (NavMesh.SamplePosition(BotOwner.Position + a * 2f, out NavMeshHit navMeshHit, 1f, -1))
                {
                    trgPos = navMeshHit.position;

                    Vector3 a2 = trgPos - BotOwner.Position;

                    float magnitude = a2.magnitude;

                    if (magnitude != 0f)
                    {
                        Vector3 a3 = a2 / magnitude;

                        num = magnitude;

                        if (NavMesh.SamplePosition(BotOwner.Position + a3 * 10f, out navMeshHit, 1f, -1))
                        {
                            trgPos = navMeshHit.position;

                            num = (trgPos - BotOwner.Position).magnitude;
                        }
                    }
                }
                if (num != 0f)
                {
                    navMeshPath_0.ClearCorners();

                    if (NavMesh.CalculatePath(BotOwner.Position, trgPos, -1, navMeshPath_0) && navMeshPath_0.status == NavMeshPathStatus.PathComplete)
                    {
                        trgPos = navMeshPath_0.corners[navMeshPath_0.corners.Length - 1];

                        Sphere(trgPos, 0.25f, Color.white, 1f);
                        Line(trgPos, BotOwner.Transform.position, 0.05f, Color.white, 1f);

                        return CheckStraightDistance(navMeshPath_0, num);
                    }
                }
                Sphere(trgPos, 0.15f, Color.yellow, 1f);

                return false;
            }

            private bool CheckStraightDistance(NavMeshPath path, float straighDist)
            {
                return path.CalculatePathLength() < straighDist * 1.2f;
            }

            private NavMeshPath navMeshPath_0 = new NavMeshPath();
            private readonly UpdateTarget updateTarget_0;
            private readonly UpdateMove updateMove_0;

            private class DebugDogFight
            {
                public static void DrawRunAway(BotOwner bot, Vector3 runposition, float linewidth, Color color, float expiretime)
                {
                    Line(runposition, bot.Memory.GoalEnemy.CurrPosition, linewidth, color, expiretime);
                    Line(runposition, bot.Position, linewidth, color, expiretime);
                    Line(bot.Position, bot.Memory.GoalEnemy.CurrPosition, linewidth, color, expiretime);
                }
            }

            public ManualLogSource Logger;

            public class LookToPoint
            {
                public static Vector3 LookToCorner()
                {
                    return Vector3.zero;
                }
            }
        }
    }
}