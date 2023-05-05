using BepInEx.Logging;
using EFT;
using HarmonyLib;
using SAIN_Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Movement.Helpers
{
    public class CustomCoverPoint
    {
        public CustomCoverPoint(Vector3 point, float coverLevel, float distance, NavMeshPath Path)
        {
            CoverPosition = point;
            CoverLevel = coverLevel;
            CoverDistance = distance;
            NavMeshPath = Path;
        }
        public NavMeshPath NavMeshPath { get; set; }
        public Vector3 CoverPosition { get; set; }
        public float CoverDistance { get; set; }
        public float CoverLevel { get; set; }
    }

    public class CoverMonitor : MonoBehaviour
    {
        private void Awake()
        {

        }
    }
}