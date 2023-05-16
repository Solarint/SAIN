using BepInEx.Logging;
using EFT;
using SAIN_Helpers;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.UserSettings.CoverConfig;

namespace SAIN.Components
{
    public class CoverComponent : MonoBehaviour
    {
        private SAINComponent SAIN;
        protected ManualLogSource Logger;

        private void Awake()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = GetComponent<SAINComponent>();
        }

        private void Update()
        {
            if (SAIN.BotOwner.Memory.GoalEnemy == null)
            {
                StopLooking();
                return;
            }
            else
            {
                LookForCover();
            }

            var enemy = SAIN.Core.Enemy;
            EnemyPosition = enemy.LastSeen.EnemyPosition;

            if (SAIN.FallingBack)
            {
                MinObstacleHeight = 1.55f;
            }
            else
            {
                MinObstacleHeight = CoverMinHeight.Value;
            }

            bool SeekCover = true;
            if (enemy.CanSee)
            {
                AtEnemyPos = false;

                if (SAIN.InCover && enemy.CanShoot)
                {
                    SeekCover = false;
                }
            }
            else
            {
                if (SAIN.InCover)
                {
                    SeekCover = false;
                }
            }

            if (SeekCover)
            {
                MoveToPoint();
            }
            else
            {
                if (SAIN.Core.Enemy.LastSeen.TimeSinceSeen > 10f)
                {
                    SAIN.BotOwner.MovementResume();
                    if (Vector3.Distance(SAIN.BotOwner.Transform.position, EnemyPosition) < 2f)
                    {
                        AtEnemyPos = true;
                        SAIN.BotOwner.MoveToEnemyData.TryMoveToEnemy(SAIN.BotOwner.Memory.GoalEnemy.CurrPosition);
                    }
                    else if (!AtEnemyPos)
                    {
                        SAIN.BotOwner.MoveToEnemyData.TryMoveToEnemy(EnemyPosition);
                    }
                }
                else
                {
                    SAIN.BotOwner.MovementPause(2f);
                    HoldInCover();
                }
            }
        }

        private bool AtEnemyPos = false;

        private void HoldInCover()
        {
            SAIN.BotOwner.Steering.LookToPoint(EnemyPosition);
        }

        Vector3 EnemyPosition;

        private void Old()
        {
            var enemy = SAIN.Core.Enemy;
            if (enemy.CanSee)
            {
                if (SAIN.FallingBack)
                {
                    LookForCover();
                }
                else
                {
                    if (SAIN.InCover && enemy.CanShoot)
                    {
                        SAIN.BotOwner.MovementPause(1f);
                        return;
                    }
                    else
                    {
                        LookForCover();
                    }
                }
            }
            else
            {
                if (!SAIN.FallingBack)
                {
                    StopLooking();
                }
            }
        }

        private void MoveToPoint()
        {
            SAIN.BotOwner.MovementResume();

            if (TargetPosition != null)
            {
                DrawDebug();

                if (SAIN.FallingBack)
                {
                    SAIN.MovementLogic.SetSprint(true);
                    SAIN.BotOwner.Steering.LookToMovingDirection();
                    SAIN.BotOwner.GoToPoint(TargetPosition.Value, false);
                }
                else
                {
                    SAIN.MovementLogic.SetSprint(false);
                    SAIN.MovementLogic.DecideMovementSpeed();
                    SAIN.BotOwner.GoToPoint(TargetPosition.Value, true);
                }

                UpdateDoorOpener();
            }
        }

        private void DrawDebug()
        {
            if (DebugTimer < Time.time)
            {
                DebugTimer = Time.time + 0.5f;
                DebugDrawer.Line(SAIN.BotOwner.MyHead.position, TargetPosition.Value, 0.05f, Color.magenta, 0.5f);
                NavMeshPath path = new NavMeshPath();
                if (NavMesh.CalculatePath(SAIN.BotOwner.Transform.position, TargetPosition.Value, NavMesh.AllAreas, path))
                {
                    SAIN.DebugDrawList.DrawTempPath(path, true, Color.magenta, Color.magenta, 0.1f, 0.5f, false);
                }
            }
        }

        private float DebugTimer = 0f;

        private void LookForCover()
        {
            if (TakeCoverCoroutine == null)
            {
                TakeCoverCoroutine = StartCoroutine(FindCover());
            }
        }

        private void StopLooking()
        {
            if (TakeCoverCoroutine != null)
            {
                StopCoroutine(TakeCoverCoroutine);
                TakeCoverCoroutine = null;
            }
        }

        public LayerMask HidableLayers = LayerMaskClass.HighPolyWithTerrainNoGrassMask;

        public float HideSensitivity = CoverHideSensitivity.Value;
        public float MinPlayerDistance = CoverMinPlayerDistance.Value;
        public float MinObstacleHeight = CoverMinHeight.Value;
        public float UpdateFrequency = CoverUpdateFrequency.Value;

        private Vector3? TargetPosition;
        private Coroutine TakeCoverCoroutine;
        private readonly Collider[] Colliders = new Collider[CoverColliderCount.Value]; // more is less performant, but more options

        private float RadiusAdd = 0f;
        private float minDistSub = 0f;

        private IEnumerator FindCover()
        {
            WaitForSeconds Wait = new WaitForSeconds(UpdateFrequency);

            while (true)
            {
                GetColliders(out int hits, CoverColliderRadius.Value + RadiusAdd, MinPlayerDistance - minDistSub);

                System.Array.Sort(Colliders, ColliderArraySortComparer);

                bool foundCover = false;
                for (int i = 0; i < hits; i++)
                {
                    if (CheckSides(Colliders[i], out NavMeshHit PlaceToGo, CoverNavSampleDistance.Value))
                    {
                        NavMeshPath Path = new NavMeshPath();
                        if (NavMesh.CalculatePath(SAIN.BotOwner.Transform.position, PlaceToGo.position, NavMesh.AllAreas, Path))
                        {
                            if (Path.status == NavMeshPathStatus.PathComplete)
                            {
                                foundCover = true;
                                TargetPosition = PlaceToGo.position;
                                break;
                            }
                        }
                    }
                }

                if (foundCover)
                {
                    minDistSub = 0f;
                    RadiusAdd = 0f;
                }
                else
                {
                    if (MinPlayerDistance - minDistSub > 3f)
                    {
                        minDistSub -= 1f;
                    }
                    RadiusAdd += 5f;
                }

                yield return new WaitForSeconds(UpdateFrequency);
            }
        }

        private void GetColliders(out int hits, float maxRange = 20f, float minPlayerDist = 10f)
        {
            for (int i = 0; i < Colliders.Length; i++)
            {
                Colliders[i] = null;
            }

            hits = Physics.OverlapSphereNonAlloc(SAIN.BotOwner.Transform.position, maxRange, Colliders, HidableLayers);

            int hitReduction = 0;
            for (int i = 0; i < hits; i++)
            {
                Vector3 toCollider = Colliders[i].transform.position - EnemyPosition;
                Vector3 toBot = SAIN.BotOwner.Transform.position - EnemyPosition;

                bool behindEnemy = Vector3.Dot(toCollider.normalized, toBot.normalized) < 0;
                bool tooShort = Colliders[i].bounds.size.y < MinObstacleHeight;
                //bool tooTall = Colliders[i].bounds.size.y > MaxObstacleHeight;
                bool tooClose = Vector3.Distance(Colliders[i].transform.position, EnemyPosition) < minPlayerDist;

                if (tooClose || tooShort || behindEnemy)
                {
                    Colliders[i] = null;
                    hitReduction++;
                }
            }

            hits -= hitReduction;
        }

        private bool CheckSides(Collider collider, out NavMeshHit placeToGo, float navSampleSize = 2f)
        {
            placeToGo = new NavMeshHit();
            if (NavMesh.SamplePosition(collider.transform.position, out NavMeshHit hit, navSampleSize, NavMesh.AllAreas))
            {
                if (!NavMesh.FindClosestEdge(hit.position, out hit, NavMesh.AllAreas))
                {
                    Logger.LogError($"Unable to find edge close to {hit.position}");
                }

                if (Vector3.Dot(hit.normal, (EnemyPosition - hit.position).normalized) < HideSensitivity)
                {
                    if (CheckRayCast(hit.position))
                    {
                        placeToGo = hit;
                        return true;
                    }
                }
                else
                {
                    if (CheckOppositeSide(hit, collider, out NavMeshHit hit2, navSampleSize))
                    {
                        placeToGo = hit2;
                        return true;
                    }
                }
            }
            else
            {
                //Logger.LogError($"Unable to find NavMesh near object {collider.name} at {collider.transform.position}");
            }
            return false;
        }

        private bool CheckOppositeSide(NavMeshHit hit, Collider collider, out NavMeshHit placeToGo, float navSampleSize = 2f)
        {
            if (NavMesh.SamplePosition(collider.transform.position - (EnemyPosition - hit.position).normalized * 2, out NavMeshHit hit2, navSampleSize, NavMesh.AllAreas))
            {
                if (!NavMesh.FindClosestEdge(hit2.position, out hit2, NavMesh.AllAreas))
                {
                    Logger.LogError($"Unable to find edge close to {hit2.position} (second attempt)");
                }

                if (Vector3.Dot(hit2.normal, (EnemyPosition - hit2.position).normalized) < HideSensitivity)
                {
                    if (CheckRayCast(hit2.position))
                    {
                        placeToGo = hit2;
                        return true;
                    }
                }
            }

            placeToGo = hit;
            return false;
        }

        private bool CheckRayCast(Vector3 point)
        {
            Vector3 enemyHead = SAIN.Core.Enemy.EnemyHeadPosition;
            Vector3 rayPoint = point;
            rayPoint.y += 0.4f;
            Vector3 direction = rayPoint - enemyHead;

            return Physics.Raycast(enemyHead, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask);
        }

        public int ColliderArraySortComparer(Collider A, Collider B)
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
                return Vector3.Distance(SAIN.BotOwner.Transform.position, A.transform.position).CompareTo(Vector3.Distance(SAIN.BotOwner.Transform.position, B.transform.position));
            }
        }

        private void UpdateDoorOpener()
        {
            SAIN.BotOwner.DoorOpener.Update();
        }

        public void Dispose()
        {
            StopAllCoroutines();
            Destroy(this);
        }
    }
}