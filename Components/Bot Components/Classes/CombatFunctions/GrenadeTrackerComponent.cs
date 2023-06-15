using CustomPlayerLoopSystem;
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

            if (EnemyGrenadeHeard())
            {
                SpotGrenade();
            }
        }

        private void Awake()
        {
            BotOwner = GetComponent<BotOwner>();
        }

        private int FrameCount = 0; 

        private bool EnemyGrenadeHeard()
        {
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

            if (!GrenadeSpotted && FrameCount == 5)
            {
                FrameCount = 0;
                Vector3 grenadePos = Grenade.transform.position;
                Vector3 headPos = BotOwner.LookSensor._headPoint;
                Vector3 grenadeDir = grenadePos - headPos;
                if (Vector3.Dot(grenadeDir.normalized, BotOwner.LookDirection.normalized) > 0f && NadeVisible(grenadePos, headPos))
                {
                    SpotGrenade();
                }
            }

            if (GrenadeSpotted && !CanReact && TimeSeen + ReactionTime < Time.time)
            {
                CanReact = true;
            }

            FrameCount++;
        }

        private void SpotGrenade()
        {
            GrenadeSpotted = true;
            TimeSeen = Time.time;
        }

        public Grenade Grenade { get; private set; }
        public Vector3 DangerPoint { get; private set; }
        public bool GrenadeSpotted { get; private set; }
        public bool CanReact { get; private set; }

        private float TimeSeen = 0f;
        private float ReactionTime = 0f;

        private bool NadeVisible(Vector3 grenadeDirection, Vector3 headPosition)
        {
            return !Physics.Raycast(headPosition, grenadeDirection, grenadeDirection.magnitude, LayerMaskClass.HighPolyWithTerrainMask);
        }
    }
}
