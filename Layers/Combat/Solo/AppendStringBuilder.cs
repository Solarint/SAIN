using BepInEx.Logging;
using EFT;
using System.Text;
using UnityEngine;
using SAIN.SAINComponent;
using System;

namespace SAIN.Layers
{
    public static class AppendStringBuilder
    {
        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(AppendStringBuilder));

        public static void AddBaseInfo(SAINComponentClass SAIN, BotOwner BotOwner, StringBuilder stringBuilder)
        {
            try
            {
                AddSAINInfo(SAIN, stringBuilder);
                AddSAINEnemy(SAIN, stringBuilder);
                AddGoalEnemy(BotOwner, stringBuilder);
                AddAimData(BotOwner, stringBuilder);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        public static void AddSAINInfo(SAINComponentClass SAIN, StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"SAIN Info:");
            var info = SAIN.Info;
            stringBuilder.AppendLabeledValue("Personality and Type", $"{info.Personality} {info.Profile.WildSpawnType}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Power of Equipment", $"{info.Profile.PowerLevel}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Start Search + Hold Ground Time", $"{info.TimeBeforeSearch} + {info.HoldGroundDelay}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Difficulty + Modifier", $"{info.Profile.BotDifficulty} + {info.Profile.DifficultyModifier}", Color.white, Color.yellow, true);
            stringBuilder.AppendLabeledValue("Shoot Modifier", $"{info.WeaponInfo.FinalModifier}", Color.white, Color.yellow, true);
        }

        public static void AddAimData(BotOwner BotOwner, StringBuilder stringBuilder)
        {
            var aimData = BotOwner.AimingData;
            if (aimData != null)
            {
                stringBuilder.AppendLabeledValue("Aim: AimComplete?", $"{aimData.IsReady}", Color.red, Color.yellow, true);
                var shoot = BotOwner.ShootData;
                stringBuilder.AppendLabeledValue("Shoot: CanShootByState", $"{shoot?.CanShootByState}", Color.red, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Shoot: Shooting", $"{shoot?.Shooting}", Color.red, Color.yellow, true);
            }
        }

        public static void AddSAINEnemy(SAINComponentClass SAIN, StringBuilder stringBuilder)
        {
            if (SAIN.HasEnemy)
            {
                stringBuilder.AppendLabeledValue("SAIN: Enemy Name", $"{SAIN.Enemy.EnemyPlayer.name}", Color.red, Color.yellow, true);
                stringBuilder.AppendLabeledValue("SAIN: Enemy Time Since Seen", $"{SAIN.Enemy.TimeSinceSeen}", Color.red, Color.yellow, true);
                stringBuilder.AppendLabeledValue("SAIN: Can Shoot", $"{SAIN.Enemy.CanShoot}", Color.red, Color.yellow, true);
                stringBuilder.AppendLabeledValue("SAIN: Is Visible", $"{SAIN.Enemy.IsVisible}", Color.red, Color.yellow, true);
            }
        }

        public static void AddGoalEnemy(BotOwner BotOwner, StringBuilder stringBuilder)
        {
            var goalEnemy = BotOwner.Memory.GoalEnemy;
            if (goalEnemy != null)
            {
                var player = goalEnemy.Person as Player;
                stringBuilder.AppendLabeledValue("EFT: Enemy Name", $"{player.name}", Color.red, Color.yellow, true);
                stringBuilder.AppendLabeledValue("EFT: Can Shoot", $"{goalEnemy.CanShoot}", Color.red, Color.yellow, true);
                stringBuilder.AppendLabeledValue("EFT: Is Visible", $"{goalEnemy.IsVisible}", Color.red, Color.yellow, true);
            }
        }
    }
}