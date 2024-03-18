using CustomPlayerLoopSystem;
using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SAIN.SAINComponent.SubComponents
{
    public class GrenadeTracker : MonoBehaviour
    {
        private BotOwner BotOwner;

        public void Initialize(Grenade grenade, Vector3 dangerPoint, float reactionTime)
        {
            DangerPoint = dangerPoint;
            Grenade = grenade;

            if (EnemyGrenadeHeard())
            {
                GrenadeSpotted = true;
            }
        }

        private void Awake()
        {
            BotOwner = GetComponent<BotOwner>();
        }

        private bool EnemyGrenadeHeard()
        {
            if (BotOwner == null || BotOwner.IsDead || Grenade == null)
            {
                return false;
            }

            return (Grenade.transform.position - BotOwner.Position).sqrMagnitude < 100f;
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
                if ((Grenade.transform.position - BotOwner.Position).sqrMagnitude < 10f * 10f)
                {
                    GrenadeSpotted = true;
                    return;
                }

                Vector3 grenadePos = Grenade.transform.position;
                Vector3 headPos = BotOwner.LookSensor._headPoint;
                Vector3 grenadeDir = grenadePos - headPos;
                if (!Physics.Raycast(headPos, grenadeDir, grenadeDir.magnitude, LayerMaskClass.HighPolyWithTerrainMask))
                {
                    GrenadeSpotted = true;
                }
            }
        }

        private void SpotGrenade()
        {
            GrenadeSpotted = true;
        }

        public Grenade Grenade { get; private set; }
        public Vector3 DangerPoint { get; private set; }
        public bool GrenadeSpotted { get; private set; }
        public bool CanReact { get; private set; }

        private bool NadeVisible(Vector3 grenadeDirection, Vector3 headPosition)
        {
            return !Physics.Raycast(headPosition, grenadeDirection, grenadeDirection.magnitude, LayerMaskClass.HighPolyWithTerrainMask);
        }
    }
}
