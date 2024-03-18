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

        public static bool TrySetExfilForBot(BotOwner bot)
        {
            var component = bot.GetComponent<SAINComponentClass>();
            if (component == null)
            {
                return false;
            }

            if (!Components.BotController.BotExtractManager.IsBotAllowedToExfil(component))
            {
                Logger.LogWarning($"{bot.name} is not allowed to use extracting logic.");
            }

            if (!SAINPlugin.BotController.BotExtractManager.TryFindExfilForBot(component))
            {
                return false;
            }

            return true;
        }

        public static bool ResetDecisionsForBot(BotOwner bot)
        {
            var component = bot.GetComponent<SAINComponentClass>();
            if (component == null)
            {
                return false;
            }

            // Do not do anything if the bot is currently in combat
            if (isBotInCombat(component))
            {
                if (SAINPlugin.DebugMode)
                    Logger.LogInfo($"{bot.name} is currently engaging an enemy; cannot reset its decisions");

                return true;
            }

            if (SAINPlugin.DebugMode)
                Logger.LogInfo($"Forcing {bot.name} to reset its decisions...");

            PropertyInfo enemyLastSeenTimeSenseProperty = AccessTools.Property(typeof(BotSettingsClass), "EnemyLastSeenTimeSense");
            if (enemyLastSeenTimeSenseProperty == null)
            {
                Logger.LogError($"Could not reset EnemyLastSeenTimeSense for {bot.name}'s enemies");
                return false;
            }

            // Force the bot to think it has not seen any enemies in a long time
            foreach (IPlayer player in bot.BotsGroup.Enemies.Keys)
            {
                bot.BotsGroup.Enemies[player].Clear();
                enemyLastSeenTimeSenseProperty.SetValue(bot.BotsGroup.Enemies[player], 1);
            }

            // Until the bot next identifies an enemy, do not search anywhere
            component.Decision.GoalTargetDecisions.IgnorePlaceTarget = true;

            // Force the bot to "forget" what it was doing
            bot.Memory.GoalTarget.Clear();
            bot.Memory.GoalEnemy = null;
            component.EnemyController.ClearEnemy();
            component.Decision.ResetDecisions();

            return true;
        }

        private static bool isBotInCombat(SAINComponentClass component)
        {
            if (component?.EnemyController?.ActiveEnemy == null)
            {
                return false;
            }

            if (component.EnemyController.ActiveEnemy.IsVisible)
            {
                return true;
            }

            if (component.BotOwner.Memory.IsUnderFire)
            {
                return true;
            }

            return false;
        }
    }
}
