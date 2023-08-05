using BepInEx.Logging;
using EFT;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.SAINComponent;
using UnityEngine;

namespace SAIN.SAINComponent.Classes
{
    public class VisionClass : SAINBase, ISAINClass
    {
        public VisionClass(SAINComponentClass component) : base(component)
        {
            FlashLightDazzle = new FlashLightDazzle(component);
        }

        public void Init()
        {
        }

        public void Update()
        {
            var Enemy = SAIN.Enemy;
            if (Enemy?.Person != null && Enemy?.IsVisible == true)
            {
                FlashLightDazzle.CheckIfDazzleApplied(Enemy.Person);
            }
        }

        public void Dispose()
        {
        }

        public FlashLightDazzle FlashLightDazzle { get; private set; }
    }
}
