using BepInEx.Logging;
using EFT;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace SAIN.Components
{
    public class HideComponent : MonoBehaviour
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
            if (SAIN == null)
            {
                Logger.LogError($"SAIN Null");
                return;
            }

            if (SAIN.BotOwner.Memory.GoalEnemy == null)
            {
                if (TakeCoverCoroutine != null)
                {
                    StopCoroutine(TakeCoverCoroutine);
                }

                Enemy = null;

                return;
            }

            var enemy = SAIN.Core.Enemy;
            if (enemy.CanSee)
            {
                Enemy = enemy.CurrentPerson.GetPlayer;

                if (TakeCoverCoroutine == null)
                {
                    TakeCoverCoroutine = StartCoroutine(Hide(Enemy));
                }
            }
        }

        private Player Enemy;

        public LayerMask HidableLayers = LayerMaskClass.HighPolyWithTerrainMask;

        public float HideSensitivity = 0;
        public float MinPlayerDistance = 5f;
        public float MinObstacleHeight = 0.5f;
        public float UpdateFrequency = 0.25f;

        private Coroutine TakeCoverCoroutine;

        private readonly Collider[] Colliders = new Collider[30]; // more is less performant, but more options

        private IEnumerator Hide(Player enemy)
        {
            WaitForSeconds Wait = new WaitForSeconds(UpdateFrequency);

            while (true)
            {
                GetColliders(enemy.Transform.position, out int hits);

                System.Array.Sort(Colliders, ColliderArraySortComparer);

                for (int i = 0; i < hits; i++)
                {
                    if (CheckSides(Colliders[i], enemy.Transform.position, out NavMeshHit PlaceToGo, 3f))
                    {
                        NavMeshPath Path = new NavMeshPath();
                        if (NavMesh.CalculatePath(SAIN.BotOwner.Transform.position, PlaceToGo.position, NavMesh.AllAreas, Path))
                        {
                            if (Path.status == NavMeshPathStatus.PathComplete)
                            {
                                SAIN.BotOwner.GoToPoint(PlaceToGo.position, false);
                                break;
                            }
                        }
                    }
                }
                yield return Wait;
            }
        }

        private void GetColliders(Vector3 enemyPosition, out int hits)
        {
            for (int i = 0; i < Colliders.Length; i++)
            {
                Colliders[i] = null;
            }

            hits = Physics.OverlapSphereNonAlloc(SAIN.BotOwner.Transform.position, 30f, Colliders, HidableLayers);

            int hitReduction = 0;
            for (int i = 0; i < hits; i++)
            {
                if (Vector3.Distance(Colliders[i].transform.position, enemyPosition) < MinPlayerDistance || Colliders[i].bounds.size.y < MinObstacleHeight)
                {
                    Colliders[i] = null;
                    hitReduction++;
                }
            }

            hits -= hitReduction;
        }

        private bool CheckSides(Collider collider, Vector3 enemyPosition, out NavMeshHit placeToGo, float navSampleSize = 2f)
        {
            placeToGo = new NavMeshHit();
            if (NavMesh.SamplePosition(collider.transform.position, out NavMeshHit hit, navSampleSize, NavMesh.AllAreas))
            {
                if (!NavMesh.FindClosestEdge(hit.position, out hit, NavMesh.AllAreas))
                {
                    Logger.LogError($"Unable to find edge close to {hit.position}");
                }

                if (Vector3.Dot(hit.normal, (enemyPosition - hit.position).normalized) < HideSensitivity)
                {
                    placeToGo = hit;
                    return true;
                }
                else
                {
                    if (CheckOppositeSide(hit, collider, enemyPosition, out NavMeshHit hit2))
                    {
                        placeToGo = hit2;
                        return true;
                    }
                }
            }
            else
            {
                Logger.LogError($"Unable to find NavMesh near object {collider.name} at {collider.transform.position}");
            }
            return false;
        }

        private bool CheckOppositeSide(NavMeshHit hit, Collider collider, Vector3 enemyPosition, out NavMeshHit placeToGo, float navSampleSize = 2f)
        {
            if (NavMesh.SamplePosition(collider.transform.position - (enemyPosition - hit.position).normalized * 2, out NavMeshHit hit2, navSampleSize, NavMesh.AllAreas))
            {
                if (!NavMesh.FindClosestEdge(hit2.position, out hit2, NavMesh.AllAreas))
                {
                    Logger.LogError($"Unable to find edge close to {hit2.position} (second attempt)");
                }

                if (Vector3.Dot(hit2.normal, (enemyPosition - hit2.position).normalized) < HideSensitivity)
                {
                    placeToGo = hit2;
                    return true;
                }
            }

            placeToGo = hit;
            return false;
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

        public void Dispose()
        {
            StopAllCoroutines();
            Destroy(this);
        }
    }
}