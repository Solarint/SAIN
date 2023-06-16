using EFT;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Classes.CombatFunctions
{
    public class ShootClass : SAINBot
    {
        public ShootClass(BotOwner owner) : base(owner)
        {
            Shoot = new GClass105(owner);
            FriendlyFire = new FriendlyFireClass(owner);
        }

        public void Update()
        {
            if (SAIN.Enemy == null)
            {
                return;
            }

            FriendlyFire.Update();

            if (SAIN.Enemy.IsVisible && FriendlyFire.ClearShot)
            {
                if (NoBushESP(SAIN.Enemy.Person))
                {
                    FriendlyFire.StopShooting();
                    return;
                }
                Shoot.Update();
            }
        }

        private static LayerMask NoBushMask => LayerMaskClass.HighPolyWithTerrainMaskAI;

        private bool NoBushESP(IAIDetails person)
        {
            if (person.GetPlayer.IsYourPlayer)
            {
                Vector3 start = SAIN.HeadPosition;
                Vector3 direction = person.MainParts[BodyPartType.body].Position - start;
                if (Physics.Raycast(start, direction, out var hitInfo, direction.magnitude, NoBushMask))
                {
                    string ObjectName = hitInfo.transform.parent?.gameObject?.name;
                    foreach (string exclusion in ExclusionList)
                    {
                        if (ObjectName.ToLower().Contains(exclusion))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static List<string> ExclusionList = new List<string> { "filbert", "fibert", "tree", "pine", "plant", "birch", "collider",
        "timber", "spruce", "bush", "metal", "wood"};

        public FriendlyFireClass FriendlyFire { get; private set; }
        private readonly GClass105 Shoot;
    }
}
