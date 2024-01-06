using Comfort.Common;
using EFT;
using HarmonyLib;
using SAIN.SAINComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SAIN.Plugin
{
    public static class External
    {
        public static bool ExtractBot(BotOwner bot)
        {
            var component = bot.GetComponent<SAINComponentClass>();
            if (component == null)
            {
                return false;
            }

            component.Info.ForceExtract = true;

            return true;
        }

        public static bool ResetDecisionsForBot(BotOwner bot)
        {
            var component = bot.GetComponent<SAINComponentClass>();
            if (component == null)
            {
                return false;
            }

            //if (SAINPlugin.DebugMode)
                Logger.LogInfo($"Forcing {bot.name} to reset its decisions...");

            PropertyInfo enemyLastSeenTimeSenseProperty = AccessTools.Property(typeof(BotSettingsClass), "EnemyLastSeenTimeSense");

            foreach (IPlayer player in bot.BotsGroup.Enemies.Keys)
            {
                if (player.Id == Singleton<GameWorld>.Instance.MainPlayer.Id)
                {
                    //continue;
                }

                bot.BotsGroup.Enemies[player].Clear();

                if (enemyLastSeenTimeSenseProperty != null)
                {
                    enemyLastSeenTimeSenseProperty.SetValue(bot.BotsGroup.Enemies[player], 1);
                }
                else
                {
                    Logger.LogError($"Could not reset settings for {bot.name}");
                }
            }

            bot.Memory.GoalTarget.Clear();
            bot.Memory.GoalEnemy = null;
            component.EnemyController.ClearEnemy();
            component.Decision.GoalTargetDecisions.IgnorePlaceTarget = true;
            component.Decision.ResetDecisions();

            return true;
        }
    }
}
