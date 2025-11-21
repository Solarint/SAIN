using EFT;
using SAIN.Classes.Transform;
using SAIN.Components;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System.Collections.Generic;
using UnityEngine;

// Found in Botowner.Looksensor
using EnemyVisionCheck = BotReportsDataClass;
using LookAllData = LookAllDataClass;

namespace SAIN.SAINComponent.Classes;

public class SAINBotLookClass : BotBase
{
    public SAINBotLookClass(BotComponent component) : base(component)
    {
        LookData = new LookAllData();
    }

    public readonly LookAllData LookData;

    // This (And the methods it calls) mirrors a large part of BSG's LookSensor
    // Look at that for potential changes between versions
    public int UpdateLook(float currentTime)
    {
        if (BotOwner.LeaveData == null || BotOwner.LeaveData.LeaveComplete)
        {
            return 0;
        }

        int numUpdated = UpdateLookForEnemies(LookData, currentTime, Bot);
        UpdateLookData(LookData);
        return numUpdated;
    }

    public void UpdateLookData(LookAllData lookData)
    {
        for (int i = 0; i < lookData.ReportsData.Count; i++)
        {
            EnemyVisionCheck enemyVision = lookData.ReportsData[i];
            BotOwner.BotsGroup.ReportAboutEnemy(enemyVision.Enemy, enemyVision.VisibleOnlyBySence, BotOwner);
        }

        if (lookData.ReportsData.Count > 0)
        {
            BotOwner.Memory.SetLastTimeSeeEnemy();
        }

        if (lookData.ShallRecalcGoal)
        {
            //BotOwner.CalcGoal();
        }

        lookData.Reset();
    }

    private static int UpdateLookForEnemies(LookAllData lookAll, float currentTime, BotComponent bot)
    {
        int updated = 0;
        var lookSensor = bot.BotOwner.LookSensor;

        var transform = bot.Transform;
        Vector3 viewPosition = transform.EyePosition;
        var weaponRoot = transform.WeaponRoot;

        // Update look sensors fields since we are not calling the original botowner code that does this.
        // We should check for changes between tarkov updates.
			lookSensor.WeaponRootPoint = weaponRoot;
			lookSensor.LookSensorShootPosition.UpdateShootPosition(weaponRoot);
			lookSensor.HeadPoint = viewPosition;

        lookAll.Reset();
        var enemies = bot.EnemyController.EnemiesArray;
        foreach (Enemy enemy in enemies)
            if (enemy.ShallCheckLook(currentTime, out float deltaTime))
            {
                enemy.EnemyInfo.CheckLookEnemy(lookAll, deltaTime);
                updated++;
            }
        return updated;
    }
}