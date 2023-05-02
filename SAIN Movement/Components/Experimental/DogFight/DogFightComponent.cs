using BepInEx.Logging;
using EFT;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static SAIN_Helpers.SAIN_Math;
using Movement.Helpers;
using SAIN_Helpers;

namespace Movement.Components
{
    public class DogFightComponent : MonoBehaviour
    {
        private void Awake()
        {
            botOwner_0 = GetComponent<BotOwner>();

            string loggerName = "SAIN-DogFight: " + botOwner_0.name;
            Logger = BepInEx.Logging.Logger.CreateLogSource(loggerName);

            DogFightState = BotDogFightStatus.none;

            StartCoroutine(DogFightLoop());
        }

        private BotOwner botOwner_0;

        private static readonly float DogFightIn = 25f;
        private static readonly float DogFightOut = 30f;

        public BotDogFightStatus DogFightState { get; private set; }

        public bool StartDogFight()
        {
            var goalEnemy = botOwner_0.Memory.GoalEnemy;
            if (goalEnemy != null && goalEnemy.IsVisible && goalEnemy.Distance < DogFightIn && CheckEnemyDistance(out Vector3 vector))
            {
                DogFightState = BotDogFightStatus.dogFight;
                return true;
            }
            return false;
        }

        public IEnumerator DogFightLoop()
        {
            while (true)
            {
                if (botOwner_0?.Memory?.GoalEnemy == null)
                {
                    if (DogFightState != BotDogFightStatus.none)
                    {
                        DogFightState = BotDogFightStatus.none;
                    }
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }

                if (botOwner_0.Memory.BotCurrentCoverInfo.UseDogFight(3f))
                {
                    if (DogFightState != BotDogFightStatus.dogFight)
                    {
                        DogFightState = BotDogFightStatus.dogFight;
                    }
                    Fight();
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }

                switch (DogFightState)
                {
                    case BotDogFightStatus.none:
                        StatusNone();
                        break;

                    case BotDogFightStatus.dogFight:
                        StatusDogFight();
                        break;

                    case BotDogFightStatus.shootFromPlace:
                        StatusShootFromPlace();
                        break;

                    default:
                        break;
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        private void StatusNone()
        {
            if (botOwner_0.Memory.GoalEnemy.IsVisible && botOwner_0.Memory.GoalEnemy.Distance < DogFightIn)
            {
                if (CheckEnemyDistance(out Vector3 vector))
                {
                    Fight();
                    DogFightState = BotDogFightStatus.dogFight;
                    return;
                }

                Fight();
                DogFightState = BotDogFightStatus.shootFromPlace;
            }
        }

        private void StatusDogFight()
        {
            if (botOwner_0.Memory.GoalEnemy.Distance > DogFightOut)
            {
                DogFightState = BotDogFightStatus.none;
                return;
            }
            Fight();
        }

        private void StatusShootFromPlace()
        {
            if (CheckEnemyDistance(out Vector3 vector) || botOwner_0.Memory.GoalEnemy.Distance > DogFightOut)
            {
                DogFightState = BotDogFightStatus.none;
                return;
            }

            Fight();
        }


        public void Fight()
        {
            if (botOwner_0.IsDead)
                return;

            if (botOwner_0.BewareGrenade?.GrenadeDangerPoint?.Grenade != null)
                return;

            if (ReactionTimer < Time.time)
            {
                var goalEnemy = botOwner_0.Memory.GoalEnemy;

                if (goalEnemy == null)
                {
                    MovingToEnemy = false;
                    return;
                }

                ReactionTimer = Time.time + 0.25f;
                if (!MovingToEnemy)
                {
                    if (goalEnemy.IsVisible && DodgeTimer < Time.time)
                    {
                        DodgeTimer = Time.time + 1f;
                        botOwner_0.Mover.SetTargetMoveSpeed(0.75f);
                        botOwner_0.SetPose(1f);
                        return;
                    }

                    if (CheckEnemyDistance(out Vector3 position))
                    {
                        botOwner_0.Mover.SetTargetMoveSpeed(1f);
                        botOwner_0.SetPose(1f);
                        botOwner_0.GoToPoint(position, true, -1f, false, true, true);
                        return;
                    }
                }

                if (goalEnemy.CanShoot)
                {
                    MovingToEnemy = false;
                    DogFightState = BotDogFightStatus.shootFromPlace;
                    return;
                }

                MovingToEnemy = true;

                if (botOwner_0.Memory.GoalEnemy.Distance > 10f)
                {
                    if (!botOwner_0.EnemyLookData.IsEnemyLookAtMeForPeriod(1f))
                    {
                        botOwner_0.Mover.SetTargetMoveSpeed(0.25f);
                        botOwner_0.SetPose(0.25f);
                    }
                    else
                    {
                        botOwner_0.Mover.SetTargetMoveSpeed(0.5f);
                        botOwner_0.SetPose(0.75f);
                    }
                }

                botOwner_0.MoveToEnemyData.TryMoveToEnemy(goalEnemy.CurrPosition);
            }
        }

        public bool CheckEnemyDistance(out Vector3 trgPos)
        {
            Vector3 a = -NormalizeFastSelf(botOwner_0.Memory.GoalEnemy.Direction);

            trgPos = Vector3.zero;

            float num = 0f;
            if (NavMesh.SamplePosition(botOwner_0.Position + a * 2f / 2f, out NavMeshHit navMeshHit, 1f, -1))
            {
                trgPos = navMeshHit.position;

                Vector3 a2 = trgPos - botOwner_0.Position;

                float magnitude = a2.magnitude;

                if (magnitude != 0f)
                {
                    Vector3 a3 = a2 / magnitude;

                    num = magnitude;

                    if (NavMesh.SamplePosition(botOwner_0.Position + a3 * 2f, out navMeshHit, 1f, -1))
                    {
                        trgPos = navMeshHit.position;

                        DebugDrawer.Sphere(trgPos, 0.15f, Color.yellow, 1f);

                        num = (trgPos - botOwner_0.Position).magnitude;
                    }
                }
            }
            if (num != 0f && num > botOwner_0.Settings.FileSettings.Move.REACH_DIST)
            {
                navMeshPath_0.ClearCorners();

                if (NavMesh.CalculatePath(botOwner_0.Position, trgPos, -1, navMeshPath_0) && navMeshPath_0.status == NavMeshPathStatus.PathComplete)
                {
                    trgPos = navMeshPath_0.corners[navMeshPath_0.corners.Length - 1];

                    DebugDrawer.Sphere(trgPos, 0.25f, Color.white, 1f);
                    DebugDrawer.Line(trgPos, botOwner_0.Transform.position, 0.05f, Color.white, 1f);

                    return CheckStraightDistance(navMeshPath_0, num);
                }
            }
            return false;
        }

        private bool CheckStraightDistance(NavMeshPath path, float straighDist)
        {
            return path.CalculatePathLength() < straighDist * 1.2f;
        }

        private readonly NavMeshPath navMeshPath_0 = new NavMeshPath();
        protected static ManualLogSource Logger { get; private set; }

        private static float ReactionTimer;
        private static float ShootTime;
        private static bool MovingToEnemy;
        private static float DodgeTimer = 0f;

        public IEnumerator Dispose()
        {
            StopAllCoroutines();
            yield break;
        }
    }
}