using EFT;
using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.SAINComponent.Classes.Mover
{
    public class SAINVaultClass : SAINBase, ISAINClass
    {
        public SAINVaultClass(SAINComponentClass sain) : base(sain)
        {
        }

        public static readonly List<SAINVaultPoint> GlobalVaultPoints = new List<SAINVaultPoint>();

        public void Init()
        {
        }

        public void Update()
        {
            TryVaulting();
        }

        public static void DebugCheckObstacles(Player player)
        {
            SpherecastCheck(player, player.WeaponRoot.position, player.LookDirection, out SAINVaultPoint notUsed, 5f);
        }

        public static void DebugVaultPointCount()
        {
            if (DebugTimer < Time.time && GlobalVaultPoints != null && GlobalVaultPoints.Count > 0)
            {
                DebugTimer = Time.time + 3f;
                Logger.LogDebug(GlobalVaultPoints.Count);
            }
        }

        private static float DebugTimer = 0;

        public SAINVaultPoint CurrentVaultPoint;
        public List<SAINVaultPoint> VaultPointHistory = new List<SAINVaultPoint>();

        public bool FindVaultPoint(NavMeshPath path, out SAINVaultPoint vaultPoint)
        {
            if (FindVaultPoint(Player, path, out vaultPoint))
            {
                CurrentVaultPoint = vaultPoint;
                VaultPointHistory.Add(vaultPoint);
                return true;
            }
            return false;
        }

        public static bool FindVaultPoint(Player player, NavMeshPath path, out SAINVaultPoint point)
        {
            point = null;
            if (path.corners.Length < 3)
            {
                return false;
            }

            int totalChecks = 0;
            Vector3[] corners = path.corners;
            int max = corners.Length - 1;

            bool foundPoint = false;

            // Loop through the corners in a navmesh path to see if obstacles between them can be vaulted over. Checks the 3rd point in a sequence, so it looks for shortcuts between triangles.
            for (int i = 0; i < max - 2; i++)
            {
                Vector3 start = corners[i];

                for (int j = i + 2; j < max; j++)
                {
                    totalChecks++;

                    Vector3 end = corners[j];
                    if (SpherecastCheck(player, start, end, out point))
                    {
                        foundPoint = true;
                        break;
                    }
                }
                if (foundPoint)
                {
                    break;
                }
            }

            if (SAINPlugin.DebugMode)
            {
                Logger.LogInfo($"{totalChecks} : {foundPoint}");
            }

            return foundPoint;
        }

        public static bool SpherecastCheck(Player player, Vector3 start, Vector3 end, out SAINVaultPoint result, float distance = 0)
        {
            // Raise up the corner positions to get a clear line of sight to the next corner to test
            start.y += 0.33f;
            end.y += 0.33f;

            Vector3 direction = end - start;

            if (distance == 0)
            {
                distance = direction.magnitude;
            }

            if (Physics.SphereCast(start, 0.1f, direction, out RaycastHit hit, direction.magnitude, LayerMaskClass.PlayerStaticCollisionsMask))
            {
                if (CheckObstacleForVault(hit, player.VaultingParameters.VaultingHeight))
                {
                    result = new SAINVaultPoint(hit.point);

                    GlobalVaultPoints.Add(result);

                    return true;
                }
            }

            result = null;
            return false;
        }

        public static bool CheckObstacleForVault(RaycastHit hit, float maxHeight)
        {
            if (hit.collider == null)
            {
                return false;
            }

            float height = hit.collider.bounds.size.y;
            bool heightGood = height < maxHeight;

            // Debug Info
            Color debugSphereColor = heightGood ? Color.green : Color.red;
            float debugSphereSize = heightGood ? 1f : 0.5f;
            DebugGizmos.Sphere(hit.collider.transform.position, debugSphereSize, debugSphereColor, true, 60f);

            return heightGood;
        }

        public float VaultMaxHeight => Player.VaultingParameters.VaultingHeight;

        public bool TryVaulting()
        {
            if (CanVault() && Player.VaultingComponent.TryVaulting())
            {
                if (SAINPlugin.DebugMode)
                {
                    Logger.LogWarning("Vault Success");
                }

                Player.OnVaulting();
                return true;
            }

            if (SAINPlugin.DebugMode)
            {
                Logger.LogWarning("Vault Fail");
            }

            return false;
        }

        public bool CanVault()
        {
            if (Player == null || Player.VaultingComponent == null || Player.VaultingGameplayRestrictions == null)
            {
                if (SAINPlugin.DebugMode)
                {
                    Logger.LogWarning("Vault Fail, Something Null");
                }

                return false;
            }

            if (Player.VaultingGameplayRestrictions.CanVaulting())
            {
                if (SAINPlugin.DebugMode)
                {
                    Logger.LogWarning("Vault Success - Player Can Vault");
                }

                return true;
            }

            if (SAINPlugin.DebugMode)
            {
                Logger.LogWarning("Vault Fail - Player Can NOT Vault");
            }

            return false;
        }

        public void Dispose()
        {
        }
    }

    public sealed class SAINVaultPoint
    {
        private static int PointCount = 0;

        public SAINVaultPoint(Vector3 pos)
        {
            Position = pos;
            TimeCreated = Time.time;
            ID = PointCount;
            PointCount++;
        }

        public readonly Vector3 Position;
        public readonly float TimeCreated;
        public readonly int ID;
    }
}