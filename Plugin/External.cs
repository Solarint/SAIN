using EFT;
using SAIN.SAINComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.Plugin
{
    public static class External
    {
        public static bool ExtractBot(BotOwner bot)
        {
            var component = bot.GetComponent<SAINComponentClass>();
            if (component == null)
            {
                return false;
            }

            component.Info.ForceExtract = true;

            return true;
        }
    }
}
