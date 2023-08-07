using BepInEx.Logging;
using EFT;
using SAIN.SAINComponent;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using System;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SAIN.Layers
{
    public static class DebugOverlay
    {
        private static readonly int OverlayCount = 11;

        private static int Selected = 0;

        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(DebugOverlay));

        public static void Update()
        {
        }

        private static void DisplayPropertyAndFieldValues(object obj, StringBuilder stringBuilder)
        {
            if (obj == null)
            {
                stringBuilder.AppendLine($"null");
                return;
            }

            Type type = obj.GetType();

            string name;
            object resultValue;

            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                name = field.Name;
                resultValue = field.GetValue(obj);
                string stringValue = null;
                if (resultValue != null)
                {
                    stringValue = resultValue.ToString();
                }

                stringBuilder.AppendLabeledValue(name, stringValue, Color.white, Color.yellow, true);
            }

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo property in properties)
            {
                name = property.Name;
                resultValue = property.GetValue(obj);
                string stringValue = null;
                if (resultValue != null)
                {
                    stringValue = resultValue.ToString();
                }

                stringBuilder.AppendLabeledValue(name, stringValue, Color.white, Color.yellow, true);
            }
        }

        public static void AddBaseInfo(SAINComponentClass sain, BotOwner botOwner, StringBuilder stringBuilder)
        {
            if (SAINPlugin.NextDebugOverlay.Value.IsDown())
            {
                Selected++;
            }
            if (SAINPlugin.PreviousDebugOverlay.Value.IsDown())
            {
                Selected--;
            }

            if (Selected > OverlayCount)
            {
                Selected = 0;
            }
            if (Selected < 0)
            {
                Selected = OverlayCount;
            }

            try
            {
                var info = sain.Info;
                var decisions = sain.Memory.Decisions;
                stringBuilder.AppendLine($"Name: [{sain.Person.Name}] Personality: [{info.Personality}] Type: [{info.Profile.WildSpawnType}]");
                stringBuilder.AppendLabeledValue("Main Decision", $"Current: {decisions.Main.Current} Last: {decisions.Main.Last}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Squad Decision", $"Current: {decisions.Squad.Current} Last: {decisions.Squad.Last}", Color.white, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Self Decision", $"Current: {decisions.Self.Current} Last: {decisions.Self.Last}", Color.white, Color.yellow, true);
                switch (Selected)
                {
                    case 0:
                        stringBuilder.AppendLine("SAIN Info");
                        stringBuilder.AppendLabeledValue("Personality and Type", $"{info.Personality} {info.Profile.WildSpawnType}", Color.white, Color.yellow, true);
                        stringBuilder.AppendLabeledValue("Power of Equipment", $"{info.Profile.PowerLevel}", Color.white, Color.yellow, true);
                        stringBuilder.AppendLabeledValue("Start Search + Hold Ground Time", $"{info.TimeBeforeSearch} + {info.HoldGroundDelay}", Color.white, Color.yellow, true);
                        stringBuilder.AppendLabeledValue("Difficulty + Modifier", $"{info.Profile.BotDifficulty} + {info.Profile.DifficultyModifier}", Color.white, Color.yellow, true);

                        stringBuilder.AppendLine("Aim Info");
                        stringBuilder.AppendLabeledValue("Shoot Modifier", $"{info.WeaponInfo.FinalModifier}", Color.white, Color.yellow, true);
                        AddAimData(botOwner, stringBuilder);
                        break;

                    case 1:
                        AddCoverInfo(sain, stringBuilder);
                        break;

                    case 2:
                        stringBuilder.AppendLine(nameof(BotOwner.AimingData));
                        DisplayPropertyAndFieldValues(botOwner.AimingData, stringBuilder);
                        break;

                    case 3:
                        stringBuilder.AppendLine(nameof(BotOwner.ShootData));
                        DisplayPropertyAndFieldValues(botOwner.ShootData, stringBuilder);
                        break;

                    case 4:
                        stringBuilder.AppendLine(nameof(SAINComponentClass.Info.FileSettings.Aiming));
                        DisplayPropertyAndFieldValues(sain.Info.FileSettings.Aiming, stringBuilder);
                        break;

                    case 5:
                        stringBuilder.AppendLine(nameof(SAINComponentClass.Info.FileSettings.Shoot));
                        DisplayPropertyAndFieldValues(sain.Info.FileSettings.Shoot, stringBuilder);
                        break;

                    case 6:
                        stringBuilder.AppendLine(nameof(SAINComponentClass.Info.FileSettings.Mind));
                        DisplayPropertyAndFieldValues(sain.Info.FileSettings.Mind, stringBuilder);
                        break;

                    case 7:
                        stringBuilder.AppendLine(nameof(SAINComponentClass.Info.FileSettings.Core));
                        DisplayPropertyAndFieldValues(sain.Info.FileSettings.Core, stringBuilder);
                        break;

                    case 8:
                        stringBuilder.AppendLine(nameof(SAINComponentClass.Enemy));
                        DisplayPropertyAndFieldValues(sain.Enemy, stringBuilder);
                        break;

                    case 9:
                        stringBuilder.AppendLine(nameof(SAINComponentClass.Enemy.Path));
                        DisplayPropertyAndFieldValues(sain.Enemy.Path, stringBuilder);
                        break;

                    case 10:
                        stringBuilder.AppendLine(nameof(SAINComponentClass.Enemy.Vision));
                        DisplayPropertyAndFieldValues(sain.Enemy.Vision, stringBuilder);
                        break;

                    case 11:
                        stringBuilder.AppendLine(nameof(BotOwner.Memory.GoalEnemy));
                        DisplayPropertyAndFieldValues(botOwner.Memory.GoalEnemy, stringBuilder);
                        break;

                    default: break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        public static void AddCoverInfo(SAINComponentClass SAIN, StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine(nameof(SAINComponentClass.Cover));
            DisplayPropertyAndFieldValues(SAIN.Cover, stringBuilder);
            DisplayPropertyAndFieldValues(SAIN.Cover.CoverInUse, stringBuilder);
        }

        public static void AddAimData(BotOwner BotOwner, StringBuilder stringBuilder)
        {
            var aimData = BotOwner.AimingData;
            if (aimData != null)
            {
                stringBuilder.AppendLine(nameof(BotOwner.AimingData));
                stringBuilder.AppendLabeledValue("Aim: AimComplete?", $"{aimData.IsReady}", Color.red, Color.yellow, true);
                var shoot = BotOwner.ShootData;
                stringBuilder.AppendLabeledValue("Shoot: CanShootByState", $"{shoot?.CanShootByState}", Color.red, Color.yellow, true);
                stringBuilder.AppendLabeledValue("Shoot: Shooting", $"{shoot?.Shooting}", Color.red, Color.yellow, true);
                DisplayPropertyAndFieldValues(BotOwner.AimingData, stringBuilder);
            }
        }
    }
}