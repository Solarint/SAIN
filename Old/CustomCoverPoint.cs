using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Helpers
{
    public class CustomCoverPoint
    {
        public CustomCoverPoint(Vector3 startPosition, Vector3 coverPosition, float coverLevel, bool canShoot)
        {
            CoverPosition = coverPosition;
            CoverLevel = coverLevel;
            CanShoot = canShoot;
            CalculatePath(startPosition, coverPosition);
        }

        private void CalculatePath(Vector3 startPosition, Vector3 coverPosition)
        {
            NavMeshPath = new NavMeshPath();
            NavMesh.CalculatePath(startPosition, coverPosition, -1, NavMeshPath);
            CoverDistance = NavMeshPath.CalculatePathLength();
        }

        public NavMeshPath NavMeshPath { get; set; }
        public Vector3 CoverPosition { get; set; }
        public float CoverDistance { get; set; }
        public float CoverLevel { get; set; }
        public bool CanShoot { get; set; } 
    }

    public class CoverMonitor : MonoBehaviour
    {
        private void Awake()
        {

        }
    }
}