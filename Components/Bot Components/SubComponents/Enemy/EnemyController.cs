using BepInEx.Logging;
using EFT;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

        private SAINComponent SAIN;

        private BotOwner BotOwner => SAIN.BotOwner;

        private void Update()
        {
            if (BotOwner == null || SAIN == null) return;
            if (ClearEnemyTimer < Time.time)
            {
                //ClearEnemyTimer = Time.time + 1f;
                //ClearEnemies();
            }

            //if (BotOwner.BotsGroup.Enemies.Count > 0)
            //{
            //    foreach (var person in BotOwner.BotsGroup.Enemies.Keys)
            //    {
            //        string id = person.ProfileId;
            //        if (!Enemies.ContainsKey(id))
            //        {
            //            SAINEnemy sainEnemy = new SAINEnemy(BotOwner, person, 1f);
            //            Enemies.Add(id, sainEnemy);
            //        }
            //    }
            //}

            var goalEnemy = BotOwner.Memory.GoalEnemy;
            if (goalEnemy != null)
            {
                string id = goalEnemy.Person.ProfileId;
                if (Enemy == null || Enemy.Person.ProfileId != id)
                {
                    Enemy = new SAINEnemy(BotOwner, goalEnemy.Person, 1f);
                }
            }
            else
            {
                goalEnemy = BotOwner.Memory.LastEnemy;
                if (goalEnemy != null)
                {
                    string id = goalEnemy.Person.ProfileId;
                    if (Enemy == null || Enemy.Person.ProfileId != id)
                    {
                        Enemy = new SAINEnemy(BotOwner, goalEnemy.Person, 1f);
                    }
                }
                else
                {
                    Enemy = null;
                }
            }
            Enemy?.Update();
        }


        private float ClearEnemyTimer;

        private void ClearEnemies()
        {
            if (Enemies.Count > 0)
            {
                foreach (string id in Enemies.Keys)
                {
                    var enemy = Enemies[id];
                    if (enemy == null || enemy.EnemyPlayer == null || enemy.EnemyPlayer?.HealthController?.IsAlive == false)
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
