using SAIN.Attributes;
using System.ComponentModel;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories
{
    public class SAINBossSettings
    {
        [Advanced(AdvancedEnum.Hidden)][DefaultValue(false)]
        public bool SET_CHEAT_VISIBLE_WHEN_ADD_TO_ENEMY = false;
    }
}