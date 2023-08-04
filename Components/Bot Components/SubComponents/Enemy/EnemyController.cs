using BepInEx.Logging;
using EFT;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Classes
{
    public class EnemyController : MonoBehaviour, ISAINSubComponent
    {
        public void Init(SAINComponent sain)
        {
            SAIN = sain;
            BotOwner = sain.BotOwner;
            Logger = sain.Logger;
            Player = sain.Player;
        }

        public SAINComponent SAIN { get; private set; }
        public BotOwner BotOwner { get; private set; }
        public ManualLogSource Logger { get; private set; }
        public Player Player { get; private set; }

        public bool HasEnemy => Enemy != null && Enemy.Person != null && Enemy.EnemyPlayer != null && (!Enemy.Person.IsAI || Enemy.Person.AIData.BotOwner.BotState == EBotState.Active);

        public SAINEnemy Enemy { get; private set; }


        private void Update()
        {
            if (BotOwner == null || SAIN == null) return;
            if (ClearEnemyTimer < Time.time)
            {
                ClearEnemyTimer = Time.time + 0.5f;
                ClearEnemies();
            }

            var goalEnemy = BotOwner.Memory.GoalEnemy;
            bool addEnemy = true;

            if (goalEnemy == null)
            {
                addEnemy = false;
            }
            else if (goalEnemy?.Person == null)
            {
                addEnemy = false;
            }
            else
            {
                if (goalEnemy.Person.IsAI && (goalEnemy.Person.AIData?.BotOwner == null || goalEnemy.Person.AIData.BotOwner.BotState != EBotState.Active))
                {
                    addEnemy = false;
                }
                if (goalEnemy.Person.IsAI && goalEnemy.Person.AIData.BotOwner.ProfileId == BotOwner.ProfileId)
                {
                    addEnemy = false;
                }
                if (!goalEnemy.Person.HealthController.IsAlive)
                {
                    addEnemy = false;
                }
            }

            if (addEnemy)
            {
                AddEnemy(goalEnemy.Person);
            }
            else
            {
                Enemy = null;
            }
        }

        public void ClearEnemy()
        {
            Enemy = null;
        }

        public void AddEnemy(IAIDetails person)
        {
            string id = person.ProfileId;

            // Check if the dictionary contains a previous SAINEnemy
            if (!Enemies.ContainsKey(id))
            {
                Enemies.Add(id, new SAINEnemy(SAIN, person));
            }

            if (Enemy != null)
            {
                string oldID = Enemy.EnemyProfileID;
                if (oldID == id)
                {
                    return;
                }
                else
                {
                    Enemy.EnemyVision.LoseSight();
                    Enemies[oldID] = Enemy;
                }
            }
            Enemy = Enemies[id];
        }

        private float ClearEnemyTimer;

        private void ClearEnemies()
        {
            if (Enemies.Count > 0)
            {
                foreach (var keyPair in Enemies)
                {
                    string id = keyPair.Key;
                    SAINEnemy enemy = keyPair.Value;
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
    }
}
