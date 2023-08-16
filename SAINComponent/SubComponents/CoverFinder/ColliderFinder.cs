using UnityEngine;
using System;
using EFT;

namespace SAIN.SAINComponent.SubComponents.CoverFinder
{
    public class ColliderFinder
    {
        private Vector3 OriginPoint;
        private Vector3 TargetPoint;

        public void GetNewColliders(out int hits, Vector3 origin, Vector3 target, Collider[] array, float boxWidth = 25f, float boxHeight = 3f)
        {
            OriginPoint = origin;
            TargetPoint = target;

            ClearColliders(array);

            var mask = LayerMaskClass.HighPolyWithTerrainMask;
            var orientation = Quaternion.identity;
            Vector3 box = new Vector3(boxWidth, boxHeight, boxWidth);

            hits = Physics.OverlapBoxNonAlloc(origin, box, array, orientation, mask);
            hits = FilterColliders(array, hits);
        }

        private void ClearColliders(Collider[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = null;
            }
        }

        /// <summary>
        /// Sorts an array of Colliders based on their Distance from bot's DrawPosition. 
        /// </summary>
        /// <param value="array">The array of Colliders to be sorted.</param>
        public void SortArrayBotDist(Collider[] array)
        {
            Array.Sort(array, ColliderArrayBotDistComparer);
        }

        /// <summary>
        /// Sorts an array of Colliders based on their Distance to the enemy.
        /// </summary>
        /// <param value="array">The array of Colliders to be sorted.</param>
        public void SortArrayEnemyDist(Collider[] array)
        {
            Array.Sort(array, ColliderArrayEnemyDistComparer);
        }

        /// <summary>
        /// Sorts an array of Colliders by their transform optionHeight. 
        /// </summary>
        /// <param value="array">The array of Colliders to be sorted.</param>
        public void SortArrayHeight(Collider[] array)
        {
            Array.Sort(array, ColliderArrayHeightComparer);
        }

        private int FilterColliders(Collider[] array, int hits)
        {
            int hitReduction = 0;
            for (int i = 0; i < hits; i++)
            {
                if (array[i].bounds.size.y < 0.66)
                {
                    array[i] = null;
                    hitReduction++;
                }
                else if (array[i].bounds.size.x < 0.1f && array[i].bounds.size.z < 0.1f)
                {
                    array[i] = null;
                    hitReduction++;
                }
            }
            hits -= hitReduction;
            return hits;
        }

        public int ColliderArrayBotDistComparer(Collider A, Collider B)
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
                float AMag = (OriginPoint - A.transform.position).sqrMagnitude;
                float BMag = (OriginPoint - B.transform.position).sqrMagnitude;
                return AMag.CompareTo(BMag);
            }
        }

        public int ColliderArrayEnemyDistComparer(Collider A, Collider B)
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
                float AMag = (TargetPoint - A.transform.position).sqrMagnitude;
                float BMag = (TargetPoint - B.transform.position).sqrMagnitude;
                return AMag.CompareTo(BMag);
            }
        }

        public int ColliderArrayHeightComparer(Collider A, Collider B)
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
                return A.transform.position.y.CompareTo(B.transform.position.y);
            }
        }
    }
}