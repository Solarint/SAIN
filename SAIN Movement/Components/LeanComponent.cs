using EFT;
using SAIN.Movement.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static SAIN.Movement.Config.DebugConfig;
using static SAIN.Movement.Config.DogFighterConfig;

namespace SAIN.Movement.Components
{
    public class DynamicLean : MonoBehaviour
    {
        //public float LastLeanTime { get; private set; } = 0f;
        //public float LeanResetTimer { get; private set; } = 0f;
        public bool IsLeaning { get; private set; } = false;
        public Vector3[] LeanCorners { get; private set; }

        public DynamicLean(bool isLeaning, Vector3[] leanCorners, BotOwner bot, float resetTime)
        {
            IsLeaning = isLeaning;
            LeanCorners = leanCorners ?? throw new System.ArgumentNullException(nameof(leanCorners));
            this.bot = bot ?? throw new System.ArgumentNullException(nameof(bot));
            this.resetTime = resetTime;
        }

        private BotOwner bot;

        private float resetTime = 0;

        private void Awake()
        {
            bot = GetComponent<BotOwner>();
            StartCoroutine(ContinuousLean());
        }

        // Runs continuously in the background to check for suitable times to use lean
        private IEnumerator ContinuousLean()
        {
            while (true)
            {
                // Check if the bot is alive before continuing
                if (bot?.GetPlayer?.HealthController?.IsAlive == false || bot?.GetPlayer == null)
                {
                    StopAllCoroutines();
                    yield break; // Exit the coroutine
                }

                if (ShouldIReset)
                {
                    if (Time.time >= resetTime)
                    {
                        StartCoroutine(ResetLeanAfterDuration(Random.Range(1f, 1.5f)));
                        resetTime = Time.time + Random.Range(1f, 1.5f);
                        yield return new WaitForSeconds(0.1f);
                    }
                }

                if (ShouldILean && !ShouldIReset)
                {
                    StartCoroutine(ExecuteLean());
                    yield return new WaitForSeconds(0.5f);
                }

                // Overall Check Frequency
                yield return new WaitForSeconds(0.1f);
            }
        }

        // Logic checks for when to execute lean or reset
        private bool ShouldILean
        {
            get
            {
                if (bot?.BotState != EBotState.Active) return false;

                if (!ScavDodgeToggle.Value && bot.IsRole(WildSpawnType.assault)) return false;

                if (!LeanToggle.Value) return false;

                if (bot?.Memory?.GoalEnemy?.CanShoot == true || bot?.Memory?.IsInCover == true) return false;

                if (bot?.Memory?.GoalEnemy == null) return false;

                return true;
            }
        }

        private bool ShouldIReset
        {
            get
            {
                if (IsLeaning)
                {
                    if (bot?.BotState != EBotState.Active) return true;

                    if (!ScavDodgeToggle.Value && bot.IsRole(WildSpawnType.assault)) return true;

                    if (!LeanToggle.Value) return true;

                    if (bot?.Memory?.GoalEnemy == null) return true;

                    if (bot?.Memory?.GoalEnemy?.IsVisible == true) return true;
                }
                return false;
            }
        }

        // Executes a DYNAMIC LEAN  
        public IEnumerator ExecuteLean()
        {
            IsLeaning = true;

            // Calculate Lean Angle
            LeanAngle(out float Angle);

            // Checks resulting angle to see if the bot should lean left or right

            // Sends the command to lean
            bot.GetPlayer.MovementContext.SetTilt((Angle > 0f) ? 5 : -5, false);

            // Debug Log
            bool right = ((Angle > 0f) ? 5 : -5) > 0f;

            if (DebugLean.Value)
            {
                System.Console.WriteLine($"{bot.name} Leaned. Leaned Right? [{right}] because [{Angle}]");
            }

            yield return null;
        }

        // Resets Lean
        private IEnumerator ResetLeanAfterDuration(float duration)
        {
            if (DebugLean.Value)
            {
                System.Console.WriteLine($"{bot.name} Started Lean Reset Coroutine");
            }

            yield return new WaitForSeconds(duration);

            ResetLean();
        }
        private void ResetLean()
        {
            if (DebugLean.Value)
            {
                System.Console.WriteLine($"{bot.name} Reset Lean");
            }

            bot.GetPlayer.MovementContext.SetTilt(0f, false);

            IsLeaning = false;
        }

        // Finds angle between corner and bot position
        private float LeanAngle(out float angle)
        {
            BotOwner bot = this.bot;
            angle = 0f;

            if (Calculate(out Vector3 leancorner1, out Vector3 leancorner2, out Vector3 normal, 10f))
            {
                angle = Vector3.SignedAngle(leancorner1, leancorner2, Vector3.up);

                if (DebugNavMesh.Value)
                {
                    System.Console.WriteLine($"LeanAngle: [{bot.name}]: Corner Angle: [{angle}]");
                }

                return angle;
            }

            if (DebugNavMesh.Value)
            {
                System.Console.WriteLine($"LeanAngle: [{bot.name}]: No Corners found");
            }

            return angle;
        }

        // Finds corners in a new NavmeshPath. Returns false if path invalid
        private bool Calculate(out Vector3 corner1, out Vector3 corner2, out Vector3 normal, float maxdistance)
        {
            BotOwner bot = this.bot;

            // Assigns positions for easy readability
            Vector3 Target = bot.Memory.GoalEnemy.CurrPosition;
            Vector3 BotPos = bot.Transform.position;
            //float BotHeadPosy = bot.MyHead.position.y;

            // Calculate a new path between bot and target
            NavMeshPath navMeshPath = new NavMeshPath();
            NavMesh.CalculatePath(BotPos, Target, -1, navMeshPath);

            List<Vector3> NavmeshCornersList = new List<Vector3>(navMeshPath.corners);

            // Raises Corners to bot height
            for (int i = 0; i < NavmeshCornersList.Count; i++)
            {
                Vector3 corner = NavmeshCornersList[i];
                corner.y += 1.3f;
                NavmeshCornersList[i] = corner;
            }

            Vector3[] raisedcorners = NavmeshCornersList.ToArray();

            if (DebugNavMesh.Value)
            {
                System.Console.WriteLine($"Corners Calculate Result: [{raisedcorners.Length}] total corners.");
            }

            if (raisedcorners.Length > 1)
            {
                SphereCastTrim(raisedcorners, out Vector3[] RaycastCorners);

                if (DebugNavMesh.Value)
                {
                    System.Console.WriteLine($"Navmesh Raycast Trim Result: [{RaycastCorners.Length}] corners");
                }

                AverageCorners(RaycastCorners, out Vector3[] averagedcorners, 1f);

                if (DebugNavMesh.Value)
                {
                    System.Console.WriteLine($"Navmesh Trim Corners Result: [{averagedcorners.Length}] corners");
                }

                if (FindLeanCorners(averagedcorners, out Vector3 LeanCorner1, out Vector3 LeanCorner2, out normal, maxdistance))
                {
                    corner1 = LeanCorner1;
                    corner2 = LeanCorner2;

                    return true;
                }
            }

            corner1 = Vector3.zero;
            corner2 = Vector3.zero;
            normal = Vector3.up;
            return false;
        }

        // Lerps to find exact corner locations
        private Vector3[] RayCast(Vector3[] navmeshcorners, out Vector3[] newcorners)
        {
            List<Vector3> cornersList = new List<Vector3>(navmeshcorners);

            bool allCornersHit = true;
            int iterationCount = 0;
            const int maxIterationCount = 50;

            while (iterationCount < maxIterationCount)
            {
                for (int i = 0; i < cornersList.Count - 1; i++)
                {
                    // Raycasts to see if corners are important or not
                    if (!Physics.SphereCast(cornersList[i], 0.25f, cornersList[i + 2], out RaycastHit hit, 10f, LayerMaskClass.HighPolyWithTerrainMaskAI))
                    {
                        cornersList.RemoveAt(i + 1);
                        allCornersHit = false;
                        continue;
                    }
                    else
                    {
                        cornersList[i + 1] = Vector3.Lerp(cornersList[i + 1], cornersList[i + 2], 0.25f);
                    }
                }

                if (allCornersHit)
                {
                    // All corners have been hit by raycast, break out of loop
                    break;
                }

                iterationCount++;
            }

            if (iterationCount >= maxIterationCount)
            {
                // Maximum iteration count reached, break out of loop
                System.Console.WriteLine("Maximum iteration count reached");
            }
            newcorners = cornersList.ToArray();

            return newcorners;
            //return corners = navMeshPath.corners;
        }

        // Removes corners that are surrounded by 2 corners that are visible to each other
        private Vector3[] SphereCastTrim(Vector3[] navmeshcorners, out Vector3[] newcorners)
        {
            List<Vector3> cornersList = new List<Vector3>(navmeshcorners);

            for (int i = 0; i < cornersList.Count - 1; i++)
            {
                // Check if the next index is within bounds before raycasting
                if (i + 2 < cornersList.Count)
                {
                    // Calculate the direction from corner 1 to corner 3
                    Vector3 direction = cornersList[i + 2] - cornersList[i];
                    float distance = direction.magnitude;

                    // Raycasts from corner 1 to corner 3 to see if it's visible
                    if (!Physics.Raycast(cornersList[i], direction, out RaycastHit hit, distance, LayerMaskClass.HighPolyWithTerrainMask))
                    {
                        // If corner 1 can see corner 3 directly, remove corner 2 as it's not important
                        cornersList.RemoveAt(i + 1);
                        // Move back in the list to reevaluate the new corner configuration
                        i--;
                    }
                }
            }

            newcorners = cornersList.ToArray();
            return newcorners;
        }

        // Removes Corners that are too close
        private Vector3[] TrimCorners(Vector3[] allcorners, out Vector3[] longcorners, float minlength)
        {
            List<Vector3> cornersList = new List<Vector3>(allcorners);

            // Remove corners with magnitude less than input minlength
            for (int i = cornersList.Count - 2; i > 0; i--)
            {
                if (cornersList.Count < 2)
                {
                    break;
                }

                if ((cornersList[i] - cornersList[i + 1]).magnitude < minlength)
                {
                    cornersList.RemoveAt(i);
                }
            }

            longcorners = cornersList.ToArray();

            return longcorners;
        }

        // Averages corners that are too close
        private Vector3[] AverageCorners(Vector3[] oldcorners, out Vector3[] averagedcorners, float minlength)
        {

            // Finds average position between close corners
            List<Vector3> cornersList = new List<Vector3>(oldcorners);

            for (int i = cornersList.Count - 2; i >= 0; i--)
            {
                if (cornersList.Count <= 2)
                {
                    break;
                }

                if ((cornersList[i] - cornersList[i + 1]).magnitude < minlength)
                {
                    cornersList[i] = Vector3.Lerp(cornersList[i], cornersList[i + 1], 0.5f);

                    cornersList.RemoveAt(i + 1);
                }
            }

            averagedcorners = cornersList.ToArray();
            return averagedcorners;
        }

        // Grabs First and Second Corners. Returns false if Not enough corners
        private bool FindLeanCorners(Vector3[] allcorners, out Vector3 firstcorner, out Vector3 secondcorner, out Vector3 normal, float maxdistance)
        {
            if (allcorners.Length < 3)
            {
                firstcorner = Vector3.zero;
                secondcorner = Vector3.zero;
                normal = Vector3.up;
                return false;
            }

            // Makes sure bots aren't leaning around corners that are too far
            if ((allcorners[0] - allcorners[1]).magnitude > maxdistance)
            {
                if (DebugNavMesh.Value)
                {
                    System.Console.WriteLine($"FindLeanCorners: First Corner is too far! Length: [{allcorners[0].magnitude}], Max Distance: [{maxdistance}]");
                }

                firstcorner = Vector3.zero;
                secondcorner = Vector3.zero;
                normal = Vector3.up;
                return false;
            }

            if (DebugNavMesh.Value)
            {
                System.Console.WriteLine($"NavMesh FindLeanCorners: Success! Drawing Red Sphere at corner 1 and blue Sphere at corner 2");

                DebugDrawer.Sphere(allcorners[1], 0.2f, Color.red, 3f);
                DebugDrawer.Sphere(allcorners[2], 0.2f, Color.blue, 3f);
            }

            Vector3 A = allcorners[0];
            Vector3 B = allcorners[1];
            Vector3 C = allcorners[2];

            firstcorner = A - B;
            secondcorner = C - B;

            // Calculate the normal (axis) using cross product
            normal = Vector3.Cross(firstcorner, secondcorner);

            return true;
        }
    }
}