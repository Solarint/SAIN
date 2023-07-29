using BepInEx.Logging;
using EFT;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UIElements;

namespace SAIN.Classes
{
    public class EnemyController : MonoBehaviour
    {
        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        public SAINEnemy Enemy { get; private set; }
        public bool HasEnemy => Enemy != null && Enemy.Person != null && Enemy.BotPlayer != null && (!Enemy.Person.IsAI || Enemy.Person.AIData.BotOwner.BotState == EBotState.Active);

        private SAINComponent SAIN;

        private BotOwner BotOwner => SAIN?.BotOwner;

        private void Update()
        {
            if (BotOwner == null || SAIN == null) return;
            if (ClearEnemyTimer < Time.time)
            {
                ClearEnemyTimer = Time.time + 1f;
                ClearEnemies();
            }

            var goalEnemy = BotOwner.Memory.GoalEnemy;
            if (IsValidEnemy(goalEnemy))
            {
                AddEnemy(goalEnemy.Person);
            }
            else
            {
                goalEnemy = BotOwner.Memory.LastEnemy;
                if (IsValidEnemy(goalEnemy))
                {
                    AddEnemy(goalEnemy.Person);
                }
                else
                {
                    Enemy = null;
                }
            }
        }

        public void ClearEnemy()
        {
            Enemy = null;
        }

        private bool IsValidEnemy(GClass478 goalEnemy)
        {
            if (goalEnemy?.Person == null)
            {
                return false;
            }

            if (goalEnemy.Person.IsAI && (goalEnemy.Person.AIData?.BotOwner == null || goalEnemy.Person.AIData.BotOwner.BotState != EBotState.Active))
            {
                return false;
            }

            if (goalEnemy.Person.IsAI && goalEnemy.Person.AIData.BotOwner.ProfileId == BotOwner.ProfileId)
            {
                return false;
            }

            if (!goalEnemy.Person.HealthController.IsAlive)
            {
                return false;
            }

            return true;
        }

        public void AddEnemy(IAIDetails person)
        {
            string id = person.ProfileId;
            
            // Check if the dictionary contains a previous SAINEnemy
            if (Enemies.ContainsKey(id))
            {
                // if we are moving to a new enemy, mark the old enemy as not visible
                if (Enemy?.Person != null)
                {
                    if (Enemy.Person.ProfileId != id)
                    {
                        Enemy.UpdateCanShoot(false);
                        Enemy.UpdateVisible(false);
                    }
                }
                Enemy = Enemies[id];
            }
            else
            {
                Enemy = new SAINEnemy(BotOwner, person, 1f);
                Enemies.Add(id, Enemy);
            }
        }


        private float ClearEnemyTimer;

        private void ClearEnemies()
        {
            if (Enemies.Count > 0)
            {
                foreach (string id in Enemies.Keys)
                {
                    var enemy = Enemies[id];
                    // Common checks between PMC and bots
                    if (enemy == null || enemy.EnemyPlayer == null || enemy.EnemyPlayer.HealthController?.IsAlive == false)
                    {
                        EnemyIDsToRemove.Add(id);
                    }
                    // Checks specific to bots
                    else if (enemy.EnemyPlayer.IsAI && (
                        enemy.EnemyPlayer.AIData?.BotOwner == null ||
                        enemy.EnemyPlayer.AIData.BotOwner.ProfileId == BotOwner.ProfileId ||
                        enemy.EnemyPlayer.AIData.BotOwner.BotState != EBotState.Active))
                    {
                        EnemyIDsToRemove.Add(id);
                    }
                }

                foreach (string idToRemove in EnemyIDsToRemove)
                {
                    Enemies.Remove(idToRemove);
                }

                EnemyIDsToRemove.Clear();
            }
        }

        public Dictionary<string, SAINEnemy> Enemies { get; private set; } = new Dictionary<string, SAINEnemy>();
        public List<Player> VisiblePlayers = new List<Player>();
        public List<string> VisiblePlayerIds = new List<string>();
        private readonly List<string> EnemyIDsToRemove = new List<string>();

        private SAINEnemy PickClosestEnemy()
        {
            SAINEnemy ChosenEnemy = null;

            SAINEnemy closestLos = null;
            SAINEnemy closestAny = null;
            SAINEnemy closestVisible = null;

            float closestDist = Mathf.Infinity;
            float closestAnyDist = Mathf.Infinity;
            float closestVisibleDist = Mathf.Infinity;
            float enemyDist;

            if (Enemies.Count > 1)
            {
                foreach (var enemy in Enemies.Values)
                {
                    if (enemy == null || !VisiblePlayerIds.Contains(enemy.EnemyPlayer.ProfileId))
                    {
                        continue;
                    }
                    if (enemy.EnemyPlayer.HealthController.IsAlive)
                    {
                        enemyDist = (enemy.Position - BotOwner.Position).sqrMagnitude;
                        if (enemy.EnemyLookingAtMe && enemy.IsVisible)
                        {
                            if (enemyDist < closestVisibleDist)
                            {
                                closestVisibleDist = enemyDist;
                                closestVisible = enemy;
                            }
                        }
                        else if (enemy.InLineOfSight)
                        {
                            if (enemyDist < closestDist)
                            {
                                closestDist = enemyDist;
                                closestLos = enemy;
                            }
                        }
                        else
                        {
                            if (enemyDist < closestAnyDist)
                            {
                                closestAnyDist = enemyDist;
                                closestAny = enemy;
                            }
                        }
                    }
                }
                if (closestVisible != null)
                {
                    ChosenEnemy = closestVisible;
                }
                else if (closestLos != null)
                {
                    ChosenEnemy = closestLos;
                }
                else
                {
                    ChosenEnemy = closestAny;
                }
            }
            else if (Enemies.Count == 1)
            {
                ChosenEnemy = Enemies.Values.First();
                if (!VisiblePlayerIds.Contains(ChosenEnemy.EnemyPlayer.ProfileId))
                {
                    ChosenEnemy = null;
                }
            }
            return ChosenEnemy;
        }

        private ManualLogSource Logger;
    }
}
