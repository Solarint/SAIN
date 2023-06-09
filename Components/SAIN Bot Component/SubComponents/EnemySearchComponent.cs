using EFT;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using BepInEx.Logging;
using Comfort.Common;
using SAIN.Helpers;

namespace SAIN.Components
{
    public class EnemySearchComponent : MonoBehaviour
    {
        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            //NavMeshAgent = BotOwner.GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (SAIN == null || BotOwner.IsDead || SAIN.GameIsEnding)
            {
                Dispose();
                return;
            }
            if (!SAIN.BotActive)
            {
                return;
            }

            if (SAIN.AILimited)
            {
                return;
            }

            CheckStuckOrReset();

            CheckShouldSprint();

            if (DistanceToDestination(CurrentDestination) < 6f)
            {
                UpdateTargetPoints();
                if (CornerPeekLoop == null)
                {
                    CornerPeekLoop = StartCoroutine(PeekCorner());
                }
            }

            if (!Peeking)
            {
                Steer(CurrentDestination);
                CornerPeekLoop = null;
                if (BotIsAtPoint(CurrentDestination))
                {
                    GoToNextPoint();
                }
            }
        }

        private void CheckStuckOrReset()
        {
            if ((SAIN.BotStuck.BotIsStuck || !SAIN.BotStuck.BotIsMoving) && SAIN.CurrentTargetPosition != null && ResetTimer < Time.time)
            {
                ResetTimer = Time.time + 1f;
                Init(SAIN.CurrentTargetPosition.Value);
            }

            if (AtFinalDestination && SAIN.CurrentTargetPosition != null)
            {
                Init(SAIN.CurrentTargetPosition.Value);
            }
        }

        private void CheckShouldSprint()
        {
            var pers = SAIN.Info.BotPersonality;
            if (RandomSprintTimer < Time.time && (pers == BotPersonality.GigaChad || pers == BotPersonality.Chad))
            {
                RandomSprintTimer = Time.time + 3f * Random.Range(0.33f, 2f);
                float chance = pers == BotPersonality.GigaChad ? 40f : 20f;
                SprintEnabled = EFT_Math.RandomBool(chance);
            }
        }

        private bool SprintEnabled = false;
        private float RandomSprintTimer = 0f;
        private float ResetTimer = 0f;

        public void Init(Vector3 targetPos)
        {
            AtFinalDestination = false;
            NavMesh.CalculatePath(BotOwner.Transform.position, targetPos, -1, Path);
            FinalDestination = targetPos;
            TotalCornerCount = Path.corners.Length;
            GoToNextPoint();
        }

        private void Steer(Vector3 pos)
        {
            if (Peeking)
            {
                BotOwner.SetTargetMoveSpeed(0.33f);
                BotOwner.SetPose(0.8f);
            }
            else if (SprintEnabled)
            {
                BotOwner.SetTargetMoveSpeed(1f);
                BotOwner.SetPose(1f);
            }
            else
            {
                BotOwner.SetTargetMoveSpeed(0.85f);
                BotOwner.SetPose(0.9f);
            }

            if (SprintEnabled && !BotOwner.Memory.IsUnderFire)
            {
                BotOwner.GetPlayer.EnableSprint(true);
                BotOwner.Steering.LookToMovingDirection();
            }
            else
            {
                BotOwner.GetPlayer.EnableSprint(false);
                float soundDistance = 999f;
                if (SAIN.LastHeardSound != null)
                {
                    soundDistance = Vector3.Distance(SAIN.LastHeardSound.Position, BotOwner.Position);
                }
                if (BotOwner.Memory.IsUnderFire)
                {
                    SAIN.Steering.ManualUpdate();
                }
                else if (soundDistance < 30f && SAIN.LastHeardSound.TimeSinceHeard < 1f)
                {
                    SAIN.Steering.ManualUpdate();
                }
                else
                {
                    pos.y += 1f;
                    BotOwner.Steering.LookToPoint(pos);
                }
            }
        }

        private void UpdateTargetPoints()
        {
            CornerA = CurrentDestination;

            if (BotOwner.Memory.GoalEnemy != null)
            {
                CornerB = BotOwner.Memory.GoalEnemy.CurrPosition;
            }
            else
            {
                CornerB = FinalDestination;
            }
        }

         private IEnumerator PeekCorner()
        {
            while (true)
            {
                if (SAIN.BotActive)
                {
                    Peeking = true;

                    Vector3 corner = CornerA;
                    Vector3 nextCorner = CornerB;
                    Vector3 FinalDest = FinalDestination;

                    if (PeekMoveDestination == Vector3.zero)
                    {
                        PeekTimer = Time.time + 3f;

                        Vector3 PeekDestination;
                        if (DistanceToDestination(FinalDest) < 15f || Vector3.Distance(FinalDest, corner) < 5f)
                        {
                            var directionToDestination = FinalDest - BotOwner.Transform.position;

                            float angle = GetSignedAngle(FinalDest, corner, BotOwner.Transform.position);
                            float rotationAngle = angle > 0 ? -90f : 90f;

                            var direction = Quaternion.Euler(0f, rotationAngle, 0f) * directionToDestination;

                            PeekDestination = FinalDest + direction;
                        }
                        else
                        {
                            var directionBetweenCorners = nextCorner - corner;
                            PeekDestination = corner - (directionBetweenCorners.normalized * 2f);
                        }

                        Vector3 peekDestination;
                        if (NavMesh.SamplePosition(PeekDestination, out var hit, 5f, -1))
                        {
                            peekDestination = hit.position;
                        }
                        else
                        {
                            var random = Random.onUnitSphere * 8f;
                            random.y = 0f;

                            if (NavMesh.SamplePosition(corner + random, out var hit2, 5f, -1))
                            {
                                peekDestination = hit2.position;
                            }
                            else
                            {
                                peekDestination = corner;
                            }
                        }

                        PeekMoveDestination = peekDestination;
                        BotOwner.GoToPoint(peekDestination, false, 0.5f, false, false);
                        BotOwner.DoorOpener.Update();
                    }

                    Steer(FinalDest);
                    BotOwner.SetTargetMoveSpeed(0.33f);
                    BotOwner.SetPose(0.8f);

                    if (BotIsAtPoint(PeekMoveDestination) || (PeekTimer < Time.time && !BotOwner.Mover.IsMoving) || SAIN.BotStuck.BotIsStuck)
                    {
                        PeekMoveDestination = Vector3.zero;
                        //BotOwner.GetPlayer.MovementContext.SetTilt(0f, false);
                        Peeking = false;
                        GoToNextPoint();
                        break;
                    }
                }

                if (SAIN.AILimited)
                {
                    yield return new WaitForSeconds(SAIN.AILimitTimeAdd);
                }

                yield return null;
            }
        }

        private float GetSignedAngle(Vector3 a, Vector3 b, Vector3 origin)
        {
            var directionToNextCorner = a - origin;
            var directionToCorner = b - origin;
            return Vector3.SignedAngle(directionToNextCorner.normalized, directionToCorner.normalized, Vector3.up);
        }

        private bool BotIsAtPoint(Vector3 point, float reachDist = 1f)
        {
            return DistanceToDestination(point) < reachDist;
        }

        private float DistanceToDestination(Vector3 point)
        {
            return Vector3.Distance(point, BotOwner.Transform.position);
        }

        private void GoToNextPoint()
        {
            CurrentCorner++;
            if (TotalCornerCount - 1 >= CurrentCorner)
            {
                BotOwner.SetTargetMoveSpeed(0.85f);
                BotOwner.SetPose(1f);
                CurrentDestination = Path.corners[CurrentCorner];
                BotOwner.GoToPoint(CurrentDestination, false, 0.5f, false, false);
                BotOwner.DoorOpener.Update();
            }
            else
            {
                AtFinalDestination = true;
            }
        }

        public void Dispose()
        {
            StopAllCoroutines();
            Destroy(this);
        }

        private BotOwner BotOwner => SAIN.BotOwner;

        private SAINComponent SAIN;

        private NavMeshAgent NavMeshAgent;

        protected ManualLogSource Logger;

        public NavMeshPath Path = new NavMeshPath();

        public Vector3 CurrentDestination;

        public Vector3 FinalDestination;

        private int CurrentCorner = 0;

        private int TotalCornerCount = 0;

        public float DistanceToNextCorner = 0f;

        public bool Peeking { get; private set; }

        public bool Moving { get; private set; }

        public bool AtFinalDestination { get; private set; }

        private Vector3 CornerA;

        private Vector3 CornerB;

        private Vector3 PeekMoveDestination = Vector3.zero;

        private Coroutine CornerPeekLoop;

        private float PeekTimer = 0f;
    }
}
