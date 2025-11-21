using SAIN.Attributes;
using SAIN.Preset.GlobalSettings;

namespace SAIN.Preset.BotSettings.SAINSettings.Categories;

public class SAINBossSettings : SAINSettingsBase<SAINBossSettings>, ISAINSettings
{
    [Hidden]
    public bool SET_CHEAT_VISIBLE_WHEN_ADD_TO_ENEMY = false;
    public override void Apply(BotSettingsComponents settings)
    {
        settings.Boss.SET_CHEAT_VISIBLE_WHEN_ADD_TO_ENEMY = SET_CHEAT_VISIBLE_WHEN_ADD_TO_ENEMY;
    }
}