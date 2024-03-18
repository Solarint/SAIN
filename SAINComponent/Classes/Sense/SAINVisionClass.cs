using BepInEx.Logging;
using EFT;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.Sense;
using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public class SAINVisionClass : SAINBase, ISAINClass
    {
        public SAINVisionClass(SAINComponentClass component) : base(component)
        {
            FlashLightDazzle = new DazzleClass(component);
        }

        public void Init()
        {
        }

        public void Update()
        {
            var Enemy = SAIN.Enemy;
            if (Enemy?.EnemyIPlayer != null && Enemy?.IsVisible == true)
            {
                FlashLightDazzle.CheckIfDazzleApplied(Enemy);
            }
        }

        public void Dispose()
        {
        }

        public DazzleClass FlashLightDazzle { get; private set; }
    }
}
