using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SAIN.Components
{
    public class GrenadeTracker : MonoBehaviour
    {
        private BotOwner BotOwner;

        public void Initialize(Grenade grenade, Vector3 dangerPoint, float reactionTime)
        {
            DangerPoint = dangerPoint;
            Grenade = grenade;
            ReactionTime = reactionTime;
        }

        private void Awake()
        {
            BotOwner = GetComponent<BotOwner>();
        }

        private void Update()
        {
            if (BotOwner.IsDead || Grenade == null)
            {
                StopAllCoroutines();
                Destroy(this);
                return;
            }

            if (!GrenadeSpotted)
            {
                if (BotOwner.LookSensor.IsPointInVisibleSector(Grenade.transform.position))
                {
                    if (NadeVisible(Grenade.transform.position, BotOwner.LookSensor._headPoint))
                    {
                        GrenadeSpotted = true;
                        TimeSeen = Time.time;
                    }
                }
            }
            else
            {
                if (TimeSeen + ReactionTime < Time.time)
                {
                    BotOwner.BewareGrenade.AddGrenadeDanger(DangerPoint, Grenade);
                }
            }
        }

        private Grenade Grenade;
        private Vector3 DangerPoint;
        private bool GrenadeSpotted = false;
        private float TimeSeen = 0f;
        private float ReactionTime = 0f;

        private static bool NadeVisible(Vector3 grenadePosition, Vector3 playerPosition)
        {
            Vector3 direction = grenadePosition - playerPosition;

            if (!Physics.Raycast(grenadePosition, direction, direction.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
            {
                return true;
            }

            return false;
        }
    }
}
