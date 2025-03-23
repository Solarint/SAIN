using EFT;
using HarmonyLib;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// Found in Botowner.Looksensor
using EnemyTotalCheck = GClass568;
using EnemyVisionCheck = GClass564;
using LookAllData = GClass589;

namespace SAIN.SAINComponent.Classes
{
    public class SAINBotLookClass : BotBase
    {
        private const float VISION_FREQ_INACTIVE_BOT_COEF = 5f;
        private const float VISION_FREQ_ACTIVE_BOT_COEF = 2f;
        private const float VISION_FREQ_CURRENT_ENEMY = 0.04f;
        private const float VISION_FREQ_UNKNOWN_ENEMY = 0.1f;
        private const float VISION_FREQ_KNOWN_ENEMY = 0.05f;

        public SAINBotLookClass(BotComponent component) : base(component)
        {
            LookData = new LookAllData();
        }

        public void Init()
        {
            base.SubscribeToPreset(null);
            _enemies = Bot.EnemyController.Enemies;
        }

        public void Dispose()
        {
        }

        private Dictionary<string, Enemy> _enemies;
        public readonly LookAllData LookData;

        public int UpdateLook()
        {
            if (BotOwner.LeaveData == null ||
                BotOwner.LeaveData.LeaveComplete) {
                return 0;
            }

            int numUpdated = UpdateLookForEnemies(LookData);
            UpdateLookData(LookData);
            return numUpdated;
        }

        public void UpdateLookData(LookAllData lookData)
        {
            for (int i = 0; i < lookData.ReportsData.Count; i++) {
                EnemyVisionCheck enemyVision = lookData.ReportsData[i];
                // TAHVOHCK: They're fixing code typos, apparently...
                BotOwner.BotsGroup.ReportAboutEnemy(enemyVision.Enemy, enemyVision.VisibleOnlyBySence);
            }

            if (lookData.ReportsData.Count > 0)
                BotOwner.Memory.SetLastTimeSeeEnemy();

            if (lookData.ShallRecalcGoal)
                BotOwner.CalcGoal();

            lookData.Reset();
        }

        private int UpdateLookForEnemies(LookAllData lookAll)
        {
            int updated = 0;
            _cachedList.Clear();
            _cachedList.AddRange(_enemies.Values);
            foreach (Enemy enemy in _cachedList) {
                if (!shallCheckEnemy(enemy))
                    continue;

                if (checkEnemy(enemy, lookAll)) {
                    updated++;
                }
            }
            _cachedList.Clear();
            return updated;
        }

        private readonly List<Enemy> _cachedList = new List<Enemy>();

        private bool shallCheckEnemy(Enemy enemy)
        {
            if (enemy?.CheckValid() != true)
                return false;

            if (!enemy.InLineOfSight ||
                !enemy.Vision.Angles.CanBeSeen) {
                setNotVis(enemy);
                return false;
            }
            return true;
        }

        private void setNotVis(Enemy enemy)
        {
            foreach (var part in enemy.EnemyInfo.AllActiveParts.Values) {
                // System.Boolean EnemyPartData::VisibleBySense()
                // has been replaced by
                // EEnemyPartVisibleType EnemyPartData::VisibleType == EEnemyPartVisibleType.Sence
                if (part.IsVisible || part.VisibleType == EEnemyPartVisibleType.Sence) {
                    //part.UpdateVision(1000f, false, false, false, BotOwner);
                    // New method doesn't use expose (or internally use?) seenCoef. It does expose deltaTime though
                    // which was previously internally calculated. My assumption is that we want the bot to instantly
                    // forget about the enemy, so a delta of 0f here should be fine.
                    part.UpdateVisibility(BotOwner, false, false, false, 0f);
                }
            }
            if (enemy.EnemyInfo.IsVisible) {
                enemy.EnemyInfo.SetVisible(false);
            }
        }

        private bool checkEnemy(Enemy enemy, LookAllData lookAll)
        {
            float delay = getDelay(enemy);
            var look = enemy.Vision.VisionChecker;
            float timeSince = Time.time - look.LastCheckLookTime;
            if (timeSince >= delay) {
                look.LastCheckLookTime = Time.time;
                // TAHVOHCK: This takes the time delta now apparently
                enemy.EnemyInfo.CheckLookEnemy(lookAll, timeSince);
                return true;
            }
            return false;
        }

        private float getDelay(Enemy enemy)
        {
            float updateFreqCoef = enemy.UpdateFrequencyCoefNormal + 1f;
            float baseDelay = calcBaseDelay(enemy) * updateFreqCoef;
            if (!enemy.IsAI) {
                return baseDelay;
            }
            var active = Bot.BotActivation;
            if (!active.BotActive || active.BotInStandBy) {
                return baseDelay * VISION_FREQ_INACTIVE_BOT_COEF;
            }
            return baseDelay * VISION_FREQ_ACTIVE_BOT_COEF;
        }

        private float calcBaseDelay(Enemy enemy)
        {
            if (enemy.IsCurrentEnemy)
                return VISION_FREQ_CURRENT_ENEMY;
            if (enemy.EnemyKnown)
                return VISION_FREQ_KNOWN_ENEMY;
            return VISION_FREQ_UNKNOWN_ENEMY;
        }
    }
}