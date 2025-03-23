using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    public class EnemyUpdaterClass : BotBase, IBotClass
    {
        public EnemyUpdaterClass(BotComponent bot) : base(bot)
        {
        }

        public void Init()
        {
            Enemies = Bot.EnemyController.Enemies;
        }

        public void Update()
        {
            if (Bot == null || Bot.EnemyController == null || !Bot.BotActive) {
                return;
            }

            if (SAINPlugin.ProfilingMode) {
                UnityEngine.Profiling.Profiler.BeginSample("Enemy Updater");
            }

            foreach (var kvp in Enemies) {
                string profileId = kvp.Key;
                Enemy enemy = kvp.Value;
                if (!checkValid(profileId, enemy))
                    continue;

                if (checkIfAlly(profileId, enemy))
                    continue;

                enemy.Update();
                enemy.Vision.VisionChecker.CheckVision(out _);
            }
            removeInvalid();
            removeAllies();

            if (SAINPlugin.ProfilingMode) {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        public void LateUpdate()
        {
            if (Bot == null || Bot.EnemyController == null || !Bot.BotActive) {
                return;
            }

            foreach (var kvp in Enemies) {
                checkValid(kvp.Key, kvp.Value);
            }
            removeInvalid();
        }

        public void Dispose()
        {
        }

        private bool checkValid(string id, Enemy enemy)
        {
            if (enemy == null || enemy.CheckValid() == false) {
                _invalidIdsToRemove.Add(id);
                return false;
            }
            return true;
        }

        private bool checkIfAlly(string id, Enemy enemy)
        {
            if (Bot.BotOwner.BotsGroup.Allies.Contains(enemy.EnemyPlayer)) {
                if (SAINPlugin.DebugMode)
                    Logger.LogWarning($"{enemy.EnemyPlayer.name} is an ally of {Bot.Player.name} and will be removed from its enemies collection");

                _allyIdsToRemove.Add(id);
                return true;
            }

            return false;
        }

        private void removeInvalid()
        {
            if (_invalidIdsToRemove.Count > 0) {
                foreach (var id in _invalidIdsToRemove) {
                    Bot.EnemyController.RemoveEnemy(id);
                }
                Logger.LogWarning($"Removed {_invalidIdsToRemove.Count} Invalid Enemies");
                _invalidIdsToRemove.Clear();
            }
        }

        private void removeAllies()
        {
            if (_allyIdsToRemove.Count > 0) {
                foreach (var id in _allyIdsToRemove) {
                    Bot.EnemyController.RemoveEnemy(id);
                }

                if (SAINPlugin.DebugMode)
                    Logger.LogWarning($"Removed {_allyIdsToRemove.Count} allies");

                _allyIdsToRemove.Clear();
            }
        }

        private Dictionary<string, Enemy> Enemies;
        private readonly List<string> _allyIdsToRemove = new List<string>();
        private readonly List<string> _invalidIdsToRemove = new List<string>();
    }
}