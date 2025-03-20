using EFT;
using HarmonyLib;
using SAIN.Preset.GlobalSettings;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.EnemyClasses;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SAIN.Components
{
    public class PropertyNames
    {
        public static string PlayerSpirit = "PlayerSpiritAura";
        public static string Memory = "Memory";
        public static string GoalEnemy = "GoalEnemy";
        public static string ShootData = "ShootData";
        public static string CanShootByState = "CanShootByState";
        public static string IsVisible = "IsVisible";
    }

    public class SAINNoBushESP : MonoBehaviour
    {
        static SAINNoBushESP()
        {
            Type botType = typeof(BotOwner);

            Type memoryType = AccessTools.Field(
                botType, PropertyNames.Memory).FieldType;

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
        private BotComponent SAIN;

        public void Init(BotOwner botOwner, BotComponent sain = null)
        {
            if (NoBushMask == 0)
            {
                NoBushMask = LayerMaskClass.HighPolyWithTerrainMaskAI | (1 << LayerMask.NameToLayer(PropertyNames.PlayerSpirit));
            }
            BotOwner = botOwner;
            SAIN = sain;
        }

        private static NoBushESPSettings Settings => SAINPlugin.LoadedPreset.GlobalSettings.Look.NoBushESP;
        private static bool UserToggle => Settings.NoBushESPToggle;
        private static bool EnhancedChecks => Settings.NoBushESPEnhanced;
        private static float EnhancedRatio => Settings.NoBushESPEnhancedRatio;
        private static float Frequency => Settings.NoBushESPFrequency;
        private static bool DebugMode => Settings.NoBushESPDebugMode;

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
            Enemy sainEnemy = SAIN?.Enemy;
            var enemy = sainEnemy?.EnemyInfo ?? BotOwner?.Memory?.GoalEnemy;
            if (enemy != null && (enemy.IsVisible || enemy.CanShoot))
            {
                IPlayer person = enemy.Person;
                if (person != null && !person.IsAI)
                {
                    if (EnhancedChecks)
                    {
                        return NoBushESPCheckEnhanced(person);
                    }
                    else
                    {
                        return NoBushESPCheck(person);
                    }
                }
            }
            return false;
        }

        public bool NoBushESPCheck(IPlayer player)
        {
            Vector3 partPos = player.MainParts[BodyPartType.body].Position;
            return RayCast(partPos, HeadPosition);
        }

        public bool NoBushESPCheckEnhanced(IPlayer player)
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
                GameObject hitObject = hit.transform?.parent?.gameObject;
                if (hitObject != null)
                {
                    string hitName = hitObject?.name?.ToLower();
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
                    enemy.SetVisible(false);

                    BotOwner.AimingManager.CurrentAiming?.LoseTarget();

                    var vision = SAIN?.EnemyController.GetEnemy(enemy.ProfileId, false)?.Vision;
                    if (vision != null)
                    {
                        bool forceOff = true;
                        vision.UpdateCanShootState(forceOff);
                        vision.UpdateVisibleState(forceOff);
                    }
                }
            }
        }

        private static LayerMask NoBushMask = 0;
        private static readonly List<string> ExclusionList = new List<string> 
        { "filbert", "fibert", "tree", "pine", "plant", "birch", "collider", "timber", "spruce", "bush", "metal", "wood", "grass" };
    }
}