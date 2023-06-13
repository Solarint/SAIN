using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

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
                Shoot.Update();
            }
        }

        public FriendlyFireClass FriendlyFire { get; private set; }
        private readonly GClass105 Shoot;
    }
}
