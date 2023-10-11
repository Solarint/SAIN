using Comfort.Common;
using EFT;
using SAIN.Preset.GlobalSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace SAIN.SAINComponent.Classes
{
    public class SAINNotLooking
    {
        private static Player MainPlayer;

        private static GeneralSettings Settings => SAINPlugin.LoadedPreset.GlobalSettings.General;

        public static float GetSpreadIncrease(BotOwner bot)
        {
            if (CheckIfPlayerNotLooking(bot))
            {
                return Settings.NotLookingAccuracyAmount;
            }

            return 0f;
        }

        public static float GetVisionSpeedIncrease(BotOwner bot)
        {
            if (CheckIfPlayerNotLooking(bot))
            {
                return Settings.NotLookingVisionSpeedModifier;
            }

            return 1f;
        }

        private static bool CheckIfPlayerNotLooking(BotOwner bot)
        {
            if (bot == null || bot.GetPlayer == null)
            {
                return false;
            }
            var enemy = bot.Memory.GoalEnemy;
            if (enemy == null)
            {
                return false;
            }
            if (GetMainPlayer())
            {
                Vector3 lookDir = MainPlayerLookDir.normalized;
                Vector3 playerPos = MainPlayerPosition;

                Vector3 botPos = bot.Position;
                Vector3 botDir = (botPos - playerPos).normalized;

                float angle = Vector3.Angle(botDir, lookDir);

                if (angle >= Settings.NotLookingAngle)
                {
                    if (!enemy.HaveSeenPersonal
                        || Time.time - enemy.PersonalSeenTime <= Settings.NotLookingTimeLimit
                        || !enemy.IsVisible)
                    {
                        if (SAINPlugin.DebugMode && DebugTimer < Time.time)
                        {
                            DebugTimer = Time.time + 1f;
                            Logger.LogDebug($"Player Not Looking at Bot");
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        private static float DebugTimer;

        private static bool GetMainPlayer()
        {
            if (MainPlayer == null)
            {
                MainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;
            }
            return MainPlayer != null;
        }

        private static Vector3 MainPlayerLookDir
        {
            get
            {
                if (MainPlayer != null)
                {
                    return MainPlayer.LookDirection;
                }
                return Vector3.down;
            }
        }
        private static Vector3 MainPlayerPosition
        {
            get
            {
                if (MainPlayer != null)
                {
                    return MainPlayer.Position;
                }
                return Vector3.zero;
            }
        }
    }
}
