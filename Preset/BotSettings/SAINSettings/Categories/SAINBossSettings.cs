using SAIN.Attributes;
using System.ComponentModel;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public class SAINBossSettings
    {
        [Hidden]
        [Default(false)]
        public bool SET_CHEAT_VISIBLE_WHEN_ADD_TO_ENEMY = false;
    }
}