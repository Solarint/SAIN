using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.Helpers
{
    internal class Utility
    {
        public static ManualLogSource Logger { get; private set; } 
            = BepInEx.Logging.Logger.CreateLogSource("SAIN Save/Load Utility");
    }
}
