using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Classes
{
    public class NoBushESP : MonoBehaviour
    {
        private SAINComponent SAIN;
        private BotOwner BotOwner => SAIN.BotOwner;
        private SAINEnemy Enemy => SAIN.Enemy;

        private void Awake()
        {
            SAIN = GetComponent<SAINComponent>();
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
        }

        private ManualLogSource Logger;

        private void Update()
        {
            if (BotOwner == null || SAIN == null) return;
            if (Enemy != null)
            {
                if (NoBushTimer < Time.time)
                {
                    NoBushTimer = Time.time + 0.25f;
                    //NoBushESPActive = NoBushESPCheck();
                }
                if (NoBushESPActive)
                {
                    //BotOwner.ShootData?.EndShoot();
                    //BotOwner.AimingData?.LoseTarget();
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
            if (Enemy != null && Enemy.Person?.GetPlayer != null && Enemy.IsVisible)
            {
                Player player = Enemy.Person.GetPlayer;
                if (player.IsYourPlayer)
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
        public static List<string> ExclusionList = new List<string> { "filbert", "fibert", "pine", "plant", "birch",
        "timber", "spruce", "bush", "grass" };
    }
}
