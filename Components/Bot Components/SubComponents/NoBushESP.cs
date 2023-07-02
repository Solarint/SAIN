using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;
using static SAIN.Editor.EditorSettings;

namespace SAIN.Classes
{
    public class NoBushESP : MonoBehaviour
    {
        private SAINComponent SAIN;
        private BotOwner BotOwner => SAIN?.BotOwner;

        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        private ManualLogSource Logger;

        private void Update()
        {
            if (SAIN == null) return;
            var enemy = BotOwner.Memory.GoalEnemy;
            if (enemy != null && enemy?.Person?.GetPlayer?.IsYourPlayer == true)
            {
                if (NoBushTimer < Time.time)
                {
                    NoBushTimer = Time.time + 0.1f;
                    NoBushESPActive = NoBushESPCheck();
                }
                if (NoBushESPActive)
                {
                    BotOwner.AimingData?.LoseTarget();
                    if (!SAIN.LayersActive)
                    {
                        BotOwner.ShootData.EndShoot();
                        enemy.SetCanShoot(false);
                    }
                }
            }
            else
            {
                NoBushESPActive = false;
            }
        }

        public bool NoBushESPActive { get; private set; } = false;

        private float NoBushTimer = 0f;

        public bool NoBushESPCheck()
        {
            if (!NoBushESPToggle.Value)
            {
                return false;
            }
            var enemy = BotOwner.Memory.GoalEnemy;
            if (enemy != null && enemy?.IsVisible == true)
            {
                Player player = enemy?.Person?.GetPlayer;
                if (player?.IsYourPlayer == true)
                {
                    Vector3 direction = player.MainParts[BodyPartType.head].Position - SAIN.HeadPosition;
                    if (Physics.Raycast(SAIN.HeadPosition, direction.normalized, out var hit, direction.magnitude, NoBushMask))
                    {
                        if (hit.transform?.parent?.gameObject == null)
                        {
                            return false;
                        }
                        string ObjectName = hit.transform?.parent?.gameObject?.name;
                        foreach (string exclusion in ExclusionList)
                        {
                            if (ObjectName.ToLower().Contains(exclusion))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static LayerMask NoBushMask => LayerMaskClass.HighPolyWithTerrainMaskAI;
        public static List<string> ExclusionList = new List<string> { "filbert", "fibert", "tree", "pine", "plant", "birch", "collider",
        "timber", "spruce", "bush", "metal", "wood", "grass" };
    }
}
