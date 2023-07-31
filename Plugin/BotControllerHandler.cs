using Comfort.Common;
using EFT;
using SAIN.Components;
using System.Collections.Generic;

namespace SAIN
{
    public class BotControllerHandler
    {
        public static void Update()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                if (BotController != null)
                {
                    ComponentAdded = false;
                    BotController = null;
                }
                return;
            }

            if (!ComponentAdded || BotController == null)
            {
                BotController = gameWorld.GetOrAddComponent<SAINBotController>();
                ComponentAdded = true;
            }
        }

        public static SAINBotController BotController { get; private set; }
        static bool ComponentAdded = false;
    }
}
