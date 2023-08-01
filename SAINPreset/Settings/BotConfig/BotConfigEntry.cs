using SAIN.BotSettings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static EFT.SpeedTree.TreeWind;

namespace SAIN.SAINPreset.Settings.BotConfig
{
    public class BotConfigEntry<T>
    {
        public string Name;
        public Type Type = typeof(T);
        public T GlobalValue;
        public bool IsGlobal = false;

        public BotConfigEntryDefinition Info;
        public BotConfigEntryValues<T> Values;
        public BotConfigEntryProperties<T> Properties;
    }

    public class BotConfigEntryValues<T>
    {
        public T Easy;
        public T Normal;
        public T Hard;
        public T Impossible;

        public T GetValue(BotDifficulty difficulty)
        {
            switch (difficulty)
            {
                case BotDifficulty.easy:
                    return Easy;

                case BotDifficulty.normal:
                    return Normal;

                case BotDifficulty.hard:
                    return Hard;

                case BotDifficulty.impossible:
                    return Impossible;

                default:
                    return Normal;
            }
        }

        public void SetValue(BotDifficulty difficulty, T value)
        {
            switch (difficulty)
            {
                case BotDifficulty.easy:
                    Easy = value; break;

                case BotDifficulty.normal:
                    Normal = value; break;

                case BotDifficulty.hard:
                    Hard = value; break;

                case BotDifficulty.impossible:
                    Impossible = value; break;

                default: break;
            }
        }

        public void SetAllValues(T value)
        {
            Easy = value;
            Normal = value;
            Hard = value;
            Impossible = value;
        }
    }

    public class BotConfigEntryProperties<T>
    {
        public T Max;
        public T Min;
        public float Rounding = 1f;
    }

    public class BotConfigEntryDefinition
    {
        public string Key;
        public string DisplayName;
        public string Description;
        public string LongDescription;
        public string ToolTip;
    }

    public static class BotConfigEntryHelpers
    {
        static FieldInfo[] SettingsSections;

        public static T GetValue<T>(BotConfigEntry<T> entry, BotDifficulty difficulty)
        {
            if (entry?.Values == null)
            {
                return default;
            }

            return entry.Values.GetValue(difficulty);
        }
        public static void SetValue<T>(BotConfigEntry<T> entry, BotDifficulty difficulty, T value)
        {
            entry?.Values?.SetValue(difficulty, value);
        }
        public static void SetAllValues<T>(BotConfigEntry<T> entry, T value)
        {
            if (entry?.Values == null)
            {
                return;
            }

            entry.Values.Easy = value;
            entry.Values.Normal = value;
            entry.Values.Hard = value;
            entry.Values.Impossible = value;
        }

        public static BotConfigEntry<T> CreateBasic<T>(T value, string name, string description = null)
        {
            BotConfigEntry<T> entry = new BotConfigEntry<T>
            {
                Name = name,
                Values = new BotConfigEntryValues<T>(),
                Info = description == null ? null : new BotConfigEntryDefinition
                {
                    Key = name,
                    Description = description
                }
            };
            entry.Values.SetAllValues(value);
            return null;
        }

        public static BotConfigEntry<float> Template(float type)
        {
            string key = null;

            return new BotConfigEntry<float>
            {
                Name = key,
                Info = new BotConfigEntryDefinition
                {
                    Key = key,
                    DisplayName = null,
                    Description = null,
                    LongDescription = null,
                    ToolTip = null
                },
                Values = new BotConfigEntryValues<float>
                {
                    Easy = default,
                    Normal = default,
                    Hard = default,
                    Impossible = default,
                },
                Properties = new BotConfigEntryProperties<float>
                {
                    Max = default,
                    Min = default,
                    Rounding = 1f,
                },
            };
        }

        public static BotConfigEntry<int> Template(int type)
        {
            string key = null;

            return new BotConfigEntry<int>
            {
                Name = key,
                Info = new BotConfigEntryDefinition
                {
                    Key = key,
                    DisplayName = null,
                    Description = null,
                    LongDescription = null,
                    ToolTip = null
                },
                Values = new BotConfigEntryValues<int>
                {
                    Easy = default,
                    Normal = default,
                    Hard = default,
                    Impossible = default,
                },
                Properties = new BotConfigEntryProperties<int>
                {
                    Max = default,
                    Min = default
                },
            };
        }

        public static BotConfigEntry<bool> Template(bool type)
        {
            string key = null;

            return new BotConfigEntry<bool>
            {
                Name = key,
                Info = new BotConfigEntryDefinition
                {
                    Key = key,
                    DisplayName = null,
                    Description = null,
                    LongDescription = null,
                    ToolTip = null
                },
                Values = new BotConfigEntryValues<bool>
                {
                    Easy = default,
                    Normal = default,
                    Hard = default,
                    Impossible = default,
                },
            };
        }
    }
}