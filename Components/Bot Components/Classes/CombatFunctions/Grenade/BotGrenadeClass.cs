using EFT;
using EFT.InventoryLogic;
using SAIN.Components;
using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using HarmonyLib;

namespace SAIN.Classes
{
    public class BotGrenadeClass : SAINBot
    {
        public BotGrenadeClass(BotOwner bot) : base(bot) 
        {
            EFTBotGrenade = new EFTBotGrenade(bot);
        }

        public EFTBotGrenade EFTBotGrenade { get; private set; }

        public void Update()
        {
            if (ActiveGrenades.Count > 0)
            {
                List<GrenadeTracker> grenades = new List<GrenadeTracker>();
                foreach (var grenade in ActiveGrenades)
                {
                    if (grenade != null)
                    {
                        grenades.Add(grenade);
                    }
                }
                ActiveGrenades.Clear();
                if (grenades.Count > 0)
                {
                    var array = grenades.ToArray();
                    grenades.Clear();
                    System.Array.Sort(array, GrenadePositionComparerer);
                    ActiveGrenades.AddRange(array);
                }
            }
        }

        public Vector3? GrenadeDangerPoint
        {
            get
            {
                if (ActiveGrenades.Count > 0)
                {
                    foreach (var tracker in ActiveGrenades)
                    {
                        if (tracker?.Grenade != null && tracker.CanReact)
                        {
                            return tracker.DangerPoint;
                        }
                    }
                }
                return null;
            }
        }

        public bool ShallThrowGrenade()
        {
            return false;
        }

        public GrenadeThrowType GetThrowType(out GrenadeThrowDirection direction, out Vector3 ThrowAtPoint)
        {
            ThrowAtPoint = default;

            if (AllowCheck())
            {
                if (CanThrowOverObstacle(out ThrowAtPoint))
                {
                    direction = GrenadeThrowDirection.Over;
                }
                else if (CanThrowAroundObstacle(out ThrowAtPoint))
                {
                    direction = GrenadeThrowDirection.Around;
                }
                else
                {
                    direction = GrenadeThrowDirection.None;
                    return GrenadeThrowType.None;
                }

                float distance = (BotOwner.Position - BotOwner.Memory.GoalEnemy.CurrPosition).magnitude;

                if (distance <= 10f)
                {
                    return GrenadeThrowType.Close;
                }
                if (distance <= 30f)
                {
                    return GrenadeThrowType.Mid;
                }
                else
                {
                    return GrenadeThrowType.Far;
                }
            }
            else
            {
                direction= GrenadeThrowDirection.None;
                return GrenadeThrowType.None;
            }
        }

        private bool CanThrowAroundObstacle(out Vector3 ThrowAtPoint)
        {
            ThrowAtPoint = Vector3.zero;

            if (!AllowCheck())
            {
                return false;
            }

            var botPos = SAIN.HeadPosition;

            var direction = BotOwner.Memory.GoalEnemy.Person.MainParts[BodyPartType.head].Position - botPos;

            float distance = direction.magnitude;

            if (distance > 50f)
            {
                return false;
            }

            var lastKnownPos = BotOwner.Memory.GoalEnemy.EnemyLastPosition;
            lastKnownPos.y += 1.45f;

            var lastKnownDirection = lastKnownPos - botPos;

            var mask = LayerMaskClass.HighPolyWithTerrainMask;

            if (Physics.Raycast(botPos, lastKnownDirection, out var hit, lastKnownDirection.magnitude + 5f, mask) && (hit.point - botPos).magnitude > lastKnownDirection.magnitude)
            {
                ThrowAtPoint = hit.point;
                return true;
            }

            return false;
        }

        private bool CanThrowOverObstacle(out Vector3 ThrowAtPoint)
        {
            ThrowAtPoint = Vector3.zero;

            if (!AllowCheck())
            {
                return false;
            }

            var enemyHead = BotOwner.Memory.GoalEnemy.Person.MainParts[BodyPartType.head].Position;
            var botHead = SAIN.HeadPosition;
            var direction = enemyHead - botHead;
            float distance = direction.magnitude;

            if (distance > 50f)
            {
                return false;
            }

            var mask = LayerMaskClass.HighPolyWithTerrainMask;

            if (Physics.Raycast(botHead, direction, out var hit, distance, mask))
            {
                if (Vector3.Distance(hit.point, botHead) < 0.33f)
                {
                    return false;
                }

                float height = hit.collider.bounds.size.y;
                var objectPos = hit.collider.transform.position;
                objectPos.y += height + 0.5f;

                var directionToHeight = objectPos - botHead;

                if (directionToHeight.magnitude > 30f)
                {
                    return false;
                }

                if (!Physics.Raycast(botHead, directionToHeight, directionToHeight.magnitude, mask))
                {
                    ThrowAtPoint = objectPos;
                    return true;
                }
            }

            return false;
        }

        private bool AllowCheck()
        {
            var nade = BotOwner.WeaponManager.Grenades;

            if (!nade.HaveGrenade)
            {
                return false;
            }

            var enemy = BotOwner.Memory.GoalEnemy;

            if (enemy == null)
            {
                return false;
            }
            if (enemy.IsVisible && enemy.CanShoot)
            {
                return false;
            }

            return true;
        }

        public void EnemyGrenadeThrown(Grenade grenade, Vector3 dangerPoint)
        {
            if (SAIN.BotActive && !SAIN.GameIsEnding)
            {
                float reactionTime = GetReactionTime(SAIN.Info.DifficultyModifier);
                var tracker = BotOwner.gameObject.AddComponent<GrenadeTracker>();
                tracker.Initialize(grenade, dangerPoint, reactionTime);
                ActiveGrenades.Add(tracker);
            }
        }

        public List<GrenadeTracker> ActiveGrenades { get; private set; } = new List<GrenadeTracker>();

        private int GrenadePositionComparerer(GrenadeTracker A, GrenadeTracker B)
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
                float AMag = (BotPosition - A.DangerPoint).sqrMagnitude;
                float BMag = (BotPosition - B.DangerPoint).sqrMagnitude;
                return AMag.CompareTo(BMag);
            }
        }

        private static bool EnemyGrenadeHeard(Vector3 grenadePosition, Vector3 playerPosition, float distance)
        {
            return (grenadePosition - playerPosition).magnitude < distance;
        }

        private static float GetReactionTime(float diffMod)
        {
            float reactionTime = 0.33f;
            reactionTime *= diffMod;
            reactionTime *= Random.Range(0.75f, 1.25f);

            float min = 0.15f;
            float max = 0.66f;

            return Mathf.Clamp(reactionTime, min, max);
        }
    }
}