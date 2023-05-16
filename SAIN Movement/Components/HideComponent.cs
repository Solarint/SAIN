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
        public float MinObstacleHeight = 1.55f;
        public float UpdateFrequency = 0.25f;

        private Coroutine TakeCoverCoroutine;

        private Collider[] Colliders = new Collider[50]; // more is less performant, but more options

        private IEnumerator Hide(Player enemy)
        {
            WaitForSeconds Wait = new WaitForSeconds(UpdateFrequency);

            while (true)
            {
                for (int i = 0; i < Colliders.Length; i++)
                {
                    Colliders[i] = null;
                }

                int hits = Physics.OverlapSphereNonAlloc(SAIN.BotOwner.Transform.position, 30f, Colliders, HidableLayers);

                int hitReduction = 0;
                for (int i = 0; i < hits; i++)
                {
                    if (Vector3.Distance(Colliders[i].transform.position, enemy.Transform.position) < MinPlayerDistance || Colliders[i].bounds.size.y < MinObstacleHeight)
                    {
                        Colliders[i] = null;
                        hitReduction++;
                    }
                }

                hits -= hitReduction;

                System.Array.Sort(Colliders, ColliderArraySortComparer);

                for (int i = 0; i < hits; i++)
                {
                    if (NavMesh.SamplePosition(Colliders[i].transform.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                    {
                        if (!NavMesh.FindClosestEdge(hit.position, out hit, NavMesh.AllAreas))
                        {
                            Logger.LogError($"Unable to find edge close to {hit.position}");
                        }

                        if (Vector3.Dot(hit.normal, (enemy.Transform.position - hit.position).normalized) < HideSensitivity)
                        {
                            NavMeshPath Path = new NavMeshPath();
                            if (NavMesh.CalculatePath(SAIN.BotOwner.Transform.position, hit.position, NavMesh.AllAreas, Path) && Path.status == NavMeshPathStatus.PathComplete)
                            {
                                SAIN.BotOwner.GoToPoint(hit.position, false);
                                break;
                            }
                        }
                        else
                        {
                            // Since the previous spot wasn't facing "away" enough from teh target, we'll try on the other side of the object
                            if (NavMesh.SamplePosition(Colliders[i].transform.position - (enemy.Transform.position - hit.position).normalized * 2, out NavMeshHit hit2, 2f, NavMesh.AllAreas))
                            {
                                if (!NavMesh.FindClosestEdge(hit2.position, out hit2, NavMesh.AllAreas))
                                {
                                    Logger.LogError($"Unable to find edge close to {hit2.position} (second attempt)");
                                }

                                if (Vector3.Dot(hit2.normal, (enemy.Transform.position - hit2.position).normalized) < HideSensitivity)
                                {
                                    NavMeshPath Path = new NavMeshPath();
                                    if (NavMesh.CalculatePath(SAIN.BotOwner.Transform.position, hit2.position, NavMesh.AllAreas, Path) && Path.status == NavMeshPathStatus.PathComplete)
                                    {
                                        SAIN.BotOwner.GoToPoint(hit2.position, false);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Logger.LogError($"Unable to find NavMesh near object {Colliders[i].name} at {Colliders[i].transform.position}");
                    }
                }
                yield return Wait;
            }
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