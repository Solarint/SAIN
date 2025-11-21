using SAIN.Helpers;
using SAIN.SAINComponent;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Components.PlayerComponentSpace;

public class ShootPlayerClass : PlayerComponentBase
{
    public ShootPlayerClass(PlayerComponent component) : base(component)
    {

    }

    public readonly List<Vector3> PlacesToShootMe = new();

    public sealed class FindPlacesToShootParameters
    {
        public float minPointDist = 5f;
        public float maxPointDist = 300f;
        public int iterationMax = 100;
        public int successMax = 5;
        public float yVal = 0.25f;
        public float navSampleRange = 0.25f;
        public float downDirDist = 10f;
    }

    public IEnumerator FindPlaceToShoot(FindPlacesToShootParameters parameters)
    {
        yield return null;
    }

    public void FindPlacesToShoot(List<Vector3> places, Vector3 directionToBot, FindPlacesToShootParameters parameters)
    {
        float minPointDist = parameters.minPointDist;
        float maxPointDist = parameters.maxPointDist;
        int iterationMax = parameters.iterationMax;
        int successMax = parameters.successMax;
        float yVal = parameters.yVal;
        float navSampleRange = parameters.navSampleRange;
        float downDirDist = parameters.downDirDist;

        LayerMask mask = LayerMaskClass.HighPolyWithTerrainMask;

        int successCount = 0;
        places.Clear();
        for (int i = 0; i < iterationMax; i++)
        {
            Vector3 start = Transform.EyePosition;
            Vector3 randomDirection = UnityEngine.Random.onUnitSphere;
            randomDirection.y = UnityEngine.Random.Range(-yVal, yVal);
            float distance = UnityEngine.Random.Range(minPointDist, maxPointDist);

            if (!Physics.Raycast(start, randomDirection, distance, mask))
            {
                Vector3 openPoint = start + randomDirection * distance;

                if (Physics.Raycast(openPoint, Vector3.down, out var rayHit2, downDirDist, mask)
                    && (rayHit2.point - start).sqrMagnitude > minPointDist * minPointDist
                    && NavMesh.SamplePosition(rayHit2.point, out var navHit2, navSampleRange, -1))
                {
                    DebugGizmos.DrawSphere(navHit2.position, 0.1f, Color.blue, 3f);
                    places.Add(navHit2.position);
                    successCount++;
                }
            }
            if (successCount >= successMax)
            {
                break;
            }
        }
    }
}