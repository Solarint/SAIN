using System.Collections.Generic;

namespace SAIN.Preset.GlobalSettings;

public interface ISAINSettings
{
    void Update();
    object GetDefaults();
    void CreateDefault();
    void UpdateDefaults(object values);
    void Init(List<ISAINSettings> list);

    void Apply(BotSettingsComponents settings);
}