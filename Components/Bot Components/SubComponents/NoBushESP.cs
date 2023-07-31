using BepInEx.Logging;
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
            NoBushMask = (LayerMaskClass.HighPolyWithTerrainMaskAI | (1 << LayerMask.NameToLayer("PlayerSpiritAura")));
            SAIN = GetComponent<SAINComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        private ManualLogSource Logger;

        private void Update()
        {
            if (SAIN == null) return;

            if (!NoBushESPToggle.Value)
            {
                NoBushESPActive = false;
                return;
            }

            var enemy = BotOwner.Memory.GoalEnemy;
            if (enemy != null && enemy?.IsVisible == true)
            {
                Player player = enemy?.Person as Player;
                if (player?.IsYourPlayer == true)
                {
                    if (NoBushTimer < Time.time)
                    {
                        NoBushTimer = Time.time + 0.1f;
                        NoBushESPActive = NoBushESPCheck(player);
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
            else
            {
                NoBushESPActive = false;
            }
        }

        public bool NoBushESPActive { get; private set; } = false;

        private float NoBushTimer = 0f;

        public bool NoBushESPCheck(IAIDetails player)
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
            return false;
        }

        private LayerMask NoBushMask;

        public static List<string> ExclusionList = new List<string> { "filbert", "fibert", "tree", "pine", "plant", "birch", "collider",
        "timber", "spruce", "bush", "metal", "wood", "grass" };
    }
}