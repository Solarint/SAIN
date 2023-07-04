using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.Plugin.Config
{
    internal abstract class ConfigAbstract
    {
        public static ConfigFile SAINConfig => SAINPlugin.SAINConfig;
    }
}
