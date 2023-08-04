using BepInEx.Logging;
using EFT;
using HarmonyLib;
using SAIN.Components;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;
using SAIN.SAINPreset.Settings;

namespace SAIN.Classes
{
    public class PropertyNames
    {
        public static string PlayerSpirit = "PlayerSpiritAura";
        public static string Memory = nameof(BotOwner.Memory);
        public static string GoalEnemy = nameof(BotOwner.Memory.GoalEnemy);
        public static string ShootData = nameof(BotOwner.ShootData);
        public static string CanShootByState = nameof(BotOwner.ShootData.CanShootByState);
        public static string IsVisible = nameof(BotOwner.Memory.GoalEnemy.IsVisible);
    }

    public class NoBushESP : MonoBehaviour
    {
        static NoBushESP()
        {
            NoBushMask = LayerMaskClass.HighPolyWithTerrainMaskAI | (1 << LayerMask.NameToLayer(PropertyNames.PlayerSpirit));
            Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(NoBushESP));

            Type botType = typeof(BotOwner);

            Type memoryType = AccessTools.Property(
                botType, PropertyNames.Memory).PropertyType;

            GoalEnemyProp = AccessTools.Property(
                memoryType, PropertyNames.GoalEnemy);

            IsVisibleProp = AccessTools.Property(
                GoalEnemyProp.PropertyType, PropertyNames.IsVisible);

            Type shootDataType = AccessTools.Property(
                botType, PropertyNames.ShootData).PropertyType;

            CanShootByState = AccessTools.PropertySetter(
                shootDataType, PropertyNames.CanShootByState);
        }

        private static readonly PropertyInfo GoalEnemyProp;
        private static readonly PropertyInfo IsVisibleProp;
        private static readonly MethodInfo CanShootByState;

        private BotOwner BotOwner;
        private SAINComponent SAIN;

        public void Init(BotOwner botOwner, SAINComponent sain = null)
        {
            BotOwner = botOwner;
            SAIN = sain;
        }

        private static GeneralSettings GeneralSettings => SAINPlugin.LoadedPreset?.GlobalSettings?.General;
        private static bool UserToggle => GeneralSettings?.NoBushESPToggle == true;
        private static bool EnhancedChecks => GeneralSettings?.NoBushESPEnhanced == true;
        private static float EnhancedRatio => GeneralSettings == null ? 0.5f : GeneralSettings.NoBushESPEnhancedRatio;
        private static float Frequency => GeneralSettings == null ? 0.1f : GeneralSettings.NoBushESPFrequency;
        private static bool DebugMode => GeneralSettings?.NoBushESPDebugMode == true;

        private static readonly ManualLogSource Logger;

        private void Update()
        {
            if (BotOwner == null || !UserToggle)
            {
                NoBushESPActive = false;
                return;
            }

            if (NoBushTimer < Time.time)
            {
                NoBushTimer = Time.time + Frequency;
                bool active = NoBushESPCheck();
                SetCanShoot(active);
            }
        }

        public bool NoBushESPActive { get; private set; } = false;

        private float NoBushTimer = 0f;
        private Vector3 HeadPosition => BotOwner.LookSensor._headPoint;

        public bool NoBushESPCheck()
        {
            var enemy = BotOwner?.Memory?.GoalEnemy;
            if (enemy != null && (enemy.IsVisible || enemy.CanShoot))
            {
                Player player = enemy?.Person as Player;
                if (player?.IsYourPlayer == true)
                {
                    if (EnhancedChecks)
                    {
                        return NoBushESPCheckEnhanced(player);
                    }
                    else
                    {
                        return NoBushESPCheck(player);
                    }
                }
            }
            return false;
        }

        public bool NoBushESPCheck(IAIDetails player)
        {
            Vector3 partPos = player.MainParts[BodyPartType.body].Position;
            return RayCast(partPos, HeadPosition);
        }

        public bool NoBushESPCheckEnhanced(IAIDetails player)
        {
            int hitCount = 0;
            int partCount = player.MainParts.Count;
            Vector3 start = HeadPosition;
            foreach (var part in player.MainParts)
            {
                if (RayCast(part.Value.Position, start))
                {
                    hitCount++;
                }
            }
            float ratio = (float)hitCount / partCount;
            bool active = ratio >= EnhancedRatio;
            if (active && DebugMode)
            {
                Logger.LogDebug($"Enhanced Active: [{ratio}] visible from hit count: [{hitCount}] / [{partCount}]. Config Value: [{EnhancedRatio}]");
            }
            return active;
        }

        private static bool RayCast(Vector3 end, Vector3 start)
        {
            Vector3 direction = end - start;
            if (Physics.Raycast(start, direction.normalized, out var hit, direction.magnitude, NoBushMask))
            {
                string hitName = hit.transform?.parent?.gameObject?.name.ToLower();
                foreach (string exclusion in ExclusionList)
                {
                    if (hitName.Contains(exclusion))
                    {
                        if (DebugMode)
                        {
                            Logger.LogDebug(exclusion);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public void SetCanShoot(bool blockShoot)
        {
            NoBushESPActive = blockShoot;
            if (blockShoot)
            {
                var enemy = BotOwner?.Memory?.GoalEnemy;
                if (enemy != null)
                {
                    if (DebugMode)
                    {
                        Logger.LogDebug("No Bush ESP active");
                    }

                    enemy.SetCanShoot(false);

                    IsVisibleProp.SetValue(enemy, false);

                    BotOwner.AimingData?.LoseTarget();
                    BotOwner.ShootData?.EndShoot();

                    // Use reflection to set the blockShoot of the property
                    var shoot = BotOwner.ShootData;
                    if (shoot != null)
                    {
                        CanShootByState.Invoke(shoot, new object[] { false });
                    }
                    var vision = SAIN?.Enemy?.EnemyVision;
                    if (vision != null)
                    {
                        vision.UpdateCanShoot(false);
                        vision.UpdateVisible(false);
                    }
                }
            }
        }

        private static readonly LayerMask NoBushMask;
        private static readonly List<string> ExclusionList = new List<string> 
        { "filbert", "fibert", "tree", "pine", "plant", "birch", "collider", "timber", "spruce", "bush", "metal", "wood", "grass" };
    }
}