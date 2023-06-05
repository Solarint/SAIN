using EFT;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using BepInEx.Logging;
using Comfort.Common;

namespace SAIN.Components
{
    public class EnemySearchComponent : MonoBehaviour
    {
        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();

            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);

            NavMeshAgent = BotOwner.GetComponent<NavMeshAgent>();

            AtFinalDestination = false;
        }

        private void Update()
        {
            if (BotOwner.IsDead || SAIN.GameIsEnding)
            {
                Dispose();
                return;
            }

            if (!SAIN.BotActive)
            {
                return;
            }

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
                float soundDistance = Vector3.Distance(SAIN.LastSoundHeardPosition, BotOwner.Position);
                float destinationDistance = Vector3.Distance(CurrentDestination, BotOwner.Position);

                if (ShouldUpdateSteer || (soundDistance < destinationDistance && SAIN.LastSoundHeardTime > Time.time - 2f))
                {
                    BotOwner.LookData.SetLookPointByHearing();
                }
                else
                {
                    BotOwner.Steering.LookToMovingDirection();
                }

                if (CornerPeekLoop != null)
                {
                    CornerPeekLoop = null;
                }

                if (BotIsAtPoint(CurrentDestination))
                {
                    GoToNextPoint();
                }
            }
        }

        public void Init(Vector3 targetPos)
        {
            NavMesh.CalculatePath(BotOwner.Transform.position, targetPos, -1, Path);
            FinalDestination = targetPos;
            TotalCornerCount = Path.corners.Length;
            
            GoToNextPoint();
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
                        PeekTimer = Time.time + 8f;

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
                        BotOwner.GoToPoint(peekDestination, false, 0.15f, false, false);
                    }

                    float soundDistance = Vector3.Distance(SAIN.LastSoundHeardPosition, BotOwner.Position);
                    float peekMoveDistance = Vector3.Distance(PeekMoveDestination, BotOwner.Position);
                    float finalDestDistance = Vector3.Distance(FinalDest, BotOwner.Position);

                    if (ShouldUpdateSteer || (soundDistance < peekMoveDistance || soundDistance < finalDestDistance) && SAIN.LastSoundHeardTime > Time.time - 2f)
                    {
                        BotOwner.LookData.SetLookPointByHearing();
                    }
                    else
                    {
                        BotOwner.Steering.LookToPoint(FinalDest);
                    }

                    BotOwner.SetTargetMoveSpeed(0.4f);

                    if (BotIsAtPoint(PeekMoveDestination) || (PeekTimer < Time.time && !BotOwner.Mover.IsMoving))
                    {
                        PeekMoveDestination = Vector3.zero;
                        //BotOwner.GetPlayer.MovementContext.SetTilt(0f, false);
                        Peeking = false;
                        GoToNextPoint();
                        break;
                    }

                    BotOwner.DoorOpener.Update();
                }

                yield return new WaitForEndOfFrame();
            }
        }

        private bool ShouldUpdateSteer => BotOwner.Memory.IsUnderFire || SAIN.Enemies.PriorityEnemy != null;

        private float GetSignedAngle(Vector3 a, Vector3 b, Vector3 origin)
        {
            var directionToNextCorner = a - origin;
            var directionToCorner = b - origin;
            return Vector3.SignedAngle(directionToNextCorner.normalized, directionToCorner.normalized, Vector3.up);
        }

        private bool BotIsAtPoint(Vector3 point, float reachDist = 0.5f)
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
                CurrentDestination = Path.corners[CurrentCorner];
                BotOwner.GoToPoint(CurrentDestination, false, 0.15f, false, false);
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
