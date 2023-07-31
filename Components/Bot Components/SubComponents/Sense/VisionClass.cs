using BepInEx.Logging;
using EFT;
using SAIN.Components;
using SAIN.Helpers;
using UnityEngine;

namespace SAIN.Classes.Sense
{
    public class VisionClass : SAINBot
    {
        public VisionClass(SAINComponent component) : base(component)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            FlashLightDazzle = new FlashLightDazzle(component);
        }

        public void Update()
        {
            var Enemy = SAIN.Enemy;
            if (Enemy?.Person != null && Enemy?.IsVisible == true)
            {
                FlashLightDazzle.CheckIfDazzleApplied(Enemy.Person);
            }
        }

        public FlashLightDazzle FlashLightDazzle { get; private set; }

        private ManualLogSource Logger;
    }
}
