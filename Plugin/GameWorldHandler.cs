using Comfort.Common;
using EFT;
using SAIN.Components;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN
{
    public class GameWorldHandler
    {
        public static void Update()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null && SAINGameWorld != null)
            {
                Object.Destroy(SAINGameWorld);
            }
            else if (gameWorld != null && SAINGameWorld == null)
            {
                SAINGameWorld = gameWorld.GetOrAddComponent<SAINGameworldComponent>();
            }
        }

        public static SAINGameworldComponent SAINGameWorld { get; private set; }
        public static SAINBotControllerComponent SAINBotController => SAINGameWorld?.SAINBotController;
        public static SAINMainPlayerComponent SAINMainPlayer => SAINGameWorld?.SAINMainPlayer;
    }
}
