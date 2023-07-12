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
                ComponentAdded = false;
                if (BotController != null)
                {
                    BotController = null;
                }
                return;
            }

            // AddorUpdateColorScheme Components to main player
            if (!ComponentAdded)
            {
                BotController = gameWorld.GetOrAddComponent<SAINBotController>();
                ComponentAdded = true;
            }
        }

        public static SAINBotController BotController { get; private set; }
        public static bool ComponentAdded { get; private set; }
    }
}
