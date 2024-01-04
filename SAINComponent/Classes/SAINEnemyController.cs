using BepInEx.Logging;
using EFT;
using SAIN.Components;
using SAIN.SAINComponent;
using SAIN.SAINComponent.BaseClasses;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public class SAINEnemyController : SAINBase, ISAINClass
    {
        public SAINEnemyController(SAINComponentClass sain) : base(sain)
        {
        }

        public void Init()
        {
        }

        private void UpdateEnemies()
        {
            foreach (var keyPair in Enemies)
            {
                string id = keyPair.Key;
                var enemy = keyPair.Value;
                var enemyPerson = enemy?.EnemyPerson;
                if (enemyPerson?.IsActive == true)
                {
                    enemy.Update();
                }
                else if (enemyPerson?.PlayerNull == true)
                {
                    EnemyIDsToRemove.Add(id);
                }
                // Redundant Checks
                // Common checks between PMC and bots
                else if (enemy == null || enemy.EnemyPlayer == null || enemy.EnemyPlayer.HealthController?.IsAlive == false)
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

        public void Update()
        {
            UpdateEnemies();
            CheckAddEnemy();
        }

        public void Dispose()
        {
            Enemies?.Clear();
        }

        public bool HasEnemy => ActiveEnemy?.EnemyPerson?.IsActive == true;

        public SAINEnemyClass ActiveEnemy { get; private set; }

        public void ClearEnemy()
        {
            ActiveEnemy = null;
        }

        public void CheckAddEnemy()
        {
            var goalEnemy = BotOwner.Memory.GoalEnemy;
            IPlayer IPlayer = goalEnemy?.Person;
            bool addEnemy = true;

            if (goalEnemy == null || IPlayer == null)
            {
                addEnemy = false;
            }
            else
            {
                if (IPlayer.IsAI && (IPlayer.AIData?.BotOwner == null || IPlayer.AIData.BotOwner.BotState != EBotState.Active))
                {
                    addEnemy = false;
                }
                if (IPlayer.ProfileId == SAIN.ProfileId)
                {
                    addEnemy = false;
                }
                if (!IPlayer.HealthController.IsAlive)
                {
                    addEnemy = false;
                }
            }

            if (addEnemy)
            {
                string id = IPlayer.ProfileId;

                // Check if the dictionary contains a previous SAINEnemy
                if (Enemies.ContainsKey(id))
                {
                    if (ActiveEnemy != Enemies[id])
                    {
                        Logger.LogInfo($"{BotOwner.name} has selected enemy {Enemies[id].EnemyPerson.BotOwner.name} as the active enemy");
                        //Logger.LogInfo($"Enemy ID = {id}; {BotOwner.name} ID = {BotOwner.ProfileId}; {Enemies[id].EnemyPerson.BotOwner.name} ID = {Enemies[id].EnemyPerson.ProfileId}; SAIN ID = {SAIN.ProfileId}");
                    }

                    ActiveEnemy = Enemies[id];
                }
                else
                {
                    SAINPersonClass enemySAINPerson;
                    BotOwner botOwner = IPlayer.AIData.BotOwner;
                    if (botOwner != null && botOwner.TryGetComponent(out SAINComponentClass enemySAIN))
                    {
                        enemySAINPerson = enemySAIN.Person;
                        ActiveEnemy = new SAINEnemyClass(SAIN, enemySAINPerson);
                        Enemies.Add(id, ActiveEnemy);
                    }
                }
            }
            else
            {
                ActiveEnemy = null;
            }
        }

        public readonly Dictionary<string, SAINEnemyClass> Enemies = new Dictionary<string, SAINEnemyClass>();
        private readonly List<string> EnemyIDsToRemove = new List<string>();
    }
}
